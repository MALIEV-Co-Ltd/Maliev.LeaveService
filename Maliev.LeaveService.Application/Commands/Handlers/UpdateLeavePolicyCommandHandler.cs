using Maliev.LeaveService.Application.Commands;
using Maliev.LeaveService.Application.Interfaces;
using MediatR;

namespace Maliev.LeaveService.Application.Commands.Handlers;

public class UpdateLeavePolicyCommandHandler : IRequestHandler<UpdateLeavePolicyCommand, CommandResult>
{
    private readonly ILeavePolicyRepository _policyRepository;

    public UpdateLeavePolicyCommandHandler(ILeavePolicyRepository policyRepository)
    {
        _policyRepository = policyRepository;
    }

    public async Task<CommandResult> Handle(UpdateLeavePolicyCommand request, CancellationToken cancellationToken)
    {
        var policy = await _policyRepository.GetByIdAsync(request.Id, cancellationToken);

        if (policy == null)
        {
            return CommandResult.Failure("Policy not found.");
        }

        if (request.DefaultEntitlement.HasValue) policy.DefaultEntitlement = request.DefaultEntitlement.Value;
        if (request.AccrualRate.HasValue) policy.AccrualRate = request.AccrualRate.Value;
        if (request.MaxCarryForward.HasValue) policy.MaxCarryForward = request.MaxCarryForward.Value;
        if (request.RequiredApprovalLevels.HasValue) policy.RequiredApprovalLevels = request.RequiredApprovalLevels.Value;
        if (request.IsActive.HasValue) policy.IsActive = request.IsActive.Value;

        await _policyRepository.UpdateAsync(policy, cancellationToken);

        return CommandResult.Success(policy.Id);
    }
}