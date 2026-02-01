#!/usr/bin/env bash
set -euo pipefail

# self-contained-build.sh
# Builds a self-contained publish and includes the SQLite DB (migrations applied)

RUNTIME="win-x64"
OUTPUT_DIR="./publish"
CONFIGURATION="Release"
VERSION=""
PUBLISH_ARGS=()

usage() {
  cat <<EOF
Usage: $0 [-r runtime] [-o output] [-c configuration] [-v version] [-h]

Options:
  -r runtime        Runtime identifier (RID), default: win-x64
  -o output         Output directory, default: ./publish
  -c configuration  Build configuration, default: Release
  -v version        Optional version override (sets MSBuild property Version)
  -h                Show this help

Example:
  $0 -r linux-x64 -o ./publish -c Release -v 1.2.0
EOF
}

while getopts ":r:o:c:v:h" opt; do
  case ${opt} in
    r ) RUNTIME="$OPTARG" ;;
    o ) OUTPUT_DIR="$OPTARG" ;;
    c ) CONFIGURATION="$OPTARG" ;;
    v ) VERSION="$OPTARG" ;;
    h ) usage; exit 0 ;;
    \? ) echo "Invalid option: -$OPTARG" 1>&2; usage; exit 1 ;;
    : ) echo "Invalid option: -$OPTARG requires an argument" 1>&2; usage; exit 1 ;;
  esac
done

echo "[1/6] Configuration"
echo "  Runtime: $RUNTIME"
echo "  Output: $OUTPUT_DIR"
echo "  Configuration: $CONFIGURATION"
if [ -n "$VERSION" ]; then echo "  Version: $VERSION"; fi

# Clean previous publish
if [ -d "$OUTPUT_DIR" ]; then
  echo "[2/6] Cleaning previous publish at $OUTPUT_DIR"
  rm -rf "$OUTPUT_DIR"
fi

# Restore
echo "[3/6] Restoring packages..."
dotnet restore

# Build publish args
PUBLISH_ARGS+=(publish -c "$CONFIGURATION" -r "$RUNTIME" --self-contained true -o "$OUTPUT_DIR")
if [ -n "$VERSION" ]; then
  PUBLISH_ARGS+=("-p:Version=$VERSION")
fi

# Publish
echo "[4/6] Publishing self-contained application..."
dotnet "${PUBLISH_ARGS[@]}"

# Try to apply migrations using dotnet ef
echo "[5/6] Applying EF migrations (dotnet ef database update)"
if dotnet ef database update; then
  echo "EF migrations applied successfully."
else
  echo "Warning: 'dotnet ef database update' failed. Attempting fallback by running the published app briefly to apply migrations (requires Program.cs to call Database.Migrate())."
  PUBLISHED_DLL="${OUTPUT_DIR}/WattRescue.dll"
  PUBLISHED_EXE="${OUTPUT_DIR}/WattRescue"
  PUBLISHED_EXE_WIN="${OUTPUT_DIR}/WattRescue.exe"

  if [ -f "$PUBLISHED_EXE_WIN" ]; then
    echo "Found Windows exe at $PUBLISHED_EXE_WIN; attempting to start it in background..."
    "$PUBLISHED_EXE_WIN" &
    PUB_PID=$!
  elif [ -f "$PUBLISHED_EXE" ]; then
    echo "Found native executable at $PUBLISHED_EXE; attempting to start it in background..."
    "$PUBLISHED_EXE" &
    PUB_PID=$!
  elif [ -f "$PUBLISHED_DLL" ]; then
    echo "Found framework-dependent DLL at $PUBLISHED_DLL; starting with 'dotnet' in background..."
    dotnet "$PUBLISHED_DLL" &
    PUB_PID=$!
  else
    echo "No published binary found to run for fallback. Skipping auto-migrate fallback." >&2
    PUB_PID=""
  fi

  if [ -n "${PUB_PID:-}" ]; then
    echo "App started with PID $PUB_PID; waiting 10 seconds for migrations to run..."
    sleep 10
    echo "Stopping app (PID $PUB_PID)"
    kill "$PUB_PID" || true
    sleep 1
  fi
fi

# Copy database into publish folder
echo "[6/6] Packaging database"
SRC_DB="./wattrescue.db"
DEST_DB="$OUTPUT_DIR/wattrescue.db"
if [ -f "$SRC_DB" ]; then
  cp -f "$SRC_DB" "$DEST_DB"
  echo "Database copied to $DEST_DB"
else
  echo "Database not found at $SRC_DB. If migrations created it in a different location, copy it manually to $OUTPUT_DIR." >&2
fi

echo "\nBuild complete. Publish directory: $OUTPUT_DIR"

echo "Make sure the published binary exists and is executable. To run the published app:"
if [[ "$RUNTIME" == win* ]]; then
  echo "  $OUTPUT_DIR/WattRescue.exe"
else
  echo "  $OUTPUT_DIR/WattRescue (native) or: dotnet $OUTPUT_DIR/WattRescue.dll"
fi
