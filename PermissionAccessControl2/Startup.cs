using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CommonCache;
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
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            //This registers the various databases, either as in-memory or via SQL Server (see appsetting.json for connection strings)
            services.RegisterDatabases(Configuration);

            services.AddDefaultIdentity<IdentityUser>()
                .AddDefaultUI(UIFramework.Bootstrap4)
                .AddEntityFrameworkStores<ApplicationDbContext>();

            //Need to register before ConfigureCookiesForExtraAuth 
            services.AddSingleton<ISimpleTimeCache>(new SimpleTimeCache());
            //This enables Cookies for authentication and adds the feature and data claims to the user
            services.ConfigureCookiesForExtraAuth(Configuration["DemoSetup:UpdateCookieOnChange"] == "True");

            services.AddSingleton(Configuration); //Needed for SuperAdmin setup
            //Register the Permission policy handlers
            services.AddSingleton<IAuthorizationPolicyProvider, AuthorizationPolicyProvider>();
            services.AddSingleton<IAuthorizationHandler, PermissionHandler>();

            //This is needed to implement the data authorize code 
            services.AddScoped<IGetClaimsProvider, GetClaimsFromUser>();
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.ConfigureGenericServicesEntities(typeof(ExtraAuthorizeDbContext), typeof(AppDbContext))
                .ScanAssemblesForDtos(Assembly.GetAssembly(typeof(ListUsersDto)))
                .RegisterGenericServices();

            //This registers the services into DI
            services.ServiceLayerStartup(Configuration);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
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
