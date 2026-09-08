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

The `production` Environment in both repositories permits only the `develop` branch. It has no reviewer rule because no independent reviewer was selected; this is not a substitute for the protected-branch PR review. Configure the following **production Environment** values only after a GitHub-hosted runner route has been verified:

| Name | Kind | Purpose | Source |
| --- | --- | --- | --- |
| `FITORA_DEPLOY_HOST` | Variable | SSH hostname/IP reachable by GitHub-hosted runner | Verified public, bastion, or private-network route |
| `FITORA_DEPLOY_PORT` | Variable | SSH port for that host | Verified route |
| `FITORA_DEPLOY_USER` | Variable | Restricted Ubuntu deploy user | Ubuntu SSH configuration |
| `FITORA_DEPLOY_SSH_KEY` | Secret | Private key for a restricted deploy user | Dedicated deploy key; never copy server `.env` here |
| `FITORA_DEPLOY_KNOWN_HOSTS` | Secret | Verified host-key entry for the actual host/port | Existing trusted `known_hosts`, verified through an administrator session |
| `FITORA_DEPLOY_ROOT` | Repository variable | `/home/fitdnu/fitora` | Ubuntu deployment inspection |
| `FITORA_TAILSCALE_CLIENT_ID` | Secret | Tailscale GitHub workload-identity client ID | Tailscale Trust credentials |
| `FITORA_TAILSCALE_AUDIENCE` | Secret | Audience for that federated identity | Tailscale Trust credentials |

`FITORA_DEPLOY_HOST` is the verified Tailscale IPv4 address or MagicDNS hostname of Ubuntu, never its LAN/NAT address. The CD jobs join Tailscale only after `verify`, as ephemeral `tag:fitora-ci` nodes, using pinned `tailscale/github-action` commit `306e68a486fd2350f2bfc3b19fcd143891a4a2d8`. They request GitHub OIDC only in `preflight` and `deploy` jobs; the CI workflows receive no `id-token: write` permission. The action's post-step logs out the runner.

For this GitHub account, both repositories use the default, non-immutable OIDC subject format. Because the CD jobs reference `production`, the tailnet federated-identity trust subjects must be exactly:

```text
repo:HuyHoangDevVN/fitora_backend:environment:production
repo:HuyHoangDevVN/fitora_ui:environment:production
```

The tailnet administrator must also require `repository` to match the corresponding repository and `ref` to be `refs/heads/develop`, then verify those claims from the first successful preflight. Give the federated identities only `auth_keys` scope and `tag:fitora-ci`. The effective tailnet policy must allow only `tag:fitora-ci` to `tag:fitora-server:22`; do not rely on this rule while an older broad allow rule remains in force.

The workflow refuses an unexpected deploy root, an invalid port, an absent secret, an unknown host key, or an incomplete archive. It does not use `ssh-keyscan` or disable host-key checking.

## One-time Ubuntu bootstrap

After these files are reviewed, bootstrap the server-side entrypoint once through the trusted administrator SSH session:

```bash
cd /home/fitdnu/fitora/fitora_backend
bash deploy/ubuntu/bootstrap-ci.sh
```

Do not create a GitHub Actions runner on the application server. Tailscale is the only GitHub-runner route: Ubuntu is a fixed `tag:fitora-server` node and each CD job is an ephemeral `tag:fitora-ci` node. Do not expose Docker/database ports, enable Tailscale SSH, advertise routes, or use an exit node. The tailnet administrator must create the tags, ACL and GitHub federated identities; this repository does not alter tailnet-wide policy automatically.

Ubuntu accepts the dedicated `fitora-deploy` account/key for CI. It has no sudo permission, has Docker access required by the existing Compose deployment, and receives ACLs only for the Fitora source root, `incoming`, `releases`, the runtime `.env`, and deployment entrypoints. Its SSH key disables agent, port and X11 forwarding plus PTY allocation. Component deployments do not require sudo; only the one-time `--component all` bootstrap of the host Nginx/TLS integration does.

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

## Deferred dependency security item

AutoMapper `13.0.1` is a direct dependency of Auth, User, Notification, Interact, and Chat. NuGet reports `GHSA-rvv3-g6hj-g44x` / CVE-2026-32933 (high severity uncontrolled-recursion DoS); affected versions are `<15.1.1`, while the available patched lines are `15.1.1` and `16.1.1`. This CI/CD change does not suppress the warning or make a major package upgrade. A separate compatibility PR must inventory request/response graph mappings, select a supported patched version and license, add regression coverage for nested/cyclic input, and verify all affected services before a security sign-off.
