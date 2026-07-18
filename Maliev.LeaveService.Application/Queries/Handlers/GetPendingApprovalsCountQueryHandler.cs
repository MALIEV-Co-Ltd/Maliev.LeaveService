using Maliev.LeaveService.Application.Interfaces;
using MediatR;

namespace Maliev.LeaveService.Application.Queries.Handlers;

public class GetPendingApprovalsCountQueryHandler : IRequestHandler<GetPendingApprovalsCountQuery, int>
{
    private readonly ILeaveRequestRepository _requestRepository;

    public GetPendingApprovalsCountQueryHandler(ILeaveRequestRepository requestRepository)
    {
        _requestRepository = requestRepository;
    }

    public async Task<int> Handle(GetPendingApprovalsCountQuery request, CancellationToken cancellationToken)
    {
        return await _requestRepository.GetPendingApprovalsCountAsync(request.ApproverId, cancellationToken);
    }
}
