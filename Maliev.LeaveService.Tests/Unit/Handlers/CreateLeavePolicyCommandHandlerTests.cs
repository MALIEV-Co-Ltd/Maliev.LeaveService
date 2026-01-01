using Moq;
using Maliev.LeaveService.Application.Commands;
using Maliev.LeaveService.Application.Commands.Handlers;
using Maliev.LeaveService.Application.Interfaces;
using Maliev.LeaveService.Domain.Entities;
using Maliev.LeaveService.Domain.Enums;
using Xunit;

namespace Maliev.LeaveService.Tests.Unit.Handlers;

public class CreateLeavePolicyCommandHandlerTests
{
    private readonly Mock<ILeavePolicyRepository> _policyRepositoryMock;
    private readonly CreateLeavePolicyCommandHandler _handler;

    public CreateLeavePolicyCommandHandlerTests()
    {
        _policyRepositoryMock = new Mock<ILeavePolicyRepository>();
        _handler = new CreateLeavePolicyCommandHandler(_policyRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_ValidRequest_ShouldCreatePolicy()
    {
        // Arrange
        var command = new CreateLeavePolicyCommand
        {
            LeaveType = LeaveType.Annual,
            DefaultEntitlement = 20,
            AccrualRate = 1.67m,
            MaxCarryForward = 5,
            RequiredApprovalLevels = 1,
            MaxConsecutiveDays = 30
        };

        _policyRepositoryMock.Setup(r => r.GetByTypeAsync(command.LeaveType, It.IsAny<CancellationToken>()))
            .ReturnsAsync((LeavePolicy?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        _policyRepositoryMock.Verify(r => r.AddAsync(It.IsAny<LeavePolicy>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}