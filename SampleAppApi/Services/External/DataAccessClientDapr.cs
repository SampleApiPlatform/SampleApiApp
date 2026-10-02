using NuGet.SampleSharedModels.DTO;
using NuGet.SampleSharedModels.Results;
using SampleAppApi.Interfaces.ExternalServices;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using NuGet.SampleSharedModels.Interfaces;

public class DataAccessClientDapr : IDataAccessClient
{
    private readonly HttpClient _httpClient;
    //private readonly ISharedServicesClient _sharedServicesClient;
    private readonly ILogger<DataAccessClientDapr> _logger;
    private readonly string _category = string.Empty;

    public DataAccessClientDapr(IHttpClientFactory httpClientFactory, 
                                    ILogger<DataAccessClientDapr> logger
                                //ISharedServicesClient sharedServicesClient
                                )
    {
        // Dapr Sidecar HTTP Port ist IMMER 3500 in Azure Container Apps
        _httpClient = httpClientFactory.CreateClient("dapr");
        //_sharedServicesClient = sharedServicesClient;
        _logger = logger;
        _category = this.GetType().Name;
    }

    public async Task<ServiceResult<IEnumerable<MovieDTORead>>> GetAll()
    {
        try
        {
            var url = "v1.0/invoke/sampledataaccessapi/method/movies";

            var result = await _httpClient.GetFromJsonAsync<ServiceResult<IEnumerable<MovieDTORead>>>(url);

            return result!;
        }
        catch (Exception ex)
        {
            var message = $"DataAccessApi unreachable: {ex.Message}";
            _logger.LogError("GetAll Exception: {message}", message);
            //await _sharedServicesClient.LogAsync(_category, $"GetAll Exception: {message}", LogLevel.Error);
            return ServiceResult<IEnumerable<MovieDTORead>>.Fail(new List<string> { message });
        }
    }

    public async Task<ServiceResult<MovieDTORead>> GetById(string id)
    {
        try
        {
            var url = $"v1.0/invoke/dataaccess/method/movies/{id}";

            var result = await _httpClient.GetFromJsonAsync<ServiceResult<MovieDTORead>>(url);

            return result!;
        }
        catch (Exception ex)
        {
            var message = $"DataAccessApi unreachable: {ex.Message}";
            _logger.LogError("GetById Exception: {message}", message);
            //await _sharedServicesClient.LogAsync(_category, $"GetById Exception: {message}", LogLevel.Error);
            return ServiceResult<MovieDTORead>.Fail(new List<string> { message });
        }
    }

    public async Task<ServiceResult<MovieDTORead>> Add(MovieDTOAdd movieDTOAdd)
    {
        try
        {
            var url = "v1.0/invoke/dataaccess/method/movies";

            var response = await _httpClient.PostAsJsonAsync(url, movieDTOAdd);

            var result = await response.Content.ReadFromJsonAsync<ServiceResult<MovieDTORead>>();

            return result!;
        }
        catch (Exception ex)
        {
            var message = $"DataAccessApi unreachable: {ex.Message}";
            _logger.LogError("Add Exception: {message}", message);
            //await _sharedServicesClient.LogAsync(_category, $"Add Exception: {message}", LogLevel.Error);
            return ServiceResult<MovieDTORead>.Fail(new List<string> { message });
        }
    }

    public async Task<ServiceResult<MovieDTORead>> Update(string id, MovieDTOUpdate movieDTOUpdate)
    {
        try
        {
            var url = $"v1.0/invoke/dataaccess/method/movies/{id}";

            var response = await _httpClient.PutAsJsonAsync(url, movieDTOUpdate);

            var result = await response.Content.ReadFromJsonAsync<ServiceResult<MovieDTORead>>();

            return result!;
        }
        catch (Exception ex)
        {
            var message = $"DataAccessApi unreachable: {ex.Message}";
            _logger.LogError("Update Exception: {message}", message);
            //await _sharedServicesClient.LogAsync(_category, $"Update Exception: {message}", LogLevel.Error);
            return ServiceResult<MovieDTORead>.Fail(new List<string> { message });
        }
    }

    public async Task<ServiceResult<bool>> Delete(string id)
    {
        try
        {
            var url = $"v1.0/invoke/dataaccess/method/movies/{id}";

            var response = await _httpClient.DeleteAsync(url);

            var result = await response.Content.ReadFromJsonAsync<ServiceResult<bool>>();

            return result!;
        }
        catch (Exception ex)
        {
            var message = $"DataAccessApi unreachable: {ex.Message}";
            _logger.LogError("Delete Exception: {message}", message);
            //await _sharedServicesClient.LogAsync(_category, $"Delete Exception: {message}", LogLevel.Error);
            return ServiceResult<bool>.Fail(new List<string> { message });
        }
    }
}
