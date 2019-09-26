using System;
using System.Threading.Tasks;
using DataKeyParts;
using DataLayer.EfCode;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using PermissionAccessControl2.Data;
using ServiceLayer.SeedDemo;

namespace PermissionAccessControl2
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            (await BuildWebHostAsync(args)).Run();
        }

        public static async Task<IWebHost> BuildWebHostAsync(string[] args)
        {
            var webHost = WebHost.CreateDefaultBuilder(args)
                .CaptureStartupErrors(true)
                .UseStartup<Startup>()
                .Build();

            //Because I might be using in-memory databases I need to make sure they are created 
            //before my startup code tries to use them
            await SetupDatabasesAndSeedAsync(webHost);
            await webHost.Services.CheckAddSuperAdminAsync();
            return webHost;
        }

        private static async Task SetupDatabasesAndSeedAsync(IWebHost webHost)
        {
            using (var scope = webHost.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var demoSetupOptions = services.GetRequiredService<IOptions<DemoSetupOptions>>();
                if (demoSetupOptions.Value.CreateAndSeed)
                {
                    //it only creates the databases and seeds them if the DemoSetup:CreateAndSeed property is true

                    using (var context = services.GetRequiredService<ApplicationDbContext>())
                    {
                        context.Database.EnsureCreated();
                    }
                    //This creates a database which is a combination of both the ExtraAuthorizeDbContext and CompanyDbContext
                    using (var context = services.GetRequiredService<CombinedDbContext>())
                    {
                        context.Database.EnsureCreated();
                    }

                    await webHost.Services.CheckSeedDataAndUserAsync();
                }
            }
        }
    }
}
