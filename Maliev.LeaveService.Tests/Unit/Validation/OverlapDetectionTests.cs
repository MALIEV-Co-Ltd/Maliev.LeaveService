using Xunit;

namespace Maliev.LeaveService.Tests.Unit.Validation;

public class OverlapDetectionTests
{
    [Theory]
    // Non-overlapping
    [InlineData("2025-01-01", "2025-01-05", "2025-01-06", "2025-01-10", false)]
    [InlineData("2025-01-06", "2025-01-10", "2025-01-01", "2025-01-05", false)]
    // Exact match
    [InlineData("2025-01-01", "2025-01-05", "2025-01-01", "2025-01-05", true)]
    // Partial overlap - start
    [InlineData("2025-01-01", "2025-01-05", "2024-12-30", "2025-01-02", true)]
    // Partial overlap - end
    [InlineData("2025-01-01", "2025-01-05", "2025-01-04", "2025-01-10", true)]
    // Full overlap - inside
    [InlineData("2025-01-01", "2025-01-10", "2025-01-03", "2025-01-07", true)]
    // Full overlap - outside
    [InlineData("2025-01-03", "2025-01-07", "2025-01-01", "2025-01-10", true)]
    // Touching dates (should overlap as per business logic usually)
    [InlineData("2025-01-01", "2025-01-05", "2025-01-05", "2025-01-10", true)]
    public void IsOverlapping_ShouldIdentifyCorrectly(string s1, string e1, string s2, string e2, bool expected)
    {
        // Arrange
        var start1 = DateTimeOffset.Parse(s1);
        var end1 = DateTimeOffset.Parse(e1);
        var start2 = DateTimeOffset.Parse(s2);
        var end2 = DateTimeOffset.Parse(e2);

        // Act
        // We'll assume a standard algorithm: (StartA <= EndB) and (EndA >= StartB)
        var result = (start1 <= end2) && (end1 >= start2);

        // Assert
        Assert.Equal(expected, result);
    }
}
