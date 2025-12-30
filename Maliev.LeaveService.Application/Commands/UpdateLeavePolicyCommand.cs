using MediatR;

namespace Maliev.LeaveService.Application.Commands;

public class UpdateLeavePolicyCommand : IRequest<CommandResult>
{
    public Guid Id { get; set; }
    public decimal? DefaultEntitlement { get; set; }
    public decimal? AccrualRate { get; set; }
    public decimal? MaxCarryForward { get; set; }
    public int? RequiredApprovalLevels { get; set; }
    public int? MaxConsecutiveDays { get; set; }
    public bool? IsActive { get; set; }
}