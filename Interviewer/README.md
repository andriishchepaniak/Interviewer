# Interviewer

Web application for running **technical interviews**: generate question plans with AI (Google Gemini), conduct structured sessions with scoring, view analytics, and produce AI-written summaries plus copy-ready email feedback.

The UI is primarily in **Ukrainian**.

The solution ships **two hosts** that share the same Data / Infrastructure / Services layers:

- **`Interviewer`** — the Blazor Web App (server-rendered UI).
- **`InterviewApi`** — a standalone ASP.NET Core REST API (JSON + streaming) intended for an external client (CORS is open for a Flutter web app).

## Features

- **Dashboard** (`/`) — list interviews with status (Created / In Progress / Completed), sorted by date.
- **New interview** (`/new`) — candidate name, position, optional CV text; generate an AI plan or start from a manual template.
- **Review / draft** (`/review`) — edit topics, questions, and hints before saving to the database.
- **Live interview** (`/interview/{id}`) — topic navigation, 0–3 scores (or skip), optional comments; progress persisted to MongoDB.
- **Report** (`/report/{id}`) — numeric analytics, per-topic averages, strengths/weaknesses, email-style summary, streaming **AI report** (HTML).

## Tech stack

| Area | Choice |
|------|--------|
| Runtime | .NET 10 (`net10.0`) |
| UI | Blazor Web App, **Interactive Server** render mode |
| Styling | Tailwind CSS v4 (CLI build step) |
| Database | MongoDB (`MongoDB.Driver`) |
| AI | Google Gemini via `Google.GenAI` SDK |

## Solution structure

```
Interviewer/                 — Blazor host (UI, `Program.cs`, state)
InterviewApi/                — ASP.NET Core REST API host (controllers, OpenAPI, CORS)
Interviewer.Data/            — Models, `IInterviewRepository`, MongoDB repository, settings
Interviewer.Infrastructure/ — `IAIService`, `GeminiAIService`
Interviewer.Services/        — `InterviewGeneratorService` (prompts, JSON parse, streaming feedback)
```

> **Note:** `Interviewer.Core` exists in the repo folder but is **not** included in `Interviewer.slnx` and is currently an empty class library.

## REST API (`InterviewApi`)

A separate ASP.NET Core host exposing the same Data/Services layers over HTTP. It enables OpenAPI in Development and applies an **open CORS policy** (`AllowAnyOrigin/Header/Method`, named `AllowFlutterWeb`) for a browser client during local dev.

Base route: `api/interviews` (`InterviewsController`).

| Method | Route | Purpose |
|--------|-------|---------|
| `GET` | `/api/interviews` | List all interviews. |
| `GET` | `/api/interviews/{id}` | Get one interview (404 if missing). |
| `POST` | `/api/interviews` | Create an interview (server assigns a Guid `Id`, overriding any supplied value). |
| `PUT` | `/api/interviews/{id}` | Update an interview (404 if missing). |
| `DELETE` | `/api/interviews/{id}` | Delete an interview (404 if missing). |
| `POST` | `/api/interviews/plan` | Generate an AI plan from `{ Position, CvText }`. |
| `POST` | `/api/interviews/feedback/stream` | Stream AI feedback as `text/plain` chunks for an `Interview` body. |

Default dev URLs (see `InterviewApi/Properties/launchSettings.json`): `https://localhost:7254` / `http://localhost:5195`.

> Unlike the Blazor host (which registers `IInterviewRepository`/`InterviewState` as singletons), the API registers `IInterviewRepository`, `IAIService`, and `IInterviewGeneratorService` as **scoped**.

## Data model (high level)

- **Interview** — `Id`, `CandidateName`, `Position`, `CvText`, `Topics`, `CreatedAt`, `FinalAiReport` (optional).
- **Topic** — `Id`, `Title`, `Questions`.
- **Question** — `Text`, `Hint`, `Score` (`null` = untouched, `-1` = skipped, `0–3` = score), `Comment`.

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [MongoDB](https://www.mongodb.com/) (local or Atlas)
- [Node.js](https://nodejs.org/) (for Tailwind CLI during build)
- Google Gemini **API key**

## Configuration

### User secrets (recommended for local dev)

Both hosts (`Interviewer` and `InterviewApi`) have a `UserSecretsId`; set secrets per project as needed. Configure:

- **`GeminiApiKey`** — Gemini API key (read by `GeminiAIService`).
- **`MongoDb`** section — bind to `MongoDbSettings`:

```json
{
  "MongoDb": {
    "ConnectionString": "mongodb://localhost:27017",
    "DatabaseName": "interviewer",
    "CollectionName": "interviews"
  },
  "GeminiApiKey": "your-key-here"
}
```

Use either `appsettings.Development.json` (do not commit secrets) or:

```bash
dotnet user-secrets set "MongoDb:ConnectionString" "mongodb://..." --project Interviewer
dotnet user-secrets set "MongoDb:DatabaseName" "interviewer" --project Interviewer
dotnet user-secrets set "MongoDb:CollectionName" "interviews" --project Interviewer
dotnet user-secrets set "GeminiApiKey" "your-key" --project Interviewer
```

### Tailwind

From the `Interviewer` project directory:

```bash
npm install
```

The `.csproj` runs Tailwind before each build:

`npx tailwindcss -i ./wwwroot/css/input.css -o ./wwwroot/css/output.css`

## Run the application

From the repository root:

```bash
dotnet restore
dotnet build

# Blazor UI host
dotnet run --project Interviewer

# REST API host (separate terminal)
dotnet run --project InterviewApi
```

Open the URL shown in the console (typically `https://localhost:5xxx`). The API serves OpenAPI at `/openapi/v1.json` in Development.

## AI models

`GeminiAIService` uses the `gemini-3-flash-preview` model identifier. If Google renames or deprecates models, update `Interviewer.Infrastructure/Implementation/GeminiAIService.cs`.

## Security notes

- **Do not** commit API keys or Mongo connection strings.
- AI-generated HTML is rendered with `MarkupString`; treat untrusted model output with care in production (see [IMPROVEMENTS.md](./IMPROVEMENTS.md)).
- `InterviewApi` uses a wide-open CORS policy (`AllowAnyOrigin`) and no authentication — intended for local dev only. Restrict origins and add auth before exposing it.

## Known gaps (quick reference)

- Dock links to **`/settings`**, but no settings page is implemented.
- **`UseStatusCodePagesWithReExecute("/not-found")`** is configured without a matching Blazor route (see improvements doc).
- **`InterviewState`** is registered as a **singleton**; it is not suitable for multi-user production without redesign.

For a full list of product and technical recommendations and a **roadmap plan**, see [IMPROVEMENTS.md](./IMPROVEMENTS.md).

## License

Specify your license here if you publish the repository.
