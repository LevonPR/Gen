#!/usr/bin/env bash
# Build MicroEvolution.apk with Unity batchmode.
# Usage:
#   UNITY_EDITOR=/path/to/Unity ./Scripts/build-android-apk.sh
# Or set UNITY_EDITOR env / pass as $1.
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
UNITY_EDITOR="${1:-${UNITY_EDITOR:-}}"

if [[ -z "${UNITY_EDITOR}" ]]; then
  for candidate in \
    "/home/ubuntu/Unity/Hub/Editor/2022.3.52f1/Editor/Unity" \
    "/opt/unity/Editor/Unity" \
    "/Applications/Unity/Hub/Editor/2022.3.52f1/Unity.app/Contents/MacOS/Unity"
  do
    if [[ -x "$candidate" ]]; then
      UNITY_EDITOR="$candidate"
      break
    fi
  done
fi

if [[ -z "${UNITY_EDITOR}" || ! -x "${UNITY_EDITOR}" ]]; then
  echo "Unity Editor not found. Install Unity 2022.3 LTS with Android Build Support,"
  echo "then rerun: UNITY_EDITOR=/path/to/Unity $0"
  exit 2
fi

mkdir -p "$ROOT/Builds/Android" "$ROOT/Logs"
LOG="$ROOT/Logs/android-build.log"

echo "Building APK with: $UNITY_EDITOR"
"$UNITY_EDITOR" \
  -quit \
  -batchmode \
  -nographics \
  -projectPath "$ROOT" \
  -executeMethod MicroEvolution.EditorTools.AndroidBuilder.BuildApkCli \
  -logFile "$LOG"

APK="$ROOT/Builds/Android/MicroEvolution.apk"
if [[ -f "$APK" ]]; then
  echo "SUCCESS: $APK"
  ls -lh "$APK"
else
  echo "Build finished but APK missing. See $LOG"
  exit 1
fi
