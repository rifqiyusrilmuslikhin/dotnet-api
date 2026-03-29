using DotnetApi.Domain.Exceptions;
using Shouldly;

namespace DotnetApi.Domain.Tests.Exceptions;

public class DomainExceptionTests
{
    [Fact]
    public void DomainException_ShouldCarryMessage()
    {
        var ex = new DomainException("Something went wrong.");

        ex.Message.ShouldBe("Something went wrong.");
    }

    [Fact]
    public void NotFoundException_ShouldFormatMessage()
    {
        var ex = new NotFoundException("User", 42);

        ex.Message.ShouldContain("User");
        ex.Message.ShouldContain("42");
        ex.EntityName.ShouldBe("User");
        ex.Key.ShouldBe(42);
    }
}
