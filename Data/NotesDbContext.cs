using Microsoft.EntityFrameworkCore;
using Notes_webapp__ASP.NET_CORE_.Models;

namespace Notes_webapp__ASP.NET_CORE_.Data
{
    public class NotesDbContext: DbContext
    {
        public NotesDbContext(DbContextOptions<NotesDbContext> options): base(options)
        {

        }

        public DbSet<Note> Notes { get; set; }
    }
}
