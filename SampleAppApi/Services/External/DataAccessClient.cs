

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
        private readonly ILogger<DataAccessClient> _logger;
        public DataAccessClient(HttpClient httpClient,
                                ILogger<DataAccessClient> logger 
                                )
        {
            _httpClient = httpClient;
            _logger = logger;
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

                var method = "movies";
                // do not use _httpClient.GetAsync("/movies). It can deliver wrong results.
                //Ex: base address = https://api.example.com/ -> then OK https://api.example.com/movies
                //base address = BaseAddress = https://api.example.com/api/v1/ -> then KO https://api.example.com/movies
                var response = await _httpClient.GetAsync(method);
                //Hier bekommst du:
                //response.StatusCode + response.IsSuccessStatusCode + Header
                //Body(noch nicht gelesen)
                if (!response.IsSuccessStatusCode)
                {
                    var body = await response.Content.ReadAsStringAsync();
                    //var url = $"{_httpClient.BaseAddress}{method}";
                    //var url =_httpClient.BaseAddress + "movies";
                    //Sometimes the baseurl can come with an slash (https://api.example.com/) or without (https://api.example.com).
                    //To handle all these nuances there is already a method that will give us the right url
                    var url = new Uri(_httpClient.BaseAddress!, method).ToString();
                    throw new ExternalApiException(
                        $"External API returned {response.StatusCode}",
                        response.StatusCode,
                        url,
                        "GET",
                        body
                    );
                }
                var result = await response.Content.ReadFromJsonAsync<ServiceResult<IEnumerable<MovieDTORead>>>();
                //bekommst du keinen Statuscode, keine Fehlerdetails, keine Kontrolle.
                return result!;
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

                var method = $"movies/{id}";
                var response = await _httpClient.GetAsync(method);
                if (!response.IsSuccessStatusCode)
                {
                    var body = await response.Content.ReadAsStringAsync();
                    //var url = $"{_httpClient.BaseAddress}{method}";
                    //var url =_httpClient.BaseAddress + "movies";
                    //Sometimes the baseurl can come with an slash (https://api.example.com/) or without (https://api.example.com).
                    //To handle all these nuances there is already a method that will give us the right url
                    var url = new Uri(_httpClient.BaseAddress!, method).ToString();
                    throw new ExternalApiException(
                        $"External API returned {response.StatusCode}",
                        response.StatusCode,
                        url,
                        "GET",
                        body
                    );
                }

                var result = await response.Content.ReadFromJsonAsync<ServiceResult<MovieDTORead>>();
                return result!;
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

                var method = "movies";
                var response = await _httpClient.PostAsJsonAsync(method, movieDTOAdd);
                if (!response.IsSuccessStatusCode)
                {
                    var body = await response.Content.ReadAsStringAsync();
                    //var url = $"{_httpClient.BaseAddress}{method}";
                    //var url =_httpClient.BaseAddress + "movies";
                    //Sometimes the baseurl can come with an slash (https://api.example.com/) or without (https://api.example.com).
                    //To handle all these nuances there is already a method that will give us the right url
                    var url = new Uri(_httpClient.BaseAddress!, method).ToString();
                    throw new ExternalApiException(
                        $"External API returned {response.StatusCode}",
                        response.StatusCode,
                        url,
                        "POST",
                        body
                    );
                }

                var result = await response.Content.ReadFromJsonAsync<ServiceResult<MovieDTORead>>();
                return result!;
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

                var method = $"movies/{id}";
                var response = await _httpClient.PutAsJsonAsync(method, movieDTOUpdate);

                if (!response.IsSuccessStatusCode)
                {
                    var body = await response.Content.ReadAsStringAsync();
                    //var url = $"{_httpClient.BaseAddress}{method}";
                    //var url =_httpClient.BaseAddress + "movies";
                    //Sometimes the baseurl can come with an slash (https://api.example.com/) or without (https://api.example.com).
                    //To handle all these nuances there is already a method that will give us the right url
                    var url = new Uri(_httpClient.BaseAddress!, method).ToString();
                    throw new ExternalApiException(
                        $"External API returned {response.StatusCode}",
                        response.StatusCode,
                        url,
                        "PUT",
                        body
                    );
                    //var message = $"DataAccessApi returned error: {response.StatusCode}";
                    //await _sharedServicesClient.LogAsync(_category, $"DataAccessClient.Update Exception: {message}", LogLevel.Error);
                    //return ServiceResult<MovieDTORead>.Fail(new List<string> { message });
                }

                var result = await response.Content.ReadFromJsonAsync<ServiceResult<MovieDTORead>>();
                return result!;
        }

        public async Task<ServiceResult<bool>> Delete(string id)
        {
            // This is how it works:
            // Delete does not have a body, so we can use _httpClient.DeleteAsync() to send the request.
                var method = $"movies/{id}";
                var response = await _httpClient.DeleteAsync(method);

                if (!response.IsSuccessStatusCode)
                {
                    var body = await response.Content.ReadAsStringAsync();
                    //var url = $"{_httpClient.BaseAddress}{method}";
                    //var url =_httpClient.BaseAddress + "movies";
                    //Sometimes the baseurl can come with an slash (https://api.example.com/) or without (https://api.example.com).
                    //To handle all these nuances there is already a method that will give us the right url
                    var url = new Uri(_httpClient.BaseAddress!, method).ToString();
                    throw new ExternalApiException(
                        $"External API returned {response.StatusCode}",
                        response.StatusCode,
                        url,
                        "DELETE",
                        body
                    );
                }

                var result = await response.Content.ReadFromJsonAsync<ServiceResult<bool>>();
                return result!;
        }
    }

}
