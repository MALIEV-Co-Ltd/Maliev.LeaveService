namespace Maliev.LeaveService.Application.Interfaces;

public interface IEmployeeServiceClient
{
    Task<IEnumerable<Guid>> GetActiveEmployeeIdsAsync(CancellationToken cancellationToken = default);
}
