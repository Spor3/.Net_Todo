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

        public static class User
        {
            public static Error NotFound => Error.NotFound(
                code: "User.NotFound",
                description: "User Not Found."
                );
            public static Error Conflict => Error.Conflict(
                code: "User.AlredyExisting",
                description: "User Already Existing."
                );
        }
    }
}
