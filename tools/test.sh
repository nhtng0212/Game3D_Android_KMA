#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "$0")/.."
export XDG_DATA_HOME="$PWD/.tools/automated-tests"
mkdir -p "$XDG_DATA_HOME" builds
engine="$PWD/.tools/godot/Godot_v4.7.2-stable_linux.x86_64"
"$engine" --headless --path . --editor --import --quit > builds/import.log 2>&1
for suite in campaign mechanics ui_layout; do
  log="builds/test-${suite}.log"
  timeout 90 "$engine" --headless --fixed-fps 60 --path . --script "tests/${suite}_test.gd" -- --test-silent > "$log" 2>&1
  if rg -q 'SCRIPT ERROR|^ERROR:|FAIL:' "$log"; then
    cat "$log"
    exit 1
  fi
  rg 'TEST: PASS' "$log"
done
