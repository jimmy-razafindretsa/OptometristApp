using OptometristApp.Components;
using OptometristApp.Data;

//builds the app with default configuration 
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
//allows the razor components to be added gracefully 
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Register your new service
builder.Services.AddScoped<DatabaseConnexionService>();
//builds the app into the app varable 
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
