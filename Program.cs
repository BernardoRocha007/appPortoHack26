using appPortoHack.Components;
using appPortoHack.API.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddControllers();

//Services para a API de atributos
builder.Services.AddSingleton<CadAtributosService>(); // Aqui eu to instanciando a classe na memoria, é como se fosse um "new CadAtributosService()" mas o .NET faz isso pra mim, e eu posso usar em qualquer controller que eu quiser, sem precisar instanciar de novo.

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


app.MapControllers(); // <-- MAPEIA TODOS OS CONTROLLERS AUTOMATICAMENTE!


app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
