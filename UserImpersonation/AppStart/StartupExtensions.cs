// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using Microsoft.Extensions.DependencyInjection;
using NetCore.AutoRegisterDi;

namespace UserImpersonation.AppStart
{
    public static class StartupExtensions
    {
        public static void UserImpersonationRegister(this IServiceCollection services)
        {
            //This registers the classes in the current assembly that end in "Service" and have a public interface
            services.RegisterAssemblyPublicNonGenericClasses()
                .Where(c => c.Name.EndsWith("Service"))
                .AsPublicImplementedInterfaces();
        }
    }
}