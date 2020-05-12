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
# アクション要求トークンの検証 C# サンプル

サービスは、アクション可能メッセージをユーザーに送信して、サービスに対する単純なタスクを完了することができます。ユーザーがメッセージに含まれるいずれかのアクションを実行すると、Microsoft によりアクション要求がサービスに対して送信されます。Microsoft からの要求には、認証ヘッダーにベアラー トークンが含まれています。このコード サンプルでは、トークンを検証して、アクション要求が Microsoft からのものであることを確認し、トークンの要求を使用して要求を検証する方法を示します。

        public async Task<HttpResponseMessage> Post([FromBody]string value)
        {
            HttpRequestMessage request = this.ActionContext.Request;

            // ベアラー トークンがあることを検証します。
            if (request.Headers.Authorization == null ||
                !string.Equals(request.Headers.Authorization.Scheme, "bearer", StringComparison.OrdinalIgnoreCase) ||
                string.IsNullOrEmpty(request.Headers.Authorization.Parameter))
            {
                return request.CreateErrorResponse(HttpStatusCode.Unauthorized, new HttpError());
            }
            
            # 認証ヘッダーからトークンを取得します 
            string bearerToken = request.Headers.Authorization.Parameter;
            
            ActionableMessageTokenValidator validator = new ActionableMessageTokenValidator();
            
            // これにより、トークンが指定されたターゲット URL に対して Microsoft によって発行されたこと、
            // つまりターゲットが対象のオーディエンスに一致することを検証します (トークンの “aud” 要求)
            // 
            // コードで、https://api.contoso.com をサービスのベース URL に置き換えます。
            // たとえば、サービスのターゲット URL が https://api.xyz.com/finance/expense?id=1234 の場合、
            // https://api.contoso.com を https://api.xyz.com に置き換えます
            ActionableMessageTokenValidationResult result = await validator.ValidateTokenAsync(bearerToken, "https://api.contoso.com");
            
            if (!result.ValidationSucceeded)
            {
                if (result.Exception != null)
                {
                    Trace.TraceError(result.Exception.ToString());
                }

                return request.CreateErrorResponse(HttpStatusCode.Unauthorized, new HttpError());
            }

            // 有効なトークンがあります。次に、送信者とアクション実行者が予想どおりであることを
            確認します。送信者は、最初にアクション可能メッセージを送信したエンティティの ID であり、 
            アクション実行者は、実際にアクションを実行したユーザーの ID です 
            // (トークン内の “sub” クレーム)。
            // 
            // 以下のコードを独自の検証ロジックに置き換える必要があります 
            // この例では、expense@contoso.com (予想される送信者) によってメールが送信され、
            アクションを実行した人のメールが john@contoso.com (予想される受信者) であることを確認します
            //
            // 応答で CARD-ACTION-STATUS ヘッダーも返す必要があります。
            // ヘッダーの値はユーザーに表示されます。
            if (!string.Equals(result.Sender, @"expense@contoso.com", StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(result.ActionPerformer, @"john@contoso.com", StringComparison.OrdinalIgnoreCase))
            {
                HttpResponseMessage errorResponse = request.CreateErrorResponse(HttpStatusCode.Forbidden, new HttpError());
                errorResponse.Headers.Add("CARD-ACTION-STATUS", "無効な送信者またはアクション実行者は許可されていません。");
                return errorResponse;
            }

            // 経費明細書を処理するための追加のビジネス ロジック コードはこちら。

            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("CARD-ACTION-STATUS", "経費は承認されました。");
            return response;
        }

このコード サンプルでは、JWT 認証に次のライブラリを使用しています。   

[Microsoft.O365.ActionableMessages.Utilities](https://www.nuget.org/packages/Microsoft.O365.ActionableMessages.Utilities)   
[System.IdentityModel.Tokens.Jwt](https://www.nuget.org/packages/System.IdentityModel.Tokens.Jwt)
        
Outlook のアクション可能メッセージの詳細については、[こちら](https://dev.outlook.com/actions)をクリックしてください。

## 著作権
Copyright (c) 2017 Microsoft.All rights reserved.


このプロジェクトでは、[Microsoft オープン ソース倫理規定](https://opensource.microsoft.com/codeofconduct/)が採用されています。詳細については、「[倫理規定の FAQ](https://opensource.microsoft.com/codeofconduct/faq/)」を参照してください。また、その他の質問やコメントがあれば、[opencode@microsoft.com](mailto:opencode@microsoft.com) までお問い合わせください。
