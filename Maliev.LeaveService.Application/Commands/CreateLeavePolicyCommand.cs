using Maliev.LeaveService.Domain.Enums;
using MediatR;

namespace Maliev.LeaveService.Application.Commands;

public class CreateLeavePolicyCommand : IRequest<CommandResult>
{
    public LeaveType LeaveType { get; set; }
    public decimal DefaultEntitlement { get; set; }
    public decimal AccrualRate { get; set; }
    public decimal MaxCarryForward { get; set; }
    public int RequiredApprovalLevels { get; set; }
    public int MaxConsecutiveDays { get; set; }
}
