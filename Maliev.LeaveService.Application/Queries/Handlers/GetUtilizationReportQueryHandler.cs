using Maliev.LeaveService.Application.DTOs.Responses;
using Maliev.LeaveService.Application.Interfaces;
using Maliev.LeaveService.Application.Queries;
using Maliev.LeaveService.Domain.Enums;
using MediatR;

namespace Maliev.LeaveService.Application.Queries.Handlers;

public class GetUtilizationReportQueryHandler : IRequestHandler<GetUtilizationReportQuery, UtilizationReportDto>
{
    private readonly ILeaveRequestRepository _requestRepository;

    public GetUtilizationReportQueryHandler(ILeaveRequestRepository requestRepository)
    {
        _requestRepository = requestRepository;
    }

    public async Task<UtilizationReportDto> Handle(GetUtilizationReportQuery request, CancellationToken cancellationToken)
    {
        // For MVP, we'll return some basic stats. 
        // In a real implementation, we'd have a specific repository method for reporting.
        var requests = await _requestRepository.GetByEmployeeIdAsync(Guid.Empty, null, cancellationToken);
        var approved = requests.Where(r => r.Status == LeaveRequestStatus.Approved).ToList();

        return new UtilizationReportDto
        {
            TotalDaysTaken = approved.Sum(r => r.TotalDays),
            DaysByType = approved.GroupBy(r => r.LeaveType)
                                .ToDictionary(g => g.Key, g => g.Sum(r => r.TotalDays)),
            AverageUtilization = approved.Any() ? approved.Average(r => r.TotalDays) : 0
        };
    }
}