using Maliev.LeaveService.Application.DTOs.Responses;
using MediatR;

namespace Maliev.LeaveService.Application.Queries;

/// <summary>
/// Query to generate a leave utilization report for the organization.
/// </summary>
public class GetLeaveUtilizationReportQuery : IRequest<LeaveUtilizationReportDto>
{
    /// <summary>
    /// Gets or sets the year for which the report should be generated.
    /// </summary>
    public int Year { get; set; }

    /// <summary>
    /// Gets or sets an optional department filter for the report.
    /// </summary>
    public Guid? DepartmentId { get; set; }
}
