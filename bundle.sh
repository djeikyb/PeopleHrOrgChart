#!/bin/sh

set -e # abort if any command has a non-zero exit code
set -o verbose # prints every line before executing it
set -o xtrace # like verbose but expands variables

PROJ="PeopleHrOrgChart"

ROOT=$(git rev-parse --show-toplevel)
BUNDLE_ROOT="$ROOT/artifacts/OrgChart.app"

mkdir -p "$BUNDLE_ROOT/Contents/Resources"
mkdir -p "$BUNDLE_ROOT/Contents/MacOS"
cp "$ROOT/$PROJ/Info.plist" "$BUNDLE_ROOT/Contents/"
cp "$ROOT/$PROJ/icon.icns" "$BUNDLE_ROOT/Contents/Resources/"
find "$ROOT/artifacts/publish/$PROJ/release_osx-arm64" -type f -perm +111 -print0 | xargs -I{} -0 cp {} "$BUNDLE_ROOT/Contents/MacOS/"
