using Pragathi.Admin.Application.Dtos;
using Pragathi.Admin.Application.Exceptions;
using Pragathi.Admin.Application.Interfaces;
using Pragathi.Admin.Domain.Entities;
using Pragathi.Admin.Domain.Enums;

namespace Pragathi.Admin.Application.Services;

public class AdminUserService : IAdminUserService
{
    private readonly IAdminUserRepository _repo;
    private readonly IPasswordHasher _hasher;

    public AdminUserService(IAdminUserRepository repo, IPasswordHasher hasher)
    {
        _repo = repo;
        _hasher = hasher;
    }

    public async Task<List<UserDto>> ListAsync()
    {
        var users = await _repo.ListAsync();
        return users.Select(ToDto).ToList();
    }

    public async Task<UserDto> CreateAsync(CreateUserRequest r)
    {
        var loginId = (r.LoginId ?? "").Trim().ToLowerInvariant();
        var name = (r.Name ?? "").Trim();

        if (loginId.Length < 3) throw new ValidationFailedException("Login ID must be at least 3 characters.");
        if (name.Length == 0) throw new ValidationFailedException("Name is required.");
        CheckPassword(r.Password);
        if (!Enum.IsDefined(r.Role)) throw new ValidationFailedException("Unknown role.");

        if (await _repo.GetByLoginIdAsync(loginId) != null)
            throw new ValidationFailedException("That login ID is already taken.");

        var user = new AdminUser
        {
            LoginId = loginId,
            Name = name,
            PasswordHash = _hasher.Hash(r.Password!),
            Role = r.Role
        };
        await _repo.AddAsync(user);
        return ToDto(user);
    }

    public async Task<UserDto> UpdateAsync(int id, UpdateUserRequest r)
    {
        var user = await _repo.GetByIdAsync(id) ?? throw new NotFoundException("User not found.");

        if (r.Name != null)
        {
            var name = r.Name.Trim();
            if (name.Length == 0) throw new ValidationFailedException("Name cannot be empty.");
            user.Name = name;
        }

        if (!string.IsNullOrEmpty(r.Password))
        {
            CheckPassword(r.Password);
            user.PasswordHash = _hasher.Hash(r.Password);
        }

        var newRole = r.Role ?? user.Role;
        var newActive = r.IsActive ?? user.IsActive;
        if (!Enum.IsDefined(newRole)) throw new ValidationFailedException("Unknown role.");

        // Rule: never leave the system without an active superuser
        var losingSuperpowers = user.Role == AdminRole.Superuser && user.IsActive
                                && (newRole != AdminRole.Superuser || !newActive);
        if (losingSuperpowers) await EnsureNotLastSuperuser();

        user.Role = newRole;
        user.IsActive = newActive;
        await _repo.UpdateAsync(user);
        return ToDto(user);
    }

    public async Task DeleteAsync(int id)
    {
        var user = await _repo.GetByIdAsync(id) ?? throw new NotFoundException("User not found.");
        if (user.Role == AdminRole.Superuser && user.IsActive) await EnsureNotLastSuperuser();
        await _repo.DeleteAsync(user);
    }

    public async Task<VerifyCredentialsResult?> VerifyAsync(VerifyCredentialsRequest r)
    {
        if (string.IsNullOrWhiteSpace(r.LoginId) || string.IsNullOrEmpty(r.Password)) return null;

        var user = await _repo.GetByLoginIdAsync(r.LoginId.Trim().ToLowerInvariant());
        if (user == null)
        {
            _hasher.Hash(r.Password);   // do the same work, so response time doesn't reveal "no such user"
            return null;
        }
        if (!user.IsActive || !_hasher.Verify(r.Password, user.PasswordHash)) return null;

        return new VerifyCredentialsResult(user.LoginId, user.Name, user.Role);
    }

    // ---- helpers ----
    private async Task EnsureNotLastSuperuser()
    {
        if (await _repo.CountActiveSuperusersAsync() <= 1)
            throw new ValidationFailedException("At least one active superuser must remain.");
    }

    private static void CheckPassword(string? password)
    {
        if (string.IsNullOrEmpty(password) || password.Length < 8)
            throw new ValidationFailedException("Password must be at least 8 characters.");
    }

    private static UserDto ToDto(AdminUser u) => new(u.Id, u.LoginId, u.Name, u.Role, u.IsActive);
}