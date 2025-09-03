# Multi-stage build for .NET 8 application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY ["AccountManagementSystem.csproj", "./"]
RUN dotnet restore "AccountManagementSystem.csproj"

# Copy everything else
COPY . .
RUN dotnet build "AccountManagementSystem.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "AccountManagementSystem.csproj" -c Release -o /app/publish

# Runtime stage - Using SDK for EF tools
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS final
WORKDIR /app

# Install EF tools
RUN dotnet tool install --global dotnet-ef

# Copy published app
COPY --from=publish /app/publish .

# Copy source code for EF tools
COPY --from=build /src /src

# Copy startup script
COPY startup.sh /app/startup.sh
RUN chmod +x /app/startup.sh

# Set environment variables
ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Development
ENV PATH="${PATH}:/root/.dotnet/tools"

# Expose port
EXPOSE 80

# Start the application using startup script
ENTRYPOINT ["/app/startup.sh"]
