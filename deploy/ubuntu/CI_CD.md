# Fitora CI/CD on Ubuntu

## Flow

`feature/*` opens a PR to `develop`. CI builds and tests only; it never connects to Ubuntu. Promote with PRs from `develop` to `staging`, then `product` where that branch exists. Deployment is manual (`workflow_dispatch`) from a protected, reviewed ref. Every run defaults to the non-mutating `preflight` mode; select `deploy` only after that exact ref has passed preflight. A deploy job rebuilds/tests that exact SHA, sends a source archive to Ubuntu, and invokes one shared server script under a ten-minute `flock` lock.

Backend and frontend are deployed independently. A backend deploy builds the shared backend image, runs existing migrations once, and recreates only backend services/gateway. A frontend deploy builds/recreates only the frontend container. Named volumes, uploads, `/home/fitdnu/fitora/fitora_backend/deploy/ubuntu/.env`, and certificates are not included in archives.

## Workflows

| Repository | CI | Manual CD |
| --- | --- | --- |
| `fitora_backend` | `.github/workflows/ci.yml` on PR/push to `develop`, `staging`, `product` | `.github/workflows/deploy-ubuntu.yml` |
| `fitora_ui` | `.github/workflows/ci.yml` on PR/push to `develop`, `staging` | `.github/workflows/deploy-ubuntu.yml` |

No automatic deployment is enabled. Do not add it until a manual production deployment has passed.

## GitHub configuration

The `production` Environment in both repositories permits protected branches only. It has no reviewer rule because no independent reviewer was selected; this is not a substitute for the protected-branch PR review. Configure the following repository values only after a GitHub-hosted runner route has been verified:

| Name | Kind | Purpose | Source |
| --- | --- | --- | --- |
| `FITORA_DEPLOY_HOST` | Variable | SSH hostname/IP reachable by GitHub-hosted runner | Verified public, bastion, or private-network route |
| `FITORA_DEPLOY_PORT` | Variable | SSH port for that host | Verified route |
| `FITORA_DEPLOY_USER` | Variable | Restricted Ubuntu deploy user | Ubuntu SSH configuration |
| `FITORA_DEPLOY_SSH_KEY` | Secret | Private key for a restricted deploy user | Dedicated deploy key; never copy server `.env` here |
| `FITORA_DEPLOY_KNOWN_HOSTS` | Secret | Verified host-key entry for the actual host/port | Existing trusted `known_hosts`, verified through an administrator session |
| `FITORA_DEPLOY_ROOT` | Repository variable | `/home/fitdnu/fitora` | Ubuntu deployment inspection |

The workflow refuses an unexpected deploy root, an invalid port, an absent secret, an unknown host key, or an incomplete archive. It does not use `ssh-keyscan` or disable host-key checking.

## One-time Ubuntu bootstrap

After these files are reviewed, bootstrap the server-side entrypoint once through the trusted administrator SSH session:

```bash
cd /home/fitdnu/fitora/fitora_backend
bash deploy/ubuntu/bootstrap-ci.sh
```

Do not create a GitHub Actions runner on the application server. The GitHub-hosted runner needs a verified route to Ubuntu; it can be public SSH, an existing bastion/VPN, or a Tailscale node temporary to the workflow. Do not expose Docker/database ports. If Tailscale is used, create and scope its auth key and ACL in the existing tailnet first, then pin the current Tailscale GitHub Action commit in both CD workflows before adding its setup step. This repository does not create a tailnet, an auth key, or a network route automatically.

## First manual deployment

1. Push the reviewed workflow/script changes to the intended branch.
2. In GitHub Actions, select **Deploy backend to Ubuntu** or **Deploy frontend to Ubuntu**, choose the reviewed protected ref, leave `mode` as **preflight**, then **Run workflow**.
3. After `verify` and `preflight` pass for that ref, run it again with `mode=deploy`. The workflow prints the deployed SHA after post-deploy checks.
4. Inspect the running revisions on Ubuntu:

   ```bash
   cat /home/fitdnu/fitora/.fitora-release/backend.sha
   cat /home/fitdnu/fitora/.fitora-release/frontend.sha
   ```

## Logs and rollback

```bash
cd /home/fitdnu/fitora/fitora_backend
docker compose --env-file deploy/ubuntu/.env -f deploy/ubuntu/compose.yml logs -f gateway auth user interact chat notification frontend
```

Frontend failure automatically restores the prior source/image because it has no schema migration. Backend failure after migration stops and retains the prior source under `/home/fitdnu/fitora/releases/backend/`; it does not auto-rollback application code across an unknown schema. Review compatibility, then perform an explicit rollback using that release copy and `bash deploy/ubuntu/deploy.sh --component backend`.
