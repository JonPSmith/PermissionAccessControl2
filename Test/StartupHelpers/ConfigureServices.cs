// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Reflection;
using DataAuthorize;
using DataLayer.EfCode;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PermissionAccessControl2.Data;
using Test.FakesAndMocks;
using TestSupport.Helpers;

namespace Test.StartupHelpers
{
    public class ConfigureServices
    {
        public ServiceProvider ServiceProvider { get; private set; }

        public ConfigureServices()
        {
            var services = new ServiceCollection();
            var connectionStringBuilder = new SqliteConnectionStringBuilder { DataSource = ":memory:" };
            var identityConnection = new SqliteConnection(connectionStringBuilder.ToString());
            identityConnection.Open();
            services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(identityConnection));

            var authAndAppConnection = new SqliteConnection(connectionStringBuilder.ToString());
            authAndAppConnection.Open();
            services.AddDbContext<AppDbContext>(options => { options.UseSqlite(authAndAppConnection); });
            services.AddDbContext<ExtraAuthorizeDbContext>(options => { options.UseSqlite(authAndAppConnection); });
            services.AddDbContext<CombinedDbContext>(options => { options.UseSqlite(authAndAppConnection); });

            //Wanted to use the line below but just couldn't get the right package for it
            //services.AddDefaultIdentity<IdentityUser>()
            services.AddIdentity<IdentityUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>();

            var startupConfig = AppSettings.GetConfiguration((Assembly)null, "demosettings.json");
            services.AddSingleton<IHostingEnvironment>(new HostingEnvironment {WebRootPath = TestData.GetTestDataDir()});
            services.AddSingleton<IConfiguration>(startupConfig);
            services.AddSingleton<IGetClaimsProvider>(new FakeGetClaimsProvider("userId", ""));

            ServiceProvider = services.BuildServiceProvider();

            //make sure the in-memory databases are created
            ServiceProvider.GetService<ApplicationDbContext>().Database.EnsureCreated();
            ServiceProvider.GetService<CombinedDbContext>().Database.EnsureCreated();
        }
    }
}