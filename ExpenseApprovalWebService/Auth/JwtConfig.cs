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
    using System.IdentityModel.Tokens;
    
    /// <summary>
    /// Configuration for JWT tokens
    /// </summary>
    public static class JwtConfig
    {
        /// <summary>
        /// OpenID metadata document for tokens coming from O365.
        /// </summary>
        public const string O365OpenIdMetadataUrl = "https://substrate.office.com/sts/common/.well-known/openid-configuration";

        /// <summary>
        /// O365 token issuer. 
        /// </summary>
        public const string O365TokenIssuer = "https://substrate.office.com/sts/";

        /// <summary>
        /// Token validation parameters when receives call from action execution agent.
        /// </summary>
        public static TokenValidationParameters GetO365TokenValidationParameters(string audience)
        {
            return new TokenValidationParameters()
            {
                ValidateIssuer = true,
                ValidIssuers = new[] { O365TokenIssuer },
                ValidateAudience = true,
                ValidAudiences = new[] { audience },
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(5),
                RequireSignedTokens = true
            };
        }
    }
}