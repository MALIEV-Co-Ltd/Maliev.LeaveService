using Maliev.LeaveService.Application.Commands;
using Maliev.LeaveService.Application.Interfaces;
using Maliev.LeaveService.Domain.Entities;
using MediatR;

namespace Maliev.LeaveService.Application.Commands.Handlers;

public class CreateLeavePolicyCommandHandler : IRequestHandler<CreateLeavePolicyCommand, CommandResult>
{
    private readonly ILeavePolicyRepository _policyRepository;

    public CreateLeavePolicyCommandHandler(ILeavePolicyRepository policyRepository)
    {
        _policyRepository = policyRepository;
    }

    public async Task<CommandResult> Handle(CreateLeavePolicyCommand request, CancellationToken cancellationToken)
    {
        var existing = await _policyRepository.GetByTypeAsync(request.LeaveType, cancellationToken);
        if (existing != null)
        {
            return CommandResult.Failure($"Policy for {request.LeaveType} already exists.");
        }

        var policy = new LeavePolicy
        {
            Id = Guid.NewGuid(),
            LeaveType = request.LeaveType,
            DefaultEntitlement = request.DefaultEntitlement,
            AccrualRate = request.AccrualRate,
            MaxCarryForward = request.MaxCarryForward,
            RequiredApprovalLevels = request.RequiredApprovalLevels,
            MaxConsecutiveDays = request.MaxConsecutiveDays,
            IsActive = true
        };

        await _policyRepository.AddAsync(policy, cancellationToken);

        return CommandResult.Success(policy.Id);
    }
}