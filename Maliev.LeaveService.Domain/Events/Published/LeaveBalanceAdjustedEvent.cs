using Maliev.LeaveService.Domain.Enums;

namespace Maliev.LeaveService.Domain.Events.Published;

public class LeaveBalanceAdjustedEvent
{
    public Guid EmployeeId { get; set; }
    public LeaveType LeaveType { get; set; }
    public int Year { get; set; }
    public decimal NewEntitled { get; set; }
    public decimal NewUsed { get; set; }
    public decimal NewPending { get; set; }
    public decimal NewCarriedForward { get; set; }
}