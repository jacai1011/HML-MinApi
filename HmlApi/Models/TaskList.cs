public class TaskList
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Order { get; set; }
    public int ProjectId { get; set; }
    public Project Project { get; set; } = null!;
    
    // Navigation property: A task list can have multiple tasks
    public List<Task> Tasks { get; set; } = new List<Task>();
}
