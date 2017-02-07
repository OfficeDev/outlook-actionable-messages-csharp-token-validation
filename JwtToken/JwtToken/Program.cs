using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Protocols;

/// <summary>
/// Sample console app that validates a bearer token.
/// </summary>
namespace JwtToken
{
    class Program
    {
        /// <summary>
        /// The O365 token issuer
        /// </summary>
        public const string O365TokenIssuer = "https://substrate.office.com/sts/";

        /// <summary>
        /// The url with the open id configuration for O365
        /// </summary>
        public const string OpenIdConfigUrl = "https://substrate.office.com/sts/common/.well-known/openid-configuration";

        /// <summary>
        /// Your web service url
        /// </summary>
        public const string audience = ""; // Specify the url of your web service

        static void Main(string[] args)
        {
            Task task = Task.Run(() => {
                
                // Specify your bearer token
                string bearerToken = ""; // Your bearer token value goes here
                ValidateTokenAsync(bearerToken);
            });
            task.Wait();
            Console.ReadLine();
        }

        private static async void ValidateTokenAsync(string tokenSample)
        {
            ConfigurationManager<OpenIdConnectConfiguration> configManager = new ConfigurationManager<OpenIdConnectConfiguration>(OpenIdConfigUrl);

            string issuer = string.Empty;
            List<SecurityToken> signingTokens = null;

            try
            {
                OpenIdConnectConfiguration config = await configManager.GetConfigurationAsync();
                issuer = config.Issuer;
                signingTokens = config.SigningTokens.ToList();
            }
            catch (Exception)
            {
                // Your error handling logic goes here.
                Console.Write("Exception retrieving open id config.");
            }

            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();

            TokenValidationParameters validationParameters = new TokenValidationParameters()
             {
                 ValidIssuer = O365TokenIssuer,
                 ValidAudience = audience,
                 ValidateLifetime = true,
                 ClockSkew = TimeSpan.FromMinutes(5),
                 RequireSignedTokens = true,
                 IssuerSigningTokens = signingTokens
             };

            try
            {
                // Token validation
                SecurityToken validatedToken = new JwtSecurityToken();

                // Reads and validates a token encoded in JSON Compact serialized format.
                ClaimsPrincipal claimsPrincipal = tokenHandler.ValidateToken(tokenSample, validationParameters, out validatedToken);
                Console.WriteLine("Validation finished");
            }
            catch (SecurityTokenValidationException e)
            {
                // Your error handling logic goes here
                Console.Write("Exception in validating token");
            }
        }
    }
}
