using Maliev.LeaveService.Domain.Enums;

namespace Maliev.LeaveService.Application.DTOs.Responses;

public class LeaveUtilizationReportDto
{
    public int Year { get; set; }
    public List<LeaveTypeUtilizationDto> UtilizationByType { get; set; } = new();
    public decimal OverallUtilizationRate { get; set; }
}

public class LeaveTypeUtilizationDto
{
    public LeaveType LeaveType { get; set; }
    public decimal TotalEntitled { get; set; }
    public decimal TotalUsed { get; set; }
    public decimal UtilizationRate => TotalEntitled > 0 ? (TotalUsed / TotalEntitled) * 100 : 0;
}
