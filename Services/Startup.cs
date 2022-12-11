using Business;
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

namespace Services
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1.0", new OpenApiInfo { Title = "Services", Version = "v1.0" });
            });
            //services.AddSwaggerGen(c =>
            //{
            //    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Services", Version = "v1" });
            //    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
            //    {
            //        Name = "Authorization",
            //        Type = SecuritySchemeType.ApiKey,
            //        Scheme = "Bearer",
            //        BearerFormat = "JWT",
            //        In = ParameterLocation.Header,
            //        Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer 1safsfsdfdfd\"",
            //    });
            //    c.AddSecurityRequirement(new OpenApiSecurityRequirement {
            //    {
            //        new OpenApiSecurityScheme {
            //            Reference = new OpenApiReference {
            //                Type = ReferenceType.SecurityScheme,
            //                    Id = "Bearer"
            //            }
            //        },
            //        new string[] {}
            //     }
            //    });
            //});
            //Get variables env
            AppSettings.Instance.Environment = Environment.GetEnvironmentVariable("ENV") ?? "develop";
            AppSettings.Instance.ApiKey = Environment.GetEnvironmentVariable("API_KEY") ?? "2f5ae96c-b558-4c7b-a590-a501ae1c3f6c";
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

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UsePathBase("/core");
            if (AppSettings.Instance.Environment.ToUpper() == "DEVELOP")
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1.0/swagger.json", "Services v1"));
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
            services.AddTransient<IDevOps, DevOps>();
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
        [Obsolete]
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
