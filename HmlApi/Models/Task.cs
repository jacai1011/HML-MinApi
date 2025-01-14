public class Task
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public bool IsComplete { get; set; } = false;

    public int TaskListId { get; set; }
    public TaskList TaskList { get; set; } = null!;
}
