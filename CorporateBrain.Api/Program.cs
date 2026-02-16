//using CorporateBrain.Infrastructure;
//using CorporateBrain.Application.Common.Interfaces;
//using Scalar.AspNetCore;
//using Swashbuckle.AspNetCore.SwaggerGen;

//var builder = WebApplication.CreateBuilder(args);

//// Add services to the container.

//builder.Services.AddControllers();
//// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
////builder.Services.AddOpenApi();
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

//// 👇 THIS IS THE KEY LINE
//// This calls the method we just wrote in DependencyInjection.cs
//// It loads all the database stuff into the API.
//builder.Services.AddInfrastructure(builder.Configuration);

//var app = builder.Build();

//// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
//    app.UseSwagger();
//    app.UseSwaggerUI();
//    //app.MapOpenApi("/openapidocument.json");
//    //app.MapScalarApiReference();
//}

//app.UseHttpsRedirection();

//app.UseAuthorization();

//app.MapControllers();

//app.Run();


using CorporateBrain.Infrastructure; // Needed to see our DI method
using CorporateBrain.Application;
using Scalar.AspNetCore; // Needed if you use interfaces here

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

builder.Services.AddOpenApi(); // This is the line that adds OpenAPI support to the project. It allows us to generate API documentation and test our endpoints through a web interface.
// 👇 THIS IS THE KEY LINE
// This calls the method we just wrote in DependencyInjection.cs
// It loads all the database stuff into the API.
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();     // exposes /openapi/v1.json
    app.MapScalarApiReference(); // exposes Scalar UI
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

//app.Run();