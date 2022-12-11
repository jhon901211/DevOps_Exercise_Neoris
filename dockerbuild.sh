#!/bin/bash
GREEN="\033[0;32m"
NOCOLOR="\033[0m"
YELLOW="\033[1;33m"

ECR_IMAGE=$1
ENV=$2
REGION=$3
DOCKER_FILE=$4

echo -e "${GREEN}Install dependencies...${NOCOLOR}"
apk add --update --no-cache curl jq py3-configobj py3-pip py3-setuptools python3 python3-dev
pip install --upgrade awscli
aws --version
echo -e "${GREEN}Assume role...${NOCOLOR}"
ROLE=$(aws sts assume-role --role-arn "$AWS_ASSUME_ROLE" --role-session-name "GitlabAccess" --output json)
AccessKeyId=$(echo "$ROLE" | jq -r '.Credentials.AccessKeyId')
SecretKeyId=$(echo "$ROLE" | jq -r '.Credentials.SecretAccessKey')
Token=$(echo "$ROLE" | jq -r '.Credentials.SessionToken')
export AWS_ACCESS_KEY_ID=$AccessKeyId
export AWS_SECRET_ACCESS_KEY=$SecretKeyId
export AWS_SESSION_TOKEN=$Token
echo -e "${GREEN}Login to ECR...${NOCOLOR}"
$(aws ecr get-login --no-include-email --region $REGION)

docker --version
if  [ "$ENV" = "production" ] 
then
  echo -e "${GREEN}Building image | $DOCKER_FILE please wait...(Prd)${NOCOLOR}"
  docker build -f $DOCKER_FILE -t $ECR_IMAGE:latest .
  echo -e "${GREEN}Pushing image...${NOCOLOR}"
  docker push $ECR_IMAGE:latest
else
  echo -e "${GREEN}Building image | $DOCKER_FILE please wait...(Dev)${NOCOLOR}"
  docker build -f $DOCKER_FILE -t $ECR_IMAGE:develop-latest .
  echo -e "${GREEN}Pushing image...${NOCOLOR}"
  docker push $ECR_IMAGE:develop-latest
fi