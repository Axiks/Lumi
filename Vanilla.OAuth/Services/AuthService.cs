using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using Vanilla.Common.Models;
using Vanilla.OAuth.Interfaces;
using Vanilla.OAuth.Models;

namespace Vanilla.OAuth.Services
{
    public class AuthService
    {
        public readonly TokenConfiguration _tokenConfig;
        public readonly UserRepository _userRepository;
        public AuthService(TokenConfiguration tokenSetting, UserRepository userRepository)
        {
            _tokenConfig = tokenSetting;
            _userRepository = userRepository;
            /*// Build a config object, using env vars and JSON providers.
            IConfigurationRoot config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .Build();

            // Get values from the config given their key and their target type.
            settings = config.GetRequiredSection("Settings").Get<Settings>();
            if (settings == null) throw new Exception("No found setting section");*/
        }
        public string GenerateToken(Guid userId)
        {
            var user = _userRepository.GetUserAsync(userId).Result;
            if (user == null) throw new Exception("Token generation denied. User with this ID not found");

            var handler = new JwtSecurityTokenHandler();

            var privateKey = Encoding.UTF8.GetBytes(_tokenConfig.PrivateKey);
            var expiresTimeSec = _tokenConfig.LifetimeSec;

            var credentials = new SigningCredentials(
                        new SymmetricSecurityKey(privateKey),
                        SecurityAlgorithms.HmacSha256);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                SigningCredentials = credentials,
                Issuer = _tokenConfig.Issuer,
                Audience = _tokenConfig.Audience,
                Expires = DateTime.UtcNow.AddSeconds(expiresTimeSec),
                Subject = GenerateClaims(user)
            };

            var token = handler.CreateToken(tokenDescriptor);
            return handler.WriteToken(token);
        }

        public bool ValidateToken(string authToken)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParameters = GetValidationParameters();

            try
            {
                SecurityToken validatedToken;
                IPrincipal principal = tokenHandler.ValidateToken(authToken, validationParameters, out validatedToken);
                return true;
            }
            catch (SecurityTokenValidationException ex)
            {
                return false;
            }
        }

        private TokenValidationParameters GetValidationParameters()
        {
            var expiresTimeSec = _tokenConfig.LifetimeSec;

            return new TokenValidationParameters()
            {
                ValidateLifetime = false, // Because there is no expiration in the generated token
                ValidateAudience = false, // Because there is no audiance in the generated token
                ValidateIssuer = false,   // Because there is no issuer in the generated token
                ValidIssuer = _tokenConfig.Issuer,
                ValidAudience = _tokenConfig.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_tokenConfig.PrivateKey)) // The same key as the one that generate the token
            };
        }

        private static ClaimsIdentity GenerateClaims(BasicUserModel user)
        {
            var ci = new ClaimsIdentity();

            ci.AddClaim(new Claim("id", user.Id.ToString()));
            if (user.Nickname is not null) ci.AddClaim(new Claim(ClaimTypes.Name, user.Nickname));

            return ci;
        }
    }
}
