FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project files
COPY ["Auth.API/Auth.API.csproj", "Auth.API/"]
COPY ["Auth.Application/Auth.Application.csproj", "Auth.Application/"]
COPY ["Auth.Domain/Auth.Domain.csproj", "Auth.Domain/"]
COPY ["Auth.Infrastructure/Auth.Infrastructure.csproj", "Auth.Infrastructure/"]
COPY ["Auth.Migrations/Auth.Migrations.csproj", "Auth.Migrations/"]
COPY ["Directory.Build.props", "./"]

# Restore dependencies
RUN dotnet restore "Auth.API/Auth.API.csproj"

# Copy all source code
COPY . .

# Build and publish
WORKDIR "/src/Auth.API"
RUN dotnet publish "Auth.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Generate runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Auth.API.dll"]
