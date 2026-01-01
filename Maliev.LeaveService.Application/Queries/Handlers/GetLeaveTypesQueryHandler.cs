using Maliev.LeaveService.Application.DTOs.Responses;
using Maliev.LeaveService.Application.Interfaces;
using MediatR;

namespace Maliev.LeaveService.Application.Queries.Handlers;

/// <summary>
/// Handler for GetLeaveTypesQuery.
/// </summary>
public class GetLeaveTypesQueryHandler : IRequestHandler<GetLeaveTypesQuery, IEnumerable<LeavePolicyDto>>
{
    private readonly ILeavePolicyRepository _policyRepository;

    public GetLeaveTypesQueryHandler(ILeavePolicyRepository policyRepository)
    {
        _policyRepository = policyRepository;
    }

    public async Task<IEnumerable<LeavePolicyDto>> Handle(GetLeaveTypesQuery request, CancellationToken cancellationToken)
    {
        var policies = await _policyRepository.GetAllAsync();

        return policies.Select(p => new LeavePolicyDto
        {
            Id = p.Id,
            LeaveType = p.LeaveType,
            DefaultEntitlement = p.DefaultEntitlement,
            RequiresApproval = p.RequiredApprovalLevels > 0,
            MaxCarryOverDays = (int)p.MaxCarryForward,
            MaxConsecutiveDays = 0, // Not in DB yet
            IsActive = p.IsActive
        });
    }
}
