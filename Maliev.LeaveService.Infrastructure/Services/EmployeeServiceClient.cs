using Maliev.LeaveService.Application.Interfaces;
using System.Net.Http.Json;
using System.Text.Json;

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

    public async Task<Guid?> GetEmployeeIdByPrincipalIdAsync(Guid principalId, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.GetAsync(
            $"employee/v1/employees/by-principal/{principalId}",
            cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        using var document = await JsonDocument.ParseAsync(
            await response.Content.ReadAsStreamAsync(cancellationToken),
            cancellationToken: cancellationToken);

        return TryGetGuid(document.RootElement, "id", "Id", "employee_id", "employeeId", "EmployeeId");
    }

    private static Guid? TryGetGuid(JsonElement element, params string[] propertyNames)
    {
        foreach (var propertyName in propertyNames)
        {
            if (element.TryGetProperty(propertyName, out var property) &&
                property.ValueKind == JsonValueKind.String &&
                Guid.TryParse(property.GetString(), out var value))
            {
                return value;
            }
        }

        return null;
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
