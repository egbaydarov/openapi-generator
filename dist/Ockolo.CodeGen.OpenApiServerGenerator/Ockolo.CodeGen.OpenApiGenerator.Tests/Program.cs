using Ockolo.CodeGen.OpenApiGenerator.Tests;
using Ockolo.CodeGen.OpenApiGenerator.Tests.Controllers;

var server = new ServerProvider<AppConfig>();

server
    .AddRequestHandler<IHaulageApiRequestHandler>((_, _) => new RequestHandlerStub());

var (start, shutdown) = server
    .StartServerAsync(
        args,
        cancellationToken: default);

await start;
await shutdown;