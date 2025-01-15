using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<ProjectDb>(opt => opt.UseInMemoryDatabase("ProjectList"));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApiDocument(config =>
{
    config.DocumentName = "ProjectAPI";
    config.Title = "ProjectAPI v1";
    config.Version = "v1";
});
var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseOpenApi();
    app.UseSwaggerUi(config =>
    {
        config.DocumentTitle = "ProjectAPI";
        config.Path = "/swagger";
        config.DocumentPath = "/swagger/{documentName}/swagger.json";
        config.DocExpansion = "list";
    });
}

RouteGroupBuilder projectItems = app.MapGroup("/projectitems");
projectItems.MapGet("/getAllProjects", GetAllProjects);
projectItems.MapGet("/getProject/{projectId}", GetProject);
projectItems.MapPost("/addProject", CreateProject);
projectItems.MapPost("/addTaskList/{projectId}", AddTaskList);
projectItems.MapPost("/addTask/{taskListId}", AddTask);
projectItems.MapDelete("/deleteTask/{taskId}", DeleteTask);
projectItems.MapDelete("/deleteTaskList/{taskListId}", DeleteTaskList);
projectItems.MapDelete("/deleteProject/{projectId}", DeleteProject);
projectItems.MapGet("/getCompleted/{projectId}", GetCompletedTaskCount);
projectItems.MapGet("/getTaskListStatus/{taskListId}", GetTaskListStatus);
projectItems.MapGet("/getProjectProgress/{projectId}", GetProjectProgress);
projectItems.MapPut("/updateTaskStatus/{taskId}", UpdateTaskStatus);
projectItems.MapPut("/reorderTaskList/{taskListId}/{reorder}", ReorderTaskList);
// updateProject
// updateTaskList

app.Run();

// Get all projects not including cascading data
static async Task<IResult> GetAllProjects(ProjectDb db)
{
    return TypedResults.Ok(await db.Projects.Select(x => new ProjectDto(x)).ToArrayAsync());
}

// Get all project data
static async Task<IResult> GetProject(int projectId, ProjectDb db)
{
    var project = await db.Projects
        .Include(p => p.TaskLists)
        .ThenInclude(tl => tl.Tasks)
        .FirstOrDefaultAsync(p => p.Id == projectId);

    if (project == null)
        return TypedResults.NotFound();

    var projectDto = new ProjectDto
    {
        Id = project.Id,
        Name = project.Name,
        DueDate = project.DueDate,
        Color = project.Color,
        TaskLists = project.TaskLists.Select(tl => new TaskListDto
        {
            Id = tl.Id,
            Name = tl.Name,
            Order = tl.Order,
            DueDate = tl.DueDate,
            Tasks = tl.Tasks.Select(t => new TaskDto
            {
                Id = t.Id,
                Title = t.Title,
                IsComplete = t.IsComplete
            }).ToList()
        }).ToList()
    };

    return TypedResults.Ok(projectDto);
}

static async Task<IResult> CreateProject(ProjectDto projectDto, ProjectDb db)
{
    var project = new Project
    {
        Name = projectDto.Name,
        DueDate = projectDto.DueDate,
        Color = projectDto.Color
    };

    db.Projects.Add(project);
    await db.SaveChangesAsync();

    projectDto = new ProjectDto(project);

    return TypedResults.Created($"/projects/{project.Id}", projectDto);
}

static async Task<IResult> AddTaskList(int projectId, TaskListDto taskListDto, ProjectDb db)
{
    var project = await db.Projects.FindAsync(projectId);
    if (project is null) return TypedResults.NotFound();

    var tasklist = new TaskList
    {
        Name = taskListDto.Name,
        Order = taskListDto.Order,
        DueDate = taskListDto.DueDate,
        ProjectId = project.Id
    };
    project.TaskLists.Add(tasklist);
    db.TaskLists.Add(tasklist);
    await db.SaveChangesAsync();

    taskListDto = new TaskListDto(tasklist);

    return TypedResults.Created($"/tasklists/{tasklist.Id}", taskListDto);
}

