using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SampleAppApi.Interfaces.ExternalServices;
using NuGet.SampleSharedModels.DTO;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using NuGet.SampleSharedModels.Interfaces;
using NuGet.SampleSharedModels.Interfaces.ServiceBus;
using Azure.Core;
using NuGet.SampleSharedModels.DTO.ServiceBus;


namespace SampleAppApi.Controllers;



//using the repositiory pattern
//ASYNC RULE
//If your method uses await, it must be async.
//If your method returns a Task directly, it must NOT be async

[ApiController]
[Route("api/bus/movies")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "AccessAsUser")]
public class MoviesBusController : ControllerBase
{
    private readonly ILogger<MoviesController> _logger;
    private readonly IDataAccessBusClient _dataAccessBusClient;
    private readonly IBusSender _busSender;

    private readonly string controllerName = string.Empty;
    public MoviesBusController(
        ILogger<MoviesController> logger,
        IBusSender busSender,
        //[FromKeyedServices("bus")] IDataAccessClient dataAccessClient,
        IDataAccessBusClient dataAccessBusClient)
    {
        _logger = logger;
        _busSender = busSender;
        _dataAccessBusClient = dataAccessBusClient;
        controllerName = GetType().Name;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<MovieDTORead>>> GetAll()
    {
        var serviceResult = await _dataAccessBusClient.GetAll();

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
        var serviceResult = await _dataAccessBusClient.GetById(id);

        if (!serviceResult.Success)
        {
            var errorText = string.Join(";", serviceResult.Errors);
            _logger.LogError($"MovieController.GetById Failed: {errorText}");  

            return BadRequest(serviceResult.Errors);
        }

        return Ok(serviceResult.Data);
    }


    

    //[Authorize]
    [HttpPost]
    public async Task<IActionResult> Add(MovieDTOAdd movieDTOAdd)
    {
        var movieDTORead = new MovieDTORead()
            {
                Id = String.Empty,
                Title = movieDTOAdd.Title,
                Year = movieDTOAdd.Year
            };

            var busRequest = new BusRequest<MovieDTORead>()
            {
                IsSucceeded = null,
                MessageId = Guid.NewGuid(),
                Action = MessageAction.Add,
                Data = movieDTORead   
            };
        var serviceResult = await _dataAccessBusClient.Add(busRequest);

        if (!serviceResult.Success)
        {
            var errorText = string.Join(";", serviceResult.Errors);
            _logger.LogError($"MovieController.Add Failed: {errorText}");  

            return BadRequest(serviceResult.Errors);
        }
        return Ok(serviceResult.Data);
    }

    
}
