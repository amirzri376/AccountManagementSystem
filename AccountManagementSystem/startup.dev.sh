#!/bin/bash

echo "Waiting for database to be ready..."
sleep 45

echo "Running database migrations..."
cd /app
dotnet ef database update --project AccountManagementSystem.csproj --verbose

if [ $? -eq 0 ]; then
    echo "Migrations completed successfully"
else
    echo "Migration failed, but continuing..."
fi

echo "Starting application with hot reload..."
cd /app
dotnet watch run --urls http://+:80
