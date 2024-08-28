using Grpc.Core;
using gRPC_AspNetCore.Protos;
using static gRPC_AspNetCore.Protos.ProductService;

namespace gRPC_AspNetCore.Services
{
    public class GrpcProductService:ProductServiceBase
    {
        public override Task<CreateProductReply> CreateProduct(IAsyncStreamReader<CreateProductRequest> requestStream, ServerCallContext context)
        {
            return base.CreateProduct(requestStream, context);
        }
    }
}
