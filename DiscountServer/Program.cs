using DiscountServer;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();
services.AddSingleton<IDiscountStorage>(sp => new FileDiscountStorage("discounts.txt"));
services.AddSingleton<WebSocketServer>();

var serviceProvider = services.BuildServiceProvider();
var server = serviceProvider.GetRequiredService<WebSocketServer>();

await server.StartAsync();