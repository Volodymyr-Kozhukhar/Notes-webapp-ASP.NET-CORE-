using Microsoft.EntityFrameworkCore;
using Notes_webapp__ASP.NET_CORE_.Data;
using Notes_webapp__ASP.NET_CORE_.Dtos;

const int PageSizeConst = 10;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<NotesDbContext>(options =>
    options.UseSqlite("Data Source=notes.db"));

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

List<NoteListItemDto> listOfNotes = new List<NoteListItemDto>();
int nextId = 1;

//----------
//GET methods
//----------

//Send notes list
app.MapGet("/api/notes", () => {
    NoteListResponseDto responceObject = new NoteListResponseDto();
    responceObject.Items = listOfNotes;
    responceObject.Total = listOfNotes.Count;
    responceObject.PageSize = PageSizeConst;
    responceObject.Page = 1;

    return responceObject;
});

//------------
//POST methods
//------------

//Create new note
app.MapPost("/api/notes", (NoteCreateDto dto) =>
{
    if (string.IsNullOrWhiteSpace(dto.Title) || string.IsNullOrWhiteSpace(dto.Content))
        return Results.BadRequest("Title and Content are required");

    NoteListItemDto newNote = new NoteListItemDto(nextId, dto.Title.Trim(), dto.Content.Trim(), DateTime.Now);
    listOfNotes.Add(newNote);

    nextId++;
    return Results.Created($"/api/notes/{newNote.Id}", newNote);
});

//-----------
//PUT methods
//-----------

//Update note
app.MapPut("/api/notes/{id}", (int id, NoteUpdateDto dto) =>
{
    NoteListItemDto? updatedNote = listOfNotes.FirstOrDefault(n => n.Id == id); 
    if (updatedNote == null)
    {
        return Results.NotFound();
    }
    if (string.IsNullOrWhiteSpace(dto.Title) || string.IsNullOrWhiteSpace(dto.Content))
        return Results.BadRequest("Title and Content are required");
    updatedNote.Title = dto.Title.Trim();
    updatedNote.Content = dto.Content.Trim();

    updatedNote.Tag = string.IsNullOrEmpty(dto.Tag?.Trim()) ? null : dto.Tag?.Trim();

    updatedNote.UpdatedAt = DateTime.Now;
    return Results.Ok(updatedNote);

});

//--------------
//DELETE methods
//--------------

//Delete note
app.MapDelete("/api/notes/{id}", (int id) =>
{
    NoteListItemDto? tmp = listOfNotes.FirstOrDefault(n => n.Id == id);
    if (tmp == null)
    {
        return Results.NotFound();
    }
    listOfNotes.Remove(tmp);
    return Results.NoContent();
});

app.Run();
