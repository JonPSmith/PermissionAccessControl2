// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.
namespace ServiceLayer.UserImpersonation
{
    public interface IImpersonationService
    {
        /// <summary>
        /// This creates an user impersonation cookie, which starts the user impersonation via the AuthCookie ValidateAsync event
        /// </summary>
        /// <param name="userId">This must be the userId of the user you want to impersonate</param>
        void StartImpersonation(string userId);

        /// <summary>
        /// This will delete the user impersonation cookie, which causes the AuthCookie ValidateAsync event to revert to the original user
        /// </summary>
        void StopImpersonation();
    }
}