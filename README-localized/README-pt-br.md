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
# Exemplo em C# de Verificação de Token de Solicitação de Ação

Serviços podem enviar mensagens acionáveis para usuários completarem tarefas simples em relação aos seus serviços. Quando um usuário executar uma das ações em uma mensagem, uma solicitação de ação será enviada pela Microsoft ao serviço. A solicitação da Microsoft conterá um token de portador no cabeçalho de autorização. Este exemplo de código mostra como verificar o token para garantir que a solicitação de ação veio mesmo da Microsoft e como usar as declarações do token para validar a solicitação.

        public async Task<HttpResponseMessage> Post([FromBody]string value)
        {
            HttpRequestMessage request = this.ActionContext.Request;

            // Valida se há um token de portador.
            if (request.Headers.Authorization == null ||
                !string.Equals(request.Headers.Authorization.Scheme, "bearer", StringComparison.OrdinalIgnoreCase) ||
                string.IsNullOrEmpty(request.Headers.Authorization.Parameter))
            {
                return request.CreateErrorResponse(HttpStatusCode.Unauthorized, new HttpError());
            }
            
            // Solicite um novo token do cabeçalho de autorização. 
            string bearerToken = request.Headers.Authorization.Parameter;
            
            ActionableMessageTokenValidator validator = new ActionableMessageTokenValidator();
            
            // Isso verificará se o token foi emitido pela Microsoft à
            // URL de destino especificada, ou seja, o destino corresponde à audiência pretendida (declaração de "AUD" no token)
            // 
            // Em seu código, substitua https://api.contoso.com pela URL base do seu serviço.
            // Por exemplo, se a URL de destino do serviço for https://api.xyz.com/finance/expense?id=1234,
            // em seguida, substitua https://api.contoso.com por https://api.xyz.com
            ActionableMessageTokenValidationResult result = await validator.ValidateTokenAsync(bearerToken, "https://api.contoso.com");
            
            if (!result.ValidationSucceeded)
            {
                if (result.Exception != null)
                {
                    Trace.TraceError(result.Exception.ToString());
                }

                return request.CreateErrorResponse(HttpStatusCode.Unauthorized, new HttpError());
            }

            // Há um token válido. Agora, verificaremos se o remetente e a ação executores são quem
            // esperamos. O remetente é a identidade da entidade que enviou a Mensagem 
            // Acionável, e o executor da ação é a identidade do usuário que 
            // executa a ação (subdeclaração no token). 
            // 
            // Você deve substituir o código abaixo por sua própria lógica de validação 
            // Neste exemplo, verificamos se o email foi enviado por expense@contoso.com (remetente esperado)
            // e o email da pessoa que executou a ação é john@contoso.com (destinatário esperado)
            //
            // Você também deve retornar o cabeçalho CARD-ACTION-STATUS na resposta.
            // O valor do cabeçalho será exibido para o usuário.
            if (!string.Equals(result.Sender, @"expense@contoso.com", StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(result.ActionPerformer, @"john@contoso.com", StringComparison.OrdinalIgnoreCase))
            {
                HttpResponseMessage errorResponse = request.CreateErrorResponse(HttpStatusCode.Forbidden, new HttpError());
                errorResponse.Headers.Add("CARD-ACTION-STATUS", "Remetente inválido ou o executor da ação não é permitido.");
                return errorResponse;
            }

            // Outro código de lógica de negócios aqui para processar o relatório de despesas.

            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("CARD-ACTION-STATUS", "A despesa foi aprovada.");
            return response;
        }

O exemplo de código está usando as seguintes bibliotecas para a validação de JWT.   

[Microsoft.O365.ActionableMessages.Utilities](https://www.nuget.org/packages/Microsoft.O365.ActionableMessages.Utilities)   
[System.IdentityModel.Tokens.Jwt](https://www.nuget.org/packages/System.IdentityModel.Tokens.Jwt)
        
Mais informações das Mensagens Acionáveis do Outlook estão disponíveis [aqui](https://dev.outlook.com/actions).

## Direitos autorais
Copyright (c) 2017 Microsoft. Todos os direitos reservados.


Este projeto adotou o [Código de Conduta do Código Aberto da Microsoft](https://opensource.microsoft.com/codeofconduct/). Para saber mais, confira [Perguntas frequentes sobre o Código de Conduta](https://opensource.microsoft.com/codeofconduct/faq/) ou contate [opencode@microsoft.com](mailto:opencode@microsoft.com) se tiver outras dúvidas ou comentários.
