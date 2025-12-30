using Maliev.LeaveService.Domain.Enums;

namespace Maliev.LeaveService.Application.DTOs.Responses;

public class LeaveBalanceDto
{
    public LeaveType LeaveType { get; set; }
    public int Year { get; set; }
    public decimal Entitled { get; set; }
    public decimal Used { get; set; }
    public decimal Pending { get; set; }
    public decimal CarriedForward { get; set; }
    public decimal Available { get; set; }
    public DateTimeOffset? ExpirationDate { get; set; }
}