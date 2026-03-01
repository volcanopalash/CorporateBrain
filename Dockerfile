# 1. The Runtime Base (The final lightweight server)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

# 2. The Build SDK (The heavy compiler)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy all the project files so Docker knows about your architecture
COPY ["CorporateBrain.Api/CorporateBrain.Api.csproj", "CorporateBrain.Api/"]
COPY ["CorporateBrain.Infrastructure/CorporateBrain.Infrastructure.csproj", "CorporateBrain.Infrastructure/"]
COPY ["CorporateBrain.Application/CorporateBrain.Application.csproj", "CorporateBrain.Application/"]
COPY ["CorporateBrain.Domain/CorporateBrain.Domain.csproj", "CorporateBrain.Domain/"]

# Restore all the NuGet packages (Semantic Kernel, EF Core, etc.)
RUN dotnet restore "CorporateBrain.Api/CorporateBrain.Api.csproj"

# Copy the actual C# code and build it
COPY . .
WORKDIR "/src/CorporateBrain.Api"
RUN dotnet build "CorporateBrain.Api.csproj" -c Release -o /app/build

# 3. The Publisher (Optimizing the code for production)
FROM build AS publish
RUN dotnet publish "CorporateBrain.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# 4. The Final Image (Packing it into the lightweight server)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# The command that runs when the server boots up
ENTRYPOINT ["dotnet", "CorporateBrain.Api.dll"]
