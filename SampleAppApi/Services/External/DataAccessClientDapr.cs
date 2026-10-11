using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using NuGet.SampleSharedModels.DTO;
using NuGet.SampleSharedModels.Results;
using SampleAppApi.Interfaces.ExternalServices;


public class DataAccessClientDapr : IDataAccessClient
{
    private const string Base = "v1.0/invoke/sampledataaccessapi/method/api/movies";

    private readonly HttpClient _httpClient;
    private readonly ILogger<DataAccessClientDapr> _logger;
    private readonly string _category = string.Empty;
    private readonly IHttpContextAccessor _httpContextAccessor;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public DataAccessClientDapr(IHttpClientFactory httpClientFactory,
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

    public async Task<ServiceResult<MovieDTORead>> Add(MovieDTOAdd movieDTOAdd)
    {
        try
        {
            using var request = BuildRequest(HttpMethod.Post, Base, movieDTOAdd);
            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
                return ServiceResult<MovieDTORead>.Fail(
                    [$"Request failed with status code: {(int)response.StatusCode}"]);

            
            var result = await response.Content.ReadFromJsonAsync<ServiceResult<MovieDTORead>>();
            return result == null
                ? ServiceResult<MovieDTORead>.Fail(["Failed to deserialize the response."])
                : result;
        }
        catch (Exception ex) { return Fail<MovieDTORead>(nameof(Add), ex); }
    }

    public async Task<ServiceResult<MovieDTORead>> Update(string id, MovieDTOUpdate movieDTOUpdate)
    {
        try
        {
            using var request = BuildRequest(HttpMethod.Put, $"{Base}/{id}", movieDTOUpdate);
            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
                return ServiceResult<MovieDTORead>.Fail(
                    [$"Request failed with status code: {(int)response.StatusCode}"]);

           
            var result = await response.Content.ReadFromJsonAsync<ServiceResult<MovieDTORead>>();
            return result == null
                ? ServiceResult<MovieDTORead>.Fail(["Failed to deserialize the response."])
                : result;
        }
        catch (Exception ex) { return Fail<MovieDTORead>(nameof(Update), ex); }
    }

    public async Task<ServiceResult<bool>> Delete(string id)
    {
        try
        {
            using var request = BuildRequest(HttpMethod.Delete, $"{Base}/{id}");
            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
                return ServiceResult<bool>.Fail(
                    [$"Request failed with status code: {(int)response.StatusCode}"]);

            var rawBody = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("Delete raw body: [{Body}] (length {Len})", rawBody, rawBody.Length);

            if (string.IsNullOrWhiteSpace(rawBody))
                return ServiceResult<bool>.Ok(true);   // Success with no content

            var result = JsonSerializer.Deserialize<ServiceResult<bool>>(rawBody, JsonOptions);
            return result == null
                ? ServiceResult<bool>.Fail(["Failed to deserialize the response."])
                : result;
        }
        catch (Exception ex) { return Fail<bool>(nameof(Delete), ex); }
    }
}