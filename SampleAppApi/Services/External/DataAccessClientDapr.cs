using NuGet.SampleSharedModels.DTO;
using NuGet.SampleSharedModels.Results;
using SampleAppApi.Interfaces.ExternalServices;


public class DataAccessClientDapr : IDataAccessClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<DataAccessClientDapr> _logger;
    private readonly string _category = string.Empty;

    

    public DataAccessClientDapr(IHttpClientFactory httpClientFactory, 
                                    ILogger<DataAccessClientDapr> logger
                                )
    {
        // Dapr Sidecar HTTP Port ist IMMER 3500 in Azure Container Apps
        _httpClient = httpClientFactory.CreateClient("dapr");
        _logger = logger;
        _category = this.GetType().Name;
    }

    public async Task<ServiceResult<IEnumerable<MovieDTORead>>> GetAll()
    {
        try
        {
            var url = "v1.0/invoke/sampledataaccessapi/method/api/movies";
            var result = await _httpClient.GetFromJsonAsync<ServiceResult<IEnumerable<MovieDTORead>>>(url);
            if(result== null)
            {
                return ServiceResult<IEnumerable<MovieDTORead>>.Fail(["result is null"]);
            }
            else if (!result.Success)
            {
                return ServiceResult<IEnumerable<MovieDTORead>>.Fail(result.Errors);
            } 
            else if (result.Data == null)
            {
                return ServiceResult<IEnumerable<MovieDTORead>>.Fail(["reuslt.Data is null"]);
            } 
            else
            {
                return ServiceResult<IEnumerable<MovieDTORead>>.Ok(result.Data);
            }
            
        }
        catch (Exception ex)
        {
            var message = $"SampleDataAccessApi exception: {ex.Message}";
            _logger.LogError("GetAll Exception: {message}", message);
            return ServiceResult<IEnumerable<MovieDTORead>>.Fail(new List<string> { message });
        }
    }

    public async Task<ServiceResult<MovieDTORead>> GetById(string id)
    {
        try
        {
            var url = $"v1.0/invoke/sampledataaccessapi/method/api/movies/{id}";

            var result = await _httpClient.GetFromJsonAsync<ServiceResult<MovieDTORead>>(url);
            if(result== null)
            {
                return ServiceResult<MovieDTORead>.Fail(["result is null"]);
            }
            else if (!result.Success)
            {
                return ServiceResult<MovieDTORead>.Fail(result.Errors);
            } 
            else if (result.Data == null)
            {
                return ServiceResult<MovieDTORead>.Fail(["reuslt.Data is null"]);
            } 
            else
            {
                return ServiceResult<MovieDTORead>.Ok(result.Data);
            }
            

        }
        catch (Exception ex)
        {
            var message = $"SampleDataAccessApi exception: {ex.Message}";
            _logger.LogError("GetById Exception: {message}", message);
            return ServiceResult<MovieDTORead>.Fail(new List<string> { message });
        }
    }

    public async Task<ServiceResult<MovieDTORead>> Add(MovieDTOAdd movieDTOAdd)
    {
        try
        {
            var url = "v1.0/invoke/sampledataaccessapi/method/api/movies";
            var response = await _httpClient.PostAsJsonAsync(url, movieDTOAdd);

            if (!response.IsSuccessStatusCode)
            {
                return ServiceResult<MovieDTORead>.Fail(new List<string> 
                { 
                    $"Request failed with status code: {(int)response.StatusCode}" 
                });
            }

            var result = await response.Content.ReadFromJsonAsync<ServiceResult<MovieDTORead>>();

            if (result == null)
            {
                return ServiceResult<MovieDTORead>.Fail(new List<string> 
                { 
                    "Failed to deserialize the response." 
                });
            }
            return result;
            
        }
        catch (Exception ex)
        {
            var message = $"SampleDataAccessApi exception: {ex.Message}";
            _logger.LogError("Add Exception: {message}", message);
            return ServiceResult<MovieDTORead>.Fail(new List<string> { message });
        }
    }

    public async Task<ServiceResult<MovieDTORead>> Update(string id, MovieDTOUpdate movieDTOUpdate)
    {
        try
        {
            var url = $"v1.0/invoke/sampledataaccessapi/method/api/movies/{id}";

            var response = await _httpClient.PutAsJsonAsync(url, movieDTOUpdate);

            if (!response.IsSuccessStatusCode)
            {
                return ServiceResult<MovieDTORead>.Fail(new List<string> 
                { 
                    $"Request failed with status code: {(int)response.StatusCode}" 
                });
            }

            var result = await response.Content.ReadFromJsonAsync<ServiceResult<MovieDTORead>>();

            if (result == null)
            {
                return ServiceResult<MovieDTORead>.Fail(new List<string> 
                { 
                    "Failed to deserialize the response." 
                });
            }

            return result;
        }
        catch (Exception ex)
        {
            var message = $"SampleDataAccessApi exception: {ex.Message}";
            _logger.LogError("Update Exception: {message}", message);
            return ServiceResult<MovieDTORead>.Fail(new List<string> { message });
        }
    }

    public async Task<ServiceResult<bool>> Delete(string id)
    {
        try
        {
            var url = $"v1.0/invoke/sampledataaccessapi/method/api/movies/{id}";
            var response = await _httpClient.DeleteAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                return ServiceResult<bool>.Fail(new List<string> 
                { 
                    $"Request failed with status code: {(int)response.StatusCode}" 
                });
            }
            var rawBody = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("Delete raw body: [{Body}] (length {Len})", rawBody, rawBody.Length);
            
            var result = await response.Content.ReadFromJsonAsync<ServiceResult<bool>>();

            if (result == null)
            {
                return ServiceResult<bool>.Fail(new List<string> 
                { 
                    "Failed to deserialize the response." 
                });
            }

            return result;
        }
        catch (Exception ex)
        {
            var message = $"SampleDataAccessApi exception: {ex.Message}";
            _logger.LogError("Delete Exception: {message}", message);
            return ServiceResult<bool>.Fail(new List<string> { message });
        }
    }
}
