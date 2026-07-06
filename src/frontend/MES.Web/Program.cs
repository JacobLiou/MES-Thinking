using MES.Web.Components;
using MES.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddBootstrapBlazor();

var mesApiBaseUrl = builder.Configuration["MesApi:BaseUrl"]
    ?? throw new InvalidOperationException("MesApi:BaseUrl is not configured.");

builder.Services.AddHttpClient<MesApiClient>(client =>
{
    client.BaseAddress = new Uri(mesApiBaseUrl);
});

builder.Services.AddScoped<StationWorkbenchState>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
