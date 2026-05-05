FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project files
COPY ["Payments.API/Payments.API.csproj", "Payments.API/"]
COPY ["Payments.Application/Payments.Application.csproj", "Payments.Application/"]
COPY ["Payments.Domain/Payments.Domain.csproj", "Payments.Domain/"]
COPY ["Payments.Infrastructure/Payments.Infrastructure.csproj", "Payments.Infrastructure/"]
COPY ["Payments.Migrations/Payments.Migrations.csproj", "Payments.Migrations/"]

# Restore dependencies
RUN dotnet restore "Payments.API/Payments.API.csproj"

# Copy all source code
COPY . .

# Build and publish
WORKDIR "/src/Payments.API"
RUN dotnet publish "Payments.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Generate runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Payments.API.dll"]
