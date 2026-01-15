using Maliev.LeaveService.Application.DTOs.Responses;
using Maliev.LeaveService.Application.Interfaces;
using Maliev.LeaveService.Application.Queries;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Maliev.LeaveService.Application.Queries.Handlers;

public class GetLeaveRequestsQueryHandler : IRequestHandler<GetLeaveRequestsQuery, IEnumerable<LeaveRequestDto>>
{
    private readonly ILeaveRequestRepository _requestRepository;
    private readonly ILogger<GetLeaveRequestsQueryHandler> _logger;

    public GetLeaveRequestsQueryHandler(ILeaveRequestRepository requestRepository, ILogger<GetLeaveRequestsQueryHandler> logger)
    {
        _requestRepository = requestRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<LeaveRequestDto>> Handle(GetLeaveRequestsQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving leave requests for employee {EmployeeId}, year {Year}",
            request.EmployeeId, request.Year);

        var requests = await _requestRepository.GetByEmployeeIdAsync(request.EmployeeId, request.Year, cancellationToken);

        return requests.Select(r => new LeaveRequestDto
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