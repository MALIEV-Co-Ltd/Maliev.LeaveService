using Maliev.LeaveService.Application.Commands.Handlers;
using Maliev.MessagingContracts.Contracts.Leave;
using MassTransit;
using LocalUndoCloseLeaveBalanceCommand = Maliev.LeaveService.Domain.Commands.UndoCloseLeaveBalanceCommand;

namespace Maliev.LeaveService.Infrastructure.Consumers;

public class UndoCloseLeaveBalanceConsumer : IConsumer<UndoCloseLeaveBalanceCommand>
{
    private readonly UndoCloseLeaveBalanceCommandHandler _handler;

    public UndoCloseLeaveBalanceConsumer(UndoCloseLeaveBalanceCommandHandler handler)
    {
        _handler = handler;
    }

    public async Task Consume(ConsumeContext<UndoCloseLeaveBalanceCommand> context)
    {
        await _handler.HandleAsync(
            new LocalUndoCloseLeaveBalanceCommand
            {
                EmployeeId = context.Message.Payload.EmployeeId
            },
            context.CancellationToken);
    }
}
