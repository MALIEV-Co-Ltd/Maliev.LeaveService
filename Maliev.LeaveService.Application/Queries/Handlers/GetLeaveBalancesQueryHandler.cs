using Maliev.LeaveService.Application.DTOs.Responses;
using Maliev.LeaveService.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Maliev.LeaveService.Application.Queries.Handlers;

public class GetLeaveBalancesQueryHandler : IRequestHandler<GetLeaveBalancesQuery, IEnumerable<LeaveBalanceDto>>
{
    private readonly ILeaveBalanceRepository _balanceRepository;
    private readonly ILogger<GetLeaveBalancesQueryHandler> _logger;

    public GetLeaveBalancesQueryHandler(ILeaveBalanceRepository balanceRepository, ILogger<GetLeaveBalancesQueryHandler> logger)
    {
        _balanceRepository = balanceRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<LeaveBalanceDto>> Handle(GetLeaveBalancesQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving leave balances for employee {EmployeeId}, year {Year}",
            request.EmployeeId, request.Year);

        var year = request.Year ?? DateTimeOffset.UtcNow.Year;
        var balances = await _balanceRepository.GetByEmployeeIdAsync(request.EmployeeId, year, cancellationToken);

        return balances.Select(b => new LeaveBalanceDto
        {
            LeaveType = b.LeaveType,
            Year = b.Year,
            Entitled = b.Entitled,
            Used = b.Used,
            Pending = b.Pending,
            CarriedForward = b.CarriedForward,
            Available = b.Available,
            ExpirationDate = b.ExpirationDate
        });
    }
}
