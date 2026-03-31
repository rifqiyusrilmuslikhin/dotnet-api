using DotnetApi.Domain.Entities;
using DotnetApi.Domain.Enums;
using DotnetApi.Domain.Interfaces;
using DotnetApi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DotnetApi.Infrastructure.Repositories;

public class UserAccountRepository : IUserAccountRepository
{
    private readonly ApplicationDbContext _context;

    public UserAccountRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<UserAccount?> GetByProviderAsync(
        AuthProvider provider,
        string providerKey,
        CancellationToken cancellationToken = default)
    {
        return await _context.UserAccounts
            .FirstOrDefaultAsync(
                a => a.Provider == provider && a.ProviderKey == providerKey.ToLowerInvariant(),
                cancellationToken);
    }

    public async Task<UserAccount?> GetLocalByUserIdAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        return await _context.UserAccounts
            .FirstOrDefaultAsync(
                a => a.UserId == userId && a.Provider == AuthProvider.Local,
                cancellationToken);
    }

    public async Task<UserAccount> AddAsync(UserAccount account, CancellationToken cancellationToken = default)
    {
        await _context.UserAccounts.AddAsync(account, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return account;
    }

    public async Task<UserAccount> UpdateAsync(UserAccount account, CancellationToken cancellationToken = default)
    {
        _context.UserAccounts.Update(account);
        await _context.SaveChangesAsync(cancellationToken);
        return account;
    }
}
