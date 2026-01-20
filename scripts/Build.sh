#!/bin/zsh

# build.sh - Build Dbx in release mode, self-contained, for all platforms

# Path setup
SCRIPT_DIR=$(dirname $0)
SRC_DIR="$SCRIPT_DIR/../"   # Dbx.csproj is in root, not in src/
OUTPUT_DIR="$SCRIPT_DIR/../publish"

# Platforms to build for
PLATFORMS=("win-x64" "linux-x64" "osx-x64" "osx-arm64")  # added Apple Silicon

# Clean previous builds
echo "Cleaning previous builds..."
rm -rf "$OUTPUT_DIR"
mkdir -p "$OUTPUT_DIR"

# Build each platform
for RID in ${PLATFORMS[@]}; do
    echo "Building for $RID..."
    PLATFORM_DIR="$OUTPUT_DIR/$RID"
    mkdir -p "$PLATFORM_DIR"

    dotnet publish "$SRC_DIR/Dbx.csproj" \
        -c Release \
        -r $RID \
        --self-contained true \
        /p:PublishSingleFile=true \
        /p:PublishTrimmed=false \
        -o "$PLATFORM_DIR"

    if [ $? -eq 0 ]; then
        echo "Successfully built $RID -> $PLATFORM_DIR"
    else
        echo "Failed to build $RID"
        exit 1
    fi
done

echo "All builds completed. Output is in $OUTPUT_DIR"
