#!/bin/sh
set -e

echo "Starting the app..."
exec dotnet project.dll
