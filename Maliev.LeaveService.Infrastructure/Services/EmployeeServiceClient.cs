using Maliev.LeaveService.Application.Interfaces;
using System.Net.Http.Json;

namespace Maliev.LeaveService.Infrastructure.Services;

public class EmployeeServiceClient : IEmployeeServiceClient
{
    private readonly HttpClient _httpClient;

    public EmployeeServiceClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IEnumerable<Guid>> GetActiveEmployeeIdsAsync(CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetFromJsonAsync<EmployeeSearchResultDto>(
            "employee/v1/reports/employees/search?EmploymentStatus=Active&PageSize=1000",
            cancellationToken);

        return response?.Results.Select(r => r.Id) ?? Enumerable.Empty<Guid>();
    }

    private class EmployeeSearchResultDto
    {
        public List<EmployeeSearchItemDto> Results { get; set; } = new();
    }

    private class EmployeeSearchItemDto
    {
        public Guid Id { get; set; }
    }
}