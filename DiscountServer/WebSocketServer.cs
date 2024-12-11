using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DiscountServer
{
    public interface IWebSocketServer
    {
        Task StartAsync();
    }

    public class WebSocketServer : IWebSocketServer
    {
        private readonly IDiscountStorage _discountStorage;

        public WebSocketServer(IDiscountStorage discountStorage)
        {
            _discountStorage = discountStorage;
        }

        public async Task StartAsync()
        {
            HttpListener httpListener = new HttpListener();
            httpListener.Prefixes.Add("http://localhost:5000/ws/");
            httpListener.Start();
            Console.WriteLine("WebSocket server started at ws://localhost:5000/ws/");

            while (true)
            {
                HttpListenerContext context = await httpListener.GetContextAsync();

                if (context.Request.IsWebSocketRequest)
                {
                    HttpListenerWebSocketContext wsContext = await context.AcceptWebSocketAsync(null);
                    Console.WriteLine("Client connected.");
                    _ = Task.Run(() => HandleWebSocketConnection(wsContext.WebSocket));
                }
                else
                {
                    context.Response.StatusCode = 400;
                    context.Response.Close();
                }
            }
        }

        private async Task HandleWebSocketConnection(WebSocket webSocket)
        {
            byte[] buffer = new byte[1024];

            while (webSocket.State == WebSocketState.Open)
            {
                WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                string jsonMessage = Encoding.UTF8.GetString(buffer, 0, result.Count);

                Console.WriteLine($"Received JSON: {jsonMessage}");

                // Deserialize request
                DiscountRequest request = JsonSerializer.Deserialize<DiscountRequest>(jsonMessage);
                int count = Math.Min(request?.Count ?? 0, 2000);

                // Fetch or generate discount codes
                var codes = FetchOrGenerateDiscountCodes(count).ToList();

                // Serialize and send response
                DiscountResponse response = new DiscountResponse { Codes = codes };
                string responseJson = JsonSerializer.Serialize(response);
                byte[] responseBytes = Encoding.UTF8.GetBytes(responseJson);

                await webSocket.SendAsync(new ArraySegment<byte>(responseBytes), WebSocketMessageType.Text, true, CancellationToken.None);
                Console.WriteLine($"Sent {codes.Count} discount codes.");
            }

            Console.WriteLine("Client disconnected.");
            await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
        }

        internal IEnumerable<string> FetchOrGenerateDiscountCodes(int count)
        {
            HashSet<string> existingCodes = _discountStorage.FetchExistingCodes();
            IEnumerable<string> newCodes = new List<string>();

            int codesToGenerate = count - existingCodes.Count;
            if (codesToGenerate > 0)
            {
                newCodes = GenerateUniqueDiscountCodes(codesToGenerate, existingCodes);
                _discountStorage.SaveNewCodes(newCodes);
            }

            return existingCodes.Take(count).Concat(newCodes).ToList();
        }

        internal IEnumerable<string> GenerateUniqueDiscountCodes(int count, HashSet<string> existingCodes)
        {
            HashSet<string> newCodes = new HashSet<string>();
            Random random = new Random();

            while (newCodes.Count < count)
            {
                int length = random.Next(7, 9); // Random length: 7 or 8 characters
                string code = GenerateRandomCode(length, random);

                if (!existingCodes.Contains(code) && newCodes.Add(code))
                {
                    existingCodes.Add(code);
                }
            }

            return newCodes.ToList();
        }

        private static string GenerateRandomCode(int length, Random random)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            char[] code = new char[length];
            for (int i = 0; i < length; i++)
            {
                code[i] = chars[random.Next(chars.Length)];
            }
            return new string(code);
        }
    }
}