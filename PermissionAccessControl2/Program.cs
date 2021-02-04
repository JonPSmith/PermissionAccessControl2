using System.Threading.Tasks;
using DataKeyParts;
using DataLayer.EfCode;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using PermissionAccessControl2.Data;
using PermissionAccessControl2.SeedDemo;

namespace PermissionAccessControl2
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            //This migrates the database and adds any seed data as required
            await SetupDatabasesAndSeedAsync(host);
            await host.RunAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });


        private static async Task SetupDatabasesAndSeedAsync(IHost webHost)
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
                    await webHost.Services.CheckAddSuperAdminAsync();
                }
            }
        }
    }
}
