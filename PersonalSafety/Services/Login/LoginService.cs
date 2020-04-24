using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using PersonalSafety.Options;
using PersonalSafety.Models;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using PersonalSafety.Contracts;
using PersonalSafety.Contracts.Enums;
using PersonalSafety.Models.ViewModels.AccountVM;

namespace PersonalSafety.Services
{
    public class LoginService : ILoginService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly JwtSettings _jwtSettings;
        private readonly TokenValidationParameters _tokenValidationParameters;
        private readonly IRefreshTokenRepository _refreshTokenRepository;

        public LoginService(UserManager<ApplicationUser> userManager, JwtSettings jwtSettings, TokenValidationParameters tokenValidationParameter, IRefreshTokenRepository refreshTokenRepository)
        {
            _userManager = userManager;
            _jwtSettings = jwtSettings;
            _tokenValidationParameters = tokenValidationParameter;
            _refreshTokenRepository = refreshTokenRepository;
        }

        public async Task<AuthenticationDetailsViewModel> GenerateAuthenticationDetailsAsync(ApplicationUser user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSettings.Secret);
            var roles = await _userManager.GetRolesAsync(user);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim("id", user.Id)
            };

            // Add the fetched roles to the list of claims in the JWToken
            claims.AddRange(roles.Select(role => new Claim(ClaimsIdentity.DefaultRoleClaimType, role)));

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                // Things to be included and encoded in the token
                Subject = new ClaimsIdentity(claims),
                // Token will expire 2 hours from which it was created
                Expires = DateTime.UtcNow.Add(_jwtSettings.TokenLifetime),
                //
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };


            var token = tokenHandler.CreateToken(tokenDescriptor);
            var refreshToken = new RefreshToken
            {
                JwtId = token.Id,
                UserId = user.Id,
                CreationDate = DateTime.UtcNow,
                ExpiryDate = DateTime.UtcNow.AddMonths(_jwtSettings.RefreshTokenExpireAfterMonths)
            };
            
            _refreshTokenRepository.RemoveUnusedTokensForUser(user.Id);
            _refreshTokenRepository.Add(refreshToken);
            _refreshTokenRepository.Save();


            var authenticationDetails = new AuthenticationDetailsViewModel
            {
                Token = tokenHandler.WriteToken(token),
                RefreshToken = refreshToken.Token
            };

            return authenticationDetails;
        }

        public APIResponseData ValidateRefreshToken(string token, string refreshToken, out ClaimsPrincipal validatedToken)
        {
            validatedToken = GetPrincipalFromToken(token);

            if (validatedToken == null)
            {
                return new APIResponseData((int)APIResponseCodesEnum.Unauthorized, 
                    new List<string>{ "Error: Invalid Bearer Token." });
            }

            if (!HasTokenExpired(validatedToken))
            {
                return new APIResponseData((int)APIResponseCodesEnum.BadRequest,
                    new List<string> { "Info: Your bearer token hasn't expired yet and can still be used to get access to the system." });
            }

            var storedRefreshToken = _refreshTokenRepository.GetByRefreshToken(refreshToken);

            if (storedRefreshToken == null)
            {
                List<string> messages = new List<string>
                {
                    "Error: This refresh token is invalid, here are the reasons:",
                    "The refresh token might have been used before.",
                    "The refresh token might have been invalidated by the system.",
                    "The refresh token has expired.",
                    "Try logging in again to retrieve new set of tokens."
                };

                return new APIResponseData((int)APIResponseCodesEnum.NotFound, messages);
            }

            var jti = validatedToken.Claims.Single(x => x.Type == JwtRegisteredClaimNames.Jti).Value;
            if (storedRefreshToken.JwtId != jti)
            {
                return new APIResponseData((int)APIResponseCodesEnum.BadRequest, 
                    new List<string>{ "Error: This refresh token does not match your bearer token. Login again." });
            }

            return null;
        }

        public ClaimsPrincipal GetPrincipalFromToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            try
            {
                var tokenValidationParameters = _tokenValidationParameters.Clone();
                tokenValidationParameters.ValidateLifetime = false;
                var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var validatedToken);
                if (!IsJwtWithValidSecurityAlgorithm(validatedToken))
                {
                    return null;
                }

                return principal;
            }
            catch
            {
                return null;
            }
        }

        private bool IsJwtWithValidSecurityAlgorithm(SecurityToken validatedToken)
        {
            return (validatedToken is JwtSecurityToken jwtSecurityToken) &&
                   jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256,
                       StringComparison.InvariantCultureIgnoreCase);
        }

        private bool HasTokenExpired(ClaimsPrincipal validatedToken)
        {
            var expiryDateUnix =
                long.Parse(validatedToken.Claims.Single(x => x.Type == JwtRegisteredClaimNames.Exp).Value);

            var expiryDateTimeUtc = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                .AddSeconds(expiryDateUnix);

            if (expiryDateTimeUtc < DateTime.UtcNow)
            {
                return true;
            }

            return false;
        }
    }
}
