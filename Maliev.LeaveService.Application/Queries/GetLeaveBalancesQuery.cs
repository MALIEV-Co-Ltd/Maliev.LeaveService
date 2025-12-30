using Maliev.LeaveService.Application.DTOs.Responses;
using MediatR;

namespace Maliev.LeaveService.Application.Queries;

public class GetLeaveBalancesQuery : IRequest<IEnumerable<LeaveBalanceDto>>
{
    public Guid EmployeeId { get; set; }
    public int? Year { get; set; }
}