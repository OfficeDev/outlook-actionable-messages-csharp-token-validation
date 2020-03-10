---
page_type: sample
products:
- office-outlook
- office-365
languages:
- javascript
extensions:
  contentType: samples
  technologies:
  - Actionable messages
  createdDate: 11/17/2016 1:46:14 PM
---
# 操作请求令牌验证 C# 示例

服务能够向用户发送可操作邮件，以针对其服务完成简单的任务。用户在邮件中执行其中一个操作时，Microsoft 将向该服务发送一个操作请求。来自 Microsoft 的请求将在授权标头中包含持有者令牌。此代码示例介绍如何验证令牌以确保操作请求来自 Microsoft，然后使用令牌中的声明验证此请求。

        public async Task<HttpResponseMessage> Post([FromBody]string value)
        {
            HttpRequestMessage request = this.ActionContext.Request;

            // 验证是否拥有持有者令牌。
            if (request.Headers.Authorization == null ||
                !string.Equals(request.Headers.Authorization.Scheme, "bearer", StringComparison.OrdinalIgnoreCase) ||
                string.IsNullOrEmpty(request.Headers.Authorization.Parameter))
            {
                return request.CreateErrorResponse(HttpStatusCode.Unauthorized, new HttpError());
            }
            
            // 从授权标头获取令牌 
            string bearerToken = request.Headers.Authorization.Parameter;
            
            ActionableMessageTokenValidator validator = new ActionableMessageTokenValidator();
            
            // 这将验证令牌已由 Microsoft 针对具体的
            // 目标 URL 签发，即与指定受众匹配的目标（令牌中的“aud”声明）
            // 
            // 在代码中，使用服务的基 URL 替换 https://api.contoso.com。
            // 例如，如果服务目标 URL 是 https://api.xyz.com/finance/expense?id=1234，
            // 那么将 https://api.contoso.com 替换为 https://api.xyz.com
            ActionableMessageTokenValidationResult result = await validator.ValidateTokenAsync(bearerToken, "https://api.contoso.com");
            
            if (!result.ValidationSucceeded)
            {
                if (result.Exception != null)
                {
                    Trace.TraceError(result.Exception.ToString());
                }

                return request.CreateErrorResponse(HttpStatusCode.Unauthorized, new HttpError());
            }

            // 我们拥有一个有效的令牌。我们现在将验证发件人和操作执行人是否是
            // 我们预计的人员。发件人是最初发送可操作邮件的实体身份， 
            // 操作执行者是实际执行操作的用户身份 
            //（令牌中的“sub”声明）。
            // 
            // 应使用自有验证逻辑替换下列代码 
            // 在此示例中，我们验证电子邮件是否是由 expense@contoso.com（预计发件人）发送的
            // 以及操作执行人是否是 john@contoso.com（预计收件人）
            //
            // 还应在响应中返回 CARD-ACTION-STATUS 标头。
            // 标头值将显示给用户。
            if (!string.Equals(result.Sender, @"expense@contoso.com", StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(result.ActionPerformer, @"john@contoso.com", StringComparison.OrdinalIgnoreCase))
            {
                HttpResponseMessage errorResponse = request.CreateErrorResponse(HttpStatusCode.Forbidden, new HttpError());
                errorResponse.Headers.Add("CARD-ACTION-STATUS", "Invalid sender or the action performer is not allowed.");
                return errorResponse;
            }

            // 这里的其他业务逻辑代码用于处理费用报表。

            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("CARD-ACTION-STATUS", "The expense was approved.");
            return response;
        }

代码示例使用下列库来进行 JWT 验证。   

[Microsoft.O365.ActionableMessages.Utilities](https://www.nuget.org/packages/Microsoft.O365.ActionableMessages.Utilities)   
[System.IdentityModel.Tokens.Jwt](https://www.nuget.org/packages/System.IdentityModel.Tokens.Jwt)
        
有关 Outlook 可操作邮件的更多信息，请参见[此处](https://dev.outlook.com/actions)。

## 版权信息
版权所有 (c) 2017 Microsoft。保留所有权利。


此项目已采用 [Microsoft 开放源代码行为准则](https://opensource.microsoft.com/codeofconduct/)。有关详细信息，请参阅[行为准则 FAQ](https://opensource.microsoft.com/codeofconduct/faq/)。如有其他任何问题或意见，也可联系 [opencode@microsoft.com](mailto:opencode@microsoft.com)。
