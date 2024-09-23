using gRPC_AspNetCore.Context;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

#region Add db context

builder.Services.AddDbContext<GrpcContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("GrpcConnectionString"));
});

#endregion

// Add services to the container.
builder.Services.AddGrpc();

builder.Services.AddGrpcReflection();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.MapGrpcService<gRPC_AspNetCore.Services.v1.GrpcProductService>();
app.MapGrpcService<gRPC_AspNetCore.Services.v2.GrpcProductService>();

if (app.Environment.IsDevelopment())
{
    app.MapGrpcReflectionService();
}

app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();
