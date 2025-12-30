using Maliev.LeaveService.Domain.Enums;
using MediatR;

namespace Maliev.LeaveService.Application.Commands;

public class SubmitLeaveRequestCommand : IRequest<CommandResult>
{
    public Guid EmployeeId { get; set; }
    public LeaveType LeaveType { get; set; }
    public DateTimeOffset StartDate { get; set; }
    public DateTimeOffset EndDate { get; set; }
    public HalfDayPeriod HalfDayPeriod { get; set; }
    public string? Reason { get; set; }
}

public class CommandResult
{
    public bool IsSuccess { get; set; }
    public string? ErrorMessage { get; set; }
    public Guid? Id { get; set; }

    public static CommandResult Success(Guid id) => new() { IsSuccess = true, Id = id };
    public static CommandResult Failure(string message) => new() { IsSuccess = false, ErrorMessage = message };
}