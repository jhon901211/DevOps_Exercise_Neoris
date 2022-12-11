using Business.Security;
using Business.Security.Interface;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Utilities.Configuration;
using Utilities.Response;
using Utilities.Security;

namespace Security
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1.0", new OpenApiInfo { Title = "Security", Version = "v1.0" });
            });

            //Get variables env
            AppSettings.Instance.Environment = Environment.GetEnvironmentVariable("ENV") ?? "develop";
            AppSettings.Instance.ApiKey = Environment.GetEnvironmentVariable("API_KEY") ?? "2f5ae96c-b558-4c7b-a590-a501ae1c3f6c";
            AppSettings.Instance.SecretName = Environment.GetEnvironmentVariable("SECRET_NAME") ?? "admin";
            AppSettings.Instance.SecretKey = Environment.GetEnvironmentVariable("SECRET_KEY") ?? "admin";
            AppSettings.Instance.ApplicationJwtKey = Environment.GetEnvironmentVariable("APPLICATION_JWT_KEY") ?? "EEeI8HmUlqsVymZ4Sjf3Qw==";
            if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("MINUTES_EXPIRATION_TIME")))
                AppSettings.Instance.MinutesExpirationTime = 10;
            else
                AppSettings.Instance.MinutesExpirationTime = Convert.ToInt32(Environment.GetEnvironmentVariable("MINUTES_EXPIRATION_TIME"));
            AppSettings.Instance.ApplicationMethodEncrypt = Environment.GetEnvironmentVariable("APPLICATION_METHOD_ENCRYPT") ?? "HS512";
            ConfigureCorsService(ref services);
            DependencyBusiness(services);
            ConfigureSecurtyToken(ref services);
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UsePathBase("/security");
            if (AppSettings.Instance.Environment.ToUpper() == "DEVELOP")
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1.0/swagger.json", "Security v1"));
            }
            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseCors("CorsPolicy");
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        /// <summary>
        /// Iniciar referencias del negocio con su interfaz de implementacion
        /// </summary>
        /// <param name="services"></param>
        private static void DependencyBusiness(IServiceCollection services)
        {
            services.AddTransient<ISecurity, Authentication>();
        }

        /// <summary>
        /// Método encargado de configurar la autenticación mediante Token.
        /// </summary>
        public void ConfigureSecurtyToken(ref IServiceCollection services)
        {
            var key = Encoding.ASCII.GetBytes(AppSettings.Instance.ApplicationJwtKey);

            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = false;
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    RequireExpirationTime = true
                };
            });
        }

        /// <summary>
        /// CorsPolicy
        /// </summary>
        private void ConfigureCorsService(ref IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy", builder =>
                {
                    builder.AllowAnyOrigin();
                    builder.WithOrigins("*");
                    builder.AllowAnyHeader();
                    builder.AllowAnyMethod();
                    builder.AllowCredentials();
                    builder.SetIsOriginAllowed(origin => true);
                });
            });
            services.AddScoped<ClaimsPrincipal>(s => s.GetService<IHttpContextAccessor>().HttpContext.User);
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_3_0).ConfigureApiBehaviorOptions(options =>
              options.InvalidModelStateResponseFactory = c =>
              {
                  Collection<string> errorList = new Collection<string>();
                  foreach (var item in c.ModelState.Values.Where(v => v.Errors.Count > 0).ToList())
                  {
                      errorList.Add(item.Errors[0].ErrorMessage);
                  }
                  return new BadRequestObjectResult(ManagerResponse<string>.PreconditionRequired("Error en la validación de datos", errorList));

              }
            );
            services.AddSingleton(TokenFactory.CreateManager(
                AppSettings.Instance.MinutesExpirationTime,
                AppSettings.Instance.ApplicationJwtKey)
            );
            services.AddControllers();
            services.AddScoped<ClaimsPrincipal>(s => s.GetService<IHttpContextAccessor>().HttpContext.User);
            services.AddMemoryCache();

        }
    }
}
