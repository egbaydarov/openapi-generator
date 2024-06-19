#!/bin/bash

# Read the API key from the file 'nuget_key'
API_KEY=$(<nuget_key)

# Check if the API key was successfully read
if [ -z "$API_KEY" ]; then
  echo "Error: nuget_key file is empty or not found"
  exit 1
fi

rm -rf ./**/bin
rm -rf ./**/obj
dotnet restore

# Iterate over all .csproj files and run dotnet pack
for csproj in ./**/*.csproj; do
  dotnet pack -c Release "$csproj"
done

# Iterate over all .nupkg files and run dotnet nuget push
for nupkg in ./**/*.nupkg; do
  dotnet nuget push "$nupkg" -k "$API_KEY" -s https://api.nuget.org/v3/index.json
done