public class Project
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateOnly DueDate { get; set; } = DateOnly.FromDateTime(DateTime.Now);
    public string Color { get; set; } = "#FFFFFF";

    // Navigation property: A project can have multiple task lists
    public List<TaskList> TaskLists { get; set; } = new List<TaskList>();
}
