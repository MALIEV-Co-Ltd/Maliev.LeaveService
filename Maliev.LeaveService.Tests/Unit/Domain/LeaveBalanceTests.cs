using Maliev.LeaveService.Domain.Entities;
using Maliev.LeaveService.Domain.Enums;
using Xunit;

namespace Maliev.LeaveService.Tests.Unit.Domain;

public class LeaveBalanceTests
{
    [Theory]
    [InlineData(20, 5, 3, 2, 20)] // 20 + 5 - 3 - 2 = 20
    [InlineData(15, 0, 5, 0, 10)] // 15 + 0 - 5 - 0 = 10
    [InlineData(10, 2, 0, 0, 12)] // 10 + 2 - 0 - 0 = 12
    [InlineData(0, 0, 0, 0, 0)]   // All zeros
    [InlineData(10, 0, 10, 0, 0)]  // Used all entitled
    [InlineData(10, 5, 15, 0, 0)]  // Used all entitled + carried forward
    public void Available_ShouldCalculateCorrectly(decimal entitled, decimal carriedForward, decimal used, decimal pending, decimal expected)
    {
        // Arrange
        var balance = new LeaveBalance
        {
            Entitled = entitled,
            CarriedForward = carriedForward,
            Used = used,
            Pending = pending
        };

        // Act
        var available = balance.Available;

        // Assert
        Assert.Equal(expected, available);
    }
}
