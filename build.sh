#!/usr/bin/env bash
# Local packaging helper for manual installs. Assembles dist/PreRoll_<version>/.
# Published releases are produced by CI (.github/workflows/release.yml), where the
# git tag is the authoritative version. Here the version is read from meta.json so
# this helper always matches the committed metadata rather than a hardcoded value.
set -euo pipefail

ROOT="$(cd "$(dirname "$0")" && pwd)"
VERSION="$(sed -n 's/.*"version"[[:space:]]*:[[:space:]]*"\([^"]*\)".*/\1/p' "$ROOT/meta.json" | head -1)"
if [ -z "$VERSION" ]; then
  echo "Could not read version from meta.json" >&2
  exit 1
fi
OUT="$ROOT/dist/PreRoll_${VERSION}"

export PATH="$HOME/.dotnet:$PATH"

dotnet build -c Release "$ROOT/Jellyfin.Plugin.PreRoll/Jellyfin.Plugin.PreRoll.csproj"

mkdir -p "$OUT"
cp "$ROOT/Jellyfin.Plugin.PreRoll/bin/Release/net9.0/Jellyfin.Plugin.PreRoll.dll" "$OUT/"
cp "$ROOT/meta.json" "$OUT/"

echo "Assembled: $OUT"
ls -la "$OUT"
