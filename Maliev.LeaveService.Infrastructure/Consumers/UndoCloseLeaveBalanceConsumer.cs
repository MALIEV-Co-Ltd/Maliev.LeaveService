using Maliev.LeaveService.Application.Commands.Handlers;
using Maliev.LeaveService.Domain.Commands;
using MassTransit;

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
        await _handler.HandleAsync(context.Message, context.CancellationToken);
    }
}
