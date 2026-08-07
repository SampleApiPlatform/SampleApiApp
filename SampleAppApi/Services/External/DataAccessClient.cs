

using SampleAppApi.Interfaces.ExternalServices;
using NuGet.SampleSharedModels.DTO;
using NuGet.SampleSharedModels.Interfaces;
using NuGet.SampleSharedModels.Results;

namespace SampleAppApi.Services.External
{
    public class DataAccessClient : IDataAccessClient
    {
        private readonly string _category = string.Empty;   
        private readonly HttpClient _httpClient;
        private readonly ISharedServicesClient _sharedServicesClient;
        public DataAccessClient(HttpClient httpClient, ISharedServicesClient sharedServicesClient)
        {
            _httpClient = httpClient;
            _sharedServicesClient = sharedServicesClient;
            _category = this.GetType().Name;
        }

       
        public async Task<ServiceResult<IEnumerable<MovieDTORead>>> GetAll()
        {
            // This is how it works:
            // Remember that in the old days, for instance with AJAX calls, you would make a request to a server and get back some data.
            // this response was a complex object with a status, messages, headers, and the actual data you wanted.
            // However _httpClient.GetFromJsonAsync<T>() return the obejct type T if everything is successful but it did not
            // it will throw an exception if the response is not successful (e.g. 404, 500, etc.)
            // we get null back without knowing the reason.
            // 2) Instead, we use _httpClient.GetAsync() first to get the response and check if it's successful
            // 3) If it's successful, we make a second call to response.Content.ReadFromJsonAsync<ServiceResult<IEnumerable<MovieDTORead>>>()
            // and we read the object as JSON and deserialize it.
            // 4) If it's not successful, we return a ServiceResult with the error message

            try
            {                
                var response = await _httpClient.GetAsync("/movies");
                //Hier bekommst du:
                //response.StatusCode + response.IsSuccessStatusCode + Header
                //Body(noch nicht gelesen)
                if (!response.IsSuccessStatusCode)
                {
                    var message = $"DataAccessApi returned error: {response.StatusCode}";
                    await _sharedServicesClient.LogAsync(_category, $"DataAccessClient.GetAll Exception: {message}", LogLevel.Error);
                    return ServiceResult<IEnumerable<MovieDTORead>>.Fail(new List<string> { message });
                }
                var result = await response.Content.ReadFromJsonAsync<ServiceResult<IEnumerable<MovieDTORead>>>();
                //bekommst du keinen Statuscode, keine Fehlerdetails, keine Kontrolle.
                return result!;
               
            }
            catch (Exception ex)
            {
                var message = $"DataAccessApi unreachable: {ex.Message}";
                await _sharedServicesClient.LogAsync(_category, $"DataAccessClient.GetAll Exception: {message}", LogLevel.Error);
                return ServiceResult<IEnumerable<MovieDTORead>>.Fail(new List<string> { message });
            }
        }

        Task<ServiceResult<IEnumerable<MovieDTORead>>> IDataAccessClient.GetAll()
        {
            throw new NotImplementedException();
        }

        // GET api/movies/{id}
        public async Task<ServiceResult<MovieDTORead>> GetById(string id)
        {
            // This is how it works:
            // Remember that in the old days, for instance with AJAX calls, you would make a request to a server and get back some data.
            // this response was a complex object with a status, messages, headers, and the actual data you wanted.
            // However _httpClient.GetFromJsonAsync<T>() return the obejct type T if everything is successful but it did not
            // it will throw an exception if the response is not successful (e.g. 404, 500, etc.)
            // we get null back without knowing the reason.
            // 2) Instead, we use _httpClient.GetAsync() first to get the response and check if it's successful
            // 3) If it's successful, we make a second call to response.Content.ReadFromJsonAsync<ServiceResult<IEnumerable<MovieDTORead>>>()
            // and we read the object as JSON and deserialize it.
            // 4) If it's not successful, we return a ServiceResult with the error message

            try
            {
                var response = await _httpClient.GetAsync($"/movies/{id}");
                if (!response.IsSuccessStatusCode)
                {
                    var message = $"DataAccessApi returned error: {response.StatusCode}";
                    await _sharedServicesClient.LogAsync(_category, $"DataAccessClient.GetById Exception: {message}", LogLevel.Error);
                    return ServiceResult<MovieDTORead>.Fail(new List<string> { message });
                }

                var result = await response.Content.ReadFromJsonAsync<ServiceResult<MovieDTORead>>();
                return result!;

            }
            catch (Exception ex)
            {
                var message = $"DataAccessApi unreachable: {ex.Message}";
                await _sharedServicesClient.LogAsync(_category, $"DataAccessClient.GetById Exception: {message}", LogLevel.Error);
                return ServiceResult<MovieDTORead>.Fail(new List<string> { message });
            }
        }

