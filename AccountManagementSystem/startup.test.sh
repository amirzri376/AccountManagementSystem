#!/bin/bash

echo "Waiting for database to be ready..."
sleep 45

echo "Setting up test database..."
cd /workspace/AccountManagementSystem
dotnet ef database update --verbose

if [ $? -eq 0 ]; then
    echo "Test database setup completed successfully"
else
    echo "Test database setup failed, but continuing..."
fi

echo "Running unit tests..."
cd /workspace
dotnet test Tests/Tests.csproj --verbosity normal

echo "Unit tests completed!"
