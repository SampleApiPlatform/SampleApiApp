using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using NuGet.SampleSharedModels.DTO;
using NuGet.SampleSharedModels.Interfaces.ServiceBus;
using NuGet.SampleSharedModels.Results;

namespace SampleAppApi.Services.External;
public class DataAccessClientBus : IDataAccessBusClient
{
    private const string Base = "v1.0/invoke/sampledataaccessapi/method/api/bus/movies";

    private readonly HttpClient _httpClient;
    private readonly ILogger<DataAccessClientDapr> _logger;
    private readonly string _category = string.Empty;
    private readonly IHttpContextAccessor _httpContextAccessor;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public DataAccessClientBus(IHttpClientFactory httpClientFactory,
                                IHttpContextAccessor httpContextAccessor,
                                ILogger<DataAccessClientDapr> logger)
    {
        // Dapr Sidecar HTTP Port ist IMMER 3500 in Azure Container Apps
        _httpClient = httpClientFactory.CreateClient("dapr");
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
        _category = this.GetType().Name;
    }

    // ── helpers ────────────────────────────────────────────────

    private HttpRequestMessage BuildRequest(HttpMethod method, string path, object? body = null)
    {
        var httpContext = _httpContextAccessor.HttpContext
            ?? throw new InvalidOperationException("No HttpContext available");

        var authHeader = httpContext.Request.Headers["Authorization"].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(authHeader) || !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("No bearer token found");

        var request = new HttpRequestMessage(method, path);
        request.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", authHeader.Substring("Bearer ".Length).Trim());

        if (body is not null)
            request.Content = new StringContent(
                JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

        return request;
    }

    private static ServiceResult<T> Validate<T>(ServiceResult<T>? result)
    {
        if (result == null) return ServiceResult<T>.Fail(["Failed to deserialize the response."]);
        if (!result.Success) return ServiceResult<T>.Fail(result.Errors);
        if (result.Data == null) return ServiceResult<T>.Fail(["result.Data is null"]);
        return ServiceResult<T>.Ok(result.Data);
    }

    private ServiceResult<T> Fail<T>(string action, Exception ex)
    {
        var message = $"SampleDataAccessApi exception: {ex.Message}";
        _logger.LogError("{Action} Exception: {Message}", action, message);
        return ServiceResult<T>.Fail(new List<string> { message });
    }

    // ── API methods ────────────────────────────────────────────

    public async Task<ServiceResult<IEnumerable<MovieDTORead>>> GetAll()
    {
        try
        {
            using var request = BuildRequest(HttpMethod.Get, Base);
            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<ServiceResult<IEnumerable<MovieDTORead>>>();
            return Validate(result);
        }
        catch (Exception ex) { return Fail<IEnumerable<MovieDTORead>>(nameof(GetAll), ex); }
    }

    public async Task<ServiceResult<MovieDTORead>> GetById(string id)
    {
        try
        {
            using var request = BuildRequest(HttpMethod.Get, $"{Base}/{id}");
            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            
            var result = await response.Content.ReadFromJsonAsync<ServiceResult<MovieDTORead>>();
            return Validate(result);
        }
        catch (Exception ex) { return Fail<MovieDTORead>(nameof(GetById), ex); }
    }

    public async Task<ServiceResult<BusRequest<MovieDTORead>>> Add(BusRequest<MovieDTORead> busRequest)
    {
        try
        {
            using var request = BuildRequest(HttpMethod.Post, $"{Base}", busRequest);
            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

                
            var result = await response.Content.ReadFromJsonAsync<ServiceResult<BusRequest<MovieDTORead>>>();
            return Validate(result);
        }
        catch (Exception ex) { 
            return Fail<BusRequest<MovieDTORead>>(nameof(GetById), ex); 
        }
    }

    public async Task<ServiceResult<MovieDTORead>> Update(string id, MovieDTOUpdate movieDTOUpdate)
    {
        throw new NotImplementedException("Methos not implemented");

    }

    public async Task<ServiceResult<bool>> Delete(string id)
    {
        throw new NotImplementedException("Methos not implemented");
    }

}