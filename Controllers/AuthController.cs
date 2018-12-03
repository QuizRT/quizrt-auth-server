using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using quizrtAuthServer.Models;

namespace quizrtAuthServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuth service;
        public AuthController(IAuth _service)
        {
            this.service = _service;
        }

        // GET api/Auth/users
        [HttpGet]
        [Route("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            List<User> users = await service.GetAllUsersAsync();
            return Ok(users);
        }


        // POST api/values
        [HttpPost]
        [Route("signup")]
        public async Task<IActionResult> SignUpUser([FromBody] User user)
        {
            if(!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if(await service.EmailExistsAsync(user.Email))
            {
                return BadRequest("ERROR! : Email already exists");
            }

            string hashedPassword = service.HashPassword(user.Password);
            user.Password = hashedPassword;

            await service.CreateUserAsync(user);
            return Ok();
        }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] Login user)
        {
            if(user == null)
            {
                return BadRequest("Invalid Request");
            }

            string tokenString = await service.LoginAsync(user.Email, user.Password);
            if(tokenString == "Incorrect Password")
            {
                return BadRequest("ERROR!: Incorrect Password");
            }

            else if(tokenString == null)
            {
                return Unauthorized();
            }

            else
            {
                CookieOptions cookie = new CookieOptions
                {
                    Expires = DateTime.Now.AddMinutes(10)
                };
                HttpContext.Response.Cookies.Append("UserLoginAPItoken", tokenString, cookie);
                return Ok(new {Token = tokenString});
            }
        }

        

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
