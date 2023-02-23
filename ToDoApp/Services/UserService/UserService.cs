using System.Security.Claims;

namespace ToDoApp.Services.UserService
{
    public class UserService : IUserService
    {
        private readonly IHttpContextAccessor _contextAccessor;

        public UserService(IHttpContextAccessor contextAccessor)
        {
            _contextAccessor = contextAccessor;
        }

        public int GetUserId()
        {
            var result = 0;

            if(_contextAccessor.HttpContext != null ) 
            {
                result = Int32.Parse(_contextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier));               
            }

            return result;
        }
    }
}
