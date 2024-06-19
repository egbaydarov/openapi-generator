using Ockolo.CodeGen.OpenApiGenerator.Tests.Controllers;
using Ockolo.CodeGen.OpenApiGenerator.Tests.Models;

namespace Ockolo.CodeGen.OpenApiGenerator.Tests;

public class RequestHandlerStub : IHaulageApiRequestHandler
{
    public Task<CreateHaulageResultResponse> CreateHaulageAsync(TripInfoRequestModel tripInfoRequestModel)
    {
        throw new NotImplementedException();
    }

    public Task<FindHaulagesByDestinationResultResponse> GetFilteredByDestinationAsync(string city, string country)
    {
        throw new NotImplementedException();
    }

    public Task<GetHaulageResultResponse> GetHaulageAsync(string id)
    {
        throw new NotImplementedException();
    }

    public Task<UpdateHaulageResultResponse> PutHaulageAsync(string id, TripInfoRequestModel tripInfoRequestModel)
    {
        throw new NotImplementedException();
    }
}