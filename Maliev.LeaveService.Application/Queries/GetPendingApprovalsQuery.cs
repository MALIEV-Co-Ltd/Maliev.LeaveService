using Maliev.LeaveService.Application.DTOs.Responses;
using MediatR;

namespace Maliev.LeaveService.Application.Queries;

public class GetPendingApprovalsQuery : IRequest<IEnumerable<LeaveRequestDto>>
{
    public Guid ApproverId { get; set; }
}