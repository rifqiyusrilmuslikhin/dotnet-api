using DotnetApi.Domain.Entities;
using DotnetApi.Infrastructure.Persistence;
using DotnetApi.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Shouldly;

namespace DotnetApi.Infrastructure.Tests.Repositories;

public class RefreshTokenRepositoryTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly RefreshTokenRepository _sut;

    public RefreshTokenRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new ApplicationDbContext(options);
        _sut = new RefreshTokenRepository(_context);
    }

    public void Dispose() => _context.Dispose();

    private async Task<User> SeedUserAsync()
    {
        var user = User.Create("test@example.com", "Test User");
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
        return user;
    }

    [Fact]
    public async Task AddAsync_ShouldPersistRefreshToken()
    {
        // Arrange
        var user = await SeedUserAsync();
        var token = RefreshToken.Create(user.Id, "test-token-value", 7);

        // Act
        var result = await _sut.AddAsync(token);

        // Assert
        result.Id.ShouldBeGreaterThan(0);
        result.Token.ShouldBe("test-token-value");
        result.UserId.ShouldBe(user.Id);

        var fromDb = await _context.RefreshTokens.FirstOrDefaultAsync(r => r.Token == "test-token-value");
        fromDb.ShouldNotBeNull();
    }

    [Fact]
    public async Task GetByTokenAsync_WithExistingToken_ShouldReturnRefreshToken()
    {
        // Arrange
        var user = await SeedUserAsync();
        var token = RefreshToken.Create(user.Id, "lookup-token", 7);
        await _context.RefreshTokens.AddAsync(token);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetByTokenAsync("lookup-token");

        // Assert
        result.ShouldNotBeNull();
        result.Token.ShouldBe("lookup-token");
        result.UserId.ShouldBe(user.Id);
    }

    [Fact]
    public async Task GetByTokenAsync_WithNonExistentToken_ShouldReturnNull()
    {
        // Act
        var result = await _sut.GetByTokenAsync("does-not-exist");

        // Assert
        result.ShouldBeNull();
    }

    [Fact]
    public async Task GetActiveByUserIdAsync_ShouldReturnOnlyActiveTokens()
    {
        // Arrange
        var user = await SeedUserAsync();

        var activeToken = RefreshToken.Create(user.Id, "active-token", 7);
        var revokedToken = RefreshToken.Create(user.Id, "revoked-token", 7);
        revokedToken.Revoke();

        await _context.RefreshTokens.AddRangeAsync(activeToken, revokedToken);
        await _context.SaveChangesAsync();

        // Act
        var result = await _sut.GetActiveByUserIdAsync(user.Id);

        // Assert
        result.Count.ShouldBe(1);
        result[0].Token.ShouldBe("active-token");
    }

    [Fact]
    public async Task UpdateAsync_ShouldPersistChanges()
    {
        // Arrange
        var user = await SeedUserAsync();
        var token = RefreshToken.Create(user.Id, "update-token", 7);
        await _context.RefreshTokens.AddAsync(token);
        await _context.SaveChangesAsync();

        // Act
        token.Revoke("replacement-token");
        var result = await _sut.UpdateAsync(token);

        // Assert
        result.IsRevoked.ShouldBeTrue();
        result.ReplacedByToken.ShouldBe("replacement-token");

        var fromDb = await _context.RefreshTokens.FirstAsync(r => r.Token == "update-token");
        fromDb.RevokedAt.ShouldNotBeNull();
        fromDb.ReplacedByToken.ShouldBe("replacement-token");
    }
}
