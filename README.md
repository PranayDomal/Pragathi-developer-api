# Pragathi Developers API

Backend for the **Green Woods** website by Pragathi Developers (Bachupally, Hyderabad). It saves the website's enquiries, site visit requests, floor plan and brochure downloads, contact messages and chat conversations as leads, and has a protected admin API for viewing and editing them.

- **Frontend:** [Pragathi-developer](https://github.com/PranayDomal/Pragathi-developer) (Angular, hosted on Vercel)
- **API:** .NET 8 Web API (hosted on Render)
- **Database:** PostgreSQL (hosted on Neon)

## How it works

Every form on the website sends one request to `POST /api/leads`. The API:

1. Cleans the phone number to international format (India `+91` by default).
2. Finds the lead with that phone number, or creates a new one.
3. Records what the visitor did as an *activity*. A site visit also creates a *visit* with date, time slot and visitors.
4. Skips saving, and answers `alreadyReceived: true`, if the same lead repeats the same activity within 5 minutes.

Activity types: `Enquiry`, `SiteVisit`, `FloorPlanDownload`, `BrochureDownload`, `ContactMessage`, `Chat`.

## Tech stack

- .NET 8, ASP.NET Core Web API
- Entity Framework Core 8 with Npgsql (PostgreSQL)
- JWT bearer authentication for the admin API
- Built-in rate limiting
- Docker (deployed on Render)

## Project structure (onion architecture)

| Project | Purpose |
|---|---|
| `PragathiApi.Domain` | Entities and enums (Lead, Activity, Visit, Configuration) |
| `PragathiApi.Application` | DTOs, interfaces, submission logic, phone helper |
| `PragathiApi.Infrastructure` | EF Core `DbContext`, migrations, repositories |
| `PragathiApi` | Controllers, `Program.cs`, startup settings |

Dependencies point inward: API → Infrastructure → Application → Domain.

## Database tables

| Table | Contents |
|---|---|
| `Leads` | One row per person, with a unique phone number |
| `Activities` | Everything a lead did (type, message, detail, home loan answer, chosen configuration) |
| `Visits` | Site visit details for `SiteVisit` activities |
| `Configurations` | The five home types (2 BHK, 3 BHK, Sky Villa, Garden Villa, Corner Villa) |

## API endpoints

### Public

| Method | Route | Description |
|---|---|---|
| `GET` | `/api/health` | Health check, returns `{"status":"ok"}` |
| `POST` | `/api/leads` | Saves a form submission (limit: 20 requests per minute per visitor) |

### Admin (needs a JWT token)

| Method | Route | Description |
|---|---|---|
| `POST` | `/api/admin/login` | Sign in with the admin ID and password, returns a token (limit: 10 attempts per minute) |
| `GET` | `/api/admin/ping` | Checks that the token is valid |
| `GET` | `/api/admin/leads`, `/activities`, `/visits`, `/configurations` | List records, newest first |
| `PUT` | `/api/admin/leads/{id}`, `/activities/{id}`, `/visits/{id}` | Edit a record |
| `DELETE` | `/api/admin/leads/{id}`, `/activities/{id}`, `/visits/{id}` | Delete a record (deleting a lead also deletes its activities and visits) |

Send the token as `Authorization: Bearer <token>`. If the admin settings below are missing, the admin API stays switched off and returns 404. Configurations are read-only.

## Configuration

Settings are read from environment variables (use `__` for `:`) or from user-secrets when developing. Never commit real values.

| Setting | Purpose |
|---|---|
| `ConnectionStrings__Default` | PostgreSQL connection string (Npgsql format) |
| `Cors__Origins__0` | Website address allowed to call the API, with no trailing `/` (add `Cors__Origins__1`, etc. for more) |
| `Admin__Id` | Admin sign-in ID |
| `Admin__Password` | Admin sign-in password |
| `Jwt__Key` | Secret used to sign tokens (a long random value) |
| `Jwt__Issuer` | Token issuer, for example `pragathi-api` |
| `Jwt__Audience` | Token audience, for example `pragathi-admin` |
| `PORT` | Port the container listens on (`8080` on Render) |
| `ASPNETCORE_FORWARDEDHEADERS_ENABLED` | Set to `true` behind a proxy so each visitor's real address is seen |

## Run locally

Requirements: .NET 8 SDK and PostgreSQL.

```powershell
# 1. Store settings outside the code (run from the solution folder)
dotnet user-secrets set "ConnectionStrings:Default" "Host=localhost;Database=pragathi;Username=postgres;Password=YOUR_PASSWORD" --project PragathiApi
dotnet user-secrets set "Admin:Id" "admin" --project PragathiApi
dotnet user-secrets set "Admin:Password" "admin" --project PragathiApi
dotnet user-secrets set "Jwt:Key" "A_LONG_RANDOM_VALUE_AT_LEAST_32_CHARACTERS" --project PragathiApi
dotnet user-secrets set "Jwt:Issuer" "pragathi-api" --project PragathiApi
dotnet user-secrets set "Jwt:Audience" "pragathi-admin" --project PragathiApi

# 2. Create the tables (Visual Studio > Package Manager Console)
Update-Database -Project PragathiApi.Infrastructure -StartupProject PragathiApi

# 3. Run
dotnet run --project PragathiApi
```

The API listens on `http://localhost:5080`. In Development, Swagger is available at `/swagger`. By default CORS allows `http://localhost:4200` (the Angular dev server).

## Deployment

- **Database:** Neon (PostgreSQL). Apply migrations once with `Update-Database -Connection "<Npgsql connection string>"`.
- **API:** Render web service using the `Dockerfile` in this repo, region next to the database. Add the settings above as environment variables. Render redeploys on every push to `main`.
- **Health check path on Render:** `/api/health`

Note: on free hosting tiers the API sleeps when idle, so the first request can take up to a minute.

## Security notes

- Secrets live only in environment variables or user-secrets, never in the repo.
- Phone numbers are unique per lead, and duplicate submissions inside 5 minutes are ignored.
- Admin passwords are compared in constant time, tokens expire after 2 hours, and sign-in attempts are rate limited.