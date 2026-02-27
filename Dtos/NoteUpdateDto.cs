namespace Notes_webapp__ASP.NET_CORE_.Dtos
{
    public class NoteUpdateDto
    {
        public string Title { get; set; } = String.Empty;
        public string Content { get; set; } = String.Empty;
        public string? Tag { get; set; }
    }
}
