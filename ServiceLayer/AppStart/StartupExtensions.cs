// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NetCore.AutoRegisterDi;

namespace ServiceLayer.AppStart
{
    public static class StartupExtensions
    {
        public static void ServiceLayerStartup(this IServiceCollection services, IConfiguration configuration)
        {
            //This registers the service layer: I only register the classes who name ends with "Service" (at the moment)
            services.RegisterAssemblyPublicNonGenericClasses(Assembly.GetExecutingAssembly())
                .Where(c => c.Name.EndsWith("Service") || c.Name.EndsWith("ServiceAsync"))
                .AsPublicImplementedInterfaces();

            //Put any code here to initialise values - you can access the configuration file via the configuration parameter
        }
    }
}