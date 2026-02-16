namespace Notes_webapp__ASP.NET_CORE_.Dtos
{
    public class NoteListResponseDto
    {
        public List<NoteListItemDto> Items { get; set; } = new List<NoteListItemDto>();
        public int Total { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
    }
}
