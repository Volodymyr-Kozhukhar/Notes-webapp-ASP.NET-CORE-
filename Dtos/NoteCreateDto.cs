namespace Notes_webapp__ASP.NET_CORE_.Dtos
{
    public class NoteCreateDto
    {
        public string Title { get; set; } = String.Empty;
        public string? Tag { get; set; }
    }
}
