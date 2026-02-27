namespace Notes_webapp__ASP.NET_CORE_.Dtos
{
    public class NoteListItemDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = String.Empty;
        public string Content { get; set; } = String.Empty;
        public string? Tag { get; set; }
        public bool IsFavorite { get; set; } = false;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public NoteListItemDto(int id, string desireableTitle, string desireableContent, DateTime time, bool isFavourite = false)
        {
            Id = id;
            Title = desireableTitle;
            Content = desireableContent;
            IsFavorite = isFavourite;
            CreatedAt = time;
            UpdatedAt = time;

        }
    }
}
