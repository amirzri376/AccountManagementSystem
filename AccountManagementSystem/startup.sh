#!/bin/bash

echo "Waiting for database to be ready..."
sleep 45

echo "Running database migrations..."
cd /app
dotnet ef database update --project /src/AccountManagementSystem.csproj --verbose

if [ $? -eq 0 ]; then
    echo "Migrations completed successfully"
else
    echo "Migration failed, but continuing..."
fi

echo "Starting application..."
cd /app/bin/Release/net8.0
dotnet AccountManagementSystem.dll
