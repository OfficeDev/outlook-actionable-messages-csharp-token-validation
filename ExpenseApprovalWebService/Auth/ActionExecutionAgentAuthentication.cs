// 
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.
// 
// Copyright (c) Microsoft Corporation
// All rights reserved.
// 
// MIT License:
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED ""AS IS"", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

namespace ExpenseApprovalWebService
{
    using System;
    using System.Configuration;
    using System.Diagnostics;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web;
    using System.Web.Http.Controllers;
    using System.Web.Http.Filters;

    using Newtonsoft.Json.Linq;

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class ActionExecutionAgentAuthentication : ActionFilterAttribute
    {
        private static readonly string AppId = "48af08dc-f6d2-435f-b2a7-069abd99c086";

        public string MicrosoftAppIdSettingName { get; set; }
        public bool DisableSelfIssuedTokens { get; set; }
        public string Audience { get; set; }
        public virtual string OpenIdConfigurationUrl { get; set; } = JwtConfig.O365OpenIdMetadataUrl;

        public override async Task OnActionExecutingAsync(HttpActionContext actionContext, CancellationToken cancellationToken)
        {
            if (Debugger.IsAttached && String.IsNullOrEmpty(AppId))
                // then auth is disabled
                return;

            var tokenExtractor = new JwtTokenExtractor(JwtConfig.GetO365TokenValidationParameters(Audience), OpenIdConfigurationUrl);
            var identity = await tokenExtractor.GetIdentityAsync(actionContext.Request);

            // No identity? Fail out.
            if (identity == null)
            {
                tokenExtractor.GenerateUnauthorizedResponse(actionContext);
                return;
            }

            // Check to make sure the app ID in the token is ours
            if (identity != null)
            {
                // If it doesn't match, throw away the identity
                if (!ValidateClaim(identity, "appid", AppId))
                {
                    identity = null;
                }
            }

            Thread.CurrentPrincipal = new ClaimsPrincipal(identity);

            // Inside of ASP.NET this is required
            if (HttpContext.Current != null)
                HttpContext.Current.User = Thread.CurrentPrincipal;

            await base.OnActionExecutingAsync(actionContext, cancellationToken);
        }

        private bool ValidateClaim(ClaimsIdentity identity, string claimType, string expectedValue)
        {
            Claim claim = identity.Claims.FirstOrDefault(c => c.Type == claimType);
            if (claim != null && claim.Value == expectedValue)
                return true;

            return false;
        }
    }
}