static async Task<IResult> AddTask(int taskListId, TaskDto taskDto, ProjectDb db)
{
    var taskList = await db.TaskLists.FindAsync(taskListId);
    if (taskList is null) return TypedResults.NotFound();

    var task = new Task
    {
        Title = taskDto.Title,
        IsComplete = taskDto.IsComplete,
        TaskListId = taskList.Id
    };
    taskList.Tasks.Add(task);
    db.Tasks.Add(task);
    await db.SaveChangesAsync();

    taskDto = new TaskDto(task);

    return TypedResults.Created($"/tasks/{task.Id}", taskDto);
}

static async Task<IResult> DeleteTask(int taskId, ProjectDb db)
{
    if (await db.Tasks.FindAsync(taskId) is Task task)
    {
        var taskList = await db.TaskLists.FindAsync(task.TaskListId);
        if (taskList is null) return TypedResults.NotFound();

        taskList.Tasks.Remove(task);
        db.Tasks.Remove(task);
        await db.SaveChangesAsync();
        return TypedResults.NoContent();
    }

    return TypedResults.NotFound();
}

static async Task<IResult> DeleteTaskList(int taskListId, ProjectDb db)
{
    if (await db.TaskLists.FindAsync(taskListId) is TaskList taskList)
    {
        var project = await db.Projects.FindAsync(taskList.ProjectId);
        if (project is null) return TypedResults.NotFound();

        project.TaskLists.Remove(taskList);
        db.TaskLists.Remove(taskList);
        await db.SaveChangesAsync();
        return TypedResults.NoContent();
    }

    return TypedResults.NotFound();
}

static async Task<IResult> DeleteProject(int projectId, ProjectDb db)
{
    if (await db.Projects.FindAsync(projectId) is Project project)
    {
        db.Projects.Remove(project);
        await db.SaveChangesAsync();
        return TypedResults.NoContent();
    }

    return TypedResults.NotFound();
}

static async Task<IResult> GetCompletedTaskCount(int projectId, ProjectDb db) 
{
    var completedTaskCount = await db.Tasks
        .Where(t => t.TaskList.ProjectId == projectId && t.IsComplete)
        .CountAsync();

    return TypedResults.Ok(completedTaskCount);
}

static async Task<IResult> GetTaskListStatus(int taskListId, ProjectDb db) 
{
    var taskCount = await db.Tasks
        .Where(t => t.TaskList.Id == taskListId)
        .CountAsync();

    if (taskCount == 0)
    {
        return TypedResults.NotFound($"No tasks found for TaskList with ID {taskListId}");
    }

    var completedTaskCount = await db.Tasks
        .Where(t => t.TaskList.Id == taskListId && t.IsComplete)
        .CountAsync();

    bool allTasksCompleted = completedTaskCount == taskCount;

    return TypedResults.Ok(allTasksCompleted);
}

static async Task<IResult> GetProjectProgress(int projectId, ProjectDb db) 
{
    var taskLists = await db.TaskLists
        .Where(tl => tl.ProjectId == projectId)
        .Include(tl => tl.Tasks)
        .ToListAsync();

    if (!taskLists.Any())
    {
        return TypedResults.NotFound($"No tasklists found for Project with ID {projectId}");
    }

    double progressPercentage = 0;
    int count = 0;

    foreach (var taskList in taskLists)
    {
        count++;
        int taskListTotalTasks = taskList.Tasks.Count;
        if (taskListTotalTasks == 0) continue;

        int taskListCompletedTasks = taskList.Tasks.Count(t => t.IsComplete);

        progressPercentage += (double)taskListCompletedTasks / taskListTotalTasks * 100;
    }

    if (count == 0 || progressPercentage == 0)
    {
        return TypedResults.Ok(0);
    }

    double totalPercentage = progressPercentage / count;

    return TypedResults.Ok(totalPercentage);
}

static async Task<IResult> UpdateTaskStatus(int taskId, ProjectDb db)
{
    var task = await db.Tasks.FindAsync(taskId);

    if (task is null) return TypedResults.NotFound();

    task.IsComplete = !task.IsComplete;

    await db.SaveChangesAsync();

    return TypedResults.NoContent();
}

static async Task<IResult> ReorderTaskList(int taskListId, int reorder, ProjectDb db)
{
    var taskList = await db.TaskLists.FindAsync(taskListId);

    if (taskList is null) return TypedResults.NotFound();

    taskList.Order = reorder;

    await db.SaveChangesAsync();

    return TypedResults.NoContent();
}