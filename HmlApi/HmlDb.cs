using Microsoft.EntityFrameworkCore;
public class HmlDb : DbContext
{
    public HmlDb(DbContextOptions<HmlDb> options) : base(options) { }

    public DbSet<Project> Projects { get; set; }
    public DbSet<TaskList> TaskLists { get; set; }
    public DbSet<Task> Tasks { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure Project -> TaskList relationship
        modelBuilder.Entity<Project>()
            .HasMany(p => p.TaskLists)
            .WithOne(tl => tl.Project)
            .HasForeignKey(tl => tl.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure TaskList -> Task relationship
        modelBuilder.Entity<TaskList>()
            .HasMany(tl => tl.Tasks)
            .WithOne(t => t.TaskList)
            .HasForeignKey(t => t.TaskListId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
