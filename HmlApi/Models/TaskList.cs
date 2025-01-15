public class TaskList
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Order { get; set; }
    public DateOnly DueDate { get; set; } = DateOnly.FromDateTime(DateTime.Now);
    public int ProjectId { get; set; }
    public Project Project { get; set; } = null!;
    public ICollection<Task> Tasks { get; } = new List<Task>();
}
