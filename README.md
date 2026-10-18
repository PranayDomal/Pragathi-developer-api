# Pragathi Developers – Backend (Microservices)

Backend for the **Green Woods** website (a copy of pragathidevelopers.in).
It is split into small services behind one front door (the Gateway).

## Architecture

```
Browser (Angular, Vercel)
        │
        ▼
   Gateway (YARP)  ──►  /api/admin/login  ──►  Auth
        │             ──►  /api/admin/**    ──►  Admin  ──►  Leads (internal)
        └─────────────►  /api/**            ──►  Leads
```

| Service | What it does | Layers | Database |
|---|---|---|---|
| **Gateway** | Single public entry point. Routes requests, handles CORS. | – | – |
| **Auth** | Checks a login with Admin and issues a JWT (2 h). | none | none |
| **Admin** | Admin users and roles, plus view/edit/delete of leads for the admin panel. | Onion (Api, Application, Domain, Infrastructure) | Own Postgres DB (`AdminUsers`) |
| **Leads** | Receives website form submissions (enquiries, visits, activities, configurations). | Onion | Postgres (leads data) |

### Roles (Admin panel)

| Role | Can do |
|---|---|
| Viewer | View everything |
| Editor | View, edit and delete leads |
| Superuser | Everything above, plus manage users (add, change role, switch on/off, reset password, delete) |

A switched-off account cannot sign in and sees a clear message. Existing sessions keep working until the token expires (2 h).

### How services trust each other

- The browser talks only to the Gateway.
- **JWT**: signed by Auth, checked by Admin (`Jwt__Key` must be identical in both).
- **Service key**: Leads, Admin and Auth send each other `X-Service-Key` (`Internal__ServiceKey` must be identical in all three) so only our own services can call the internal endpoints.

## Repository layout

```
Gateway/PragathiApi.Gateway
Services/
  Admin/{Pragathi.Admin.Api, .Application, .Domain, .Infrastructure}
  Auth/PragathiApi.Auth
  Leads/{PragathiApi, PragathiApi.Application, PragathiApi.Domain, PragathiApi.Infrastructure}
```

Each service folder has its own `Dockerfile`.

## Configuration (environment variables)

Use `__` for nested settings. **Never commit real values.**

| Service | Variables |
|---|---|
| Leads | `ConnectionStrings__Default`, `Internal__ServiceKey` |
| Admin | database connection string, `Jwt__Key`, `Internal__ServiceKey`, `Services__Leads__BaseUrl`, `Seed__SuperuserId`, `Seed__SuperuserPassword` |
| Auth | `Jwt__Key`, `Internal__ServiceKey`, `Services__Admin__BaseUrl` |
| Gateway | `ReverseProxy__Clusters__{auth\|admin\|leads}__Destinations__d1__Address`, `Cors__Origins__0` (frontend URL) |
| All on Render | `PORT=8080`, `ASPNETCORE_FORWARDEDHEADERS_ENABLED=true` (Auth, Gateway, Leads) |

On first start, Admin applies its migrations and creates the first Superuser from the `Seed__...` values when no users exist.

## Run locally

1. Set the values above with `dotnet user-secrets` (or `appsettings.Development.json`, which is git-ignored).
2. Start Leads, Admin, Auth and Gateway (Visual Studio multiple startup projects, or `dotnet run` in each).
3. The Gateway's local address is `http://localhost:5080`.

Health check on every service: `GET /api/health`.

## Deployment (Render, free plan)

Four web services, all Docker, branch `main`:

| Service | Root directory | Dockerfile |
|---|---|---|
| pragathi-leads | `Services/Leads` | `Services/Leads/Dockerfile` |
| pragathi-admin | `Services/Admin` | `Services/Admin/Dockerfile` |
| pragathi-auth | `Services/Auth` | `Services/Auth/Dockerfile` |
| pragathi-gateway | `Gateway` | `Gateway/Dockerfile` |

- Health check path: `/api/health` (leave empty on the Gateway so checks don't wake Leads).
- **Cold starts:** free services sleep after ~15 minutes idle and take ~50 s to wake. The first login after a quiet period can take 1 to 1.5 minutes while Gateway, Auth and Admin wake in turn.

## Tech

.NET 8, ASP.NET Core, EF Core 8 + Npgsql (Neon Postgres), YARP, JWT (HS256), Docker.