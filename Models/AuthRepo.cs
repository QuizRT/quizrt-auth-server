using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Consul;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.EntityFrameworkCore;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Sockets;

namespace quizrtAuthServer.Models
{
    public class AuthRepo : IAuth
    {
        AuthContext context;
        private readonly string Salt = "gj+oAMieIg+2B/eoxA31+w==";
        private readonly byte[] Saltbyte;
        public AuthRepo(AuthContext _context)
        {
            this.context = _context;
            Saltbyte = Encoding.ASCII.GetBytes(Salt);
        }
        public async Task<List<User>> GetAllUsersAsync()
        {
            List<User> users = await context.Users.ToListAsync();
            return users;  
        }
        public Task<bool> EmailExistsAsync(string email)
        {
            return context.Users.AnyAsync(x => x.Email == email);
        }
        public Task<bool> UserExistsAsync(User user)
        {
            return context.Users.AnyAsync(x => x.Email == user.Email);
        }
        public async Task CreateUserAsync(User user)
        {
            bool doesUserExist = await UserExistsAsync(user);
            if(!doesUserExist)
            {
                context.Users.Add(user);
                await context.SaveChangesAsync();
            }
        }
        public async Task DeleteUserByIdAsync(int id)
        {
            var user = await context.Users.FindAsync(id);
            if(user == null)
            {
                return;
            }
            context.Users.Remove(user);
            await context.SaveChangesAsync();
        }
        public async Task<User> GetUserByEmailAsync(string email)
        {
            var user = await context.Users.FirstOrDefaultAsync(x => x.Email == email);
            return user;
        }
        public string HashPassword(string Password)
        {
            string hashpassword = Convert.ToBase64String(KeyDerivation.Pbkdf2(
            password: Password,
            salt: Saltbyte,
            prf: KeyDerivationPrf.HMACSHA1,
            iterationCount: 10000,
            numBytesRequested: 256 / 8));
            return hashpassword;
        }
        public Dictionary<string, string> GetUserDetailsFromToken(string Token)
        {
            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
            JwtSecurityToken token = handler.ReadJwtToken(Token);
            Dictionary<string, string> userDetails = new Dictionary<string, string>();
            userDetails.Add("UserID", token.Claims.First(c1 => c1.Type == "UserID").Value);
            userDetails.Add("Name", token.Claims.First(c1 => c1.Type == "Name").Value);
            userDetails.Add("Email", token.Claims.First(c1 => c1.Type == "Email").Value);
            return userDetails;
        }
        public async Task<string> LoginAsync(string email, string password)
        {
            bool correctEmail = await EmailExistsAsync(email);
            if(correctEmail)
            {
                User user = await GetUserByEmailAsync(email);
                string hashPassword = HashPassword(password);
                if(hashPassword == user.Password)
                {
                    Chilkat.Global glob = new Chilkat.Global();
                    glob.UnlockBundle("Anything for 30-day trial");

                    Chilkat.Rsa rsaKey = new Chilkat.Rsa();

                    rsaKey.GenerateKey(1024);

                    var rsaPrivateKey = rsaKey.ExportPrivateKeyObj();
                    var rsaPublicKey = rsaKey.ExportPublicKeyObj();

                    var rsaPublicKeyAsString = rsaKey.ExportPublicKey();

                    Chilkat.JsonObject jwtHeader = new Chilkat.JsonObject();
                    jwtHeader.AppendString("alg", "RS256");
                    jwtHeader.AppendString("typ", "JWT");

                    Chilkat.JsonObject claims = new Chilkat.JsonObject();
                    claims.AppendString("UserID", user.UserID.ToString());
                    claims.AppendString("Name", user.Name);
                    claims.AppendString("Email", user.Email);

                    Chilkat.Jwt jwt = new Chilkat.Jwt();
                    int currentDateTime = jwt.GenNumericDate(0);
                    claims.AddIntAt(-1, "exp", currentDateTime + 3000);
                    
                    string token = jwt.CreateJwtPk(jwtHeader.Emit(), claims.Emit(), rsaPrivateKey);

                    using (var client = new ConsulClient())
                    {
                        //string ConsulIp = Environment.GetEnvironmentVariable("MACHINE_LOCAL_IPV4");
                        //string ConsulIpHost = "http://" + ConsulIp + ":8500";
                        string ConsulIpHost = "http://consul:8500";
                        Console.WriteLine(ConsulIpHost);
                        client.Config.Address = new Uri(ConsulIpHost);
                        var putPair = new KVPair("secretkey")
                        {
                            Value = Encoding.UTF8.GetBytes(rsaPublicKeyAsString)
                        };

                        var putAttempt = await client.KV.Put(putPair);
                    }

                    return token;
                }

                else
                {
                    return "Incorrect Password";
                }           
            }
            else
            {
                return null;
            }
        }

    }
}