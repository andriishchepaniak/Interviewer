# Interviewer — improvement proposals and roadmap

This document complements [README.md](./README.md). It captures **product** ideas, **technical / architecture** changes, and a **phased plan** you can execute in order.

---

## Executive summary

The app already delivers a clear flow: **create → edit plan → run interview → report**. The highest-impact next steps are: **fix correctness bugs in navigation and status**, **make server state safe for multiple users**, **persist the AI report** (the `FinalAiReport` field is unused), and **harden AI + HTML handling** before any public deployment.

---

## Product (business / UX) improvements

### Interview lifecycle and clarity

1. **True “completed” state**  
   Today, dashboard “Completed” relies on `FinalAiReport`, but nothing in the UI writes that field. Define completion explicitly: e.g. “Mark interview complete” and/or auto-complete when AI summary is generated and saved.

2. **Templates page** (`/templates`)  
   The nav dock points here, but the page is a stub. Options: saved question banks per role, company-wide templates, import/export JSON, duplicate from past interview.

3. **Settings page** (`/settings`)  
   Either implement it (API key override for admins, default language, scoring rubric text) or remove the nav item to avoid dead ends.

4. **Candidate / GDPR**  
   CV text and scores are sensitive. Add a short privacy notice, optional retention policy, and “delete interview” from the dashboard.

5. **Collaboration**  
   Multiple interviewers per candidate, shared notes, or read-only links for hiring managers.

6. **Localization**  
   Mix of Ukrainian UI and English score labels (“Wrong”, “Strong Hire”). Pick primary languages and align copy.

7. **Offline / resilience**  
   When Gemini fails, show actionable errors (not only empty lists). Offer “retry” or “continue with manual plan.”

8. **Exports**  
   PDF report, shareable link, or ATS-friendly summary beyond clipboard email text.

### Differentiation

9. **Structured rubrics**  
   Let users define weights per topic or required bar for “hire.”

10. **Question bank analytics**  
    Track which questions discriminate well across candidates (longer-term, needs volume).

---

## Technical and architecture improvements

### Critical (before multi-user or production)

1. **`InterviewState` lifetime**  
   Registered as `Singleton`, so all users share one in-memory interview. Switch to **scoped** state per circuit, or **eliminate long-lived state** and load everything from the repository by `id` on each page (recommended for Blazor Server at scale).

2. **Persist streamed AI report**  
   When `GenerateFeedbackAsync` finishes, save HTML (or sanitized HTML) to `Interview.FinalAiReport` and `UpdateAsync`. This fixes dashboard status and lets users reopen reports without regenerating.

3. **Report page: back navigation**  
   The “back to interview” control navigates to `/interview` without `{Id}`; it should use `State.Current.Id` (e.g. `/interview/{id}`).

4. **HTML from AI (`MarkupString`)**  
   Risk of XSS if the model returns scripts or event handlers. Sanitize with a trusted library (e.g. **HtmlSanitizer**) or render as plain text / markdown compiled safely.

5. **Error handling and logging**  
   Replace `Console.WriteLine` in services and pages with `ILogger<T>`. Surface user-visible messages for repository and AI failures.

### Configuration and ops

6. **Document `MongoDb` + `GeminiApiKey` in README**  
   Already started in README; keep in sync with any env var names for containers.

7. **Model name configuration**  
   Move Gemini model id to configuration for environment-specific overrides.

8. **Health checks**  
   Mongo connectivity + optional Gemini ping for deployment probes.

9. **CI pipeline**  
   `dotnet build`, optional `npm ci` + Tailwind build, static analysis.

### Code quality and structure

10. **`Interviewer.Core`**  
    Either delete the unused project or move shared DTOs / domain types there and reference from Data/Services.

11. **Repository interface location**  
    `IInterviewRepository` lives in `Interviewer.Data`; consider a thin abstraction if you want the host to depend only on Core + abstractions.

12. **JSON deserialization for AI topics**  
    Use a dedicated DTO for AI response shape vs. persistence `Topic` (decouple `Id` assignment and validation).

13. **Antiforgery / forms**  
    Review Blazor Server guidance for sensitive actions (delete, regenerate) if you add more POST-style operations.

14. **Status code / not-found**  
    `UseStatusCodePagesWithReExecute("/not-found")` — add a `NotFound.razor` route or remove re-execution path.

### Optional architectural directions

15. **Blazor WebAssembly + API**  
    If you need horizontal scale without sticky sessions, split API + WASM client.

16. **Alternative AI providers**  
    Keep `IAIService` as the seam; add OpenAI/Azure for fallback or A/B testing.

17. **Real-time**  
    SignalR hub if multiple viewers should see live scoring (niche for panel interviews).

---

## Phased implementation plan

Use this as a sequenced backlog; adjust pacing to your goals.

### Phase 1 — Correctness and trust (1–2 weeks)

| # | Task |
|---|------|
| 1.1 | Fix report → interview back link to include interview `Id`. |
| 1.2 | Persist AI report to `FinalAiReport` on completion; reload on open. |
| 1.3 | Change `InterviewState` from singleton to scoped **or** remove reliance on it across users (load by id). |
| 1.4 | Add `ILogger` and user-visible error states for AI + DB. |
| 1.5 | Add `/not-found` page or align status code middleware. |

**Exit criteria:** Multiple concurrent browser sessions do not clobber each other; completed interviews show correctly on home; no broken primary navigation.

### Phase 2 — Security and production hygiene (1–2 weeks)

| # | Task |
|---|------|
| 2.1 | Sanitize AI HTML or switch to safe rendering. |
| 2.2 | Secrets only via user secrets / Key Vault / env; sample `appsettings` without real values. |
| 2.3 | Configurable Gemini model name; rate limiting / timeout for AI calls. |
| 2.4 | Health checks + Dockerfile (optional) for deployment. |

**Exit criteria:** Safe enough for a small pilot on the public internet with real candidate data.

### Phase 3 — Product depth (2–4+ weeks)

| # | Task |
|---|------|
| 3.1 | Implement **Templates** (minimum: list + apply template on `/new`). |
| 3.2 | Implement **Settings** or remove dock link. |
| 3.3 | Delete interview + confirmation; optional data export. |
| 3.4 | Rubric weights / hire bar in report analytics. |

**Exit criteria:** Nav has no dead links; repeatable interview setup without regenerating from CV each time.

### Phase 4 — Scale and polish (ongoing)

| # | Task |
|---|------|
| 4.1 | Authentication (org/tenant isolation). |
| 4.2 | PDF export or shared read-only report link. |
| 4.3 | E2E tests (Playwright) for main flows. |
| 4.4 | Evaluate WASM + API if Blazor Server limits become binding. |

---

## Quick wins (can be done in a single day)

- Fix `/interview` back link on the report page.  
- Hide or implement `/settings` and flesh out `/templates` minimally.  
- Save `FinalAiReport` after streaming completes.  
- Register `InterviewState` as scoped.  

---

## Closing note

The layered layout (Data / Infrastructure / Services / UI) is a solid base. The biggest leap in **product truthfulness** is aligning **persistence** (`FinalAiReport`, completion rules) with what the UI promises, and fixing **server-side state** so the app behaves correctly for more than one interviewer at a time.
