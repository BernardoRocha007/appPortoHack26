using Microsoft.EntityFrameworkCore;
using appPortoHack.Components;
using appPortoHack.API.Data;
using appPortoHack.API.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddControllers();

// 1. Configuração do Banco de Dados SQL Server via Entity Framework Core
builder.Services.AddDbContext<ConexaoDB>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Injeção de Dependência dos Serviços
builder.Services.AddSingleton<CadAtributosService>(); // Regras e Atributos da NCM
builder.Services.AddScoped<PlanilhaService>();        // Leitor de planilhas CSV / XLSX
builder.Services.AddScoped<ProdutoService>();         // Catálogo de Produtos integrado ao SQL
builder.Services.AddScoped<DuimpService>();           // Emissão e consulta de DUIMPs integrado ao SQL

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();
app.UseAntiforgery();
app.MapStaticAssets();

app.MapControllers(); // Mapeia todos os controllers da API

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
