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
using ToDoApp.ServiceError;
using ToDoApp.Services.TodoService;
using ToDoApp.Services.UserService;

namespace ToDoApp.Controllers
{

    [Route("todo")]
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


        [HttpPost(), Authorize]
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
        [HttpPut("{todoId:int}"), Authorize]
        public async Task<IActionResult> UpsertTodo(int todoId, TodoDto request)
        {
            int userId = _userService.GetUserId();

            var upsertTodo = new Todo
            {
                Title = request.Title,
                Description = request.Description,
                Start = request.Start,
                End = request.End,
            };

            ErrorOr<Todo> updatedTodo = await _todoService.UpdateTodo(userId,todoId, upsertTodo);

            return updatedTodo.Match(
                updated => NoContent(),
                errors => Problem(errors));
        }

        [HttpDelete("{todoId:int}"), Authorize]
        public async Task<IActionResult> DeleteTodo(int todoId)
        {
            int userId = _userService.GetUserId();
            ErrorOr<Deleted> deletedTodo = await _todoService.DeleteTodo(userId,todoId);
            return deletedTodo.Match(
                deleted => NoContent(),
                errors => Problem(errors));
        }

        [HttpGet(), Authorize]
        public async Task<IActionResult> GetTodosByUserId()
        {
            int userId = _userService.GetUserId();
            ErrorOr<List<Todo>> todos = await _todoService.GetTodosByUserId(userId);

            return todos.Match(
                todosList => Ok(todosList),
                errors => Problem(errors));
        }

        [HttpGet("{todoId:int}"), Authorize]
        public async Task<IActionResult> GetSingleTodo(int todoId)
        {
            int userId = _userService.GetUserId();
            ErrorOr<Todo> todo = await _todoService.GetSingleTodo(userId, todoId);

            return todo.Match(
                singleTodo => Ok(singleTodo),
                errors => Problem(errors));
        }

    }
}