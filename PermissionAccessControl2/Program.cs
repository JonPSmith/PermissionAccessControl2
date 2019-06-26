using System.Threading.Tasks;
using DataLayer.EfCode;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
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
                .UseStartup<Startup>()
                .Build();

            //Because I might be using in-memory databases I need to make sure they are created 
            //before my startup code tries to use them
            SetupDatabases(webHost);
            await webHost.Services.CheckAddSuperAdminAsync();
            await webHost.Services.CheckSeedDataAndUserAsync();
            return webHost;
        }

        private static void SetupDatabases(IWebHost webHost)
        {
            using (var scope = webHost.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                using (var context = services.GetRequiredService<ApplicationDbContext>())
                {
                    context.Database.EnsureCreated();
                }
                //This creates a database which is a combination of both the ExtraAuthorizeDbContext and AppDbContext
                using (var context = services.GetRequiredService<CombinedDbContext>())
                {
                    context.Database.EnsureCreated();
                }
            }
        }
    }
}
