FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project files
COPY ["Web/Web.csproj", "Web/"]
COPY ["Application/Application.csproj", "Application/"]
COPY ["Domain/Domain.csproj", "Domain/"]
COPY ["Infrastructure/Infrastructure.csproj", "Infrastructure/"]
COPY ["Migrations/Migrations.csproj", "Migrations/"]
COPY ["Directory.Build.props", "./"]

# Restore dependencies
RUN dotnet restore "Web/Web.csproj"

# Copy all source code
COPY . .

# Build and publish
WORKDIR "/src/Web"
RUN dotnet publish "Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Generate runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_HTTP_PORTS=8080
USER app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Web.dll"]
