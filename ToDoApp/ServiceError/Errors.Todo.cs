using ErrorOr;

namespace ToDoApp.ServiceError
{
    public static class Errors
    {
        public static class Todo
        {
            public static Error NotFound => Error.NotFound(
                code: "Todo.NotFound",
                description: "Todo Not Found."
                );
        }
    }
}
