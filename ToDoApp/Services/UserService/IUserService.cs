using ErrorOr;
using ToDoApp.Data.Model;

namespace ToDoApp.Services.UserService
{
    public interface IUserService
    {
        public int GetUserId();
        Task<ErrorOr<Created>> CreateUser(User user);
        Task<ErrorOr<User>> TakePss(User user);
        Task<ErrorOr<Updated>> UpdateUser(User user);
        Task<ErrorOr<Deleted>> DeleteUser(int userId);
    }
}
