#!/usr/bin/env bash

#exit if any command fails
set -e

artifactsFolder="./artifacts"

if [ -d $artifactsFolder ]; then
  rm -R $artifactsFolder
fi

dotnet restore

dotnet test ./test/Invio.Immutable.Tests/Invio.Immutable.Tests.csproj -c Release -f netcoreapp1.0

dotnet pack -c Release -o ../../artifacts
