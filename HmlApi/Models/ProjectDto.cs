public class ProjectDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateOnly DueDate { get; set; }
    public string Color { get; set; } = string.Empty;
    public List<TaskListDto> TaskLists { get; set; } = new();
    public ProjectDto() { }
    public ProjectDto(Project project)
        : this()
    {
        Id = project.Id;
        Name = project.Name;
        DueDate = project.DueDate;
        Color = project.Color;

        TaskLists.AddRange(project.TaskLists.Select(tl => new TaskListDto(tl)).ToList());
    }
}