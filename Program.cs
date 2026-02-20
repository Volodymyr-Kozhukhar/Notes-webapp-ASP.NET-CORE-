using Notes_webapp__ASP.NET_CORE_.Dtos;
using System.Linq;

const int PageSizeConst = 10;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

List<NoteListItemDto> listOfNotes = new List<NoteListItemDto>();
int nextId = 1;

//GET metods
app.MapGet("/api/notes", () => {
    NoteListResponseDto responceObject = new NoteListResponseDto();
    responceObject.Items = listOfNotes;
    responceObject.Total = listOfNotes.Count;
    responceObject.PageSize = PageSizeConst;
    responceObject.Page = 1;

    return responceObject;
});

//POST methods
app.MapPost("/api/notes", (NoteCreateDto dto) =>
{
    if (string.IsNullOrWhiteSpace(dto.Title))
        return Results.BadRequest("Title is required");

    NoteListItemDto newNote = new NoteListItemDto(nextId, dto.Title.Trim(), DateTime.Now);
    listOfNotes.Add(newNote);

    nextId++;
    return Results.Created($"/api/notes/{newNote.Id}", newNote);
});

//DELETE methods
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
