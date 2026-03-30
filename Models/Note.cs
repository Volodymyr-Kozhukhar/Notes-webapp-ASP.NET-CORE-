namespace Notes_webapp__ASP.NET_CORE_.Models
{
    public class Note
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;

        public string? Tag { get; set; }

        public bool IsFavorite { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}
