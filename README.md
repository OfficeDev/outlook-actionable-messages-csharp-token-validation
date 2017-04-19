# Action Request Token Verification C# Sample

Services can send actionable messages to users to complete simple tasks against their services. When users perform one of the actions in the messages, an action request will be sent by Microsoft to the service. The request from Microsoft will contain a bearer token in the authorization header. This code sample shows how to verify the token to ensure the action request is from Microsoft, and use the claims in the token to validate the request.

        public async Task<HttpResponseMessage> Post([FromBody]string value)
        {
            HttpRequestMessage request = this.ActionContext.Request;
            
            // Validate that we have a bearer token.
            if (request.Headers.Authorization == null ||
                !string.Equals(request.Headers.Authorization.Scheme, "bearer", StringComparison.OrdinalIgnoreCase) ||
                string.IsNullOrEmpty(request.Headers.Authorization.Parameter))
            {
                return request.CreateErrorResponse(HttpStatusCode.Unauthorized, "Bearer token not found.");
            }

            // Validate that the bearer token is valid.
            string bearerToken = request.Headers.Authorization.Parameter;
            ActionableMessageTokenValidator validator = new ActionableMessageTokenValidator();
            ActionableMessageTokenValidationResult result = await validator.ValidateTokenAsync(bearerToken, "https://api.contoso.com");
            if (!result.ValidationSucceeded)
            {
                if (result.Exception != null)
                {
                    Trace.TraceError(result.Exception.ToString());
                }

                return request.CreateErrorResponse(HttpStatusCode.Unauthorized, "Invalid bearer token");
            }

            // We have a valid token. We will next verify the sender and the action performer.
            // In this example, we verify that the email is sent bylob@contoso.com
            // and the action performer is someone with a @contoso.com email address.
            if (!string.Equals(result.Sender, @"lob@contoso.com", StringComparison.OrdinalIgnoreCase) ||
                !result.ActionPerformer.ToLower().StartsWith("john@contoso.com"))
            {
                return request.CreateErrorResponse(HttpStatusCode.Forbidden, string.Empty);
            }

            // Process the request.
            
            return Request.CreateResponse(HttpStatusCode.OK);
        }

More information Outlook Actionable Messages is available [here](https://dev.outlook.com/actions).

## Copyright
Copyright (c) 2016 Microsoft. All rights reserved.