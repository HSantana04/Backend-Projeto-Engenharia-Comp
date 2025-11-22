using Microsoft.EntityFrameworkCore;
using FluentValidation;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using FinanSmartAI.Application.DTOs;
using FinanSmartAI.Application.Services;
using FinanSmartAI.Application.Validators;
using FinanSmartAI.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// ============================================================================
// ADICIONAR SERVIÇOS (Services)
// ============================================================================

// 1. Controllers
builder.Services.AddControllers();

// 2. Swagger
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "FinanSmartAI API",
        Version = "v1",
        Description = "API para gerenciamento de finanças pessoais"
    });
});

// 3. Banco de Dados (DbContext)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection") ??
    "Server=(localdb)\\mssqllocaldb;Database=FinanSmartAI;Trusted_Connection=true;"));

// 4. Services (Injeção de Dependência)
builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// 5. FluentValidation - Registrar Manualmente
builder.Services.AddScoped<IValidator<CreateTransactionRequest>, CreateTransactionRequestValidator>();
builder.Services.AddScoped<IValidator<UpdateTransactionRequest>, UpdateTransactionRequestValidator>();

// 6. JWT Authentication
var jwtSecretKey = builder.Configuration["Jwt:SecretKey"] ?? "your-secret-key-here-minimum-32-characters!!!";

builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey)),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "FinanSmartAI",
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "FinanSmartAIUsers",
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// 7. CORS (Cross-Origin Resource Sharing)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// 8. Logging
builder.Services.AddLogging(config =>
{
    config.AddConsole();
    config.AddDebug();
});

// ============================================================================
// CONSTRUIR A APLICAÇÃO
// ============================================================================

var app = builder.Build();

// ============================================================================
// CRIAR BANCO DE DADOS SE NÃO EXISTIR
// ============================================================================

using (var scope = app.Services.CreateScope())
{
    try
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Criar banco de dados e tabelas se não existirem
        dbContext.Database.EnsureCreated();

        Console.WriteLine("✅ Banco de dados criado/verificado com sucesso!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Erro ao criar banco de dados: {ex.Message}");
    }
}

// ============================================================================
// MIDDLEWARE DE ERRO GLOBAL
// ============================================================================

app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ ERRO NÃO TRATADO: {ex}");

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = 500;

        var errorResponse = new
        {
            message = "Erro interno do servidor",
            detail = app.Environment.IsDevelopment() ? ex.Message : null,
            timestamp = DateTime.UtcNow
        };

        await context.Response.WriteAsJsonAsync(errorResponse);
    }
});

// ============================================================================
// CONFIGURAR O PIPELINE HTTP
// ============================================================================

// 1. Swagger (Desenvolvimento)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "FinanSmartAI API v1");
        options.RoutePrefix = "swagger";
    });
}

// 2. HTTPS Redirection
app.UseHttpsRedirection();

// 3. CORS
app.UseCors("AllowAll");

// 4. Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// 5. Controllers
app.MapControllers();

// ============================================================================
// EXECUTAR A APLICAÇÃO
// ============================================================================

app.Run();