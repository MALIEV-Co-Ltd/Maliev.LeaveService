using Maliev.LeaveService.Application.Commands;
using Maliev.LeaveService.Application.Interfaces;
using Maliev.LeaveService.Domain.Entities;
using Maliev.LeaveService.Domain.Enums;
using MediatR;

namespace Maliev.LeaveService.Application.Commands.Handlers;

/// <summary>
/// Handler for ApproveLeaveRequestCommand.
/// </summary>
public class ApproveLeaveRequestCommandHandler : IRequestHandler<ApproveLeaveRequestCommand, CommandResult>
{
    private readonly ILeaveRequestRepository _requestRepository;
    private readonly ILeaveBalanceRepository _balanceRepository;
    private readonly ILeaveApprovalRepository _approvalRepository;

    public ApproveLeaveRequestCommandHandler(
        ILeaveRequestRepository requestRepository,
        ILeaveBalanceRepository balanceRepository,
        ILeaveApprovalRepository approvalRepository)
    {
        _requestRepository = requestRepository;
        _balanceRepository = balanceRepository;
        _approvalRepository = approvalRepository;
    }

    public async Task<CommandResult> Handle(ApproveLeaveRequestCommand request, CancellationToken cancellationToken)
    {
        var leaveRequest = await _requestRepository.GetByIdAsync(request.RequestId, cancellationToken);
        if (leaveRequest == null)
        {
            return CommandResult.Failure("Leave request not found.");
        }

        if (leaveRequest.Status != LeaveRequestStatus.Pending)
        {
            return CommandResult.Failure($"Cannot approve a request with status {leaveRequest.Status}.");
        }

        var balance = await _balanceRepository.GetByEmployeeAndTypeAsync(
            leaveRequest.EmployeeId, 
            leaveRequest.LeaveType, 
            leaveRequest.StartDate.Year, 
            cancellationToken);

        if (balance == null)
        {
            return CommandResult.Failure("Leave balance not found.");
        }

        // Update Request Status
        leaveRequest.Status = LeaveRequestStatus.Approved;
        leaveRequest.UpdatedAt = DateTimeOffset.UtcNow;

        // Update Balance
        balance.Pending -= leaveRequest.TotalDays;
        balance.Used += leaveRequest.TotalDays;

        // Record Approval
        var approval = new LeaveApproval
        {
            Id = Guid.NewGuid(),
            LeaveRequestId = leaveRequest.Id,
            ApproverId = request.ApproverId,
            Status = ApprovalStatus.Approved,
            Comments = request.Comments,
            DecidedAt = DateTimeOffset.UtcNow
        };

        await _requestRepository.UpdateAsync(leaveRequest, cancellationToken);
        await _balanceRepository.UpdateAsync(balance, cancellationToken);
        await _approvalRepository.AddAsync(approval, cancellationToken);

        return CommandResult.Success(leaveRequest.Id);
    }
}
