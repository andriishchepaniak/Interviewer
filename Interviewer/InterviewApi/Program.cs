using Interviewer.Data.Interfaces;
using Interviewer.Data.Repositories;
using Interviewer.Data.Settings;
using Interviewer.Infrastructure.Gemini;
using Interviewer.Infrastructure.Interfaces;
using Interviewer.Services;
using Interviewer.Services.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<MongoDbSettings>(
    builder.Configuration.GetSection("MongoDb"));

builder.Services.Configure<PromptOptions>(builder.Configuration.GetSection("Prompts"));

// Add services to the container.
builder.Services.AddScoped<IInterviewRepository, MongoInterviewRepository>();

builder.Services.AddScoped<IAIService, GeminiAIService>();

builder.Services.AddScoped<IInterviewGeneratorService, InterviewGeneratorService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFlutterWeb", policy =>
    {
        policy.AllowAnyOrigin()  // Allows any port/origin (perfect for local dev)
              .AllowAnyHeader()  // Allows any headers (like Content-Type)
              .AllowAnyMethod(); // Allows GET, POST, PUT, DELETE
    });
});

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseCors("AllowFlutterWeb");

app.UseAuthorization();

app.MapControllers();

app.Run();
