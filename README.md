# Action Request Token Verification C# Sample

Services can send actionable messages to users to complete simple tasks against their services. When users perform one of the actions in the messages, an action request will be sent by Microsoft to the service. The request from Microsoft will contain a bearer token in the authorization header. This code sample shows how to verify the token to ensure the action request is from Microsoft, and use the claims in the token to validate the request.

More information Outlook Actionable Messages is available [here](https://dev.outlook.com/actions).

### Requirements
* System.IdentityModel.Tokens.Jwt version 4.x Nuget package
* Microsoft.IdentityModel.Protocol.Extensions version 1.x Nuget Package
* An assembly reference to System.IdentityModel

### Console Application
This project consists of a simple Console Application that validates a given bearer token. An example of an encoded jwt token can be found [here](https://jwt.io/).

In the file Program.cs, fill in the values for the following variables:
```
public const string audience = ""; 
string bearerToken = "";
```
The validation of the token returns a [ClaimsPrincipal](https://msdn.microsoft.com/en-us/library/system.security.claims.claimsprincipal(v=vs.110).aspx) object that can be further examined:
```
 ClaimsPrincipal claimsPrincipal = tokenHandler.ValidateToken(tokenSample, validationParameters, out validatedToken);
```

## Copyright
Copyright (c) 2016 Microsoft. All rights reserved.