#!/usr/bin/env bash
set -euo pipefail

root="/home/fitdnu/fitora"
deploy="$root/fitora_backend/deploy/ubuntu"

[[ "$(id -un)" == "fitdnu" ]] || { echo "Run as fitdnu." >&2; exit 1; }
[[ -d "$root/fitora_backend" && -d "$root/fitora_ui" && -f "$deploy/.env" ]] || { echo "Unexpected Fitora deployment root." >&2; exit 1; }
for script in "$deploy/deploy.sh" "$deploy/ci-deploy.sh" "$deploy/bootstrap-ci.sh"; do
  sed -i 's/\r$//' "$script"
  chmod 750 "$script"
done
install -d -m 700 "$root/incoming" "$root/releases"
