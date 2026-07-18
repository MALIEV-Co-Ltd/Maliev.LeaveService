using Maliev.LeaveService.Domain.Enums;

namespace Maliev.LeaveService.Domain.Entities;

/// <summary>
/// Represents the leave balance for a specific employee, leave type, and year.
/// </summary>
public class LeaveBalance
{
    /// <summary>
    /// Gets or sets the unique identifier for the leave balance record.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier of the employee.
    /// </summary>
    public Guid EmployeeId { get; set; }

    /// <summary>
    /// Gets or sets the type of leave (e.g., Annual, Sick).
    /// </summary>
    public LeaveType LeaveType { get; set; }

    /// <summary>
    /// Gets or sets the calendar year for which this balance applies.
    /// </summary>
    public int Year { get; set; }

    /// <summary>
    /// Gets or sets the total number of days the employee is entitled to for the year.
    /// </summary>
    public decimal Entitled { get; set; }

    /// <summary>
    /// Gets or sets the total number of leave days already used by the employee.
    /// </summary>
    public decimal Used { get; set; }

    /// <summary>
    /// Gets or sets the total number of leave days in pending requests.
    /// </summary>
    public decimal Pending { get; set; }

    /// <summary>
    /// Gets or sets the total number of leave days carried forward from the previous year.
    /// </summary>
    public decimal CarriedForward { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the carried forward leave expires.
    /// </summary>
    public DateTimeOffset? ExpirationDate { get; set; }

    /// <summary>
    /// Gets the current number of available leave days.
    /// Calculated as (Entitled + CarriedForward - Used - Pending).
    /// </summary>
    public decimal Available => Entitled + CarriedForward - Used - Pending;
}
