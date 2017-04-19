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

namespace SurveyDemoService
{
    using System;
    using System.Diagnostics;
    using System.ServiceModel.Web;
    using System.Threading.Tasks;

    using Microsoft.O365.ActionableMessages.Authentication;
    using Models;

    /// <summary>
    /// APIs for the survey web service.
    /// </summary>
    public class SurveyDemoService : ISurveyDemoService
    {
        /// <summary>
        /// The 'Bearer' token type.
        /// </summary>
        private const string BearerTokenType = "bearer";

        /// <inheritdoc />
        public async Task<SurveyResponse> PostSurveyAsync(SurveyRequest request)
        {
            SurveyResponse response = new Models.SurveyResponse();

            // Validate that we have a bearer token.
            string authorization = WebOperationContext.Current.IncomingRequest.Headers["authorization"];
            if (string.IsNullOrEmpty(authorization))
            {
                response.IsError = true;
                response.Message = "Bearer token not found.";
                return response;
            }

            string[] parts = authorization.Split(' ');
            if (parts.Length != 2 ||
                !string.Equals(parts[0], BearerTokenType, StringComparison.OrdinalIgnoreCase) ||
                string.IsNullOrEmpty(parts[1]))
            {
                response.IsError = true;
                response.Message = "Bearer token not found.";
                return response;
            }

            // Validate that the bearer token is valid.
            //
            // Replace [WEB SERVICE URL] with your service domain URL.
            // For example, if the service URL is https://api.contoso.com/finance/expense?id=1234,
            // then replace [WEB SERVICE URL] with https://api.contoso.com
            string bearerToken = parts[1];
            ActionableMessageTokenValidator validator = new ActionableMessageTokenValidator();
            ActionableMessageTokenValidationResult result = await validator.ValidateTokenAsync(bearerToken, "[WEB SERVICE URL]");

            if (!result.ValidationSucceeded)
            {
                if (result.Exception != null)
                {
                    Trace.TraceError(result.Exception.ToString());
                }

                response.IsError = true;
                response.Message = "Invalid bearer token.";
                return response;
            }

            // We have a valid token. We will next verify the sender and the action performer.
            // In this example, we verify that the email is sent by Contoso LOB system
            // and the action performer is john@contoso.com.
            if (!string.Equals(result.Sender, @"survey@contoso.com", StringComparison.OrdinalIgnoreCase) ||
                !result.ActionPerformer.EndsWith("@contoso.com"))
            {
                response.IsError = true;
                response.Message = "Invalid sender or action performer.";
                return response;
            }

            // Process the request.

            return response;
        }
    }
}
