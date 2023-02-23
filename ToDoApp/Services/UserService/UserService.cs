using ErrorOr;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using ToDoApp.Data;
using ToDoApp.Data.Model;
using ToDoApp.ServiceError;

namespace ToDoApp.Services.UserService
{
    public class UserService : IUserService
    {
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly DataContext _dataContext;

        public UserService(IHttpContextAccessor contextAccessor, DataContext dataContext)
        {
            _contextAccessor = contextAccessor;
            _dataContext = dataContext;
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

        public async Task<ErrorOr<Created>> CreateUser(User user)
        {
            var exUser = await _dataContext.User.Where(c => c.Name == user.Name).FirstOrDefaultAsync();

            if (exUser != null)
            {
                return Errors.User.Conflict;
            }
            _dataContext.User.Add(user);
            await _dataContext.SaveChangesAsync();

            return Result.Created;
        }
        public async Task<ErrorOr<User>> TakePss(User user)
        {
            var exUser = await _dataContext.User.Where(c => c.Name == user.Name).FirstOrDefaultAsync();
            if (exUser == null)
            {
                return Errors.User.NotFound;
            }

            return exUser;
        }
        public async Task<ErrorOr<Updated>> UpdateUser(User user)
        {
            var upsertUser = await _dataContext.User.FindAsync(user.Id);
            var existingUserName = await  _dataContext.User.Where(c => c.Name == user.Name).FirstOrDefaultAsync();

            if (upsertUser == null)
            {//Todo : Tornare il giusto errore
                return Errors.User.NotFound;
            }
            if(existingUserName != null)
            {
                return Errors.User.Conflict;
            }
            //Aggiorno i dati nuovi dell utente e salvo i cambiamenti
            upsertUser.Name = user.Name;
            upsertUser.HashPassword = user.HashPassword;
            upsertUser.HashSaltPassword = user.HashSaltPassword;

            await _dataContext.SaveChangesAsync();

            return Result.Updated;
        }
        public async Task<ErrorOr<Deleted>> DeleteUser(int userId)
        {
            var user = await _dataContext.User.FindAsync(userId);
            if (user == null)
            {//Todo : Tornare giusto errore
                return Errors.User.NotFound;
            }
            user.isDeleted = true;
            await _dataContext.SaveChangesAsync();

            return Result.Deleted;
        }
    }
}
