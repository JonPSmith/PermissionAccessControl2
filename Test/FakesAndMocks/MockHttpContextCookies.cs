// Copyright (c) 2019 Jon P Smith, GitHub: JonPSmith, web: http://www.thereformedprogrammer.net/
// Licensed under MIT license. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.Primitives;
using Moq;

namespace Test.FakesAndMocks
{
    public class MockHttpContextCookies
    {
        /// <summary>
        /// This only mocks the Cookies part of the HttpContext
        /// </summary>
        public HttpContext MockContext { get; private set; }

        public Dictionary<string, string> RequestCookies { get; } = new Dictionary<string, string>();
       
        public Dictionary<string, StringValues> ResponseCookies { get; } = new Dictionary<string, StringValues>();

        public MockHttpContextCookies(ClaimsPrincipal curUser = null)
        {
            var responseHeaderDict = new HeaderDictionary(ResponseCookies);

            var mockContext = new Mock<HttpContext>(MockBehavior.Strict);
            var mockRequest = new Mock<HttpRequest>();
            mockRequest.SetupGet(x => x.Cookies).Returns(new RequestCookieCollection(RequestCookies));
            var mockResponse = new Mock<HttpResponse>();
            mockResponse.SetupGet(x => x.Cookies).Returns(new ResponseCookies(responseHeaderDict, null));

            //Need to set the features otherwise HttpContext won't support the use of that feature
            var defaultContext = new DefaultHttpContext();
            defaultContext.Features[typeof(IRequestCookiesFeature)] =
                new RequestCookiesFeature(new RequestCookieCollection(RequestCookies));
            defaultContext.Features[typeof(IResponseCookies)] = new ResponseCookies(responseHeaderDict, null);

            mockContext.SetupGet(x => x.Features).Returns(defaultContext.Features);
            mockContext.SetupGet(x => x.Request).Returns(mockRequest.Object);
            mockContext.SetupGet(x => x.Response).Returns(mockResponse.Object);
            mockContext.SetupGet(x => x.User).Returns(curUser);

            MockContext = mockContext.Object;
        }

    }
}