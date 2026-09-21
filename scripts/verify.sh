#!/usr/bin/env bash
# One-command project harness: build + full test suite + style check.
# Run this after any change (AI-assisted or manual) before considering it done.
set -euo pipefail
cd "$(dirname "${BASH_SOURCE[0]}")/.."

export PATH="$HOME/.dotnet:$PATH"

echo "==> dotnet build"
dotnet build --nologo

echo "==> dotnet test"
dotnet test --nologo

echo "==> dotnet format (style check)"
dotnet format --verify-no-changes --no-restore

echo "All checks passed."
