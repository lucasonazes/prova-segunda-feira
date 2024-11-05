using System.Security.Cryptography.X509Certificates;
using api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<AppDataContext>();

builder.Services.AddCors(
    options => options.AddPolicy("Total Acess",
        configs => configs
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod())
);

var app = builder.Build();

// Home Route - Test if the application is running
app.MapGet("/", () => "To do API");


// Create Funcionario
app.MapPost("/api/funcionario/cadastrar", ([FromBody] Funcionario funcionario, [FromServices] AppDataContext ctx) => 
{
    ctx.Funcionarios.Add(funcionario);
    ctx.SaveChanges();
    return Results.Created("", funcionario);
});


// List Funcionario
app.MapGet("/api/funcionario/listar", ([FromServices] AppDataContext ctx) => 
{   
    List<Funcionario> funcionarios = ctx.Funcionarios.ToList();

    if (funcionarios.Count <= 0) {
        return Results.NotFound("Não há nenhum funcionário cadastrado");
    }

    return Results.Ok(funcionarios);
});

// Create Folha
app.MapPost("/api/folha/cadastrar", ([FromBody] api.Models.FolhaPagamento folha, [FromServices] AppDataContext ctx) => 
{
    if (folha.FuncionarioId <= 0) return Results.BadRequest("Insira um Id de funcionário válido");

    Funcionario? funcionario = ctx.Funcionarios.Find(folha.FuncionarioId);

    if (funcionario == null) return Results.NotFound("Funcionário não encontrado, verifique o Id");

    folha.Funcionario = funcionario;

    /* Lógica pra calcular salário e impostos*/
    folha.SalarioBruto = folha.Quantidade * folha.Valor;

    if (folha.SalarioBruto >= 1903.99 && folha.SalarioBruto <= 2826.65) {
        folha.ImpostoIrrf = 142.80;
    } else if (folha.SalarioBruto >= 2826.66 && folha.SalarioBruto <= 3751.05) {
        folha.ImpostoIrrf = 354.80;
    } else if (folha.SalarioBruto >= 3751.06 && folha.SalarioBruto <= 4664.68) {
        folha.ImpostoIrrf = 636.13;
    } else if (folha.SalarioBruto > 4664.68) {
        folha.ImpostoIrrf = 869.36;
    } else folha.ImpostoIrrf = 0.00;

    if (folha.SalarioBruto <= 1693.72) {
        folha.ImpostoInss = folha.SalarioBruto * 0.08;
    } else if (folha.SalarioBruto >= 1693.73 && folha.SalarioBruto <= 2822.90) {
        folha.ImpostoInss = folha.SalarioBruto * 0.09;
    } else if (folha.SalarioBruto >= 2822.91 && folha.SalarioBruto <= 5645.80) {
        folha.ImpostoInss = folha.SalarioBruto * 0.11;
    } else if (folha.SalarioBruto > 5645.81) {
        folha.ImpostoInss = 621.03;
    } else folha.ImpostoInss = 0.00;

    folha.ImpostoFgts = folha.SalarioBruto * 0.08;

    folha.SalarioLiquido = folha.SalarioBruto - folha.ImpostoIrrf - folha.ImpostoInss;

    ctx.Folhas.Add(folha);
    ctx.SaveChanges();

    return Results.Created("/api/folha/cadastrar", folha);
});

// List Funcionario
app.MapGet("/api/folha/listar", ([FromServices] AppDataContext ctx) => 
{   
    List<FolhaPagamento> folhas = ctx.Folhas.ToList();

    foreach (FolhaPagamento folha in folhas) {
        Funcionario? funcionario = ctx.Funcionarios.Find(folha.FuncionarioId);
        folha.Funcionario = funcionario;
    }

    if (folhas.Count <= 0) {
        return Results.NotFound("Não há nenhuma folha registrada");
    }

    return Results.Ok(folhas);
});

app.MapGet("/api/folha/buscar/{cpf}/{mes}/{ano}", ([FromServices] AppDataContext ctx, string cpf, int mes, int ano) =>
{   
    List<Funcionario> funcionarios = ctx.Funcionarios.ToList();
    List<FolhaPagamento> folhas = ctx.Folhas.ToList();

    foreach (Funcionario funcionario in funcionarios) {
        foreach (FolhaPagamento folha in folhas) {
            if (folha.Mes == mes && folha.Ano == ano) {
                if (folha.FuncionarioId == funcionario.Id) {
                    return Results.Ok(folha);
                }
            }
        }
    }

    return Results.NotFound("Folha não encontrada");
});

app.UseCors("Total Acess");
app.Run();
