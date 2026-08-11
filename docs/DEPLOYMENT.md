# Deployment

FinanceOS will be deployed automatically to the existing virtual machine that runs k3s.

## Current VM baseline

The VM already contains:

- `kube-system`
  - Traefik
  - CoreDNS
  - metrics-server
  - local-path-provisioner
- `cert-manager`
- `default`
  - `immopredict-api`
  - `immopredict-client`
- `vectis`
  - `vectis-web`
  - `vectis-db` PostgreSQL 16.14
  - PostgreSQL backup CronJob

FinanceOS must be isolated from the existing workloads in its own namespace.

## Deployment target

The deployment foundation is:

- GitHub Actions validates backend and web builds.
- A separate deployment workflow runs after the CI workflow succeeds on `main`.
- The deployment workflow can also be started manually from GitHub Actions with **Run workflow**.
- GitHub Actions builds and pushes Docker images to GitHub Container Registry.
- GitHub Actions connects to the VM over SSH.
- GitHub Actions sends only the Kubernetes manifests to a temporary folder on the VM.
- k3s applies the Kubernetes manifests from that temporary folder.
- k3s updates the running deployments to the exact commit image tag.
- `financeos-finance-api`, `financeos-budget-api` and `financeos-notification-api` are deployed when their Kubernetes secrets and backing PostgreSQL/RabbitMQ services are available.

## Required GitHub secrets

Configure these repository secrets before enabling real deployment:

- `VM_HOST`: public IP address or DNS name of the virtual machine.
- `VM_USER`: SSH user used for deployment.
- `VM_SSH_KEY`: private SSH key authorized on the VM.
- `VM_SSH_PORT`: optional SSH port. Defaults to `22` when absent.

The workflow uses `GITHUB_TOKEN` to push images to GHCR.

`VM_HOST` must contain only the host, for example `203.0.113.10` or `server.example.com`. Do not include `ssh://`, `user@`, or a port in `VM_HOST`; use `VM_USER` and `VM_SSH_PORT` for those.

## VM prerequisites

The VM must have:

- Git
- k3s with `kubectl` available through passwordless sudo for the deployment user
- access to GitHub Container Registry for FinanceOS images
- Traefik and cert-manager kept as shared cluster services

If GHCR packages remain private, create a pull secret in the `financeos` namespace:

```bash
kubectl -n financeos create secret docker-registry ghcr-pull-secret \
  --docker-server=ghcr.io \
  --docker-username="$GITHUB_USER" \
  --docker-password="$GHCR_TOKEN"
```

## Deployment command

The deployment workflow can run automatically after `ci`, or manually from GitHub Actions. It runs:

```bash
tar -czf financeos-k8s.tar.gz infrastructure/k8s
scp financeos-k8s.tar.gz "$VM_USER@$VM_HOST:/tmp/financeos-k8s.tar.gz"
ssh "$VM_USER@$VM_HOST"
rm -rf /tmp/financeos-deploy
mkdir -p /tmp/financeos-deploy
tar -xzf /tmp/financeos-k8s.tar.gz -C /tmp/financeos-deploy
sudo -n kubectl apply -k /tmp/financeos-deploy/infrastructure/k8s/overlays/production
sudo -n kubectl -n financeos set image deployment/financeos-gateway gateway=ghcr.io/tghrayt/finance-os/gateway:$IMAGE_TAG
sudo -n kubectl -n financeos set image deployment/financeos-finance-api finance-api=ghcr.io/tghrayt/finance-os/finance-api:$IMAGE_TAG
sudo -n kubectl -n financeos set image deployment/financeos-budget-api budget-api=ghcr.io/tghrayt/finance-os/budget-api:$IMAGE_TAG
sudo -n kubectl -n financeos set image deployment/financeos-notification-api notification-api=ghcr.io/tghrayt/finance-os/notification-api:$IMAGE_TAG
sudo -n kubectl -n financeos set image deployment/financeos-web web=ghcr.io/tghrayt/finance-os/web:$IMAGE_TAG
sudo -n kubectl -n financeos rollout status deployment/financeos-gateway
sudo -n kubectl -n financeos rollout status deployment/financeos-finance-api
sudo -n kubectl -n financeos rollout status deployment/financeos-budget-api
sudo -n kubectl -n financeos rollout status deployment/financeos-notification-api
sudo -n kubectl -n financeos rollout status deployment/financeos-web
rm -rf /tmp/financeos-deploy /tmp/financeos-k8s.tar.gz
```

If the deployment user cannot use passwordless sudo, allow only kubectl for that user:

```bash
echo "$USER ALL=(root) NOPASSWD: /usr/local/bin/kubectl, /usr/bin/kubectl" | sudo tee /etc/sudoers.d/financeos-deploy
sudo chmod 440 /etc/sudoers.d/financeos-deploy
```

## Ingress

Traefik and cert-manager are already present on the VM. FinanceOS is exposed through:

```text
https://financeos.51-210-40-78.sslip.io
```

The production Ingress uses the `letsencrypt-http` ClusterIssuer and stores the certificate in the `financeos-tls` secret.

The production overlay also keeps `infrastructure/k8s/overlays/production/ingress.example.yaml` as a reference for future custom domains.

Public API traffic is routed through the gateway under `/api`. Finance endpoints are exposed as:

```text
https://financeos.51-210-40-78.sslip.io/api/v1/finance/...
```

Budget endpoints are exposed as:

```text
https://financeos.51-210-40-78.sslip.io/api/v1/budget/...
```

Notification endpoints are exposed as:

```text
https://financeos.51-210-40-78.sslip.io/api/v1/notification/...
```

## Finance API Kubernetes secrets

Before deploying `financeos-finance-api`, create the runtime secret in the VM cluster. Replace the values with the actual PostgreSQL and RabbitMQ endpoints used by the `financeos` namespace.

```bash
sudo kubectl create namespace financeos --dry-run=client -o yaml | sudo kubectl apply -f -

sudo kubectl -n financeos create secret generic financeos-finance-api-secrets \
  --from-literal=finance-database='Host=POSTGRES_HOST;Port=5432;Database=financeos;Username=financeos;Password=CHANGE_ME' \
  --from-literal=rabbitmq-host='RABBITMQ_HOST' \
  --from-literal=rabbitmq-username='guest' \
  --from-literal=rabbitmq-password='CHANGE_ME' \
  --dry-run=client -o yaml | sudo kubectl apply -f -
```

`Finance__ApplyMigrationsOnStartup=true` is configured in the deployment so the initial `finance` schema migration can be applied automatically when the service starts.

`financeos-budget-api` currently reuses the same PostgreSQL connection secret and applies a separate EF Core `budget` schema with `Budget__ApplyMigrationsOnStartup=true`.

`financeos-notification-api` also reuses the same PostgreSQL connection secret for now and applies a separate EF Core `notification` schema with `Notification__ApplyMigrationsOnStartup=true`. It consumes RabbitMQ events emitted by Budget Service and stores in-app notifications.

This will be refined later with dedicated database/RabbitMQ manifests or managed services, rollback and deeper health verification.
