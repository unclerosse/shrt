using Microsoft.EntityFrameworkCore;

namespace ShRt.Data;

public class LinksContext : DbContext
{
    public LinksContext(DbContextOptions<LinksContext> options) : base(options) { }
    
    public DbSet<Models.Link>? Links { get; set; }
}