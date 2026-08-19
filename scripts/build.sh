#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
CONFIG="${1:-Release}"

if ! command -v dotnet >/dev/null 2>&1; then
  echo "Missing required command: dotnet" >&2
  exit 1
fi

"$ROOT/scripts/setup-build-env.sh"
dotnet build "$ROOT/CustomKnight.sln" -c "$CONFIG"
