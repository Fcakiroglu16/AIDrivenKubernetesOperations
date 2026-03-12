# AI-Driven Kubernetes Operations

A hands-on repository demonstrating **GitOps-based deployment** of multiple ASP.NET Core microservices onto a local Kubernetes cluster using **ArgoCD**, **Docker Compose**, and **Azure DevOps** — all orchestrated with AI-assisted tooling.

---

## Table of Contents

- [Overview](#overview)
- [Architecture](#architecture)
- [Project Structure](#project-structure)
- [Prerequisites](#prerequisites)
- [Getting Started](#getting-started)
  - [1. Clone the Repository](#1-clone-the-repository)
  - [2. Build Docker Images](#2-build-docker-images)
  - [3. Deploy via ArgoCD](#3-deploy-via-argocd)
- [Services](#services)
- [Kubernetes Manifests](#kubernetes-manifests)
- [Health Checks](#health-checks)
- [ArgoCD GitOps](#argocd-gitops)
- [Azure DevOps Integration](#azure-devops-integration)
- [Security Practices](#security-practices)

---

## Overview

This project showcases how to:

- Run **6 independent ASP.NET Core (.NET 10) APIs** as containerized microservices
- Build Docker images locally with **Docker Compose**
- Deploy to a local multi-node **Kubernetes cluster** (kind / docker-desktop)
- Manage deployments declaratively with **ArgoCD** (GitOps)
- Track work with **Azure DevOps** (project: `Startup`)
- Apply **production-grade** Kubernetes best practices: HPA, liveness/readiness probes, security contexts, topology spread constraints

---

## Architecture

```
┌─────────────────────────────────────────────────────────┐
│                    GitHub Repository                     │
│               (k8s manifests — source of truth)          │
└───────────────────────────┬─────────────────────────────┘
                            │ GitOps sync (ArgoCD watches)
                            ▼
┌─────────────────────────────────────────────────────────┐
│              Local Kubernetes Cluster                    │
│    (kind — docker-desktop context, 4 nodes)             │
│                                                         │
│  ┌───────────┐  ┌───────────┐  ┌───────────┐           │
│  │ app-api   │  │ app2-api  │  │ app3-api  │  ...       │
│  │ namespace │  │ namespace │  │ namespace │           │
│  └───────────┘  └───────────┘  └───────────┘           │
│                                                         │
│  ArgoCD (argocd namespace) — syncs each app             │
└─────────────────────────────────────────────────────────┘
            ▲
            │ docker compose build
┌───────────┴─────────────────────────────────────────────┐
│              Local Docker Images                         │
│  app-api:latest  app2-api:latest  app3-api:latest  ...  │
└─────────────────────────────────────────────────────────┘
```

---

## Project Structure

```
AIDrivenKubernetesOperations/
├── App.API/              # Service 1 — ASP.NET Core (.NET 10)
├── App2.API/             # Service 2 — ASP.NET Core (.NET 10)
├── App3.API/             # Service 3 — ASP.NET Core (.NET 10)
├── App4.API/             # Service 4 — ASP.NET Core (.NET 10)
├── App5.API/             # Service 5 — ASP.NET Core (.NET 10)
├── App6.API/             # Service 6 — ASP.NET Core (.NET 10)
├── k8s/
│   ├── app2-api/         # Kubernetes manifests for App2.API
│   │   ├── namespace.yaml
│   │   ├── configmap.yaml
│   │   ├── secret.yaml
│   │   ├── deployment.yaml
│   │   ├── service.yaml
│   │   └── hpa.yaml
│   ├── app5-api/         # Kubernetes manifests for App5.API
│   └── app6-api/         # Kubernetes manifests for App6.API
├── docker-compose.yml    # Builds all service images
├── docker-compose.override.yml
└── AIDrivenKubernetesOperations.slnx
```

---

## Prerequisites

| Tool | Version | Purpose |
|------|---------|---------|
| [.NET SDK](https://dotnet.microsoft.com/download) | 10.0+ | Build & run APIs |
| [Docker Desktop](https://www.docker.com/products/docker-desktop/) | Latest | Container runtime |
| [kubectl](https://kubernetes.io/docs/tasks/tools/) | Latest | Cluster interaction |
| [kind](https://kind.sigs.k8s.io/) | Latest | Local multi-node cluster |
| [ArgoCD CLI](https://argo-cd.readthedocs.io/en/stable/cli_installation/) | Latest | Optional — UI available |

**Kubernetes cluster** must have ArgoCD installed in the `argocd` namespace:

```powershell
kubectl create namespace argocd
kubectl apply -n argocd -f https://raw.githubusercontent.com/argoproj/argo-cd/stable/manifests/install.yaml
```

**ArgoCD port-forward** (required for MCP / CLI access):

```powershell
Start-Job -ScriptBlock { kubectl port-forward svc/argocd-server -n argocd 62358:443 }
```

---

## Getting Started

### 1. Clone the Repository

```powershell
git clone https://github.com/Fcakiroglu16/AIDrivenKubernetesOperations.git
cd AIDrivenKubernetesOperations
```

### 2. Build Docker Images

Build all service images locally with Docker Compose:

```powershell
docker compose build
```

Or build a single service:

```powershell
docker compose build app2.api
```

> Images are tagged `app2-api:latest`, `app5-api:latest`, etc.  
> Kubernetes manifests use `imagePullPolicy: IfNotPresent` — no registry push required.

### 3. Deploy via ArgoCD

Apply the ArgoCD Application manifest for a service. Example for `app2-api`:

```powershell
# Create the ArgoCD Application (auto-sync enabled)
argocd app create app2-api \
  --repo https://github.com/Fcakiroglu16/AIDrivenKubernetesOperations.git \
  --path k8s/app2-api \
  --dest-server https://kubernetes.default.svc \
  --dest-namespace app2-system \
  --sync-policy automated \
  --auto-prune \
  --self-heal
```

Then trigger a sync:

```powershell
argocd app sync app2-api
```

Verify deployment:

```powershell
kubectl get pods -n app2-system
kubectl get svc -n app2-system
```

---

## Services

Each service is an ASP.NET Core Minimal API targeting **.NET 10** with:

| Service | Docker Image | Namespace | Endpoint |
|---------|-------------|-----------|----------|
| App.API | `app-api:latest` | `app-system` | `/weatherforecast` |
| App2.API | `app2-api:latest` | `app2-system` | `/weatherforecast` |
| App3.API | `app3-api:latest` | `app3-system` | `/weatherforecast` |
| App4.API | `app4-api:latest` | `app4-system` | `/weatherforecast` |
| App5.API | `app5-api:latest` | `app5-system` | `/weatherforecast` |
| App6.API | `app6-api:latest` | `app6-system` | `/weatherforecast` |

All services expose port **8080** internally (Service maps `80 → 8080`).

---

## Kubernetes Manifests

Each service under `k8s/<service-name>/` contains:

| File | Description |
|------|-------------|
| `namespace.yaml` | Dedicated namespace isolation |
| `configmap.yaml` | Non-sensitive config (`ASPNETCORE_ENVIRONMENT`, URLs, logging) |
| `secret.yaml` | Sensitive configuration (Opaque secret) |
| `deployment.yaml` | 2 replicas, rolling update, security hardening |
| `service.yaml` | ClusterIP, port 80 → 8080 |
| `hpa.yaml` | HPA: min 2 / max 10 replicas, CPU 70% / Memory 80% |

**Deployment highlights:**
- `readOnlyRootFilesystem: true` + `/tmp` emptyDir volume
- `runAsNonRoot: true`, drops ALL Linux capabilities
- `seccompProfile: RuntimeDefault`
- `TopologySpreadConstraints` — pods spread across nodes
- Zero-downtime rolling updates (`maxUnavailable: 0`, `maxSurge: 1`)
- `terminationGracePeriodSeconds: 60`

---

## Health Checks

All APIs implement Kubernetes-compatible health check endpoints:

| Endpoint | Type | Behavior |
|----------|------|----------|
| `GET /healthz/live` | Liveness | Returns `200` if process is alive (no dependency checks) |
| `GET /healthz/ready` | Readiness | Runs checks tagged `"ready"` — pod removed from Service if unhealthy |

These map directly to the Kubernetes probes in `deployment.yaml`:

```yaml
livenessProbe:
  httpGet:
    path: /healthz/live
    port: http
  initialDelaySeconds: 10
  periodSeconds: 15

readinessProbe:
  httpGet:
    path: /healthz/ready
    port: http
  initialDelaySeconds: 5
  periodSeconds: 10
```

---

## ArgoCD GitOps

This repository uses **GitOps** with ArgoCD:

- ArgoCD watches the `k8s/<service>/` path on the `main` branch
- Any `git push` automatically triggers a sync
- **Automated sync** with `prune` and `selfHeal` enabled — cluster state always matches Git
- ArgoCD is accessible locally via port-forward on `https://localhost:62358`

Cluster status:

```powershell
# Check all ArgoCD apps
kubectl get applications -n argocd

# Check pods for a specific service
kubectl get pods -n app2-system
kubectl logs -n app2-system -l app.kubernetes.io/name=app2-api --tail=50
```

---

## Azure DevOps Integration

Project tracking is managed in **Azure DevOps**:

- **Organization**: `f-cakiroglu`
- **Project**: `Startup`
- **Work item types**: Epics, Issues, Tasks
- **Current backlog**: 2 open Tasks

The MCP (Model Context Protocol) server for Azure DevOps enables AI-assisted work item management directly from the development environment.

---

## Security Practices

This project follows OWASP and Kubernetes security best practices:

- **No secrets in code** — managed via Kubernetes `Secret` objects
- **Read-only root filesystem** — prevents runtime file tampering
- **Non-root containers** — `runAsNonRoot: true`
- **Minimal capabilities** — all Linux capabilities dropped
- **Seccomp profile** — `RuntimeDefault` applied to all pods
- **Resource limits** — CPU and memory bounded to prevent noisy-neighbor issues
- **Pod topology spread** — prevents single-node failure taking down all replicas
