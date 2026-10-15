using Pragathi.Admin.Domain.Entities;

namespace Pragathi.Admin.Application.Interfaces;

public interface IAdminUserRepository
{
    Task<List<AdminUser>> ListAsync();
    Task<AdminUser?> GetByIdAsync(int id);
    Task<AdminUser?> GetByLoginIdAsync(string loginId);
    Task<int> CountActiveSuperusersAsync();   // so we never delete the last boss
    Task AddAsync(AdminUser user);
    Task UpdateAsync(AdminUser user);
    Task DeleteAsync(AdminUser user);
}