using Maliev.LeaveService.Application.DTOs.Responses;
using MediatR;

namespace Maliev.LeaveService.Application.Queries;

/// <summary>
/// Query to retrieve leave requests for a specific employee.
/// </summary>
public class GetLeaveRequestsQuery : IRequest<IEnumerable<LeaveRequestDto>>
{
    public Guid EmployeeId { get; set; }
    public int? Year { get; set; }
}
