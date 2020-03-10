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
# Пример кода C# для проверки маркера запроса

Службы могут отправлять пользователям интерактивные сообщения для выполнения простых задач в отношении своих служб. При выполнении пользователем одного из действий в сообщении, ему будет отправлен запрос на обслуживание от Майкрософт. Запрос от Майкрософт будет содержать маркер носителя в заголовке авторизации. В этом примере кода показано, как проверить, чтобы убедиться, что запрос получен от Майкрософт, и использовать утверждения в маркере для проверки запроса.

        public async Task<HttpResponseMessage> Post([FromBody]string value)
        {
            запрос HttpRequestMessage = this.ActionContext.Request;

            // Подтвердить, что у нас есть маркер носителя.
            if (request.Headers.Authorization == null ||
                !string.Equals(request.Headers.Authorization.Scheme, "bearer", StringComparison.OrdinalIgnoreCase) ||
                string.IsNullOrEmpty(request.Headers.Authorization.Parameter))
            {
                return request.CreateErrorResponse(HttpStatusCode.Unauthorized, new HttpError());
            }
            
            // Получить маркер из заголовка авторизации 
            строка bearerToken = request.Headers.Authorization.Parameter;
            
            ActionableMessageTokenValidator validator = new ActionableMessageTokenValidator();
            
            // Данным действием будет подтверждено, что маркер был выдан корпорацией Майкрософт
            // для указанного конечного URL-адреса, т. е. цель совпадает с целевой аудиторией (утверждение "aud" в маркере)
            // 
            // В вашем коде замените https://api.contoso.com на базовый URL-адрес вашей службы.
            // Например, если целевой URL-адрес службы — https://api.xyz.com/finance/expense?id=1234,
            // замените https://api.contoso.com на https://api.xyz.com
            ActionableMessageTokenValidationResult result = await validator.ValidateTokenAsync(bearerToken, "https://api.contoso.com");
            
            if (!result.ValidationSucceeded)
            {
                if (result.Exception != null)
                {
                    Trace.TraceError(result.Exception.ToString());
                }

                return request.CreateErrorResponse(HttpStatusCode.Unauthorized, new HttpError());
            }

            // У нас действительный маркер. Теперь нужно убедиться, что отправитель и исполнитель действия являются теми,
            // кого мы ожидаем. Отправитель — это идентификация субъекта, который изначально отправил интерактивное 
            // сообщение, а исполнитель действия — это идентификация пользователя, который в действительности 
            выполнил действие (утверждение "sub" в маркере). 
            // 
            // Вам потребуется заменить код ниже на собственную логику подтверждения 
            // В этом примере мы проверяем, что сообщение отправлено expense@contoso.com (ожидаемый отправитель)
            // и электронная почта лица, выполнившего действие, — john@contoso.com (ожидаемый получатель)
            //
            // Вам потребуется также вернуть заголовок CARD-ACTION-STATUS в ответе.
            // Значение заголовка будет отображаться для пользователя.
            if (!string.Equals(result.Sender, @"expense@contoso.com", StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(result.ActionPerformer, @"john@contoso.com", StringComparison.OrdinalIgnoreCase))
            {
                HttpResponseMessage errorResponse = request.CreateErrorResponse(HttpStatusCode.Forbidden, new HttpError());
                errorResponse.Headers.Add("CARD-ACTION-STATUS", "Недопустимый отправитель или исполнитель действия не разрешен.");
                return errorResponse;
            }

            // Дальнейший представленный здесь код бизнес-логики предназначен для обработки отчета о расходах.

            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("CARD-ACTION-STATUS", "Расход утвержден.");
            return response;
        }

Образец кода использует указанные ниже библиотеки для проверки JWT.   

[Microsoft.O365.ActionableMessages.Utilities](https://www.nuget.org/packages/Microsoft.O365.ActionableMessages.Utilities)   
[System.IdentityModel.Tokens.Jwt](https://www.nuget.org/packages/System.IdentityModel.Tokens.Jwt)
        
Дополнительные сведения об интерактивных сообщениях в Outlook доступны [здесь](https://dev.outlook.com/actions).

## Авторские права
(c) Корпорация Майкрософт (Microsoft Corporation), 2017. Все права защищены.


Этот проект соответствует [Правилам поведения разработчиков открытого кода Майкрософт](https://opensource.microsoft.com/codeofconduct/). Дополнительные сведения см. в разделе [часто задаваемых вопросов о правилах поведения](https://opensource.microsoft.com/codeofconduct/faq/). Если у вас возникли вопросы или замечания, напишите нам по адресу [opencode@microsoft.com](mailto:opencode@microsoft.com).
