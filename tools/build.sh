#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "$0")/.."
export XDG_DATA_HOME="$PWD/.tools/xdg-data"
export XDG_CONFIG_HOME="$PWD/.tools/xdg-config"
engine="$PWD/.tools/godot/Godot_v4.7.2-stable_linux.x86_64"
mkdir -p builds
"$engine" --headless --path . --editor --import --quit > builds/import.log 2>&1
"$engine" --headless --path . --export-debug Android builds/BLACK_MARKET.apk > builds/export-android.log 2>&1
if rg -q '^ERROR:|SCRIPT ERROR' builds/export-android.log; then cat builds/export-android.log; exit 1; fi
# The bundled 4.7.2 binary exporter rewrites both providers to .fileprovider.
python3 tools/fix_android_manifest.py builds/BLACK_MARKET.apk builds/android-unaligned.tmp
.tools/android-sdk/build-tools/35.0.1/zipalign -f -P 16 4 builds/android-unaligned.tmp builds/BLACK_MARKET.apk
.tools/android-sdk/build-tools/35.0.1/apksigner sign --ks .tools/android/debug.keystore --ks-key-alias androiddebugkey --ks-pass pass:android builds/BLACK_MARKET.apk
rm builds/android-unaligned.tmp
"$engine" --headless --path . --export-release Linux builds/BLACK_MARKET.x86_64 > builds/export-linux.log 2>&1
if rg -q '^ERROR:|SCRIPT ERROR' builds/export-linux.log; then cat builds/export-linux.log; exit 1; fi
.tools/android-sdk/build-tools/35.0.1/apksigner verify --verbose builds/BLACK_MARKET.apk
.tools/android-sdk/build-tools/35.0.1/zipalign -c -P 16 4 builds/BLACK_MARKET.apk
sha256sum builds/BLACK_MARKET.apk builds/BLACK_MARKET.x86_64 > builds/SHA256SUMS.txt
ls -lh builds/BLACK_MARKET.apk builds/BLACK_MARKET.x86_64
