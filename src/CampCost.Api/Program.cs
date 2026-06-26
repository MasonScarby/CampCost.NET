using CampCost.Api.Middleware;
using CampCost.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// --- Infrastructure: DB + Plaid + all scoped services ---
builder.Services.AddInfrastructure(builder.Configuration);

// --- JWT auth: validates Supabase-issued RS256 tokens ---
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["Supabase:Url"] + "/auth/v1";
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer   = false,
            ValidateAudience = false,
            ValidateLifetime = true
        };
    });

// --- CORS: allow the Vite dev server origins ---
builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins("http://localhost:5173", "http://localhost:5174")
              .AllowAnyHeader()
              .AllowAnyMethod()));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();
app.MapGet("/", () => Results.Redirect("/swagger/index.html"));

app.Run();
