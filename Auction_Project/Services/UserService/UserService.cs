using Auction_Project.Models.Users;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Auction_Project.Services.UserService
{
    public class UserService : IUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;

        public UserService(IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
        }


        public void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        public bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512(passwordSalt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return computedHash.SequenceEqual(passwordHash);
            }
        }

        public string CreateToken(User user)
        {
            List<Claim> claims = new List<Claim>();
            if (user.IsAdmin)
            {
                claims.Add(new Claim(ClaimTypes.Role, "Admin"));
            }
            else
            {
                claims.Add(new Claim(ClaimTypes.Role, "User"));
            }


            claims.Add(new Claim(JwtRegisteredClaimNames.Sub, _configuration["Jwt:Subject"]));
            claims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())); // jti = token id
            claims.Add(new Claim(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString()));// issued at DateTime
            claims.Add(new Claim("UserName", user.UserName));


            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var signInCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken
                (
                    _configuration["Jwt:Issuer"],
                    _configuration["Jwt:Audience"],
                    claims,
                    expires: DateTime.UtcNow.AddMinutes(30),
                    signingCredentials: signInCredentials
                );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string GetMyName()
        {
            var result = string.Empty;
            if (_httpContextAccessor.HttpContext != null)
            {
                result = _httpContextAccessor.HttpContext.User.FindFirstValue("UserName");
            }
            return result;
        }

        public string GetMyRole()
        {
            var result = string.Empty;
            if (_httpContextAccessor.HttpContext != null)
            {
                result = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.Role);
            }
            return result;
        }

        public bool IsValidCNP(string cnp)
        {
            int[] c = new int[13];
            if (cnp.Trim().Length != 13)
            {
                return false;
            }
            if (cnp.Trim() == "0000000000000")
            {
                return false;
            }
            for (int i = 0; i <= 12; i++)
            {
                try
                {
                    c[i] = System.Convert.ToInt32(cnp.Substring(i, 1));
                }
                catch (Exception)
                {
                    return false;
                }
            }
            int sum = c[0] * 2 + c[1] * 7 + c[2] * 9 + c[3] * 1 + c[4] * 4 + c[5] * 6 + c[6] * 3 + c[7] * 5 + c[8] * 8 + c[9] * 2 + c[10] * 7 + c[11] * 9;
            int control = sum % 11;
            if (control == 10)
            {
                control = 1;
            }
            if (control != c[12])
            {
                return false;
            }
            return true;
        }

        public bool IsValidEmail(string email)
        {
            var trimmedEmail = email.Trim();

            if (trimmedEmail.EndsWith("."))
            {
                return false;
            }
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == trimmedEmail;
            }
            catch
            {
                return false;
            }
        }

        public int AgeFromCnp(string cnp)
        {
            int y = Convert.ToInt32(cnp.Substring(1, 2));
            if(Convert.ToInt32(cnp.Substring(0,1)) < 5){
                y = 1900 + y;
            }
            else
            {
                y = 2000 + y;
            }
            int m = Convert.ToInt32(cnp.Substring(3, 2));
            int d = Convert.ToInt32(cnp.Substring(5, 2));
            DateTime bd = new DateTime(y, m, d);
            TimeSpan span = DateTime.Now - bd;
            DateTime zeroTime = new DateTime(1, 1, 1);
            return (zeroTime + span).Year - 1;
        }
    }
}