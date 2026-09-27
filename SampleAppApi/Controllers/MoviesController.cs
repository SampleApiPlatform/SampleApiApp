using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SampleAppApi.Interfaces.ExternalServices;
using NuGet.SampleSharedModels.DTO;
using NuGet.SampleSharedModels.Interfaces;
using NuGet.SampleSharedModels.Results;

namespace SampleAppApi.Controllers;



//using the repositiory pattern
//ASYNC RULE
//If your method uses await, it must be async.
//If your method returns a Task directly, it must NOT be async

[ApiController]
[Route("api/movies")]
public class MoviesController : ControllerBase
{
    private readonly ISharedServicesClient _sharedServicesClient;
    private readonly IDataAccessClient _dataAccessClient;

    private readonly string controllerName = string.Empty;
    public MoviesController(
        ISharedServicesClient sharedServicesClient, 
        IDataAccessClient dataAccessClient)
    {
        _sharedServicesClient = sharedServicesClient;
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
                var errorText = ServiceResult<IEnumerable<MovieDTORead>>.ErrorsToString(serviceResult.Errors);

                await _sharedServicesClient.LogAsync(
                    controllerName,
                    $"MovieController.GetAll Failed: {errorText}",
                    LogLevel.Error
                );

                return BadRequest(serviceResult.Errors);
            }

            return Ok(serviceResult.Data);
        //}
        //catch (Exception ex)
        //{
        //    var message = $"MovieController.GetAll Exception: {ex.Message}";
        // 
        //    await _sharedServicesClient.LogAsync(
        //        controllerName,
        //        message,
        //        LogLevel.Error
        //    );
        // 
        //    return StatusCode(500, message);
        //}
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<MovieDTORead>> GetById(string id)
    {
        //try
        //{
            var serviceResult = await _dataAccessClient.GetById(id);

            if (!serviceResult.Success)
            {
                var errorText = ServiceResult<IEnumerable<MovieDTORead>>.ErrorsToString(serviceResult.Errors);

                await _sharedServicesClient.LogAsync(
                    controllerName,
                    $"MovieController.GetById Failed: {errorText}",
                    LogLevel.Error
                );

                return BadRequest(serviceResult.Errors);
            }

            return Ok(serviceResult.Data);
        //}
        //catch (Exception ex)
        //{
        //    var message = $"MovieController.GetById Exception: {ex.Message}";

        //    await _sharedServicesClient.LogAsync(
        //        controllerName,
        //        message,
        //        LogLevel.Error
        //    );

        //    return StatusCode(500, message);
        //}
    }


    

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Add(MovieDTOAdd movieDTOAdd)
    {
        //try
        //{
            var serviceResult = await _dataAccessClient.Add(movieDTOAdd);

            if (!serviceResult.Success)
            {
                var errorText = ServiceResult<IEnumerable<MovieDTORead>>.ErrorsToString(serviceResult.Errors);

                await _sharedServicesClient.LogAsync(
                    controllerName,
                    $"MovieController.Add Failed: {errorText}",
                    LogLevel.Error
                );

                return BadRequest(serviceResult.Errors);
            }

            return Ok(serviceResult.Data);
        //}
        //catch (Exception ex)
        //{
        //    var message = $"MovieController.Add Exception: {ex.Message}";

        //    await _sharedServicesClient.LogAsync(
        //        controllerName,
        //        message,
        //        LogLevel.Error
        //    );

        //    return StatusCode(500, message);
        //}

    }

    [Authorize]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, MovieDTOUpdate movieDTOUpdate)
    {
        //try
        //{
            var serviceResult = await _dataAccessClient.Update(id, movieDTOUpdate);

            if (!serviceResult.Success)
            {
                var errorText = ServiceResult<IEnumerable<MovieDTORead>>.ErrorsToString(serviceResult.Errors);

                await _sharedServicesClient.LogAsync(
                    controllerName,
                    $"MovieController.Update Failed: {errorText}",
                    LogLevel.Error
                );

                return BadRequest(serviceResult.Errors);
            }

            return Ok(serviceResult.Data);
        //}
        //catch (Exception ex)
        //{
        //    var message = $"MovieController.Update Exception: {ex.Message}";

        //    await _sharedServicesClient.LogAsync(
        //        controllerName,
        //        message,
        //        LogLevel.Error
        //    );

        //    return StatusCode(500, message);
        //}
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        //try
        //{
            var serviceResult = await _dataAccessClient.Delete(id);

            if (!serviceResult.Success)
            {
                var errorText = ServiceResult<IEnumerable<MovieDTORead>>.ErrorsToString(serviceResult.Errors);

                await _sharedServicesClient.LogAsync(
                    controllerName,
                    $"MovieController.Delete Failed: {errorText}",
                    LogLevel.Error
                );

                return BadRequest(serviceResult.Errors);
            }

            return Ok(serviceResult.Data);
        //}
        //catch (Exception ex)
        //{
        //    var message = $"MovieController.Delete Exception: {ex.Message}";

        //    await _sharedServicesClient.LogAsync(
        //        controllerName,
        //        message,
        //        LogLevel.Error
        //    );

        //    return StatusCode(500, message);
        //}

    }
}
