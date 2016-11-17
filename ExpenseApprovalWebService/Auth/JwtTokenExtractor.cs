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
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IdentityModel.Tokens;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Security.Claims;
    using System.Threading.Tasks;
    using System.Web.Http.Controllers;
    using Microsoft.IdentityModel.Protocols;

    public class JwtTokenExtractor
    {
        /// <summary>
        /// Shared of OpenIdConnect configuration managers (one per metadata URL)
        /// </summary>
        private static readonly Dictionary<string, ConfigurationManager<OpenIdConnectConfiguration>> _openIdMetadataCache =
            new Dictionary<string, ConfigurationManager<OpenIdConnectConfiguration>>();

        /// <summary>
        /// Token validation parameters for this instance
        /// </summary>
        private readonly TokenValidationParameters _tokenValidationParameters;

        /// <summary>
        /// OpenIdConnect configuration manager for this instances
        /// </summary>
        private readonly ConfigurationManager<OpenIdConnectConfiguration> _openIdMetadata;

        public JwtTokenExtractor(TokenValidationParameters tokenValidationParameters, string metadataUrl)
        {
            // Make our own copy so we can edit it
            _tokenValidationParameters = tokenValidationParameters.Clone();

            if (!_openIdMetadataCache.ContainsKey(metadataUrl))
                _openIdMetadataCache[metadataUrl] = new ConfigurationManager<OpenIdConnectConfiguration>(metadataUrl);

            _openIdMetadata = _openIdMetadataCache[metadataUrl];

            _tokenValidationParameters.ValidateAudience = true;
            _tokenValidationParameters.RequireSignedTokens = true;
        }

        public async Task<ClaimsIdentity> GetIdentityAsync(HttpRequestMessage request)
        {
            if (request.Headers.Authorization != null)
                return await GetIdentityAsync(request.Headers.Authorization.Scheme, request.Headers.Authorization.Parameter).ConfigureAwait(false);
            return null;
        }

        public async Task<ClaimsIdentity> GetIdentityAsync(string authorizationHeader)
        {
            if (authorizationHeader == null)
                return null;

            string[] parts = authorizationHeader?.Split(' ');
            if (parts.Length == 2)
                return await GetIdentityAsync(parts[0], parts[1]).ConfigureAwait(false);
            return null;
        }

        public async Task<ClaimsIdentity> GetIdentityAsync(string scheme, string parameter)
        {
            // No header in correct scheme?
            if (!string.Equals(scheme, "bearer", StringComparison.OrdinalIgnoreCase))
                return null;

            // Issuer isn't allowed? No need to check signature
            if (!HasAllowedIssuer(parameter))
                return null;

            try
            {
                ClaimsPrincipal claimsPrincipal = await ValidateTokenAsync(parameter).ConfigureAwait(false);
                return claimsPrincipal.Identities.OfType<ClaimsIdentity>().FirstOrDefault();
            }
            catch (Exception e)
            {
                Trace.TraceWarning("Invalid token. " + e.ToString());
                return null;
            }
        }

        public void GenerateUnauthorizedResponse(HttpActionContext actionContext)
        {
            string host = actionContext.Request.RequestUri.DnsSafeHost;
            actionContext.Response = actionContext.Request.CreateResponse(HttpStatusCode.Unauthorized);
            actionContext.Response.Headers.Add("WWW-Authenticate", string.Format("Bearer realm=\"{0}\"", host));
            return;
        }

        private bool HasAllowedIssuer(string jwtToken)
        {
            JwtSecurityToken token = new JwtSecurityToken(jwtToken);
            if (_tokenValidationParameters.ValidIssuer != null && _tokenValidationParameters.ValidIssuer == token.Issuer)
                return true;

            if ((_tokenValidationParameters.ValidIssuers ?? Enumerable.Empty<string>()).Contains(token.Issuer))
                return true;

            return false;
        }

        private async Task<ClaimsPrincipal> ValidateTokenAsync(string jwtToken)
        {
            // _openIdMetadata only does a full refresh when the cache expires every 5 days
            OpenIdConnectConfiguration config = null;
            try
            {
                config = await _openIdMetadata.GetConfigurationAsync().ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Trace.TraceError($"Error refreshing OpenId configuration: {e}");

                // No config? We can't continue
                if (config == null)
                    throw;
            }

            // Update the signing tokens from the last refresh
            _tokenValidationParameters.IssuerSigningTokens = config.SigningTokens;

            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();

            try
            {
                SecurityToken parsedToken;
                ClaimsPrincipal principal = tokenHandler.ValidateToken(jwtToken, _tokenValidationParameters, out parsedToken);
                return principal;
            }
            catch (SecurityTokenSignatureKeyNotFoundException)
            {
                string keys = string.Join(", ", ((config?.SigningTokens) ?? Enumerable.Empty<SecurityToken>()).Select(t => t.Id));
                Trace.TraceError("Error finding key for token. Available keys: " + keys);
                throw;
            }
        }
    }
}