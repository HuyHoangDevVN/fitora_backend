#!/usr/bin/env bash
set -euo pipefail

deploy_dir="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
backend_dir="$(cd -- "$deploy_dir/../.." && pwd)"
ui_dir="$(cd -- "$backend_dir/../fitora_ui" && pwd)"
env_file="$deploy_dir/.env"
domain="fitora.fitdnu.id.vn"
component="all"

if [[ $# -gt 0 ]]; then
  [[ $# -eq 2 && "$1" == "--component" ]] || { printf 'Usage: %s [--component all|backend|frontend]\n' "$0" >&2; exit 2; }
  component="$2"
fi
[[ "$component" == "all" || "$component" == "backend" || "$component" == "frontend" ]] || { printf 'Unknown component: %s\n' "$component" >&2; exit 2; }

fail() { printf 'ERROR: %s\n' "$*" >&2; exit 1; }
wait_for_status() {
  local url="$1" expected="$2" status=""
  for _ in {1..30}; do
    status="$(curl --silent --output /dev/null --write-out '%{http_code}' "$url" || true)"
    [[ "$status" == "$expected" ]] && return 0
    sleep 2
  done
  fail "Expected ${url} to return ${expected}, got ${status:-no response}."
}
command -v docker >/dev/null || fail "Docker is not installed. Install Docker Engine and the Docker Compose plugin, then run again."
docker compose version >/dev/null 2>&1 || fail "Docker Compose plugin is unavailable. Install it, then run again."
[[ -f "$ui_dir/package-lock.json" ]] || fail "Expected sibling frontend repository at $ui_dir."

if [[ ! -f "$env_file" ]]; then
  cp "$deploy_dir/.env.example" "$env_file"
  chmod 600 "$env_file"
  printf 'Created %s. Set ACME_EMAIL, then run this script again.\n' "$env_file"
  exit 1
fi

if ! grep -Eq '^ACME_EMAIL=[^[:space:]@]+@[^[:space:]@]+$' "$env_file"; then
  fail "Set a real ACME_EMAIL in $env_file before requesting the HTTPS certificate."
fi

command -v openssl >/dev/null || fail "openssl is required to create first-run secrets."
for key in JWT_SECRET MYSQL_ROOT_PASSWORD MYSQL_PASSWORD MONGO_PASSWORD RABBITMQ_PASSWORD ELASTIC_PASSWORD; do
  if grep -q "^${key}=CHANGE_ME$" "$env_file"; then
    value="$(openssl rand -hex 32)"
    sed -i "s/^${key}=CHANGE_ME$/${key}=${value}/" "$env_file"
    printf 'Generated %s.\n' "$key"
  fi
done

cd "$deploy_dir"
compose=(docker compose --env-file "$env_file" -f compose.yml)
"${compose[@]}" config --quiet

if [[ "$component" == "all" ]]; then
  sudo -n true || fail "Passwordless sudo is required only to add the Fitora Nginx vhost and TLS renewal timer."
fi

if [[ "$component" == "all" || "$component" == "backend" ]]; then
  "${compose[@]}" build auth
  "${compose[@]}" up -d --wait mariadb mongodb redis rabbitmq elasticsearch
  "${compose[@]}" --profile tools run --rm volume-init
  "${compose[@]}" --profile tools run --rm migrate
  "${compose[@]}" up -d auth user interact chat notification gateway
fi

if [[ "$component" == "all" || "$component" == "frontend" ]]; then
  "${compose[@]}" build frontend
  "${compose[@]}" up -d frontend
fi

if [[ "$component" == "all" ]]; then
  sudo install -d -m 755 /var/www/fitora-acme /etc/nginx/sites-available /etc/nginx/sites-enabled
  if ! sudo test -f "/etc/letsencrypt/live/${domain}/fullchain.pem"; then
    sudo install -m 644 nginx.host.http.conf /etc/nginx/sites-available/fitora
    sudo ln -sfn /etc/nginx/sites-available/fitora /etc/nginx/sites-enabled/fitora
    sudo nginx -t && sudo systemctl reload nginx
    sudo docker run --rm -v /etc/letsencrypt:/etc/letsencrypt -v /var/www/fitora-acme:/var/www/fitora-acme certbot/certbot:v2.11.0 certonly --webroot -w /var/www/fitora-acme -d "$domain" --email "$(grep '^ACME_EMAIL=' "$env_file" | cut -d= -f2-)" --agree-tos --non-interactive
  fi
  sudo install -m 644 nginx.host.https.conf /etc/nginx/sites-available/fitora
  sudo install -m 755 fitora-cert-renew /usr/local/sbin/fitora-cert-renew
  sudo install -m 644 fitora-cert-renew.service /etc/systemd/system/fitora-cert-renew.service
  sudo install -m 644 fitora-cert-renew.timer /etc/systemd/system/fitora-cert-renew.timer
  sudo systemctl daemon-reload
  sudo systemctl enable --now fitora-cert-renew.timer
  sudo nginx -t && sudo systemctl reload nginx
fi

if [[ "$component" == "all" || "$component" == "frontend" ]]; then
  wait_for_status "https://${domain}/login" 200
fi
if [[ "$component" == "all" || "$component" == "backend" ]]; then
  wait_for_status "https://${domain}/api/auth/auth/check-cookie" 200
  wait_for_status "https://${domain}/api/auth/admin/is-authorized" 401
fi
printf 'Fitora is available at https://%s\n' "$domain"
