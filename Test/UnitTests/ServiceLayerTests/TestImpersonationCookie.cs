// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Security.Cryptography;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using ServiceLayer.UserImpersonation;
using ServiceLayer.UserImpersonation.Concrete;
using ServiceLayer.UserImpersonation.Concrete.Internal;
using Test.FakesAndMocks;
using Xunit;
using Xunit.Extensions.AssertExtensions;

namespace Test.UnitTests.ServiceLayerTests
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
            var mocks = new MockHttpContextCookies();
            var eProvider = new EphemeralDataProtectionProvider();

            //ATTEMPT
            var cookie = new ImpersonationCookie();
            cookie.AddUpdateCookie("Hello world", eProvider, mocks.MockContext.Response.Cookies);

            //VERIFY
            mocks.ResponseCookies.Count.ShouldEqual(1);
            mocks.ResponseCookies["Set-Cookie"].ShouldNotBeNull();
            mocks.ResponseCookies["Set-Cookie"][0].ShouldStartWith("UserImpersonation=");
        }

        [Fact]
        public void ReadEncryptedCookie()
        {
            //SETUP
            var mocks = new MockHttpContextCookies();
            var eProvider = new EphemeralDataProtectionProvider();
            var cookie = new ImpersonationCookie();

            //ATTEMPT
            mocks.RequestCookies["UserImpersonation"] = eProvider.CreateProtector(cookie.EncryptPurpose).Protect("Hello world");
            var data = cookie.GetCookieInValue(eProvider, mocks.MockContext.Request.Cookies, mocks.MockContext.Response.Cookies);

            //VERIFY
            data.ShouldEqual("Hello world");
        }

        [Fact]
        public void ReadEncryptedCookieBadDeletesCookie()
        {
            //SETUP
            var mocks = new MockHttpContextCookies();
            var eProvider = new EphemeralDataProtectionProvider();
            var cookie = new ImpersonationCookie();

            //ATTEMPT
            mocks.RequestCookies["UserImpersonation"] = "???";
            var ex = Assert.Throws<CryptographicException>(() => 
                cookie.GetCookieInValue(eProvider, mocks.MockContext.Request.Cookies, mocks.MockContext.Response.Cookies));

            //VERIFY
            ex.Message.ShouldStartWith("An error occurred during a cryptographic operation.");
            mocks.ResponseCookies["Set-Cookie"].ShouldNotBeNull();
            mocks.ResponseCookies["Set-Cookie"][0].ShouldEndWith("expires=Thu, 01 Jan 1970 00:00:00 GMT; path=/; samesite=lax");
        }

        [Fact]
        public void ReadNonExistentCookie()
        {
            //SETUP
            var mocks = new MockHttpContextCookies();
            var eProvider = new EphemeralDataProtectionProvider();
            var cookie = new ImpersonationCookie();

            //ATTEMPT
            var data = cookie.GetCookieInValue(eProvider, mocks.MockContext.Request.Cookies, mocks.MockContext.Response.Cookies);

            //VERIFY
            data.ShouldBeNull();
        }

        [Fact]
        public void TestCookieExists()
        {
            //SETUP
            var mocks = new MockHttpContextCookies();
            var eProvider = new EphemeralDataProtectionProvider();
            var cookie = new ImpersonationCookie();
            cookie.AddUpdateCookie("Hello world", eProvider, mocks.MockContext.Response.Cookies);

            mocks.RequestCookies["UserImpersonation"] = "???";

            //ATTEMPT

            //VERIFY
            cookie.Exists(mocks.MockContext.Request.Cookies).ShouldBeTrue();
        }


        [Fact]
        public void TestCookieDelete()
        {
            //SETUP
            var mocks = new MockHttpContextCookies();
            var cookie = new ImpersonationCookie();

            mocks.ResponseCookies["Set-Cookie"] = "Some data";

            //ATTEMPT
            cookie.Delete(mocks.MockContext.Response.Cookies);

            //VERIFY
            mocks.ResponseCookies["Set-Cookie"][1].ShouldEndWith("expires=Thu, 01 Jan 1970 00:00:00 GMT; path=/; samesite=lax");
        }

    }
}