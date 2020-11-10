#!/usr/bin/env sh
set -e
dotnet run --no-launch-profile -f net5 -p "$(dirname "$0")/src" -- "$@"
