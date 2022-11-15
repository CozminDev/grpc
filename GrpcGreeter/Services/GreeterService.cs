using Google.Protobuf;
using Grpc.Core;
using GrpcGreeter;
using System.IO;

namespace GrpcGreeter.Services
{
    public class GreeterService : Greeter.GreeterBase
    {
        private readonly ILogger<GreeterService> _logger;
        public GreeterService(ILogger<GreeterService> logger)
        {
            _logger = logger;
        }

        public override Task<HelloReply> Unary(HelloRequest request, ServerCallContext context)
        {
            return Task.FromResult(new HelloReply
            {
                Message = "Hello " + request.Name
            });
        }

        public override async Task ServerStreaming(HelloRequest request, IServerStreamWriter<HelloReply> responseStream, ServerCallContext context)
        {
            for(int i = 0; i < 3; i++)
            {
                await responseStream.WriteAsync(new HelloReply() { Message = $"Hello reply the {i}th time" });
            }

            _logger.LogInformation($"Client streaming: {request.Name}");
        }

        public override async Task<HelloReply> ClientStreaming(IAsyncStreamReader<HelloRequest> requestStream, ServerCallContext context)
        {
            await foreach (var request in requestStream.ReadAllAsync())
            {
                _logger.LogInformation($"Client streaming: {request.Name}");
            }

            return new HelloReply()
            {
                Message = "Completed"
            };
        }

        public override async Task BidirectionalStreaming(IAsyncStreamReader<HelloRequest> requestStream, IServerStreamWriter<HelloReply> responseStream, ServerCallContext context)
        {
            await foreach (var request in requestStream.ReadAllAsync())
            {
                _logger.LogInformation($"Bidirectional streaming: {request.Name}");
            }

            for(int i = 0; i < 5; i++){
                await responseStream.WriteAsync(new HelloReply() { Message = $"Hello reply the {i}th time time:{DateTime.UtcNow}"});
            }
        }

        public override async Task VideoStreaming(FileRequest request, IServerStreamWriter<ChunkMsg> responseStream, ServerCallContext context)
        {
            using FileStream source = new FileStream(request.FilePath, FileMode.Open, FileAccess.Read, FileShare.None, bufferSize: 1000, useAsync: true);

            byte[] buffer = new byte[1000];

            while (await source.ReadAsync(buffer, 0, buffer.Length) != 0)
            {
                ByteString byteString = ByteString.CopyFrom(buffer);

                await responseStream.WriteAsync(new ChunkMsg() { Chunk = byteString });
            }
        }
    }
}