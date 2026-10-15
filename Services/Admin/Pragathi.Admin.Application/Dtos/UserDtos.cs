using Pragathi.Admin.Domain.Enums;

namespace Pragathi.Admin.Application.Dtos;

// What we send OUT when listing users (no password hash!)
public record UserDto(int Id, string LoginId, string Name, AdminRole Role, bool IsActive);

// What the superuser sends to create or edit a user
public record CreateUserRequest(string? LoginId, string? Name, string? Password, AdminRole Role);
public record UpdateUserRequest(string? Name, string? Password, AdminRole? Role, bool? IsActive);

// Used by the Auth service to check a login
public record VerifyCredentialsRequest(string? LoginId, string? Password);
public record VerifyCredentialsResult(string LoginId, string Name, AdminRole Role);