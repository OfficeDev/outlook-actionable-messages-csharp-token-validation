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

namespace ExpenseApprovalWebService.Controllers
{
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Security.Claims;
    using System.Web.Http;

    // Replace [WEB SERVICE URL] with your service domain URL.
    // For example, if the service URL is https://api.contoso.com/finance/expense?id=1234,
    // then replace [WEB SERVICE URL] with https://api.contoso.com
    [ActionExecutionAgentAuthentication(Audience = "[WEB SERVICE URL]")]
    public class ExpenseController : ApiController
    {
        // POST api/expense
        public HttpResponseMessage Post([FromBody]string value)
        {
            if (!ValidateSenderClaim() || !ValidateSubjectClaim())
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, string.Empty);
            }

            // Process your request.


            return Request.CreateResponse(HttpStatusCode.OK);
        }

        private bool ValidateSenderClaim()
        {
            ClaimsIdentity ci = this.RequestContext.Principal.Identity as ClaimsIdentity;
            Claim senderClaim = ci.Claims.FirstOrDefault(c => string.Equals("sender", c.Type, System.StringComparison.OrdinalIgnoreCase));

            // sender claim will contain the email address of the sender.
            // Validate that the email is sent by your organization.

            return true;
        }

        private bool ValidateSubjectClaim()
        {
            // sub claim will contain the email of the person who performed the action.
            ClaimsIdentity ci = this.RequestContext.Principal.Identity as ClaimsIdentity;
            Claim subjectClaim = ci.Claims.FirstOrDefault(c => string.Equals("sub", c.Type, System.StringComparison.OrdinalIgnoreCase));

            // subject claim will contain the email of the person who performed the action.
            // Validate that the person has the priviledge to perform this action.

            return true;
        }
    }
}
