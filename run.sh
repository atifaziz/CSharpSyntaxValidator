#!/usr/bin/env bash
set -e
dotnet run --no-launch-profile -f netcoreapp2.2 -p "$(dirname "$0")/src" -- "$@"
