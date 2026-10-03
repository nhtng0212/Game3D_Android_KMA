#!/usr/bin/env bash
set -euo pipefail
cd -- "$(dirname -- "${BASH_SOURCE[0]}")"
mkdir -p Documentation
exec ./Builds/Linux/BlackMarket.x86_64 -screen-fullscreen 0 -screen-width 1280 -screen-height 720 -logFile "$PWD/Documentation/player.log" "$@"
