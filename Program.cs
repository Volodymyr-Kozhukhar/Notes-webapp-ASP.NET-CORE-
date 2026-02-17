using Notes_webapp__ASP.NET_CORE_.Dtos;

const int PageSizeConst = 10;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

List<NoteListItemDto> listOfNotes = new List<NoteListItemDto>();
int nextId = 1;

app.MapPost("/api/notes", (NoteCreateDto dto) =>
{
    if (string.IsNullOrWhiteSpace(dto.Title))
        return Results.BadRequest("Title is required");

    NoteListItemDto newNote = new NoteListItemDto(nextId, dto.Title, DateTime.Now);
    listOfNotes.Add(newNote);

    nextId++;
    return Results.Ok(newNote);
});



app.MapGet("/api/notes", () => {
    NoteListResponseDto responceObject = new NoteListResponseDto();
    responceObject.Items = listOfNotes;
    responceObject.Total = listOfNotes.Count;
    responceObject.PageSize = PageSizeConst;
    responceObject.Page = 1;

    return responceObject;
});

app.Run();
