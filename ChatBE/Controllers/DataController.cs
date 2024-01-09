using DataStoreApi.Models;
using DataStoreApi.Services;
using Microsoft.AspNetCore.Mvc;
using System.Net.WebSockets;




namespace DataStoreApi.Controllers;

//Some notes for myself on model binding

//Model binding is an important process here. It involves mapping data from an HTTP request to the parameters of a controller action or to properties of a model class. It can extract data from various sources - query string params, form fields, route values, and request bodies.

//Model binding uses default conventions to determine how to mach request data to parameters or properties. 

//Model binding retrieves data from various sources such as route data, form fields, and query strings. It provides the data to controllers in method para

//The record of what data is bound to the model, and any binding or validation errors, is stored in ControllerBase.ModelState or PageModel.ModelState. To find out if this process was successful, the app checks the ModelState.IsValid flag. 

//Note also that ASP.NET MCV already handles 400 error messages automatically, so no need to do so. 



public class WebSocketController : ControllerBase
{
    [Route("/ws")]
    public async Task Get()
    {
        if (HttpContext.WebSockets.IsWebSocketRequest)
        {
            using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
            await Echo(webSocket);
        }
        else
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        }
    }

private static async Task Echo(WebSocket webSocket)
{
    var buffer = new byte[1024 * 4];
    var receiveResult = await webSocket.ReceiveAsync(
        new ArraySegment<byte>(buffer), CancellationToken.None);

    while (!receiveResult.CloseStatus.HasValue)
    {
        await webSocket.SendAsync(
            new ArraySegment<byte>(buffer, 0, receiveResult.Count),
            receiveResult.MessageType,
            receiveResult.EndOfMessage,
            CancellationToken.None);

        receiveResult = await webSocket.ReceiveAsync(
            new ArraySegment<byte>(buffer), CancellationToken.None);
    }

    await webSocket.CloseAsync(
        receiveResult.CloseStatus.Value,
        receiveResult.CloseStatusDescription,
        CancellationToken.None);
}
}


[ApiController]
[Route("api/main")]
public class DataController: ControllerBase
{
    private readonly DataService _dataService;

    //Constructor to initialize _dataService
    public DataController(DataService dataservice) => _dataService = dataservice;

    [HttpGet]
    public async Task<List<Data>> Get() => await _dataService.GetAsync();

    [HttpGet("{id}")]
    public async Task<ActionResult<Data>> GetByIdAsync(string id)
    {
        var data = await _dataService.GetByIdAsync(id);

        if(data == null)
        {
            return NotFound();
        }
        return Ok(data);
    }

    //
    [HttpPost]
    public async Task<ActionResult<Data>> Post(Data newData)
    { try
        {
        await _dataService.CreateAsync(newData);
        return CreatedAtAction(nameof(Get), new {id = newData.Id, location=newData.Location});    

        }
      catch(Exception ex)
      {
        return StatusCode(500);
      }
           
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteAll()
    {
        try
        {
            await _dataService.DeleteAsync();
            return Ok("All records deleted successfully");
        }
        catch(Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }
  
}