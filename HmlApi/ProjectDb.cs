using Microsoft.EntityFrameworkCore;
public class ProjectDb : DbContext
{
    public ProjectDb(DbContextOptions<ProjectDb> options) : base(options) { }
    public required DbSet<Project> Projects { get; set; }
    public required DbSet<TaskList> TaskLists { get; set; }
    public required DbSet<Task> Tasks { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Project -> TaskList relationship
        modelBuilder.Entity<Project>()
            .HasMany(p => p.TaskLists)
            .WithOne(tl => tl.Project)
            .HasForeignKey(tl => tl.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        // TaskList -> Task relationship
        modelBuilder.Entity<TaskList>()
            .HasMany(tl => tl.Tasks)
            .WithOne(t => t.TaskList)
            .HasForeignKey(t => t.TaskListId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
