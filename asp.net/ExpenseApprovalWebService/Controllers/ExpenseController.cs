//------------------------------------------------------------------------------
//
// Copyright (c) Microsoft Corporation.
// All rights reserved.
//
// This code is licensed under the MIT License.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files(the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and / or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions :
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//
//------------------------------------------------------------------------------

namespace ExpenseApprovalWebService.Controllers
{
    using System;
    using System.Diagnostics;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Web.Http;

    using Microsoft.O365.ActionableMessages.Authentication;

    /// <summary>
    /// APIs for the expense web service.
    /// </summary>
    public class ExpenseController : ApiController
    {
        /// <summary>
        /// The 'Bearer' token type.
        /// </summary>
        private const string BearerTokenType = "bearer";

        /// <summary>
        /// The POST method for the expense controller.
        /// </summary>
        /// <param name="value">Value from the POST request body.</param>
        /// <returns>The asynchronous task.</returns>
        // POST api/expense
        public async Task<HttpResponseMessage> Post([FromBody]string value)
        {
            HttpRequestMessage request = this.ActionContext.Request;

            // Validate that we have a bearer token.
            if (request.Headers.Authorization == null ||
                !string.Equals(request.Headers.Authorization.Scheme, BearerTokenType, StringComparison.OrdinalIgnoreCase) ||
                string.IsNullOrEmpty(request.Headers.Authorization.Parameter))
            {
                return request.CreateErrorResponse(HttpStatusCode.Unauthorized, "Bearer token not found.");
            }

            // Validate that the bearer token is valid.
            //
            // Replace [WEB SERVICE URL] with your service domain URL.
            // For example, if the service URL is https://api.contoso.com/finance/expense?id=1234,
            // then replace [WEB SERVICE URL] with https://api.contoso.com
            string bearerToken = request.Headers.Authorization.Parameter;
            ActionableMessageTokenValidator validator = new ActionableMessageTokenValidator();
            ActionableMessageTokenValidationResult result = await validator.ValidateTokenAsync(bearerToken, "[WEB SERVICE URL]");

            if (!result.ValidationSucceeded)
            {
                if (result.Exception != null)
                {
                    Trace.TraceError(result.Exception.ToString());
                }

                return request.CreateErrorResponse(HttpStatusCode.Unauthorized, "Invalid bearer token");
            }

            // We have a valid token. We will next verify the sender and the action performer.
            // In this example, we verify that the email is sent by Contoso LOB system
            // and the action performer has to be someone with @contoso.com email.
            if (!string.Equals(result.Sender, @"lob@contoso.com", StringComparison.OrdinalIgnoreCase) ||
                !result.ActionPerformer.ToLower().EndsWith("@contoso.com"))
            {
                return request.CreateErrorResponse(HttpStatusCode.InternalServerError, string.Empty);
            }

            // Process your request.

            return Request.CreateResponse(HttpStatusCode.OK);
        }
    }
}
