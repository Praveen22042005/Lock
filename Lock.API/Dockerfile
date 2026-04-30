# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files first for better caching
COPY ["Lock.API/Lock.API.csproj", "Lock.API/"]
COPY ["Lock.Shared/Lock.Shared.csproj", "Lock.Shared/"]

# Restore dependencies
RUN dotnet restore "Lock.API/Lock.API.csproj"

# Copy all source files
COPY . .

# Build and publish
WORKDIR "/src/Lock.API"
RUN dotnet publish "Lock.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "Lock.API.dll"]
