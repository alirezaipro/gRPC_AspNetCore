using Azure.Core;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using gRPC_AspNetCore.Context;
using gRPC_AspNetCore.Models;
using gRPC_AspNetCore.Protos;
using gRPC_AspNetCore.Protos.v1;
using Microsoft.EntityFrameworkCore;
using static gRPC_AspNetCore.Protos.v1.ProductService;

namespace gRPC_AspNetCore.Services.v1
{
    public class GrpcProductService
        (GrpcContext ctx)
        : ProductServiceBase
    {

        public override async Task CreateProduct(IAsyncStreamReader<CreateProductRequest> requestStream, IServerStreamWriter<CreateProductReply> responseStream, ServerCallContext context)
        {
            int createdProductsCount = 0;

            while (await requestStream.MoveNext())
            {
                ctx.Products.Add(new Product()
                {
                    CreateDate = DateTime.Now,
                    Description = requestStream.Current.Description,
                    Price = requestStream.Current.Price,
                    Title = requestStream.Current.Title,
                });

                createdProductsCount++;
            }

            await ctx.SaveChangesAsync();

            await responseStream.WriteAsync(new CreateProductReply
            {
                CreatedItemsCount = createdProductsCount,
                Message = "Products created successfully",
                Status = 200
            });
        }

        public override async Task<GetProductByIdReply> GetProductById(GetProductByIdRequest request, ServerCallContext context)
        {
            Product? product = await ctx.Products.FirstOrDefaultAsync(p => p.Id == request.Id);

            if (product == null)
                return null;

            #region Headers

            Metadata headers = new Metadata()
            {
                {"fName","Ali" },
                {"lName","Rezaei" },
                {"age","24"}
            };

            await context.WriteResponseHeadersAsync(headers);

            #endregion

            context.ResponseTrailers.Add("FirstName", "Ali");
            context.ResponseTrailers.Add("LastName", "Rezaei");
            context.ResponseTrailers.Add("SuccessMessage", "GetProdutByIdSuccessfullyDone");

            return new GetProductByIdReply
            {
                CreateDate = Timestamp.FromDateTime(DateTime.SpecifyKind(product.CreateDate, DateTimeKind.Utc)),
                Description = product.Description,
                Id = product.Id,
                Price = product.Price,
                Tilte = product.Title
            };
        }

        public override async Task GetAllProducts(GetAllProductsRequest request, IServerStreamWriter<GetAllProductsReply> responseStream, ServerCallContext context)
        {
            int skip = (request.Page - 1) * request.Take;

            List<Product> products = await ctx.Products
                .Skip(skip)
                .Take(request.Take)
                .ToListAsync();

            foreach (var product in products)
            {
                await responseStream.WriteAsync(new GetAllProductsReply
                {
                    CreateDate = Timestamp.FromDateTime(DateTime.SpecifyKind(product.CreateDate, DateTimeKind.Utc)),
                    Description = product.Description,
                    Id = product.Id,
                    Price = product.Price,
                    Tilte = product.Title
                });
            };
        }

        public override async Task<RemoveProductByIdReply> RemoveProductById(IAsyncStreamReader<RemoveProductByIdRequest> requestStream, ServerCallContext context)
        {
            int removedItemsCount = 0;

            while (await requestStream.MoveNext())
            {
                Product? product = await ctx.Products.FirstOrDefaultAsync(p => p.Id == requestStream.Current.Id);

                if (product == null)
                    continue;

                ctx.Products.Remove(product);

                removedItemsCount++;
            }

            await ctx.SaveChangesAsync();

            return new RemoveProductByIdReply()
            {
                Message = "Remoed products successfully done",
                RemovedItemsCount = removedItemsCount,
                Status = 200
            };
        }

        public override async Task<UpdateProductReply> UpdateProduct(UpdateProductRequest request, ServerCallContext context)
        {
            Product? product = await ctx.Products.FirstOrDefaultAsync(p => p.Id == request.Id);

            if (product == null)
                return null;

            product.Title = request.Title;
            product.Description = request.Description;
            product.Price = request.Price;

            ctx.Products.Update(product);
            await ctx.SaveChangesAsync();

            return new UpdateProductReply()
            {
                Message = "Updated product successfully done",
                Status = 200,
                UpdatedItemsCount = 1
            };
        }
    }
}
