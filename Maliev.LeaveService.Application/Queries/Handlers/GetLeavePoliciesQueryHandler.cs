using Maliev.LeaveService.Application.DTOs.Responses;
using Maliev.LeaveService.Application.Interfaces;
using MediatR;

namespace Maliev.LeaveService.Application.Queries.Handlers;

public class GetLeavePoliciesQueryHandler : IRequestHandler<GetLeavePoliciesQuery, IEnumerable<LeavePolicyDto>>
{
    private readonly ILeavePolicyRepository _policyRepository;

    public GetLeavePoliciesQueryHandler(ILeavePolicyRepository policyRepository)
    {
        _policyRepository = policyRepository;
    }

    public async Task<IEnumerable<LeavePolicyDto>> Handle(GetLeavePoliciesQuery request, CancellationToken cancellationToken)
    {
        var policies = await _policyRepository.GetAllAsync(cancellationToken);

        return policies.Select(p => new LeavePolicyDto
        {
            Id = p.Id,
            LeaveType = p.LeaveType,
            DefaultEntitlement = p.DefaultEntitlement,
            RequiresApproval = p.RequiredApprovalLevels > 0,
            MaxCarryOverDays = (int)p.MaxCarryForward,
            IsActive = p.IsActive
        });
    }
}