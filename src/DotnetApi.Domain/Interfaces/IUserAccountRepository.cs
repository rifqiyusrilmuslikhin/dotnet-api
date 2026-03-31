using DotnetApi.Domain.Entities;
using DotnetApi.Domain.Enums;

namespace DotnetApi.Domain.Interfaces;

public interface IUserAccountRepository
{
    Task<UserAccount?> GetByProviderAsync(AuthProvider provider, string providerKey, CancellationToken cancellationToken = default);
    Task<UserAccount?> GetLocalByUserIdAsync(int userId, CancellationToken cancellationToken = default);
    Task<UserAccount> AddAsync(UserAccount account, CancellationToken cancellationToken = default);
    Task<UserAccount> UpdateAsync(UserAccount account, CancellationToken cancellationToken = default);
}
