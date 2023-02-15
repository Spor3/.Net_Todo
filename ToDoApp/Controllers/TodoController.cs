using ErrorOr;
using Microsoft.AspNetCore.Mvc;
using ToDoApp.Data.Model;
using ToDoApp.Dto;
using ToDoApp.ServiceError;
using ToDoApp.Services.TodoService;

namespace ToDoApp.Controllers
{
    public class TodoController : ErrorController
    {
       private readonly ITodoService _todoService;

      public TodoController(ITodoService todoService)
        {
            _todoService = todoService;
        }

      [HttpPost("user")]
      public async Task<IActionResult> CreateNewUser(UserDto request)
        {
            var newUser = new User
            {
                Email = request.Email,
                Name = request.Name,
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
        [HttpPut("user")]
        public async Task<IActionResult> UpsertUser(int userId, UserDto request)
        {
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
        public async Task<IActionResult> DeleteUser(int userId)
        {
            ErrorOr<Deleted> deletedUser = await _todoService.DeleteUser(userId);

            return deletedUser.Match(
                deleted => NoContent(),
                errors => Problem(errors));
        }

        [HttpPost("todo")]
        public async Task<IActionResult> CreateTodo(int userId ,TodoDto request)
        {
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
        [HttpPut("todo/{todoId:int}")]
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

        [HttpDelete("todo/{todoId:int}")]
        public async Task<IActionResult> DeleteTodo(int todoId)
        {
            ErrorOr<Deleted> deletedTodo = await _todoService.DeleteTodo(todoId);
            return deletedTodo.Match(
                deleted => NoContent(),
                errors => Problem(errors));
        }

        [HttpGet("todo/{userId:int}")]
        public async Task<IActionResult> GetTodosByUserId(int userId)
        {
            ErrorOr<List<Todo>> todos = await _todoService.GetTodosByUserId(userId);

            return todos.Match(
                todosList => Ok(todosList),
                errors => Problem(errors));
        }

        [HttpGet("todo/{userId:int}/{todoId:int}")]
        public async Task<IActionResult> GetSingleTodo(int userId, int todoId)
        {
            ErrorOr<Todo> todo = await _todoService.GetSingleTodo(userId, todoId);

            return todo.Match(
                singleTodo => Ok(singleTodo),
                errors => Problem(errors));
        }

    }
}