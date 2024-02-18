using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Khalid.Core.Framework
{
    public class AuthenticatedUserService<TUser, TDbContext> : IAuthenticatedUserService<TUser>
        where TUser : class, IUser
        where TDbContext : DbContext
    {
        private IHttpContextAccessor HttpContextAccessor { get; }
        private TDbContext DbContext { get; }

        private const string Secret = "a4c857c2834b4b32aa3b9c7df62c4d9ff14c96185d8e433b901aa1e25561bcf6d5793e41173c4ef6b909d0f9445f045c54ec16c5bb1e4ed785ac11de66e1a34b8fb1fa4b142e4cf9a3ed0905323bb10c";
        private const string AOUTH_KEY_NAME = "Auth";
        private const string ClaimName = "UserId";

        public AuthenticatedUserService(IServiceProvider serviceProvider)
        {
            HttpContextAccessor = serviceProvider.GetRequiredService<IHttpContextAccessor>();
            DbContext = serviceProvider.GetRequiredService<TDbContext>();
        }
        public TUser GetAuthenticatedUser()
        {
            return HttpContextAccessor.HttpContext.Items["User"] as TUser;
        }

        public async Task<bool> TryAttachUserToContext()
        {
            try
            {
                string token = String.Empty;
                if (!HttpContextAccessor.HttpContext.Request.Cookies.TryGetValue(AOUTH_KEY_NAME, out token))
                    token = HttpContextAccessor.HttpContext.Request.Headers[AOUTH_KEY_NAME].FirstOrDefault()?.Split(" ").Last();

                if (token != null)
                {
                    var tokenHandler = new JwtSecurityTokenHandler();
                    var key = Encoding.ASCII.GetBytes(Secret);
                    tokenHandler.ValidateToken(token, new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        // set clockskew to zero so tokens expire exactly at token expiration time (instead of 5 minutes later)
                        ClockSkew = TimeSpan.Zero
                    }, out SecurityToken validatedToken);

                    var jwtToken = (JwtSecurityToken)validatedToken;
                    var userId = int.Parse(jwtToken.Claims.First(x => x.Type == ClaimName).Value);

                    // attach user to context on successful jwt validation
                    HttpContextAccessor.HttpContext.Items["User"] = await DbContext.Set<TUser>().FindAsync(userId);
                    HttpContextAccessor.HttpContext.Items["AuditTrailUserId"] = userId;

                    return true;
                }
            }
            catch
            {
                // do nothing if jwt validation fails
                // user is not attached to context so request won't have access to secure routes
            }
            return false;
        }

        //public void AttachWebUser(string userId)
        //{
        //    var token = GenerateJwtToken(userId);
        //    SetWebToken(token);
        //}
        public string AttachUserToResponse(string userId)
        {
            var token = GenerateJwtToken(userId);
            HttpContextAccessor.HttpContext.Response.Headers.Append(AOUTH_KEY_NAME, token);
            SetWebToken(token);
            return token;
        }
        public void DeAttachUser()
        {
            ClearToken();
        }

        private void ClearToken()
        {
            if (HttpContextAccessor.HttpContext.Request.Cookies.TryGetValue(AOUTH_KEY_NAME, out var token))
                HttpContextAccessor.HttpContext.Response.Cookies.Delete(AOUTH_KEY_NAME);
        }

        private void SetWebToken(string token)
        {
            ClearToken();
            if (HttpContextAccessor.HttpContext.Request.IsHttps)
            {
                HttpContextAccessor.HttpContext.Response.Cookies.Append(AOUTH_KEY_NAME, token, new CookieOptions
                {
                    SameSite = SameSiteMode.None,
                    Secure = true,
                    Expires = DateTime.UtcNow.AddDays(1)
                });
            }
            else
            {
                HttpContextAccessor.HttpContext.Response.Cookies.Append(AOUTH_KEY_NAME, token, new CookieOptions
                {
                    //SameSite = SameSiteMode.None,
                    //Secure = true,
                    Expires = DateTime.UtcNow.AddDays(1)
                });
            }
        }


        public string GenerateJwtToken(string userId)
        {

            // generate token that is valid for 7 days
            var tokenHandler = new JwtSecurityTokenHandler()
            {
                MaximumTokenSizeInBytes = 5,
            };
            var key = Encoding.ASCII.GetBytes(Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] {
                    new Claim(ClaimName,userId)
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenStr = tokenHandler.WriteToken(token);
            return tokenStr;
        }

        public bool HasPermission(params Enum[] roles)
        {
            var user = GetAuthenticatedUser();
            return user != null &&
            (!roles.Any() || roles.Any(s => user.Roles.Contains(s)));

        }
    }


    public interface IAuthenticatedUserService<TUser> where TUser : IUser
    {
        string AttachUserToResponse(string userId);

        TUser GetAuthenticatedUser();

        Task<bool> TryAttachUserToContext();
        //void AttachWebUser(string userId);
        void DeAttachUser();

        string GenerateJwtToken(string userId);

        bool HasPermission(params Enum[] permission);
    }
}
