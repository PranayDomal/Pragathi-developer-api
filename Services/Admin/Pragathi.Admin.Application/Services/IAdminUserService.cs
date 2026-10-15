using Pragathi.Admin.Application.Dtos;

namespace Pragathi.Admin.Application.Services;

public interface IAdminUserService
{
    Task<List<UserDto>> ListAsync();
    Task<UserDto> CreateAsync(CreateUserRequest request);
    Task<UserDto> UpdateAsync(int id, UpdateUserRequest request);
    Task DeleteAsync(int id);
    Task<VerifyCredentialsResult?> VerifyAsync(VerifyCredentialsRequest request);
}