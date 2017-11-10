using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using netCoreWepApiAuthJWT.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using netCoreWepApiAuthJWT.AuthJWT;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace netCoreWepApiAuthJWT
{
    public class Startup
    {
        private const string SecretKey = "iNivDmHLpUA223sqsfhqGbMRdRj1PVkH"; // todo: get this from somewhere secure
        private readonly SymmetricSecurityKey _signingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(SecretKey));


        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }


        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Inject my Context Service
            services.AddTransient<myAppplicationContext, myAppplicationContext>();

            
            //Inject EFCore database context with dependency injection (we pass/Consume the default Connection String in app.setting.json)
            services.AddDbContext<Model.myAppplicationContext>(options => options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            // Inject IJwtFactory Service
            services.AddSingleton<IJwtFactory, JwtFactory>();

            services.AddIdentity<AppUser, IdentityRole>
               (options =>
               {
                   // configure identity options
                   options.Password.RequireDigit = false;
                   options.Password.RequireLowercase = false;
                   options.Password.RequireUppercase = false;
                   options.Password.RequireNonAlphanumeric = false;
                   options.Password.RequiredLength = 6;
               })
               .AddEntityFrameworkStores<myAppplicationContext>();   /* if not added this error shows up: 
                                                                    Microsoft.AspNetCore.Identity.IUserStore`1[netCoreWepApiAuthJWT.Model.AppUser]' while attempting to activate 'Microsoft.AspNetCore.Identity.UserManager`1[netCoreWepApiAuthJWT.Model.AppUser]'.   */


            /* Read the JwtIssuerOptions settings from the config file to configure the JwtIssuerOptions and set it up for injection. */
            // jwt wire up
            // Get options from app settings. (appsettings.json)
            var jwtAppSettingOptions = Configuration.GetSection(nameof(JwtIssuerOptions));


            /* Configure JwtIssuerOptions:  JwtIssuerOptions class that will allow us to set some of the JWT registered claim
            values as well as provide some additional functionality we require when signing the tokens we generate. */
            // Configure JwtIssuerOptions
            services.Configure<JwtIssuerOptions>(options =>
            {
                options.Issuer = jwtAppSettingOptions[nameof(JwtIssuerOptions.Issuer)];
                options.Audience = jwtAppSettingOptions[nameof(JwtIssuerOptions.Audience)];
                options.SigningCredentials = new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256);
            });

            // api user claim policy: https://docs.microsoft.com/en-us/aspnet/core/security/authorization/claims
            services.AddAuthorization(options =>
            {
                options.AddPolicy("ApiUser", policy => policy.RequireClaim(claimsToAdd.Strings.JwtClaimIdentifiers.Rol, claimsToAdd.Strings.JwtClaims.ApiAccess)); // "rol": "api_access"
            }); /* Adds a policy named 'ApiUser', ApiUser policy checks for the presence of an "rol": claim  on the incoming token payload  with value of "api_access".
                We then apply the policy using the Policy property on the AuthorizeAttribute attribute in our Controller to specify the policy name; 
                Only Identity with the stated claims will be Authorise to access the Controller, Action we apply this Policy.   
             */

            // Add service and create Policy with options
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAngular",                             //Policy == Angular, Enable policy in Controller: [EnableCors("AllowAngular")]
                    builder => builder.WithOrigins("http://localhost:4200")
                                      .AllowAnyHeader()
                                      .AllowAnyMethod());
            });
            // Add framework services.
            services.AddMvc();
        }


         
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();


            /* tell the ASP.NET Core middleware that we want to use JWT authentication on incoming requests... incoming requests might contain JWTs */
            var jwtAppSettingOptions = Configuration.GetSection(nameof(JwtIssuerOptions));
            // sets up the validation parameters that we’re going to pass to ASP.NET’s JWT bearer authentication middleware.
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwtAppSettingOptions[nameof(JwtIssuerOptions.Issuer)],

                ValidateAudience = true,
                ValidAudience = jwtAppSettingOptions[nameof(JwtIssuerOptions.Audience)],

                ValidateIssuerSigningKey = true,
                IssuerSigningKey = _signingKey,

                RequireExpirationTime = false,
                ValidateLifetime = false,
                ClockSkew = TimeSpan.Zero
            };

            /* add the JWT token bearer middleware that tells the application that we expect JWT tokens as part of the authentication
            and authorisation processes and to automatically challenge and authenticate. */
            app.UseJwtBearerAuthentication(new JwtBearerOptions
            {
                AutomaticAuthenticate = true,
                AutomaticChallenge = true,
                TokenValidationParameters = tokenValidationParameters
            });

            // To enable CORS for your entire application add the CORS middleware to your request pipeline: before any call to UseMvc
            // Shows UseCors with CorsPolicyBuilder.
            app.UseCors(builder =>
               builder.WithOrigins("http://localhost:4200")
                            .AllowAnyHeader()
                            .AllowAnyMethod());

            app.UseMvc();
        }
    }
}



/* https://fullstackmark.com/post/10/user-authentication-with-angular-and-asp-net-core 
 * https://github.com/mmacneil/AngularASPNETCoreAuthentication/blob/71c4285f1ba472492f74f10c81461d5778b53b29/src/dotnetGigs/Controllers/AccountsController.cs
 * 
 * https://goblincoding.com/2016/07/03/issuing-and-authenticating-jwt-tokens-in-asp-net-core-webapi-part-i/
 * https://goblincoding.com/2016/07/07/issuing-and-authenticating-jwt-tokens-in-asp-net-core-webapi-part-ii/
 * 
 * 
 * CORS:
 * https://elanderson.net/2016/11/cross-origin-resource-sharing-cors-in-asp-net-core/
 
 *To use EF Core the application needs to install the following packages
 
 * Install-Package Microsoft.EntityFrameworkCore.SqlServer
 
* Install-Package Microsoft.EntityFrameworkCore.Tools
 
* Install-Package Microsoft.EntityFrameworkCore.SqlServer.Design
 
 */
