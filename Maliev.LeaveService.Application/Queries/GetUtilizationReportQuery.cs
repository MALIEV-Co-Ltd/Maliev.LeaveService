using Maliev.LeaveService.Application.DTOs.Responses;
using MediatR;

namespace Maliev.LeaveService.Application.Queries;

public class GetUtilizationReportQuery : IRequest<UtilizationReportDto>
{
    public Guid? DepartmentId { get; set; }
    public DateTimeOffset? StartDate { get; set; }
    public DateTimeOffset? EndDate { get; set; }
}
