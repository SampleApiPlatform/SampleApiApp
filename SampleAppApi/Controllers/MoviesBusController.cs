using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SampleAppApi.Interfaces.ExternalServices;
using NuGet.SampleSharedModels.DTO;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Google.Api;
using Microsoft.Azure.Amqp.Framing;
using System.Runtime.CompilerServices;


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
    private readonly IDataAccessClient _dataAccessClient;
    private readonly IBusSender _busSender;

    private readonly string controllerName = string.Empty;
    public MoviesBusController(
        ILogger<MoviesController> logger,
        IBusSender busSender,
        [FromKeyedServices("bus")] IDataAccessClient dataAccessClient)
    {
        _logger = logger;
        _busSender = busSender;
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


    

    //[Authorize]
    [HttpPost]
    public async Task<IActionResult> Add(MovieDTOAdd movieDTOAdd)
    {
        //if (movieDTORead.Id == null)
        //{
        //    movieDTORead.Id = Guid.NewGuid().ToString(); // necessary to detect duplicate records in the queue
        //}
        var messageId = Guid.NewGuid(); // this nis the reference for the table
        //TODO: insert in the database a record with reuestId=messageId, status=peding
        await _busSender.SendAsync(messageId, string.Empty, MessageAction.Add, movieDTOAdd);
        return Accepted(messageId);
    }

    //[Authorize]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, MovieDTOUpdate movieDTOUpdate)
    {
        var messageId = Guid.NewGuid(); // this nis the reference for the table
        //TODO: insert in the database a record with reuestId=messageId, status=peding
        await _busSender.SendAsync(messageId, id, MessageAction.Update, movieDTOUpdate);
        return Accepted(messageId);   
    }

    //[Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var messageId = Guid.NewGuid(); // this nis the reference for the table
        //TODO: insert in the database a record with reuestId=messageId, status=peding
        var dummyData = new MovieDTORead()
        {
            Id= Guid.Empty.ToString(),
            Title = String.Empty,
            Year = 0
        };
        await _busSender.SendAsync(messageId, id, MessageAction.Delete, dummyData);
        return Accepted(messageId); 
    }
}
