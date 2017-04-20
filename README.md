# Action Request Token Verification C# Sample

Services can send actionable messages to users to complete simple tasks against their services. When a user performs one of the actions in a message, an action request will be sent by Microsoft to the service. The request from Microsoft will contain a bearer token in the authorization header. This code sample shows how to verify the token to ensure the action request is from Microsoft, and use the claims in the token to validate the request.

        public async Task<HttpResponseMessage> Post([FromBody]string value)
        {
            HttpRequestMessage request = this.ActionContext.Request;

            // Validate that we have a bearer token.
            if (request.Headers.Authorization == null ||
                !string.Equals(request.Headers.Authorization.Scheme, "bearer", StringComparison.OrdinalIgnoreCase) ||
                string.IsNullOrEmpty(request.Headers.Authorization.Parameter))
            {
                return request.CreateErrorResponse(HttpStatusCode.Unauthorized, new HttpError());
            }

            // Replace https://api.contoso.com with your service domain URL.
            // For example, if the service URL is https://api.xyz.com/finance/expense?id=1234,
            // then replace https://api.contoso.com with https://api.xyz.com
            string bearerToken = request.Headers.Authorization.Parameter;
            ActionableMessageTokenValidator validator = new ActionableMessageTokenValidator();
            ActionableMessageTokenValidationResult result = await validator.ValidateTokenAsync(bearerToken, "https://api.contoso.com");

            if (!result.ValidationSucceeded)
            {
                if (result.Exception != null)
                {
                    Trace.TraceError(result.Exception.ToString());
                }

                return request.CreateErrorResponse(HttpStatusCode.Unauthorized, new HttpError());
            }

            // We have a valid token. We will verify the sender and the action performer. 
            // You should replace the code below with your own validation logic.
            // In this example, we verify that the email is sent by expense@contoso.com
            // and the action performer has to be someone with @contoso.com email.
            //
            // You should also return the CARD-ACTION-STATUS header in the response.
            // The value of the header will be displayed to the user.
            if (!string.Equals(result.Sender, @"expense@contoso.com", StringComparison.OrdinalIgnoreCase) ||
                !result.ActionPerformer.ToLower().EndsWith("@contoso.com"))
            {
                HttpResponseMessage errorResponse = request.CreateErrorResponse(HttpStatusCode.Forbidden, new HttpError());
                errorResponse.Headers.Add("CARD-ACTION-STATUS", "Invalid sender or the action performer is not allowed.");
                return errorResponse;
            }

            // Further business logic code here to process the expense report.

            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("CARD-ACTION-STATUS", "The expense was approved.");
            return response;
        }

The code sample is using the following libraries for JWT validation.   

[Microsoft.O365.ActionableMessages.Utilities](https://www.nuget.org/packages/Microsoft.O365.ActionableMessages.Utilities)   
[System.IdentityModel.Tokens.Jwt](https://www.nuget.org/packages/System.IdentityModel.Tokens.Jwt)
        
More information Outlook Actionable Messages is available [here](https://dev.outlook.com/actions).

## Copyright
Copyright (c) 2017 Microsoft. All rights reserved.