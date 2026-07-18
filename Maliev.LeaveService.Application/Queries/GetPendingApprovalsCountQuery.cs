using MediatR;

namespace Maliev.LeaveService.Application.Queries;

public class GetPendingApprovalsCountQuery : IRequest<int>
{
    public Guid ApproverId { get; set; }
}
