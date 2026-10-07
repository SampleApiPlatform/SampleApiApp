using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SampleAppApi.Interfaces.ExternalServices;
using NuGet.SampleSharedModels.DTO;


namespace SampleAppApi.Controllers;



//using the repositiory pattern
//ASYNC RULE
//If your method uses await, it must be async.
//If your method returns a Task directly, it must NOT be async

[ApiController]
[Route("api/movies")]
public class MoviesController : ControllerBase
{
    private readonly ILogger<MoviesController> _logger;
    private readonly IDataAccessClient _dataAccessClient;

    private readonly string controllerName = string.Empty;
    public MoviesController(
        ILogger<MoviesController> logger,
        IDataAccessClient dataAccessClient)
    {
        _logger = logger;
        _dataAccessClient = dataAccessClient;
        controllerName = GetType().Name;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<MovieDTORead>>> GetAll()
    {
        //try
        //{
            var serviceResult = await _dataAccessClient.GetAll();

            if (!serviceResult.Success)
            {
                var errorText = string.Join(";", serviceResult.Errors);
                _logger.LogError($"MovieController.GetAll Failed: {errorText}");    

                return BadRequest(serviceResult.Errors);
            }

            return Ok(serviceResult.Data);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<MovieDTORead>> GetById(string id)
    {
            var serviceResult = await _dataAccessClient.GetById(id);

            if (!serviceResult.Success)
            {
                var errorText = string.Join(";", serviceResult.Errors);
                _logger.LogError($"MovieController.GetById Failed: {errorText}");  

                return BadRequest(serviceResult.Errors);
            }

            return Ok(serviceResult.Data);
    }


    

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Add(MovieDTOAdd movieDTOAdd)
    {
            var serviceResult = await _dataAccessClient.Add(movieDTOAdd);

            if (!serviceResult.Success)
            {
                var errorText = string.Join(";", serviceResult.Errors);
                _logger.LogError($"MovieController.Add Failed: {errorText}");
                return BadRequest(serviceResult.Errors);
            }

            return Ok(serviceResult.Data);
    }

    [Authorize]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, MovieDTOUpdate movieDTOUpdate)
    {
            var serviceResult = await _dataAccessClient.Update(id, movieDTOUpdate);

            if (!serviceResult.Success)
            {
                var errorText = string.Join(";", serviceResult.Errors);
                _logger.LogError($"MovieController.Update Failed: {errorText}");    
                return BadRequest(serviceResult.Errors);
            }

            return Ok(serviceResult.Data);
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
            var serviceResult = await _dataAccessClient.Delete(id);

            if (!serviceResult.Success)
            {
                var errorText = string.Join(";", serviceResult.Errors);
                _logger.LogError($"MovieController.Delete Failed: {errorText}");
                return BadRequest(serviceResult.Errors);
            }

            return Ok(serviceResult.Data);
    }
}
