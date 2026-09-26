using Microsoft.EntityFrameworkCore;
using Notes_webapp__ASP.NET_CORE_.Data;
using Notes_webapp__ASP.NET_CORE_.Dtos;
using Notes_webapp__ASP.NET_CORE_.Models;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<NotesDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Notes") ?? "Data Source=notes.db"));

var app = builder.Build();
app.UseExceptionHandler(handler => handler.Run(async context =>
{
    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
    await context.Response.WriteAsJsonAsync(new { error = "Unable to complete the request. Please try again." });
}));
app.UseDefaultFiles();
app.UseStaticFiles();

// Apply only pending migrations; existing notes are preserved.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<NotesDbContext>();
    await db.Database.MigrateAsync();
}

app.MapGet("/api/notes", async (NotesDbContext db) =>
{
    var notes = await db.Notes.AsNoTracking().OrderBy(note => note.Id).ToListAsync();
    return new NoteListResponseDto
    {
        Items = notes.Select(ToDto).ToList(),
        Total = notes.Count,
        Page = 1,
        PageSize = Math.Max(10, notes.Count)
    };
});

app.MapGet("/api/notes/{id:int}", async (int id, NotesDbContext db) =>
{
    var note = await db.Notes.AsNoTracking().FirstOrDefaultAsync(note => note.Id == id);
    return note is null ? Results.NotFound() : Results.Ok(ToDto(note));
});

app.MapPost("/api/notes", async (NoteCreateDto dto, NotesDbContext db) =>
{
    var error = ValidateNote(dto.Title, dto.Content, dto.Tag);
    if (error is not null) return Results.BadRequest(error);

    var now = DateTime.UtcNow;
    var note = new Note
    {
        Title = dto.Title.Trim(),
        Content = dto.Content.Trim(),
        Tag = NormalizeTag(dto.Tag),
        CreatedAt = now,
        UpdatedAt = now
    };
    db.Notes.Add(note);
    await db.SaveChangesAsync();
    return Results.Created($"/api/notes/{note.Id}", ToDto(note));
});

app.MapPut("/api/notes/{id:int}", async (int id, NoteUpdateDto dto, NotesDbContext db) =>
{
    var note = await db.Notes.FindAsync(id);
    if (note is null) return Results.NotFound();
    var error = ValidateNote(dto.Title, dto.Content, dto.Tag);
    if (error is not null) return Results.BadRequest(error);

    note.Title = dto.Title.Trim();
    note.Content = dto.Content.Trim();
    note.Tag = NormalizeTag(dto.Tag);
    note.UpdatedAt = DateTime.UtcNow;
    await db.SaveChangesAsync();
    return Results.Ok(ToDto(note));
});

app.MapDelete("/api/notes/{id:int}", async (int id, NotesDbContext db) =>
{
    var note = await db.Notes.FindAsync(id);
    if (note is null) return Results.NotFound();
    db.Notes.Remove(note);
    await db.SaveChangesAsync();
    return Results.NoContent();
});

app.Run();

static string? ValidateNote(string? title, string? content, string? tag)
{
    if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(content))
        return "Title and content are required.";
    if (title.Length > 200 || content.Length > 20000 || tag?.Length > 50)
        return "Maximum lengths: title 200, content 20000, tag 50 characters.";
    return null;
}

static string? NormalizeTag(string? tag) => string.IsNullOrWhiteSpace(tag) ? null : tag.Trim();

static NoteListItemDto ToDto(Note note) => new(note.Id, note.Title, note.Content, note.CreatedAt, note.IsFavorite)
{
    Tag = note.Tag,
    UpdatedAt = note.UpdatedAt
};
