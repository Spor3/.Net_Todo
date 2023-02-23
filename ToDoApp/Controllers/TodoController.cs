using ErrorOr;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using ToDoApp.Data.Model;
using ToDoApp.Dto;
using ToDoApp.ServiceError;
using ToDoApp.Services.TodoService;
using ToDoApp.Services.UserService;

namespace ToDoApp.Controllers
{
    public class TodoController : ErrorController
    {
       private readonly ITodoService _todoService;
        private readonly IConfiguration _config;
        private readonly IUserService _userService;

        public TodoController(ITodoService todoService, IConfiguration config, IUserService userService)
        {
            _todoService = todoService;
            _config = config;
            _userService = userService;
        }

      [HttpPost("user")]
      public async Task<IActionResult> CreateNewUser(UserDto request)
        {
            CreatePasswordHash(request.Password, out byte[] passwordHash, out byte[] passwordHashSal);
            var newUser = new User
            {
                Email = request.Email,
                Name = request.Name,
                HashPassword= passwordHash,
                HashSaltPassword= passwordHashSal
            };
            ErrorOr<Created> userCreated =  await _todoService.CreateUser(newUser);

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
                Email = request.Email,
                Name = request.Name,
            };
            ErrorOr<User> logUser = await _todoService.TakePss(newUser);

            return logUser.Match(
                exsisting => VerifyPasswordHas(request.Password, exsisting.HashPassword, exsisting.HashSaltPassword) ? //creao token e lo rispondo
                             Ok(CreateToken(exsisting)) : BadRequest(),
                errors => Problem(errors));

        }
        [HttpPut("user")]
        public async Task<IActionResult> UpsertUser(UserDto request)
        {
            int userId = _userService.GetUserId();
            var newUser = new User
            {
                Id = userId,
                Email = request.Email,
                Name = request.Name,
            };
        
            ErrorOr<Updated> updatedUser = await _todoService.UpdateUser(newUser);

            return updatedUser.Match(
                updated => NoContent(),
                errors => Problem(errors));
        }

        [HttpDelete("user/{userId:int}")]
        public async Task<IActionResult> DeleteUser()
        {
            int userId = _userService.GetUserId();
            ErrorOr<Deleted> deletedUser = await _todoService.DeleteUser(userId);

            return deletedUser.Match(
                deleted => NoContent(),
                errors => Problem(errors));
        }

        [HttpPost("todo"), Authorize]
        public async Task<IActionResult> CreateTodo(TodoDto request)
        {
            int userId = _userService.GetUserId();
            var newTodo = new Todo
            {
                Title = request.Title,
                Description = request.Description,
                Start = request.Start,
                End = request.End,
            };
            ErrorOr<Todo> finalTodo = await _todoService.CreateTodo(userId, newTodo);

            return finalTodo.Match(
                created => CreatedAtAction(
                    actionName: nameof(CreateTodo),
                    routeValues: new { id = created.Id },
                    value: created),
                errors => Problem(errors)
                );
        }
        [HttpPut("todo/{todoId:int}"), Authorize]
        public async Task<IActionResult> UpsertTodo(int todoId, TodoDto request)
        {
            
            var upsertTodo = new Todo
            {
                Title = request.Title,
                Description = request.Description,
                Start = request.Start,
                End = request.End,
            };

            ErrorOr<Todo> updatedTodo = await _todoService.UpdateTodo(todoId, upsertTodo);

            return updatedTodo.Match(
                updated => NoContent(),
                errors => Problem(errors));
        }

        [HttpDelete("todo/{todoId:int}"), Authorize]
        public async Task<IActionResult> DeleteTodo(int todoId)
        {
            ErrorOr<Deleted> deletedTodo = await _todoService.DeleteTodo(todoId);
            return deletedTodo.Match(
                deleted => NoContent(),
                errors => Problem(errors));
        }

        [HttpGet("todo"), Authorize]
        public async Task<IActionResult> GetTodosByUserId()
        {
            int userId = _userService.GetUserId();
            ErrorOr<List<Todo>> todos = await _todoService.GetTodosByUserId(userId);

            return todos.Match(
                todosList => Ok(todosList),
                errors => Problem(errors));
        }

        [HttpGet("todo/{todoId:int}"), Authorize]
        public async Task<IActionResult> GetSingleTodo(int todoId)
        {
            int userId = _userService.GetUserId();
            ErrorOr<Todo> todo = await _todoService.GetSingleTodo(userId, todoId);

            return todo.Match(
                singleTodo => Ok(singleTodo),
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
        private bool VerifyPasswordHas(string password, byte[] passwordHash, byte[] passwordHashSalt )
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