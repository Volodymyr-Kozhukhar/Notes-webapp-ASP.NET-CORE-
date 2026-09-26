# StudyNotes

A small local notes app built with ASP.NET Core 8 Minimal APIs, Entity Framework Core, SQLite, and plain JavaScript.

## Features

- Create, list, edit, and delete notes.
- Optional tags, including editing and clearing tags.
- Persistent SQLite storage and automatic application of pending EF Core migrations on startup.
- Required-field and length validation: title 200, content 20,000, tag 50 characters.
- Loading/empty states, request errors, cancel editing, and delete confirmation.
- Note text is rendered as text, not HTML. Timestamps for new changes use UTC.

## Run

Install a compatible .NET SDK and the .NET 8 ASP.NET Core runtime. From this directory:

```powershell
dotnet restore
dotnet run --no-restore --no-launch-profile -- --urls http://127.0.0.1:5265
```

Open http://127.0.0.1:5265. Stop with Ctrl+C. The app creates `notes.db` in the working directory and preserves it between runs. Keep this database private and back it up while the app is stopped. Existing migrations are retained; no database reset is required.

For a separate database, set `ConnectionStrings__Notes` to `Data Source=another-path.db`, or pass `--ConnectionStrings:Notes="Data Source=another-path.db"` after `--` in the run command.

If the local NuGet cache has missing DLL files, restore to a separate project cache with `dotnet restore --packages obj/nuget-packages --force`, then use the run command above.

## Verification

With PowerShell 7, after restoring dependencies:

```powershell
./tests/smoke.ps1
```

The script builds the project and starts it on a temporary loopback port with a separate temporary database. It checks CRUD, tags, invalid input, missing notes, static HTML, and persistence after restarting the server. It stops its server and removes its temporary files. It does not modify the normal `notes.db`.

Browser check: add a tagged note, edit it, click Edit again and confirm the latest values appear, cancel editing, reload the page, then delete the note. Stop the server and try Reload notes to check the error message.

## API

| Method | Path | Result |
| --- | --- | --- |
| GET | `/api/notes` | `{ items, total, page, pageSize }` |
| GET | `/api/notes/{id}` | One note, or 404 |
| POST | `/api/notes` | Create; returns 201 with a Location header |
| PUT | `/api/notes/{id}` | Update; returns 200 or 404 |
| DELETE | `/api/notes/{id}` | Delete; returns 204 or 404 |

POST and PUT accept `{ "title": "Example", "content": "Note text", "tag": "study" }`. Tag is optional. Invalid fields return 400. The list returns all notes in creation order in one page; `pageSize` is at least 10 and grows to fit the list.

## Scope

This is a single-user local portfolio app. There are no accounts or access controls; anyone with network access to the server can change its notes. Keep it bound to loopback. Authentication, public deployment, pagination, search, and a favorites interface are outside this version.
