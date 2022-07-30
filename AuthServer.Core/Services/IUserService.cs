using AuthServer.Core.Dtos;
using SharedLibrary.Dtos;

namespace AuthServer.Core.Services
{
    public interface IUserService
    {
        Task<Response<UserDto>> CreateUserAsync(CreateUserDto createUserDto);
        Task<Response<UserDto>> GetUserByNameAsync(string userName);
    }
}
