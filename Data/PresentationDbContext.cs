using Microsoft.EntityFrameworkCore;

namespace PresentationMeetup.Data;

public class PresentationDbContext : DbContext
{
    public PresentationDbContext(DbContextOptions<PresentationDbContext> options) 
        : base(options)
    { }

    public DbSet<Presentation> Presentations { get; set; }
    public DbSet<Slide> Slides { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }
}
