using Maliev.LeaveService.Domain.Enums;

namespace Maliev.LeaveService.Application.DTOs.Responses;

public class UtilizationReportDto
{
    public decimal TotalDaysTaken { get; set; }
    public Dictionary<LeaveType, decimal> DaysByType { get; set; } = new();
    public decimal AverageUtilization { get; set; }
}