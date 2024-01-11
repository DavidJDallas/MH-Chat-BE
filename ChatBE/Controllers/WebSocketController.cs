using Microsoft.AspNetCore.Mvc;
using System.Net.WebSockets;
namespace DataStoreApi.Controllers;

//Some notes for myself on model binding




public class WebSocketController : ControllerBase
{
    // A list to hold all connected clients
    private static List<(WebSocket, WebSocket?)> _socketPairs = new List<(WebSocket, WebSocket?)>();

    [Route("/ws")]
    public async Task Get()
    {
        //If it's a websocket request, execute below block.
        if (HttpContext.WebSockets.IsWebSocketRequest)
        {
            var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
            var unpairedSocket = _socketPairs.FirstOrDefault(pair => pair.Item2 == null);
            if (unpairedSocket.Item1 != null)
            {
                // If there's an unpaired client, pair them with the new client
                _socketPairs.Remove(unpairedSocket);
                _socketPairs.Add((unpairedSocket.Item1, webSocket));
            }
            else
            {
                // If there's no unpaired client, add the new client as an unpaired client
                _socketPairs.Add((webSocket, null));
            }
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
        var receiveResult = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

        while (!receiveResult.CloseStatus.HasValue)
        {
            // Find the client's pair and send the message to them
            var pair = _socketPairs.FirstOrDefault(p => p.Item1 == webSocket || p.Item2 == webSocket);
            var otherSocket = pair.Item1 == webSocket ? pair.Item2 : pair.Item1;
            if (otherSocket != null && otherSocket.State == WebSocketState.Open)
            {
                await otherSocket.SendAsync(new ArraySegment<byte>(buffer, 0, receiveResult.Count), receiveResult.MessageType, receiveResult.EndOfMessage, CancellationToken.None);
            }

            receiveResult = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
        }

        await webSocket.CloseAsync(receiveResult.CloseStatus.Value, receiveResult.CloseStatusDescription, CancellationToken.None);
        // Remove the client and their pair when they disconnect
        var pairToRemove = _socketPairs.FirstOrDefault(p => p.Item1 == webSocket || p.Item2 == webSocket);
        _socketPairs.Remove(pairToRemove);
    }
}