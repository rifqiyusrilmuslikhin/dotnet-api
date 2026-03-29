using DotnetApi.Domain.Entities;
using DotnetApi.Infrastructure.Persistence;
using DotnetApi.Infrastructure.Repositories;
using Shouldly;
using Microsoft.EntityFrameworkCore;

namespace DotnetApi.Infrastructure.Tests.Repositories;

public class UserRepositoryTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly UserRepository _sut;

    public UserRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // isolated per test class
            .Options;

        _context = new ApplicationDbContext(options);
        _sut = new UserRepository(_context);
    }

    // ─── AddAsync ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task AddAsync_ShouldPersistUser()
    {
        var user = User.Create("john@example.com", "John Doe", "hashed");

        var saved = await _sut.AddAsync(user);

        saved.Email.ShouldBe("john@example.com");
        var inDb = await _context.Users.FirstOrDefaultAsync(u => u.Email == "john@example.com");
        inDb.ShouldNotBeNull();
    }

    // ─── GetByIdAsync ─────────────────────────────────────────────────────────

    [Fact]
    public async Task GetByIdAsync_WithExistingId_ShouldReturnUser()
    {
        var user = User.Create("john@example.com", "John Doe", "hashed");
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var result = await _sut.GetByIdAsync(user.Id);

        result.ShouldNotBeNull();
        result!.Email.ShouldBe("john@example.com");
    }

    [Fact]
    public async Task GetByIdAsync_WithNonExistentId_ShouldReturnNull()
    {
        var result = await _sut.GetByIdAsync(9999);

        result.ShouldBeNull();
    }

    // ─── GetByEmailAsync ──────────────────────────────────────────────────────

    [Fact]
    public async Task GetByEmailAsync_WithExistingEmail_ShouldReturnUser()
    {
        var user = User.Create("john@example.com", "John Doe", "hashed");
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var result = await _sut.GetByEmailAsync("john@example.com");

        result.ShouldNotBeNull();
    }

    [Fact]
    public async Task GetByEmailAsync_WithNonExistentEmail_ShouldReturnNull()
    {
        var result = await _sut.GetByEmailAsync("nobody@example.com");

        result.ShouldBeNull();
    }

    // ─── ExistsByEmailAsync ───────────────────────────────────────────────────

    [Fact]
    public async Task ExistsByEmailAsync_WithExistingEmail_ShouldReturnTrue()
    {
        var user = User.Create("john@example.com", "John Doe", "hashed");
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        var result = await _sut.ExistsByEmailAsync("john@example.com");

        result.ShouldBeTrue();
    }

    [Fact]
    public async Task ExistsByEmailAsync_WithNonExistentEmail_ShouldReturnFalse()
    {
        var result = await _sut.ExistsByEmailAsync("nobody@example.com");

        result.ShouldBeFalse();
    }

    // ─── UpdateAsync ──────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateAsync_ShouldPersistChanges()
    {
        var user = User.Create("john@example.com", "Old Name", "hashed");
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        user.UpdateProfile("New Name");
        await _sut.UpdateAsync(user);

        var updated = await _context.Users.FirstAsync(u => u.Id == user.Id);
        updated.FullName.ShouldBe("New Name");
    }

    public void Dispose() => _context.Dispose();
}
