// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System;
using DataLayer.EfCode;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PermissionAccessControl2.Data;

namespace PermissionAccessControl2
{
    /// <summary>
    /// NOTE: This class is in the ASP.NET application to get a reference to the ApplicationDbContext class
    /// </summary>
    public static class AddDatabases
    {
        public static void RegisterDatabases(this IServiceCollection services, IConfiguration configuration)
        {
            var type = configuration["DemoSetup:DatabaseSetup"];
            if (type == "Permanent")
            {
                //we are dealing with real databases

                services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

                services.AddDbContext<AppDbContext>(options =>
                    options.UseSqlServer(configuration.GetConnectionString("DemoDatabaseConnection")));
                services.AddDbContext<ExtraAuthorizeDbContext>(options =>
                    options.UseSqlServer(configuration.GetConnectionString("DemoDatabaseConnection")));
                services.AddDbContext<CombinedDbContext>(options =>
                    options.UseSqlServer(configuration.GetConnectionString("DemoDatabaseConnection")));
            }
            else if (type == "InMemory")
            {
                //we are dealing with in-memory databases

                var aspNetAuthConnection = SetupSqliteInMemoryConnection();
                services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(aspNetAuthConnection));
                var appExtraConnection = SetupSqliteInMemoryConnection();
                services.AddDbContext<AppDbContext>(options => options.UseSqlite(appExtraConnection));
                services.AddDbContext<ExtraAuthorizeDbContext>(options => options.UseSqlite(appExtraConnection));
                services.AddDbContext<CombinedDbContext>(options => options.UseSqlite(appExtraConnection));
            }
            else
            {
                throw new ApplicationException("This needs a section called 'DemoSetup', with property 'DatabaseSetup' in it");
            }
        }

        private static SqliteConnection SetupSqliteInMemoryConnection()
        {
            var connectionStringBuilder = new SqliteConnectionStringBuilder { DataSource = ":memory:" };
            var connectionString = connectionStringBuilder.ToString();
            var connection = new SqliteConnection(connectionString);
            connection.Open();  //see https://github.com/aspnet/EntityFramework/issues/6968
            return connection;
        }
    }
}