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


using CorporateBrain.Api.OpenApi;
using CorporateBrain.Application;
using CorporateBrain.Infrastructure; // Needed to see our DI method
using CorporateBrain.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore; // Needed if you use interfaces here
using System.Text;

//var builder = WebApplication.CreateBuilder(args);

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    // 🚨 FIX: Disable inotify file watchers to prevent Linux memory crashes on free tiers!
    // We don't need this anyway because Docker containers are read-only in production.
    ContentRootPath = AppContext.BaseDirectory
});

// Immediately after creating the builder, explicitly disable the configuration reload
builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: false);
builder.Configuration.AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: false);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"]!)),
            ///ClockSkew = TimeSpan.Zero // Optional: Reduce default clock skew
        };
    });

// Add services to the container.
builder.Services.AddControllers();

builder.Services.AddHttpContextAccessor(); // This allows us to access the current HTTP context in our services, which is useful for things like getting the current user's information from the JWT token. // 🚨 This unlocks the JWT data globally!

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
}); // This is the line that adds OpenAPI support to the project. It allows us to generate API documentation and test our endpoints through a web interface.
// 👇 THIS IS THE KEY LINE
// This calls the method we just wrote in DependencyInjection.cs
// It loads all the database stuff into the API.
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();

builder.Services.AddCors(options =>
{
    // WARNING: "AllowAnyOrigin" is great for testing and port folios. 
    // In strict enterprise, you would lock this down to your specific frontend URL!
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader());
});

var app = builder.Build();

// New: Tell the app it's running behind a secure cloud proxy!
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedProto
});

app.UseCors("AllowAll");

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
app.MapOpenApi();     // exposes /openapi/v1.json
app.MapScalarApiReference(); // exposes Scalar UI
//}

app.UseHttpsRedirection();
app.UseAuthentication(); // This line enables authentication middleware, which checks for JWT tokens in incoming requests and validates them according to the configuration we set up earlier.
app.UseAuthorization();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        // This command automatically runs any pending migrations on startup!
        context.Database.Migrate();
        Console.WriteLine("Database migrations applied successfully.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"An error occurred while migrating the database: {ex.Message}");
    }
}

app.Run();
