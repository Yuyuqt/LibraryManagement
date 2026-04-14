using Database.Models;

namespace LibraryManagement.Backend.Features.Users
{
    public interface IUserService
    {
        public Task<UserCreateResponse> CreateUser(UserCreateRequest request);
    }

    public class UserService : IUserService
    {
        private readonly LibraryManagementContext _context;

        public UserService(LibraryManagementContext context) { 
            _context = context;
        }

        public Task<UserCreateResponse> CreateUser(UserCreateRequest request)
        {
            throw new NotImplementedException();
        }
    }

    public class UserCreateRequest {
        
    }

    public class UserCreateResponse { 
    }
}
