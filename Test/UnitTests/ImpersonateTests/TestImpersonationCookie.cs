// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: httpContext://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Security.Cryptography;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Test.FakesAndMocks;
using UserImpersonation.Concrete;
using Xunit;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests.ImpersonateTests
{
    public class TestImpersonationCookie
    {
        [Fact]
        public void TestDataProtectionProvider()
        {
            //SETUP
            var eProvider = new EphemeralDataProtectionProvider();

            //ATTEMPT
            var provider = eProvider.CreateProtector("XXX");
            var encrypted = provider.Protect("Hello world");
            var unencrypted = provider.Unprotect(encrypted);

            //VERIFY
            unencrypted.ShouldEqual("Hello world");
        }

        [Fact]
        public void AddEncryptedCookie()
        {
            //SETUP
            var httpContext = new DefaultHttpContext();
            var eProvider = new EphemeralDataProtectionProvider();

            //ATTEMPT
            var cookie = new ImpersonationCookie(httpContext, eProvider);
            cookie.AddUpdateCookie("Hello world");

            //VERIFY
            httpContext.Response.Headers.Keys.Count.ShouldEqual(1);
            httpContext.Response.Headers["Set-Cookie"].ShouldNotBeNull();
            httpContext.Response.Headers["Set-Cookie"][0].ShouldStartWith("UserImpersonation=");
        }

        [Fact]
        public void ReadEncryptedCookie()
        {
            //SETUP
            var httpContext = new DefaultHttpContext();
            var eProvider = new EphemeralDataProtectionProvider();
            var cookie = new ImpersonationCookie(httpContext, eProvider);
            var codedData = eProvider.CreateProtector(cookie.EncryptPurpose).Protect("Hello world");

            //ATTEMPT
            httpContext.AddRequestCookie("UserImpersonation", codedData);

            var data = cookie.GetCookieInValue();

            //VERIFY
            data.ShouldEqual("Hello world");
        }

        [Fact]
        public void ReadEncryptedCookieBadDeletesCookie()
        {
            //SETUP
            var httpContext = new DefaultHttpContext();
            var eProvider = new EphemeralDataProtectionProvider();
            var cookie = new ImpersonationCookie(httpContext, eProvider);

            //ATTEMPT
            httpContext.AddRequestCookie("UserImpersonation", "???");
            var ex = Assert.Throws<CryptographicException>(() => 
                cookie.GetCookieInValue());

            //VERIFY
            ex.Message.ShouldStartWith("An error occurred during a cryptographic operation.");
            httpContext.Response.Headers["Set-Cookie"].ShouldNotBeNull();
            httpContext.Response.Headers["Set-Cookie"][0].ShouldEndWith("expires=Thu, 01 Jan 1970 00:00:00 GMT; path=/; httponly");
        }

        [Fact]
        public void ReadNonExistentCookie()
        {
            //SETUP
            var httpContext = new DefaultHttpContext();
            var eProvider = new EphemeralDataProtectionProvider();
            var cookie = new ImpersonationCookie(httpContext, eProvider);

            //ATTEMPT
            var data = cookie.GetCookieInValue();

            //VERIFY
            data.ShouldBeNull();
        }

        [Fact]
        public void TestCookieExists()
        {
            //SETUP
            var httpContext = new DefaultHttpContext();
            var eProvider = new EphemeralDataProtectionProvider();
            var cookie = new ImpersonationCookie(httpContext, eProvider);
            cookie.AddUpdateCookie("Hello world");

            httpContext.AddRequestCookie("UserImpersonation", "???");

            //ATTEMPT

            //VERIFY
            cookie.Exists(httpContext.Request.Cookies).ShouldBeTrue();
        }


        [Fact]
        public void TestCookieDelete()
        {
            //SETUP
            var httpContext = new DefaultHttpContext();
            var cookie = new ImpersonationCookie(httpContext, null);

            httpContext.Response.Headers["Set-Cookie"] = "Some data";

            //ATTEMPT
            cookie.Delete();

            //VERIFY
            httpContext.Response.Headers["Set-Cookie"][1].ShouldEndWith("expires=Thu, 01 Jan 1970 00:00:00 GMT; path=/; httponly");
        }

    }
}