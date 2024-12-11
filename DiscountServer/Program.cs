using DiscountServer;
using Microsoft.Extensions.DependencyInjection;

var serviceProvider = new ServiceCollection()
     .AddScoped<IDiscountCodeGenerator, DiscountCodeGenerator>()
     .AddScoped<IFileDiscountStorage, FileDiscountStorage>()
     .AddScoped<IWebSocketServer>(provider => {
         var generator = provider.GetRequiredService<IDiscountCodeGenerator>();
         var storage = provider.GetRequiredService<IFileDiscountStorage>();
         return new WebSocketServer("http://localhost:5000/", generator, storage);
     })
     .BuildServiceProvider();

var cancellationTokenSource = new CancellationTokenSource();
var webSocketServer = serviceProvider.GetRequiredService<IWebSocketServer>();

// Uruchamiamy serwer WebSocket w tle
var serverTask = webSocketServer.StartAsync(cancellationTokenSource.Token);

Console.WriteLine("Press Enter to exit...");
Console.ReadLine();

// Zatrzymujemy serwer
cancellationTokenSource.Cancel();
await serverTask;