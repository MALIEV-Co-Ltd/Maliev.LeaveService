using Maliev.LeaveService.Application.DTOs.Responses;
using Maliev.LeaveService.Application.Interfaces;
using MediatR;

namespace Maliev.LeaveService.Application.Queries.Handlers;

public class GetPendingApprovalsQueryHandler : IRequestHandler<GetPendingApprovalsQuery, IEnumerable<LeaveRequestDto>>
{
    private readonly ILeaveRequestRepository _requestRepository;

    public GetPendingApprovalsQueryHandler(ILeaveRequestRepository requestRepository)
    {
        _requestRepository = requestRepository;
    }

    public async Task<IEnumerable<LeaveRequestDto>> Handle(GetPendingApprovalsQuery request, CancellationToken cancellationToken)
    {
        var pendingRequests = await _requestRepository.GetPendingApprovalsAsync(request.ApproverId, cancellationToken);

        return pendingRequests.Select(r => new LeaveRequestDto
        {
            Id = r.Id,
            EmployeeId = r.EmployeeId,
            LeaveType = r.LeaveType,
            StartDate = r.StartDate,
            EndDate = r.EndDate,
            TotalDays = r.TotalDays,
            HalfDayPeriod = r.HalfDayPeriod,
            Reason = r.Reason,
            Status = r.Status,
            CreatedAt = r.CreatedAt
        });
    }
}
