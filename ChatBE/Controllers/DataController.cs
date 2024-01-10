using DataStoreApi.Models;
using DataStoreApi.Services;
using Microsoft.AspNetCore.Mvc;
using System.Net.WebSockets;
namespace DataStoreApi.Controllers;

//Some notes for myself on model binding




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

//Method below echos back the message to the client.
private static async Task Echo(WebSocket webSocket)
{
    //A buffer is a temporary data storage area used to hold data that is being transferred between 2 locations or processes that operate at different speeds or within different characteristics. 


    // Create an array of bytes of size 4096 (4mb) to temporarily store data.
    var buffer = new byte[1024 * 4];
    Console.WriteLine($"Buffer: {buffer}");

    var receiveResult = await webSocket.ReceiveAsync(
        new ArraySegment<byte>(buffer), CancellationToken.None);

    Console.WriteLine(receiveResult);

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

