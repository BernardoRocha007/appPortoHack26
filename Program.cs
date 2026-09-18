using appPortoHack.Components;
using appPortoHack.API.Services;
using Microsoft.EntityFrameworkCore;
using appPortoHack26.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddControllers();

//Services para a API
builder.Services.AddSingleton<CadAtributosService>(); // Aqui eu to instanciando a classe na memoria, é como se fosse um "new CadAtributosService()" mas o .NET faz isso pra mim, e eu posso usar em qualquer controller que eu quiser, sem precisar instanciar de novo.
builder.Services.AddSingleton<BancoDadosService>();
builder.Services.AddSingleton<PlanilhaService>();
builder.Services.AddSingleton<ProdutoService>();
builder.Services.AddSingleton<DuimpService>();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=banco_hackathon.db")); // adicioanando banco

var app = builder.Build();
app.Services.GetRequiredService<BancoDadosService>(); //força a instanciação do serviço de banco de dados para que ele carregue os dados do arquivo JSON na memória RAM ao iniciar o aplicativo.

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
