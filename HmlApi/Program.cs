using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<HmlDb>(opt => opt.UseInMemoryDatabase("HmlList"));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApiDocument(config =>
{
    config.DocumentName = "HmlAPI";
    config.Title = "HmlAPI v1";
    config.Version = "v1";
});
var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseOpenApi();
    app.UseSwaggerUi(config =>
    {
        config.DocumentTitle = "HmlAPI";
        config.Path = "/swagger";
        config.DocumentPath = "/swagger/{documentName}/swagger.json";
        config.DocExpansion = "list";
    });
}

app.MapGet("/projects", async (HmlDb db) =>
    await db.Projects.Include(p => p.TaskLists)
                     .ThenInclude(tl => tl.Tasks)
                     .ToListAsync());

app.MapPost("/add", async (Project project, HmlDb db) =>
{
    db.Projects.Add(project);
    await db.SaveChangesAsync();

    return Results.Created($"/add/{project.Id}", project);
});

app.Run();