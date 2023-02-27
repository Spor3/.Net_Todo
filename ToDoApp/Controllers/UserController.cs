using ErrorOr;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using ToDoApp.Data.Model;
using ToDoApp.Dto;
using ToDoApp.Services.UserService;

namespace ToDoApp.Controllers
{
    [Route("user")]
    public class UserController : ErrorController
    {
        private readonly IConfiguration _config;
        private readonly IUserService _userService;

        public UserController( IConfiguration config, IUserService userService)
        {
            _config = config;
            _userService = userService;
        }
        [HttpPost()]
        public async Task<IActionResult> CreateNewUser(UserDto request)
        {
            CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordHashSal);
            var newUser = new User
            {
                Name = request.Name,
                HashPassword = passwordHash,
                HashSaltPassword = passwordHashSal,
                isDeleted = false
            };
            ErrorOr<Created> userCreated = await _userService.CreateUser(newUser);

            return userCreated.Match(
                created => CreatedAtAction(
                    actionName: nameof(CreateNewUser),
                    routeValues: new { id = newUser.Id },
                    value: newUser),
                errors => Problem(errors)
                );
        }
        [HttpPost("login")]
        public async Task<IActionResult> LogUser(UserDto request)
        {
            var newUser = new User
            {
                Name = request.Name,
            };
            ErrorOr<User> logUser = await _userService.TakePss(newUser);

            return logUser.Match(
                exsisting => VerifyPasswordHas(request.Password, exsisting.HashPassword, exsisting.HashSaltPassword) ? //creao token e lo rispondo
                             Ok(new {token = CreateToken(exsisting)}) : BadRequest(),
                errors => Problem(errors));

        }
        [HttpPut(), Authorize]
        public async Task<IActionResult> UpsertUser(UserDto request)
        {
            int userId = _userService.GetUserId();
            CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordHashSal);
            var newUser = new User
            {
                Id = userId,
                Name = request.Name,
                HashPassword = passwordHash,
                HashSaltPassword = passwordHashSal,
            };

            ErrorOr<Updated> updatedUser = await _userService.UpdateUser(newUser);

            return updatedUser.Match(
                updated => NoContent(),
                errors => Problem(errors));
        }

        [HttpDelete(), Authorize]
        public async Task<IActionResult> DeleteUser()
        {
            int userId = _userService.GetUserId();
            ErrorOr<Deleted> deletedUser = await _userService.DeleteUser(userId);

            return deletedUser.Match(
                deleted => NoContent(),
                errors => Problem(errors));
        }

        //Create a new passwordhash and salt start by paswword(req)
        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordHashSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordHashSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }
        //Funzione che virifica la password con un password(req) e passHash e passSalt(DB)
        private bool VerifyPasswordHas(string password, byte[] passwordHash, byte[] passwordHashSalt)
        {
            using (var hmac = new HMACSHA512(passwordHashSalt))
            {
                var computedHas = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return computedHas.SequenceEqual(passwordHash);
            }
        }

        private string CreateToken(User user)
        {
            List<Claim> claims = new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Name),
            };

            var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_config.GetSection("AppSetting:Token").Value));
            var cred = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddDays(2),
                signingCredentials: cred);
            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            return jwt;
        }
    }
}
