using Maliev.LeaveService.Application.DTOs.Responses;
using MediatR;

namespace Maliev.LeaveService.Application.Queries;

/// <summary>
/// Query to retrieve all available leave types and their policies.
/// </summary>
public class GetLeaveTypesQuery : IRequest<IEnumerable<LeavePolicyDto>>
{
}
