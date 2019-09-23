// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.
namespace UserImpersonation
{
    public interface IImpersonationService
    {
        /// <summary>
        /// This creates an user impersonation cookie, which starts the user impersonation via the AuthCookie ValidateAsync event
        /// </summary>
        /// <param name="userId">This must be the userId of the user you want to impersonate</param>
        /// <param name="userName">the name to show as being impersonated</param>
        /// <param name="keepOwnPermissions">true if the original user's permissions should be used</param>
        /// <returns>error message, or null if OK</returns>
        string StartImpersonation(string userId, string userName, bool keepOwnPermissions);

        /// <summary>
        /// This will delete the user impersonation cookie, which causes the AuthCookie ValidateAsync event to revert to the original user
        /// </summary>
        /// <returns>error message, or null if OK</returns>
        string StopImpersonation();
    }
}