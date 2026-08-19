#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
HKMANAGED="$ROOT/HKManaged"
CACHE="$ROOT/.cache/setup-hk"
SETUP_HK_JS="$CACHE/index.js"

require_cmd() {
  if ! command -v "$1" >/dev/null 2>&1; then
    echo "Missing required command: $1" >&2
    exit 1
  fi
}

require_cmd curl
require_cmd node

if [[ ! -f "$HKMANAGED/Assembly-CSharp.dll" ]]; then
  echo "Setting up Hollow Knight modding API in $HKMANAGED"
  mkdir -p "$HKMANAGED" "$CACHE"
  if [[ ! -f "$SETUP_HK_JS" ]]; then
    curl -fsSL "https://raw.githubusercontent.com/BadMagic100/setup-hk/v1/dist/index.js" -o "$SETUP_HK_JS"
  fi
  export INPUT_apiPath="$HKMANAGED"
  export INPUT_dependencyFilePath="$ROOT/ModDependencies.txt"
  node "$SETUP_HK_JS"
fi

PROPS="$ROOT/CustomKnight/LocalBuildProperties.props"
if [[ ! -f "$PROPS" ]]; then
  cp "$ROOT/CustomKnight/LocalBuildProperties_example.props" "$PROPS"
fi

echo "Build environment ready."
