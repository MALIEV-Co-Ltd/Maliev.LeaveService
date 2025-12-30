using Maliev.LeaveService.Application.DTOs.Responses;
using MediatR;

namespace Maliev.LeaveService.Application.Queries;

public class GetLeavePoliciesQuery : IRequest<IEnumerable<LeavePolicyDto>>
{
}