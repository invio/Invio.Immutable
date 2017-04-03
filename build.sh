#!/usr/bin/env bash

#exit if any command fails
set -e

artifactsFolder="./artifacts"

if [ -d $artifactsFolder ]; then
  rm -R $artifactsFolder
fi

dotnet restore ./src/Invio.Immutable
dotnet restore ./test/Invio.Immutable.Tests

dotnet test ./test/Invio.Immutable.Tests/Invio.Immutable.Tests.csproj -c Release -f netcoreapp1.0

dotnet pack ./src/Invio.Immutable -c Release -o ../../artifacts
