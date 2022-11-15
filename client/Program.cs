using Grpc.Net.Client;
using GrpcGreeter;

CancellationTokenSource source = new CancellationTokenSource();

using var channel = GrpcChannel.ForAddress("http://localhost:5042");
var client = new Greeter.GreeterClient(channel);

HelloReply replyUnary = await client.UnaryAsync(new HelloRequest() { Name = "Cozmin" });
Console.WriteLine("Greeting Unary: " + replyUnary.Message);
Console.WriteLine();

using var serverStream = client.ServerStreaming(new HelloRequest() { Name = "Cozmin" });

while (await serverStream.ResponseStream.MoveNext(source.Token))
{
    Console.WriteLine("Greeting Server Streaming: " + serverStream.ResponseStream.Current);
}

Console.WriteLine();

using var clientStream = client.ClientStreaming();

for (var i = 0; i < 3; i++)
{
    await clientStream.RequestStream.WriteAsync(new HelloRequest() { Name = "Cozmin" });
}
await clientStream.RequestStream.CompleteAsync();

HelloReply replyClientStreaming = await clientStream;
Console.WriteLine("Greeting Client Streaming: " + replyClientStreaming.Message);
Console.WriteLine();

using var bidirectionalStreaming = client.BidirectionalStreaming();

for (var i = 0; i < 3; i++)
{
    await bidirectionalStreaming.RequestStream.WriteAsync(new HelloRequest() { Name = $"Cozmin + {i} time:{DateTime.UtcNow}" });
}

var readTask = Task.Run(async () =>
{
    while (await bidirectionalStreaming.ResponseStream.MoveNext(source.Token))
    {
        Console.WriteLine("Greeting Bidirectional Streaming: " + bidirectionalStreaming.ResponseStream.Current);
    }
});

await bidirectionalStreaming.RequestStream.CompleteAsync();
await readTask;

Console.WriteLine("Greeting Bidirectional Streaming: " + replyClientStreaming.Message);
Console.WriteLine();

Console.WriteLine("Press any key to exit...");
Console.ReadKey();
