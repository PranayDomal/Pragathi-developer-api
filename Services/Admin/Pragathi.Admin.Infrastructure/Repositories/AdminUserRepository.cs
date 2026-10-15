using Microsoft.EntityFrameworkCore;
using Pragathi.Admin.Application.Interfaces;
using Pragathi.Admin.Domain.Entities;
using Pragathi.Admin.Domain.Enums;
using Pragathi.Admin.Infrastructure.Data;

namespace Pragathi.Admin.Infrastructure.Repositories;

public class AdminUserRepository : IAdminUserRepository
{
    private readonly AdminDb _db;
    public AdminUserRepository(AdminDb db) => _db = db;

    public Task<List<AdminUser>> ListAsync() =>
        _db.AdminUsers.AsNoTracking().OrderBy(u => u.Id).ToListAsync();

    public Task<AdminUser?> GetByIdAsync(int id) =>
        _db.AdminUsers.FirstOrDefaultAsync(u => u.Id == id);

    public Task<AdminUser?> GetByLoginIdAsync(string loginId) =>
        _db.AdminUsers.FirstOrDefaultAsync(u => u.LoginId == loginId);

    public Task<int> CountActiveSuperusersAsync() =>
        _db.AdminUsers.CountAsync(u => u.Role == AdminRole.Superuser && u.IsActive);

    public async Task AddAsync(AdminUser user)
    {
        _db.AdminUsers.Add(user);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(AdminUser user)
    {
        _db.AdminUsers.Update(user);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(AdminUser user)
    {
        _db.AdminUsers.Remove(user);
        await _db.SaveChangesAsync();
    }
}