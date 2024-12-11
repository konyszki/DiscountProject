using System.Net;
using System.Net.WebSockets;
using System.Text;
using Newtonsoft.Json;

namespace DiscountServer
{
    public interface IWebSocketServer
    {
        Task StartAsync(CancellationToken cancellationToken);
    }

    public class WebSocketServer : IWebSocketServer
    {
        private readonly HttpListener _httpListener;
        private readonly string _url;
        private readonly IDiscountCodeGenerator _discountCodeGenerator;
        private readonly IFileDiscountStorage _fileStorage;

        public WebSocketServer(string url, IDiscountCodeGenerator discountCodeGenerator, IFileDiscountStorage fileStorage)
        {
            _url = url;
            _discountCodeGenerator = discountCodeGenerator;
            _fileStorage = fileStorage;
            _httpListener = new HttpListener();
            _httpListener.Prefixes.Add(url);
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _httpListener.Start();
            Console.WriteLine($"Server started at {_url}");

            while (!cancellationToken.IsCancellationRequested)
            {
                var context = await _httpListener.GetContextAsync();
                if (context.Request.IsWebSocketRequest)
                {
                    await HandleWebSocketConnectionAsync(context, cancellationToken);
                }
                else
                {
                    context.Response.StatusCode = 400;
                    context.Response.Close();
                }
            }
        }

        private async Task HandleWebSocketConnectionAsync(HttpListenerContext context, CancellationToken cancellationToken)
        {
            var webSocketContext = await context.AcceptWebSocketAsync(null);
            var webSocket = webSocketContext.WebSocket;

            await ProcessClientRequestsAsync(webSocket, cancellationToken);
        }

        private async Task ProcessClientRequestsAsync(WebSocket webSocket, CancellationToken cancellationToken)
        {
            var buffer = new byte[1024 * 4];

            while (!cancellationToken.IsCancellationRequested)
            {
                WebSocketReceiveResult result;
                ArraySegment<byte> segment = new ArraySegment<byte>(buffer);

                try
                {
                    result = await webSocket.ReceiveAsync(segment, cancellationToken);
                }
                catch (Exception)
                {
                    break;
                }

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Close", cancellationToken);
                    break;
                }

                var incomingMessage = Encoding.UTF8.GetString(buffer, 0, result.Count);
                Console.WriteLine($"Received message: {incomingMessage}");

                var request = JsonConvert.DeserializeObject<DiscountRequest>(incomingMessage);

                var codes = await _discountCodeGenerator.GenerateDiscountCodesAsync(request.Count);

                await _fileStorage.SaveDiscountCodesToFileAsync(codes);

                var responseMessage = JsonConvert.SerializeObject(new { Codes = codes });
                var responseBuffer = Encoding.UTF8.GetBytes(responseMessage);

                await webSocket.SendAsync(new ArraySegment<byte>(responseBuffer), WebSocketMessageType.Text, true, cancellationToken);
            }

            webSocket.Dispose();
        }
    }
}