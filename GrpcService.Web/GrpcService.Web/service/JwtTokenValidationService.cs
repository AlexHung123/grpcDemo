using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace GrpcService.Web.service
{
    public class JwtTokenValidationService
    {
        public async Task<TokenModel> GenerateTokenAsync(UserModel model)
        {
            if(model.UserName == "admin" && model.PassWord == "1234")
            {
                var claims = new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub,"email@123.com"),
                    new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString()),
                    new Claim(JwtRegisteredClaimNames.UniqueName,"admin"),
                };

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("111-2222222222--3333333333333333333333333444--6"));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var token = new JwtSecurityToken(
                    "localhost",
                    "localhost",
                    claims,
                    expires:DateTime.UtcNow.AddMinutes(10),
                    signingCredentials:creds);


                return new TokenModel
                {
                    Token = new JwtSecurityTokenHandler().WriteToken(token),
                    Expiration = token.ValidTo,
                    Success = true
                };
            }

            return new TokenModel
            {
                Success = false
            };

        }
    }

    public class TokenModel
    {
        public string Token { get; set; }

        public DateTime Expiration { get; set; }

        public bool Success { get; set; }

    }

    public class UserModel
    {
        [Required]
        public string UserName { get; set; }

        [Required]
        public string PassWord { get; set; }
    }
}
