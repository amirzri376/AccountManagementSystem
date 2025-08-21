# Use .NET 8.0 runtime for the final image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Use .NET 8.0 SDK for building
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project file first (for better caching)
COPY ["LoginSystem.csproj", "./"]

# Install dependencies from project file:
# - Swashbuckle.AspNetCore (Swagger)
# - Microsoft.EntityFrameworkCore.SqlServer
# - Microsoft.EntityFrameworkCore.Tools
# - Microsoft.EntityFrameworkCore.Design
# - Microsoft.AspNetCore.Authentication.JwtBearer
# - BCrypt.Net-Next
RUN dotnet restore "LoginSystem.csproj"

# Copy all source code
COPY . .

# Build the application
RUN dotnet build "LoginSystem.csproj" -c Release -o /app/build

# Publish the application
FROM build AS publish
RUN dotnet publish "LoginSystem.csproj" -c Release -o /app/publish

# Create final runtime image
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "LoginSystem.dll"]
