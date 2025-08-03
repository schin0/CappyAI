using CappyAI.Application.UseCases;
using CappyAI.Domain.Interfaces;
using CappyAI.Infrastructure.ExternalServices;
using CappyAI.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpClient();

builder.Services.AddScoped<IGeradorQuebraGelo, GeradorQuebraGeloRepository>();
builder.Services.AddScoped<IObterContextoUsuario, ObterContextoUsuarioService>();
builder.Services.AddScoped<IIAGeradorQuebraGelo, IAGeradorQuebraGeloService>();
builder.Services.AddScoped<GerarIdeiasQuebraGelo>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
