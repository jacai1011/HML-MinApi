
public class TaskListDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Order { get; set; }
    public DateOnly DueDate { get; set; }
    public List<TaskDto> Tasks { get; set; } = new();
    public TaskListDto() { }
    public TaskListDto(TaskList taskList)
        : this()
    {
        Id = taskList.Id;
        Name = taskList.Name;
        Order = taskList.Order;
        DueDate = taskList.DueDate;

        Tasks.AddRange(taskList.Tasks.Select(t => new TaskDto(t)).ToList());
    }
}