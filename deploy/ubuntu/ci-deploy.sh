#!/usr/bin/env bash
set -euo pipefail

CI_DEPLOY_VERSION=1
root="/home/fitdnu/fitora"
component=""
sha=""
archive=""

while [[ $# -gt 0 ]]; do
  case "$1" in
    --component) component="${2:-}"; shift 2 ;;
    --sha) sha="${2:-}"; shift 2 ;;
    --archive) archive="${2:-}"; shift 2 ;;
    *) printf 'Usage: %s --component backend|frontend --sha <40-hex> --archive <path>\n' "$0" >&2; exit 2 ;;
  esac
done

fail() { printf 'ERROR: %s\n' "$*" >&2; exit 1; }
[[ "$component" == "backend" || "$component" == "frontend" ]] || fail "Invalid component."
[[ "$sha" =~ ^[0-9a-f]{40}$ ]] || fail "Invalid commit SHA."
[[ -d "$root/fitora_backend/deploy/ubuntu" && -d "$root/fitora_ui" ]] || fail "Unexpected Fitora root: $root"
[[ -f "$root/fitora_backend/deploy/ubuntu/.env" ]] || fail "Runtime .env is missing."

if [[ "${FITORA_DEPLOY_LOCK_HELD:-}" != "1" ]]; then
  exec env FITORA_DEPLOY_LOCK_HELD=1 flock -w 600 "$root/.deploy.lock" "$0" --component "$component" --sha "$sha" --archive "$archive"
fi

incoming="$root/incoming"
releases="$root/releases/$component"
mkdir -p "$incoming" "$releases"
[[ -f "$archive" ]] || fail "Archive does not exist."
archive_real="$(realpath -- "$archive")"
[[ "$archive_real" == "$incoming"/* && -f "$archive_real" ]] || fail "Archive must be an existing file under $incoming."
tar -tzf "$archive_real" >/dev/null || fail "Archive is invalid."
if tar -tzf "$archive_real" | grep -Eq '(^/|(^|/)\.\.(/|$))'; then
  fail "Archive contains an unsafe path."
fi

candidate="$(mktemp -d "$root/.${component}-candidate.XXXXXX")"
cleanup() { rm -rf -- "$candidate"; }
trap cleanup EXIT
mkdir "$candidate/source"
tar -xzf "$archive_real" -C "$candidate/source"

if [[ "$component" == "backend" ]]; then
  [[ -f "$candidate/source/deploy/ubuntu/compose.yml" && -f "$candidate/source/deploy/ubuntu/deploy.sh" ]] || fail "Backend archive is incomplete."
  install -D -m 600 "$root/fitora_backend/deploy/ubuntu/.env" "$candidate/source/deploy/ubuntu/.env"
else
  [[ -f "$candidate/source/package-lock.json" && -f "$candidate/source/package.json" ]] || fail "Frontend archive is incomplete."
fi

if [[ "$component" == "backend" ]]; then
  live="$root/fitora_backend"
else
  live="$root/fitora_ui"
fi
backup="$releases/${sha}-previous"
[[ -d "$live" ]] || fail "Live source directory is missing: $live"
mv "$live" "$backup"
mv "$candidate/source" "$live"

if (cd "$root/fitora_backend" && bash deploy/ubuntu/deploy.sh --component "$component"); then
  mkdir -p "$root/.fitora-release"
  printf '%s\n' "$sha" > "$root/.fitora-release/${component}.sha"
  rm -f -- "$archive_real"
  printf 'Deployed %s %s\n' "$component" "$sha"
  exit 0
fi

if [[ "$component" == "frontend" ]]; then
  failed="$releases/${sha}-failed"
  mv "$live" "$failed"
  mv "$backup" "$live"
  (cd "$root/fitora_backend" && bash deploy/ubuntu/deploy.sh --component frontend) || fail "Frontend rollback failed; inspect $failed."
  fail "Frontend deploy failed and source/image were restored."
fi

fail "Backend deploy failed after source swap. Previous source is at $backup; no automatic rollback occurs after migrations."
