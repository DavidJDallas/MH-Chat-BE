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


    // Create an array of bytes of size 4096 (4mb) to temporarily store data. Remeber that primitive data types don't need new operators but certain data types do.
    var buffer = new byte[1024 * 4];    


    //Method that waits for data to be received from the WebSocket. The data is then stored in the buffer. Async so won't block rest of the data while it waits. 

    //ArraySegment<T> is a wrapped around an array that delimits a range of elements in that array. The original array must be 1-dimensional and have zero-based indexing. 

    var receiveResult = await webSocket.ReceiveAsync(
        new ArraySegment<byte>(buffer), CancellationToken.None
    );

    //Loop conditional below will continue as long as the WebSocket connection remains open. 
    while (!receiveResult.CloseStatus.HasValue)
    {
        //The 'echo' part - the data that was just received is sent back over the WebSocket. 
        await webSocket.SendAsync(
            new ArraySegment<byte>(buffer, 0, receiveResult.Count),
            receiveResult.MessageType,
            receiveResult.EndOfMessage,
            CancellationToken.None);

        //Reassigning receiveResult to receive the next chunk of data from the WebSocket. Allows the method to process a continous stream of data chunks from the WebSocket. Each iteration of the while loop handles one chunk of data. 
        receiveResult = await webSocket.ReceiveAsync(
            new ArraySegment<byte>(buffer), CancellationToken.None
            );
    }


    //Once the WebSocket connection is closed (!receiveResult.CloseStatus.HasValue !== true) then close the WebSocket as per below:
    await webSocket.CloseAsync(
        receiveResult.CloseStatus.Value,
        receiveResult.CloseStatusDescription,
        CancellationToken.None);
}
}

