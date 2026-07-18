using Moq;
using Maliev.LeaveService.Application.Commands;
using Maliev.LeaveService.Application.Commands.Handlers;
using Maliev.LeaveService.Application.Interfaces;
using Maliev.LeaveService.Domain.Entities;
using Maliev.LeaveService.Domain.Enums;
using Xunit;

namespace Maliev.LeaveService.Tests.Unit.Handlers;

public class UpdateLeavePolicyCommandHandlerTests
{
    private readonly Mock<ILeavePolicyRepository> _policyRepositoryMock;
    private readonly UpdateLeavePolicyCommandHandler _handler;

    public UpdateLeavePolicyCommandHandlerTests()
    {
        _policyRepositoryMock = new Mock<ILeavePolicyRepository>();
        _handler = new UpdateLeavePolicyCommandHandler(_policyRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ExistingPolicy_ShouldUpdate()
    {
        // Arrange
        var policyId = Guid.NewGuid();
        var command = new UpdateLeavePolicyCommand
        {
            Id = policyId,
            DefaultEntitlement = 25,
            IsActive = false
        };

        var existingPolicy = new LeavePolicy { Id = policyId, LeaveType = LeaveType.Annual, DefaultEntitlement = 20, IsActive = true };

        _policyRepositoryMock.Setup(r => r.GetByIdAsync(policyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingPolicy);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(25, existingPolicy.DefaultEntitlement);
        Assert.False(existingPolicy.IsActive);
        _policyRepositoryMock.Verify(r => r.UpdateAsync(existingPolicy, It.IsAny<CancellationToken>()), Times.Once);
    }
}
