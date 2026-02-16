using Notes_webapp__ASP.NET_CORE_.Dtos;
using System.Data;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

NoteListItemDto item1 = new NoteListItemDto(1, "Frist note", DateTime.Now);
NoteListItemDto item2 = new NoteListItemDto(2, "Second note", DateTime.Now, true);
NoteListResponseDto itemsList = new NoteListResponseDto();
itemsList.Items.Add(item1);
itemsList.Items.Add(item2);
itemsList.Total = itemsList.Items.Count;
itemsList.PageSize = 10;
itemsList.Page = 1;

app.MapGet("/api/notes", () => itemsList);

app.Run();
