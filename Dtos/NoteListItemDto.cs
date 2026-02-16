namespace Notes_webapp__ASP.NET_CORE_.Dtos
{
    public class NoteListItemDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = String.Empty;
        public string? Tag { get; set; }
        public bool IsFavorite { get; set; } = false;
        public DateTime CreatedAt { get; set; }

        public NoteListItemDto(int id, string desireableTitle, DateTime time, bool isFavourite = false)
        {
            Id = id;
            Title = desireableTitle;
            IsFavorite = isFavourite;
            CreatedAt = time;
        }
    }
}
