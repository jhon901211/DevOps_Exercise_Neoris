#!/bin/bash
GREEN="\033[0;32m"
NOCOLOR="\033[0m"  
RED="\033[0;31m"
YELLOW="\033[1;33m"  
CYAN="\033[0;36m"

ENVIRONMENT=$1
ECS_SERVICE=$2
ECS_CLUSTER=$3
REGION=$4



echo -e "${GREEN}Install dependencies...${NOCOLOR}"
yum -y install jq
aws --version

echo -e "${GREEN}Assume role...${NOCOLOR}"
ROLE=$(aws sts assume-role --role-arn "$AWS_ASSUME_ROLE" --role-session-name "GitlabAccess" --output json)
AccessKeyId=$(echo "$ROLE" | jq -r '.Credentials.AccessKeyId')
SecretKeyId=$(echo "$ROLE" | jq -r '.Credentials.SecretAccessKey')
Token=$(echo "$ROLE" | jq -r '.Credentials.SessionToken')
export AWS_ACCESS_KEY_ID=$AccessKeyId
export AWS_SECRET_ACCESS_KEY=$SecretKeyId
export AWS_SESSION_TOKEN=$Token

printf "========== Deploy service ${GREEN} ${ECS_SERVICE} ${NOCOLOR} in ${GREEN} ${ENVIRONMENT} ${NOCOLOR} ==========\n"
printf "starting deployment validation......\n"
#get pre-deploy event
describeServicesResponse=$(aws ecs describe-services --services ${ECS_SERVICE} --cluster ${ECS_CLUSTER} --region ${REGION} --query 'services[0].{eventsId: events[0].id}')
previousEventId=$(echo "$describeServicesResponse" | jq -r '.eventsId')
#update service
reponse=$(aws ecs update-service --force-new-deployment --service ${ECS_SERVICE} --deployment-configuration 'maximumPercent=200,minimumHealthyPercent=100' --cluster ${ECS_CLUSTER} --region ${REGION} --query 'services[].loadBalancers[*].{targetGroupArn: targetGroupArn}')
printf "waiting for the creation of new tasks...\n\n"

describeServicesResponse=$(aws ecs describe-services --services ${ECS_SERVICE} --cluster ${ECS_CLUSTER} --region ${REGION} --query 'services[0].{deploymentCount: length(deployments[*]), desiredCount: desiredCount, runningCount: runningCount, pendingCount: pendingCount, deploymentId: deployments[0].id}')
deploymentCount=$(echo "$describeServicesResponse" | jq -r '.deploymentCount')
desiredCount=$(echo "$describeServicesResponse" | jq -r '.desiredCount')
runningCount=$(echo "$describeServicesResponse" | jq -r '.runningCount')
pendingCount=$(echo "$describeServicesResponse" | jq -r '.pendingCount')
deploymentId=$(echo "$describeServicesResponse" | jq -r '.deploymentId')
deploymentRetries=`expr $desiredCount \* 4`

