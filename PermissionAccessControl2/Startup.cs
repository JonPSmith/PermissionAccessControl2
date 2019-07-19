using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CommonCache;
using CommonCache.AppStart;
using DataAuthorize;
using DataLayer.EfCode;
using FeatureAuthorize.PolicyCode;
using GenericServices.Setup;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PermissionAccessControl2.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ServiceLayer.AppStart;
using ServiceLayer.CodeCalledInStartup;
using ServiceLayer.UserServices;
using Swashbuckle.AspNetCore.Swagger;

namespace PermissionAccessControl2
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
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => false; //!!!!!!!!!!!!!!!!!!!!!! Turned off
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            //This registers the various databases, either as in-memory or via SQL Server (see appsetting.json for connection strings)
            services.RegisterDatabases(Configuration);

            services.AddDefaultIdentity<IdentityUser>()
                .AddDefaultUI(UIFramework.Bootstrap4)
                .AddEntityFrameworkStores<ApplicationDbContext>();

            //Need to register before ConfigureCookiesForExtraAuth 

            //This enables Cookies for authentication and adds the feature and data claims to the user
            services.ConfigureCookiesForExtraAuth(Configuration["DemoSetup:UpdateCookieOnChange"] == "True");

            services.AddSingleton(Configuration); //Needed for SuperAdmin setup
            //Register the Permission policy handlers
            services.AddSingleton<IAuthorizationPolicyProvider, AuthorizationPolicyProvider>();
            services.AddSingleton<IAuthorizationHandler, PermissionHandler>();

            //This is needed to implement the data authorize code 
            services.AddScoped<IGetClaimsProvider, GetClaimsFromUser>();
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            //This defines the ITimeStore used by CommonCache
            services.AddScoped<ITimeStore, ExtraAuthorizeDbContext>();
            //This sets up the CommonCache 
            services.CommonCacheStartup(Configuration);
            //This registers the services into DI
            services.ServiceLayerStartup(Configuration);

            //This has to come after the CommonCache settings
            services.ConfigureGenericServicesEntities(typeof(ExtraAuthorizeDbContext), typeof(CompanyDbContext))
                .ScanAssemblesForDtos(Assembly.GetAssembly(typeof(ListUsersDto)))
                .RegisterGenericServices();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info { Title = "My API V1", Version = "v1" });

                //see https://docs.microsoft.com/en-us/aspnet/core/tutorials/getting-started-with-swashbuckle?view=aspnetcore-2.1&tabs=visual-studio%2Cvisual-studio-xml#xml-comments
                // Set the comments path for the Swagger JSON and UI.
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                if (!File.Exists(xmlPath))
                    throw new InvalidOperationException("The XML file does not exist for Swagger - see link above for more info.");
                c.IncludeXmlComments(xmlPath);
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();

                // Enable middleware to serve generated Swagger as a JSON endpoint.
                app.UseSwagger();

                // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.), 
                // specifying the Swagger JSON endpoint.
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
                });
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
