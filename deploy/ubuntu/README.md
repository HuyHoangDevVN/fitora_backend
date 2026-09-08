# Fitora on Ubuntu

This runs the existing .NET 8 services, MariaDB, MongoDB, Redis, RabbitMQ, Elasticsearch, and a Vite production build. The existing host Nginx is the single public proxy: Fitora containers listen only on `127.0.0.1:8081` and `127.0.0.1:8082`, so no database or backend port is public. Named Docker volumes retain databases, queues, search data, and uploads after restart/redeploy.

## First deployment

1. Put `fitora_backend` and `fitora_ui` side-by-side.
2. Point DNS `fitora.fitdnu.id.vn` to this host and allow inbound TCP 80/443. If Cloudflare proxies this hostname, its origin must reach this Ubuntu host on port 80 for the initial HTTP-01 certificate request.
3. From backend run:

   ```bash
   bash deploy/ubuntu/deploy.sh
   ```

4. The first run creates `deploy/ubuntu/.env`. Set `ACME_EMAIL`, then rerun. It generates the JWT/database/broker/search secrets once, without printing or overwriting them.

The script checks Compose without printing resolved values, checks that its two localhost ports are free, waits for dependencies, applies the four existing EF migrations sequentially, installs only the Fitora Nginx virtual host, requests/renews HTTPS, then checks UI, API and an unauthenticated admin route through the proxy.

No account is seeded: registration is the supported source flow and there is no canonical seed. Register the first account in the UI; create/assign `ADMIN` using the existing authenticated admin flow if needed.

## Operations

From `fitora_backend/deploy/ubuntu`:

```bash
docker compose --env-file .env -f compose.yml logs -f gateway auth user interact chat notification
docker compose --env-file .env -f compose.yml restart gateway auth user interact chat notification
sudo systemctl status fitora-cert-renew.timer
bash deploy.sh
```

`bash deploy.sh` is also the update command after pulling both repositories. It does not run `down -v`, prune Docker, reset Git, or recreate secrets. Do not remove the named `fitora_*` volumes unless intentionally discarding the new Ubuntu data.

For acceptance after deploy, register a normal account, create/read a post, upload a file, then open two browser sessions to verify chat/notifications. The script deliberately does not create test users or application records.
