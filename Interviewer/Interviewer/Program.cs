using Interviewer.Components;
using Interviewer.Data.Interfaces;
using Interviewer.Data.Repositories;
using Interviewer.Data.Settings;
using Interviewer.Infrastructure.Gemini;
using Interviewer.Infrastructure.Interfaces;
using Interviewer.Services;
using Interviewer.Services.Options;
using Interviewer.State;
using Microsoft.AspNetCore.Components;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<PromptOptions>(builder.Configuration.GetSection(PromptOptions.SectionName));

builder.Services.Configure<MongoDbSettings>(
    builder.Configuration.GetSection("MongoDb"));

builder.Services.AddSingleton<IInterviewRepository, MongoInterviewRepository>();

builder.Services.AddScoped(sp =>
{
    return new HttpClient();
    //{
    //    BaseAddress = new Uri("https://localhost:7254/")
    //};
});
// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddSingleton<InterviewState>();
builder.Services.AddScoped<IAIService, GeminiAIService>();
builder.Services.AddScoped<IInterviewGeneratorService, InterviewGeneratorService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
