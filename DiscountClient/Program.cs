using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

public class DiscountRequest
{
    public int Count { get; set; } // Number of discount codes requested
}

public class DiscountResponse
{
    public List<string> Codes { get; set; }
}

internal class WebSocketClient
{
    public static async Task Main(string[] args)
    {
        using (ClientWebSocket client = new ClientWebSocket())
        {
            Uri serverUri = new Uri("ws://localhost:5000/ws/");
            await client.ConnectAsync(serverUri, CancellationToken.None);
            Console.WriteLine("Connected to the server.");

            int requestCount = 500;
            DiscountRequest request = new DiscountRequest { Count = requestCount };
            string requestJson = JsonSerializer.Serialize(request);

            byte[] requestBytes = Encoding.UTF8.GetBytes(requestJson);
            await client.SendAsync(new ArraySegment<byte>(requestBytes), WebSocketMessageType.Text, true, CancellationToken.None);
            Console.WriteLine($"Requested {requestCount} discount codes.");

            await ReceiveMessages(client);

            await client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client closing", CancellationToken.None);
            Console.WriteLine("Client disconnected.");
        }
    }

    private static async Task ReceiveMessages(ClientWebSocket client)
    {
        byte[] buffer = new byte[1024 * 10]; // Increase buffer size for large responses

        WebSocketReceiveResult result = await client.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
        string responseJson = Encoding.UTF8.GetString(buffer, 0, result.Count);

        // Deserialize response
        DiscountResponse response = JsonSerializer.Deserialize<DiscountResponse>(responseJson);
        Console.WriteLine($"Received {response.Codes.Count} discount codes:");
        foreach (string code in response.Codes)
        {
            Console.WriteLine($"{code} Length:{code.Length}");
        }
    }
}