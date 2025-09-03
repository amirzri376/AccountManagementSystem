#!/bin/bash

echo "Waiting for database to be ready..."
sleep 45

echo "Debug: Checking what's in /workspace directory..."
ls -la /workspace

echo "Setting up integration test database..."
cd /workspace/AccountManagementSystem
dotnet ef database update --verbose

if [ $? -eq 0 ]; then
    echo "Integration test database setup completed successfully"
else
    echo "Integration test database setup failed, but continuing..."
fi

echo "Running integration tests..."
cd /workspace
dotnet test IntegrationTests/IntegrationTests.csproj --verbosity normal

echo "Integration tests completed!"
