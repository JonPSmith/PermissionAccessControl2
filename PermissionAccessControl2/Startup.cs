using System;
using System.IO;
using System.Linq;
using System.Reflection;
using AuthorizeSetup;
using DataKeyParts;
using DataLayer.EfCode;
using FeatureAuthorize;
using FeatureAuthorize.PolicyCode;
using GenericServices.Setup;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PermissionAccessControl2.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ServiceLayer.AppStart;
using ServiceLayer.UserServices;
using Swashbuckle.AspNetCore.Swagger;
using UserImpersonation.AppStart;
using Microsoft.Extensions.Hosting;

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
																.AddEntityFrameworkStores<ApplicationDbContext>();

												services.AddSingleton(Configuration); //Needed for SuperAdmin setup
												services.Configure<DemoSetupOptions>(Configuration.GetSection("DemoSetup"));

												//Register the Permission policy handlers
												services.AddSingleton<IAuthorizationPolicyProvider, AuthorizationPolicyProvider>();
												services.AddSingleton<IAuthorizationHandler, PermissionHandler>();

												//This is needed to implement the data authorize code 
												services.AddScoped<IGetClaimsProvider, GetClaimsFromUser>();

												//This registers/sets up the services in these projects. 
												services.ServiceLayerRegister();
												services.UserImpersonationRegister();

												//This enables Cookies for authentication and adds the feature and data claims to the user
												services.ConfigureCookiesForExtraAuth();

												//This has to come after the ConfigureCookiesForExtraAuth settings, which sets up the IAuthChanges
												services.ConfigureGenericServicesEntities(typeof(ExtraAuthorizeDbContext), typeof(CompanyDbContext))
																.ScanAssemblesForDtos(Assembly.GetAssembly(typeof(ListUsersDto)))
																.RegisterGenericServices();

												services.AddControllersWithViews();
												//I add Swagger so that you can test the FrontEndController that provides the permissions of the current user
												services.AddSwaggerGen(c =>
												{
																c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "My API V1", Version = "v1" });

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
								public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
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

												//This should come AFTER the app.UseAuthentication() call
												if (Configuration["DemoSetup:UpdateCookieOnChange"] == "True")
												{
																//If UpdateCookieOnChange this adds a header which has the time that the user's claims were updated
																//thanks to https://stackoverflow.com/a/48610119/1434764
																app.Use((context, next) =>
																{
																				var lastTimeUserPermissionsSet = context.User.Claims
																								.SingleOrDefault(x => x.Type == PermissionConstants.LastPermissionsUpdatedClaimType)?.Value;
																				if (lastTimeUserPermissionsSet != null)
																								context.Response.Headers["Last-Time-Users-Permissions-Updated"] = lastTimeUserPermissionsSet;
																				return next.Invoke();
																});
												}

												app.UseRouting();

												app.UseAuthentication();
												app.UseAuthorization();

												app.UseEndpoints(endpoints =>
												{
																endpoints.MapControllerRoute(
																				name: "default",
																				pattern: "{controller=Home}/{action=Index}/{id?}");
																endpoints.MapRazorPages();
												});
								}
				}
}