if [ "$desiredCount" \> "0" ] 
then
	targetGropups=$(aws ecs describe-services --services ${ECS_SERVICE} --cluster ${ECS_CLUSTER} --region ${REGION} --query 'services[].loadBalancers[].{targetGroupArn: targetGroupArn}' --output text)
	deploymentCompleted=0
	deploymentSuccess=0
	while [ "${deploymentCount}" -gt 1 ] & [ "${deploymentCompleted}" -eq 0 ];
	do
		all_task_by_service=$(aws ecs list-tasks --cluster ${ECS_CLUSTER} --started-by ${deploymentId} --query 'taskArns[*]' --output text --region ${REGION})
		for task in ${all_task_by_service}
		do 
			#get the desired status and the last reported status in the service
			describeTasks=$(aws ecs describe-tasks --cluster ${ECS_CLUSTER} --tasks ${task} --query 'tasks[].{desiredStatus: desiredStatus, lastStatus: lastStatus, privateIpv4Address: containers[].networkInterfaces[].privateIpv4Address, stopCode: stopCode, stoppedReason: stoppedReason}' --output json)
			lastStatus=$(echo "$describeTasks" | jq -r '.[0].lastStatus')
			desiredStatus=$(echo "$describeTasks" | jq -r '.[0].desiredStatus')
			stopCode=$(echo "$describeTasks" | jq -r '.[0].stopCode')
			stoppedReason=$(echo "$describeTasks" | jq -r '.[0].stoppedReason')
			ipTask=$(echo "$describeTasks" | jq -r '.[0].privateIpv4Address[0]')
			taskTemp=$(echo $task | cut -d "/" -f 3)
			if  [ "$desiredStatus" = "RUNNING" ] 
			then
				printf "◘ deploying service ${YELLOW} ${ECS_SERVICE}${NOCOLOR} (Id Task (${ipTask} | ${taskTemp}) | desired status:${GREEN} ${desiredStatus} ${NOCOLOR}| lastStatus: ${GREEN} ${lastStatus} ${NOCOLOR} \n"
			elif [ "$desiredStatus" = "STOPPED" ] 
			then
				printf "◘ Deploying service ${YELLOW} ${ECS_SERVICE}${NOCOLOR} (Id Task (${ipTask} | ${taskTemp}) | desired status:${RED} ${desiredStatus} ${NOCOLOR}| lastStatus: ${GREEN} ${lastStatus} ${NOCOLOR} \n"
				printf "  |--> stopCode:${CYAN} ${stopCode} ${NOCOLOR}| stoppedReason:${CYAN} ${stoppedReason} ${NOCOLOR}\n"
			else
				printf "◘ Deploying service ${YELLOW} ${ECS_SERVICE}${NOCOLOR} (Id Task (${ipTask} | ${taskTemp}) | desired status:${YELLOW} ${desiredStatus} ${NOCOLOR}| lastStatus: ${GREEN} ${lastStatus} ${NOCOLOR} \n"
				printf "  |--> stopCode:${CYAN} ${stopCode} ${NOCOLOR}| stoppedReason:${CYAN} ${stoppedReason} ${NOCOLOR}\n"
			fi
			#evaluate the health check in the associated target group
			for targetGroup in ${targetGropups}
			do
				targetGroupStatus=$(aws elbv2 describe-target-health --target-group-arn ${targetGroup} --targets Id=${ipTask} --query 'TargetHealthDescriptions[].{Id: Target.Id, State: TargetHealth.State}')
				targetGroupName=$(echo $targetGroup | cut -d "/" -f 2)
				ipAddress=$(echo "$targetGroupStatus" | jq -r '.[0].Id')
				state=$(echo "$targetGroupStatus" | jq -r '.[0].State')
				if  [ "$state" = "healthy" ] 
				then
					printf "  |--> Target Group:${YELLOW} ${targetGroupName} ${NOCOLOR}| IP address:${GREEN} ${ipAddress} ${NOCOLOR}| State:${GREEN} ${state} ${NOCOLOR}\n"
				elif [ "$state" = "unhealthy" ] 
				then
					printf "  |--> Target Group:${YELLOW} ${targetGroupName} ${NOCOLOR}| IP address:${GREEN} ${ipAddress} ${NOCOLOR}| State:${RED} ${state} ${NOCOLOR}\n"
				else
					printf "  |--> Target Group:${YELLOW} ${targetGroupName} ${NOCOLOR}| IP address:${GREEN} ${ipAddress} ${NOCOLOR}| State:${CYAN} ${state} ${NOCOLOR}\n"
				fi
				
			done
			#get the details of the deployment event
			describeServicesResponse=$(aws ecs describe-services --services ${ECS_SERVICE} --cluster ${ECS_CLUSTER} --region ${REGION} --query 'services[0].{deploymentCount: length(deployments[*]), events: events[0].message, date: events[0].createdAt, eventsId: events[0].id, desiredCount: desiredCount, runningCount: runningCount, pendingCount: pendingCount}')
			deploymentCount=$(echo "$describeServicesResponse" | jq -r '.deploymentCount')
			eventMessage=$(echo "$describeServicesResponse" | jq -r '.events')
			eventDate=$(echo "$describeServicesResponse" | jq -r '.date')
			eventId=$(echo "$describeServicesResponse" | jq -r '.eventsId')
			desiredCount=$(echo "$describeServicesResponse" | jq -r '.desiredCount')
			runningCount=$(echo "$describeServicesResponse" | jq -r '.runningCount')
			pendingCount=$(echo "$describeServicesResponse" | jq -r '.pendingCount')
			printf "  |--> ${eventDate} - ${eventMessage}\n\n"
			
			if [ "$eventId" != "$previousEventId" ]; 
			then
				# if [ $eventMessage =~ .*"deployment completed".* ] || [ $eventMessage =~ .*"has reached a steady state".* ];
				# if [[ "$eventMessage" =  *"has reached a steady state"* ]];
				# then
				#     deploymentCompleted=1
				#     deploymentSuccess=1
				# else 
				#     printf "${runningCount} Running | ${pendingCount} Pending | ${desiredCount} Desired\n"
				#     printf "Validating task status. Please wait...\n\n"
				# fi
				case "$eventMessage" in
					*"deployment completed"* ) 
						deploymentCompleted=1
						deploymentSuccess=1
					;;
					*"has reached a steady state"* ) 
						deploymentCompleted=1
						deploymentSuccess=1
					;;
					* ) 
						printf "${runningCount} Running | ${pendingCount} Pending | ${desiredCount} Desired\n"
						printf "Validating task status. Please wait...\n\n"
					;;
				esac                    
			fi
			sleep 1;
		done

		#evaluate if the deployment has generated failed tasks
		listTasksStoppedCount=$(aws ecs list-tasks --cluster ${ECS_CLUSTER} --started-by ${deploymentId} --desired-status STOPPED --query 'length(taskArns[*])')
		if [ -z "$all_task_by_service" ]; 
		then 
			printf "${runningCount} Running | ${pendingCount} Pending | ${desiredCount} Desired\n"
			if [ "$listTasksStoppedCount" -gt 0 ]
			then

				printf "Failed tasks ${RED}${listTasksStoppedCount}${NOCOLOR}. Waiting for the creation of new tasks...\n"
				listTasksStopped=$(aws ecs list-tasks --cluster ${ECS_CLUSTER} --started-by ${deploymentId} --desired-status STOPPED --output text --query 'taskArns')
				for taskStopped in  ${listTasksStopped}
				do
					describeTaskStopped=$(aws ecs describe-tasks --cluster ${ECS_CLUSTER} --tasks ${taskStopped} --query 'tasks[*].{stopCode: stopCode, stoppedReason: stoppedReason, desiredStatus: desiredStatus, lastStatus: lastStatus, taskArn: taskArn}')
					stopCode=$(echo "$describeTaskStopped" | jq -r '.[0].stopCode')
					stoppedReason=$(echo "$describeTaskStopped" | jq -r '.[0].stoppedReason')
					desiredStatus=$(echo "$describeTaskStopped" | jq -r '.[0].desiredStatus')
					lastStatus=$(echo "$describeTaskStopped" | jq -r '.[0].lastStatus')
					taskArnTemp=$(echo "$describeTaskStopped" | jq -r '.[0].taskArn')
					taskArn=$(echo $taskArnTemp | cut -d "/" -f 3)
					printf "◘ Tasks = ${taskArn}\n"
					printf "    --> stopCode=${CYAN} ${stopCode}${NOCOLOR}\n"
					printf "    --> desiredStatus=${YELLOW} ${desiredStatus}${NOCOLOR}\n"
					printf "    --> lastStatus=${YELLOW} ${lastStatus}${NOCOLOR}\n"
					printf "    --> stoppedReason=${CYAN} ${stoppedReason}${NOCOLOR} \n\n"
				done  
			else
				
				printf "Waiting for the creation of new tasks...\n\n"
			fi
		fi
		#if the number of attempts is met, the process ends as failed
		if [ "$listTasksStoppedCount" -ge "${deploymentRetries}" ] 
		then
			deploymentCompleted=1
			deploymentSuccess=0
			deploymentCount=1
		fi

	done
	
	##
	if  [ "$deploymentSuccess" -eq 1 ] 
	then
		printf "${GREEN}deployment completed successfully!!!!${NOCOLOR}\n"
	else 
		printf "${RED}deployment failed.${NOCOLOR}\n\n"
		printf "Reason:\n"
		listTasksStoppedCount=$(aws ecs list-tasks --cluster ${ECS_CLUSTER} --started-by ${deploymentId} --desired-status STOPPED --query 'length(taskArns[*])')
		printf "Failed tasks ${RED}${listTasksStoppedCount}${NOCOLOR}.\n"
		listTasksStopped=$(aws ecs list-tasks --cluster ${ECS_CLUSTER} --started-by ${deploymentId} --desired-status STOPPED --output text --query 'taskArns')
		for taskStopped in  ${listTasksStopped}
		do
			describeTaskStopped=$(aws ecs describe-tasks --cluster ${ECS_CLUSTER} --tasks ${taskStopped} --query 'tasks[*].{stopCode: stopCode, stoppedReason: stoppedReason, desiredStatus: desiredStatus, lastStatus: lastStatus, taskArn: taskArn}')
			stopCode=$(echo "$describeTaskStopped" | jq -r '.[0].stopCode')
			stoppedReason=$(echo "$describeTaskStopped" | jq -r '.[0].stoppedReason')
			desiredStatus=$(echo "$describeTaskStopped" | jq -r '.[0].desiredStatus')
			lastStatus=$(echo "$describeTaskStopped" | jq -r '.[0].lastStatus')
			taskArnTemp=$(echo "$describeTaskStopped" | jq -r '.[0].taskArn')
			taskArn=$(echo $taskArnTemp | cut -d "/" -f 3)
			printf "◘ Tasks = ${taskArn}\n"
			printf "    --> stopCode=${CYAN} ${stopCode}${NOCOLOR}\n"
			printf "    --> desiredStatus=${YELLOW} ${desiredStatus}${NOCOLOR}\n"
			printf "    --> lastStatus=${YELLOW} ${lastStatus}${NOCOLOR}\n"
			printf "    --> stoppedReason=${CYAN} ${stoppedReason}${NOCOLOR} \n"
		done  
		exit 1
	fi
else
	printf "\nThe service was not updated because the desired count is 0....deployment ${GREEN}success${NOCOLOR}!!!"
fi