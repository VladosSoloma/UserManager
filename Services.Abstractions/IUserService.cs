using System.Security.Claims;
using Contracts;
using Microsoft.Graph;

namespace Services.Abstractions
{
    public interface IUserService
    {
        Task<IEnumerable<UserDto>> GetUsersAsync();
        Task<UserDto> AddUserAsync(RegisterUserDto user);
        Task<UserDto> UpdateUserAsync(UpdateUserDto user);
        Task DeleteUserAsync(ClaimsPrincipal user, string userId);
    }
}
