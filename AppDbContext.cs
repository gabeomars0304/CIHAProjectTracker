using Microsoft.EntityFrameworkCore;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<TrackedItem> Items => Set<TrackedItem>();
    public DbSet<ItemTask> Tasks => Set<ItemTask>();
    public DbSet<ItemTag> Tags => Set<ItemTag>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<TrackedItem>(e =>
        {
            e.HasMany(i => i.Tasks).WithOne()
            .HasForeignKey(t => t.TrackedItemId)
            .OnDelete(DeleteBehavior.Cascade);

            e.HasMany(i => i.Tags).WithMany(t => t.Items);
        });

        b.Entity<ItemTask>(e =>
        {
            e.HasMany(t => t.SubTasks).WithOne()
            .HasForeignKey(t => t.ParentId)
            .OnDelete(DeleteBehavior.ClientCascade);
        });

        b.Entity<ItemTag>(e =>
        {
            e.Property(t => t.Name).HasMaxLength(100);
            e.HasIndex(t => t.Name).IsUnique();
            e.Property(t => t.ColorHex).HasMaxLength(7);
            e.HasData(
                new ItemTag { Id = 1, Name = "Project" },
                new ItemTag { Id = 2, Name = "Action Item" },
                new ItemTag { Id = 3, Name = "Project On Hold" },
                new ItemTag { Id = 4, Name = "Issues/Blockers" });
        });
    }
}
