# DailyNotes: Roadmap

## Done

| Item | Notes |
|---|---|
| **Clean Architecture refactor** | `DailyNotes.Application` layer added; all controllers inject service interfaces, no direct DbContext access |
| **Provider stubs registered** | `NullEmailProvider`, `NullFileStorageProvider`, `NullAiVisionProvider`, `NullSpeechProvider` registered in DI — ready to swap for real implementations |
| **Transaction management** | `WorkNoteService.CreateAsync` (WorkDay auto-create) and `QuizAttemptService.SubmitAsync` (scoring) wrapped in DB transactions |
| **TopicNote security fix** | `GET /api/topics/{id}/notes` now correctly scopes TopicNotes by tenant + user |

---

## Third-Party Integrations

| Feature | Description |
|---|---|
| **Jira Integration** | Sync tasks/time entries with Jira issues, log work via `external_source`/`external_id` linkage |
| **Salesforce Integration** | Link notes/tasks to Salesforce cases or opportunities |
| **Open Source Tools** | GitLab, Redmine, OpenProject — same adapter pattern via `integration_connections` |
| **No-Code Automation** | Zapier, Power Automate, n8n — connect via API keys + webhooks |

## AI & Automation (MCP)

| Feature | Description |
|---|---|
| **AI / MCP Integration** | Autocompletion, summaries, plus KB: AI-generated quizzes, semantic search, gap analysis, OCR, speech-to-text |
| **Semantic Search** | pgvector embeddings for natural language queries across all content |
| **MCP Server** | Expose DailyNotes tools to AI agents: search_notes, create_note, quiz_me, summarize_topic, etc. |
| **MCP Client** | DailyNotes calls AI models for summarization, quiz generation, OCR, transcription |

## Bots & Communication

| Feature | Description |
|---|---|
| **MS Teams Bot** | Slash commands (`/dailynotes log`), meeting auto-notes, channel digests via Bot Framework |
| **Slack Bot** | Slash commands, note notifications, daily summaries via Bolt SDK |
| **Discord** | Bot commands + notifications for smaller/dev teams |
| **Zoom** | Webhook on meeting end → auto-create note with duration, attach transcript |

## Email & Extensions

| Feature | Description |
|---|---|
| **Gmail Integration** | Inbound: import emails as notes via Gmail API; Outbound: send digests; OAuth 2.0 via Google Cloud |
| **Outlook Integration** | Inbound/outbound via Microsoft Graph API (shares OAuth with Entra ID + Office 365) |
| **Browser Extension** | "Save to DailyNotes" button in Gmail/Outlook web — clips email as a work note |
| **VS Code Extension** | Sidebar panel, status bar timer, slash commands, git commit auto-logging — thin client over existing API |

## Cloud & Infrastructure

| Feature | Description |
|---|---|
| **Real provider implementations** | Swap null stubs for real `IEmailProvider` (SendGrid/SES), `IFileStorageProvider` (Azure Blob/S3), `IAiVisionProvider`, `ISpeechProvider` |
| **Microsoft Entra ID** | Replace JWT auth with Entra ID (migration path documented in `docs/architecture.md`) |
| **Google Docs** | Link/embed Google Docs via Drive API Picker, stored as cloud attachments |
| **Office 365** | Link/embed OneDrive/SharePoint docs via Microsoft Graph API |
| **Google Calendar Sync** | Two-way sync of work day time entries with Google Calendar events |
| **Capacitor** | Native iOS/Android shell for app store distribution + full native APIs |
| **GitHub Actions CI** | Automated build, test, Docker publish on push/PR |

## Knowledge Base & Education

| Feature | Description |
|---|---|
| **Spaced Repetition** | Auto-resurface quiz questions based on forgetting curves |
| **Grade Calculator** | Weighted GPA tracking, what-if grade scenarios |
| **Study Planner** | AI-suggested study blocks based on due dates + quiz performance |
| **Flashcards** | AI-generated flashcards from topic notes via MCP |
| **LMS Integration** | Google Classroom, Canvas, Blackboard — sync courses/assignments |
| **LinkedIn Learning** | OAuth sync: enrolled courses, completion %, certificates via LinkedIn API |
| **Udemy** | Sync enrolled courses + progress via Udemy API |
| **Coursera** | Course catalog + completion via partner API |
| **Pluralsight** | Skill IQ scores, course history, skill assessments |

## Advanced Capture & Sync

| Feature | Description |
|---|---|
| **OCR** | Azure AI Vision / Google Cloud Vision — handwriting → searchable text |
| **Speech-to-Text** | Azure Speech / Whisper API — voice notes → transcribed text |
| **Live Dictation** | Real-time transcription while speaking → text in Lexical |
| **reMarkable Sync** | Import handwritten pages via reMarkable Cloud API → OCR |
| **OneNote Sync** | Two-way sync via Microsoft Graph API |

## Additional Core Features

| Feature | Description |
|---|---|
| **Full-text search (PostgreSQL FTS)** | Replace in-memory JSON content filtering with `tsvector` GIN indexes (schema ready; `SearchService` needs updating) |
| **Soft deletes** | Add `IsDeleted` + `DeletedAt` to entities; update services to filter deleted records |
| **API versioning** | Add `/api/v1/` prefix; version service interfaces |
| **FluentValidation** | Add request validators in `DailyNotes.Application/Validators/` |
| **Request/Response DTOs** | Decouple API contract from EF entities; add mapping layer in Application |
| **Export** | CSV/Excel export by date range, project, task |
| **Monthly Goals** | Restore deferred Monthly Goals & Monthly Goal Tasks tables |
| **Developer Portal** | API key management, Swagger docs, webhook subscription UI |
| **Push Notifications** | Web Push (VAPID) + Capacitor Push plugin for native |
