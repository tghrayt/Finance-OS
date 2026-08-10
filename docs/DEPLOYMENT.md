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

## Phase 0 target

The deployment foundation is:

- GitHub Actions validates backend and web builds.
- A separate deployment workflow runs after the CI workflow succeeds on `main`.
- GitHub Actions builds and pushes Docker images to GitHub Container Registry.
- GitHub Actions connects to the VM over SSH.
- The VM pulls the latest repository state.
- k3s applies the Kubernetes manifests from `infrastructure/k8s`.
- k3s updates the running deployments to the exact commit image tag.

No production business feature is deployed in Phase 0.

## Required GitHub secrets

Configure these repository secrets before enabling real deployment:

- `VM_HOST`: public IP address or DNS name of the virtual machine.
- `VM_USER`: SSH user used for deployment.
- `VM_SSH_KEY`: private SSH key authorized on the VM.
- `VM_APP_PATH`: absolute path of the FinanceOS checkout on the VM.

The workflow uses `GITHUB_TOKEN` to push images to GHCR.

## VM prerequisites

The VM must have:

- Git
- k3s with `kubectl` available
- access to the FinanceOS GitHub repository
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

The deployment workflow runs:

```bash
cd "$VM_APP_PATH"
git fetch origin main
git reset --hard origin/main
kubectl apply -k infrastructure/k8s/overlays/production
kubectl -n financeos set image deployment/financeos-gateway gateway=ghcr.io/tghrayt/finance-os/gateway:$IMAGE_TAG
kubectl -n financeos set image deployment/financeos-web web=ghcr.io/tghrayt/finance-os/web:$IMAGE_TAG
kubectl -n financeos rollout status deployment/financeos-gateway
kubectl -n financeos rollout status deployment/financeos-web
```

## Ingress

Traefik and cert-manager are already present on the VM. The production overlay contains an example ingress at `infrastructure/k8s/overlays/production/ingress.example.yaml`.

Do not apply it as-is. Rename it to `ingress.yaml`, replace `financeos.example.com` with the real domain, choose the real cert-manager ClusterIssuer, and add it to the production `kustomization.yaml`.

This will be refined later with environment-specific secrets, database migrations, rollback and deeper health verification.