        public async Task<ServiceResult<MovieDTORead>> Add(MovieDTOAdd movieDTOAdd)
        {
            // This is how it works:
            // Remember that in the old days, for instance with AJAX calls, you would make a request to a server and get back some data.
            // this response was a complex object with a status, messages, headers, and the actual data you wanted.
            // For POST and PUT we do not use PutAsync / PostAsync because we are sending a Body
            // Instead, we use _httpClient.PostAsJsonAsync() first to get the response and check if it's successful
            // If it's successful, we desrialize the response.Content.ReadFromJsonAsync<ServiceResult<MovieDTORead>>()
            // and we read the object as JSON and deserialize it.
            // If it's not successful, we return a ServiceResult with the error message

            try
            {
                var response = await _httpClient.PostAsJsonAsync($"/movies", movieDTOAdd);
                if (!response.IsSuccessStatusCode)
                {
                    var message = $"DataAccessApi returned error: {response.StatusCode}";
                    await _sharedServicesClient.LogAsync(_category, $"DataAccessClient.Add Exception: {message}", LogLevel.Error);
                    return ServiceResult<MovieDTORead>.Fail(new List<string> { message });
                }

                var result = await response.Content.ReadFromJsonAsync<ServiceResult<MovieDTORead>>();
                return result!;

            }
            catch (Exception ex)
            {
                var message = $"DataAccessApi unreachable: {ex.Message}";
                await _sharedServicesClient.LogAsync(_category, $"DataAccessClient.Add Exception: {message}", LogLevel.Error);
                return ServiceResult<MovieDTORead>.Fail(new List<string> { message });
            }
        }

        public async Task<ServiceResult<MovieDTORead>> Update(string id, MovieDTOUpdate movieDTOUpdate)
        {
            // This is how it works:
            // Remember that in the old days, for instance with AJAX calls, you would make a request to a server and get back some data.
            // this response was a complex object with a status, messages, headers, and the actual data you wanted.
            // For POST and PUT we do not use PutAsync / PostAsync because we are sending a Body
            // Instead, we use _httpClient.PostAsJsonAsync() first to get the response and check if it's successful
            // If it's successful, we desrialize the response.Content.ReadFromJsonAsync<ServiceResult<MovieDTORead>>()
            // and we read the object as JSON and deserialize it.
            // If it's not successful, we return a ServiceResult with the error message

            try
            {
                var response = await _httpClient.PutAsJsonAsync($"/movies/{id}", movieDTOUpdate);

                if (!response.IsSuccessStatusCode)
                {
                    var message = $"DataAccessApi returned error: {response.StatusCode}";
                    await _sharedServicesClient.LogAsync(_category, $"DataAccessClient.Update Exception: {message}", LogLevel.Error);
                    return ServiceResult<MovieDTORead>.Fail(new List<string> { message });
                }

                var result = await response.Content.ReadFromJsonAsync<ServiceResult<MovieDTORead>>();
                return result!;

            }
            catch (Exception ex)
            {
                var message = $"DataAccessApi unreachable: {ex.Message}";
                await _sharedServicesClient.LogAsync(_category, $"DataAccessClient.Update Exception: {message}", LogLevel.Error);
                return ServiceResult<MovieDTORead>.Fail(new List<string> { message });
            }
        }

        public async Task<ServiceResult<bool>> Delete(string id)
        {
            // This is how it works:
            // Delete does not have a body, so we can use _httpClient.DeleteAsync() to send the request.


            try
            {
                var response = await _httpClient.DeleteAsync($"/movies/{id}");

                if (!response.IsSuccessStatusCode)
                {
                    var message = $"DataAccessApi returned error: {response.StatusCode}";
                    await _sharedServicesClient.LogAsync(_category, $"DataAccessClient.Delete Exception: {message}", LogLevel.Error);
                    return ServiceResult<bool>.Fail(new List<string> { message });
                }

                var result = await response.Content.ReadFromJsonAsync<ServiceResult<bool>>();
                return result!;

            }
            catch (Exception ex)
            {
                var message = $"DataAccessApi unreachable: {ex.Message}";
                await _sharedServicesClient.LogAsync(_category, $"DataAccessClient.Delete Exception: {message}", LogLevel.Error);
                return ServiceResult<bool>.Fail(new List<string> { message });
            }
        }



        Task<ServiceResult<MovieDTORead>> IDataAccessClient.GetById(string id)
        {
            throw new NotImplementedException();
        }
    }

}
