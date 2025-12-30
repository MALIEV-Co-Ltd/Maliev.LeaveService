using Maliev.LeaveService.Domain.Enums;

namespace Maliev.LeaveService.Domain.Entities;

/// <summary>
/// Represents a formal request for time off by an employee.
/// </summary>
public class LeaveRequest
{
    /// <summary>
    /// Gets or sets the unique identifier for the leave request.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the employee making the request.
    /// </summary>
    public Guid EmployeeId { get; set; }

    /// <summary>
    /// Gets or sets the type of leave being requested (e.g., Annual, Sick).
    /// </summary>
    public LeaveType LeaveType { get; set; }

    /// <summary>
    /// Gets or sets the start date and time of the leave.
    /// </summary>
    public DateTimeOffset StartDate { get; set; }

    /// <summary>
    /// Gets or sets the end date and time of the leave.
    /// </summary>
    public DateTimeOffset EndDate { get; set; }

    /// <summary>
    /// Gets or sets the total number of days requested.
    /// </summary>
    public decimal TotalDays { get; set; }

    /// <summary>
    /// Gets or sets the specific period for a half-day leave request, if applicable.
    /// </summary>
    public HalfDayPeriod HalfDayPeriod { get; set; }

    /// <summary>
    /// Gets or sets the reason provided by the employee for the leave request.
    /// </summary>
    public string? Reason { get; set; }

    /// <summary>
    /// Gets or sets the current status of the leave request (e.g., Pending, Approved).
    /// </summary>
    public LeaveRequestStatus Status { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the request was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the request was last updated.
    /// </summary>
    public DateTimeOffset? UpdatedAt { get; set; }

    /// <summary>
    /// Gets or sets the collection of approvals associated with this leave request.
    /// </summary>
    public ICollection<LeaveApproval> Approvals { get; set; } = new List<LeaveApproval>();
}