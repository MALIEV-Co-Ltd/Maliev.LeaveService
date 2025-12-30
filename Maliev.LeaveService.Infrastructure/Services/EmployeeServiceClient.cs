using System.Net.Http.Json;

namespace Maliev.LeaveService.Infrastructure.Services;

public class EmployeeServiceClient
{
    private readonly HttpClient _httpClient;

    public EmployeeServiceClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IEnumerable<Guid>> GetActiveEmployeeIdsAsync(CancellationToken cancellationToken = default)
    {
        // Mock implementation for now
        // In a real system, this would call the Employee Service
        // return await _httpClient.GetFromJsonAsync<IEnumerable<Guid>>("api/employees/active", cancellationToken) ?? Enumerable.Empty<Guid>();
        return await Task.FromResult(new List<Guid>());
    }
}