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
# Ejemplo de comprobación de token de solicitud de acción en C#

Los servicios pueden enviar mensajes accionables a los usuarios para que realicen tareas sencillas en sus servicios. Cuando un usuario realiza una de las acciones de un mensaje, Microsoft enviará una solicitud de acción al servicio. La solicitud de Microsoft contendrá un token de portador en el encabezado de la autorización. En este ejemplo de código se muestra cómo comprobar el token para garantizar que la solicitud de acción es de Microsoft, y usar las notificaciones del token para validar la solicitud.

        public async Task<HttpResponseMessage> Post([FromBody]string value)
        {
            HttpRequestMessage request = this.ActionContext.Request;

            // Validar que tenemos el token de portador.
            if (request.Headers.Authorization == null ||
                !string.Equals(request.Headers.Authorization.Scheme, "bearer", StringComparison.OrdinalIgnoreCase) ||
                string.IsNullOrEmpty(request.Headers.Authorization.Parameter))
            {
                return request.CreateErrorResponse(HttpStatusCode.Unauthorized, new HttpError());
            }
            
            // Obtener el token del encabezado de autorización 
            string bearerToken = request.Headers.Authorization.Parameter;
            
            ActionableMessageTokenValidator validator = new ActionableMessageTokenValidator();
            
            // Esto validará que el token fue emitido por Microsoft para la
            // dirección URL de destino especificada, es decir, el destino coincide con el público deseado (notificación "aud" de token)
            // 
            // En su código, reemplace https://api.contoso.com por la dirección URL base del servicio.
            // Por ejemplo, si la dirección URL del servicio de destino es https://api.xyz.com/finance/expense?id=1234,
            // entonces reemplace https://api.contoso.com por https://api.xyz.com
            ActionableMessageTokenValidationResult result = await validator.ValidateTokenAsync(bearerToken, "https://api.contoso.com");
            
            if (!result.ValidationSucceeded)
            {
                if (result.Exception != null)
                {
                    Trace.TraceError(result.Exception.ToString());
                }

                return request.CreateErrorResponse(HttpStatusCode.Unauthorized, new HttpError());
            }

            // Ya tenemos un token válido. Ahora verificaremos que el remitente y el ejecutante de la acción sean quiénes
            // nosotros deseamos. El remitente es la identidad de la entidad que envió inicialmente el mensaje 
            // que requiere acción, y el ejecutante de la acción es la identidad del usuario que realmente 
            // realizó la acción (notificación "sub" de token) 
            // 
            // Debería reemplazar el código siguiente con su propia lógica de validación 
            // En este ejemplo, comprobamos que el correo electrónico es enviado por expense@contoso.com (remitente esperado)
            // y el correo electrónico de la persona que realizó la acción es john@contoso.com (destinatario esperado)
            //
            // También debería devolver el encabezado CARD-ACTION-STATUS en la respuesta.
            // El valor del encabezado se mostrará al usuario.
            if (!string.Equals(result.Sender, @"expense@contoso.com", StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(result.ActionPerformer, @"john@contoso.com", StringComparison.OrdinalIgnoreCase))
            {
                HttpResponseMessage errorResponse = request.CreateErrorResponse(HttpStatusCode.Forbidden, new HttpError());
                errorResponse.Headers.Add("CARD-ACTION-STATUS", "Invalid sender or the action performer is not allowed.");
                return errorResponse;
            }

            // Código de lógica empresarial adicional para procesar el informe de gastos.

            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("CARD-ACTION-STATUS", "The expense was approved.");
            return response;
        }

El código de ejemplo usa las siguientes bibliotecas para la validación de JWT.   

[Microsoft.O365.ActionableMessages.Utilities](https://www.nuget.org/packages/Microsoft.O365.ActionableMessages.Utilities)   
[System.IdentityModel.Tokens.Jwt](https://www.nuget.org/packages/System.IdentityModel.Tokens.Jwt)
        
Puede encontrar más información sobre los mensajes accionables de Outlook [aquí](https://dev.outlook.com/actions).

## Derechos de autor
Copyright (c) 2017 Microsoft. Todos los derechos reservados.


Este proyecto ha adoptado el [Código de conducta de código abierto de Microsoft](https://opensource.microsoft.com/codeofconduct/). Para obtener más información, vea [Preguntas frecuentes sobre el código de conducta](https://opensource.microsoft.com/codeofconduct/faq/) o póngase en contacto con [opencode@microsoft.com](mailto:opencode@microsoft.com) si tiene otras preguntas o comentarios.
