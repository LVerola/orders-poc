using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Orders.Api.Models;
using Orders.Api;
using System.Text.Json;

[ApiController]
[Route("analytics")]
public class AnalyticsController : ControllerBase
{
  private readonly OrdersDbContext _db;

  public AnalyticsController(OrdersDbContext db)
  {
      _db = db;
  }

  [HttpPost("ask")]
  public async Task<IActionResult> Ask([FromBody] AnalyticsQuestionDto dto)
  {
    var ollamaUrl = $"{Environment.GetEnvironmentVariable("OLLAMA_URL") ?? "http://localhost:11434"}/api/generate";

    var pedidos = await _db.Orders
        .Select(o => new { o.Id, o.Cliente, o.Produto, o.Valor, o.Status, o.DataCriacao })
        .ToListAsync();

    var resumoPedidos = $"Pedidos atuais:\n" +
        string.Join("\n", pedidos.Select(p =>
            $"- Id: {p.Id}, Cliente: {p.Cliente}, Produto: {p.Produto}, Valor: {p.Valor}, Status: {p.Status}, Data: {p.DataCriacao:yyyy-MM-dd}"));

    var contexto = $@"
      Você é um assistente especializado em pedidos de uma loja online.
      Baseie suas respostas nos dados reais abaixo, fornecendo uma resposta clara e amigável e principalmente objetiva:
      {resumoPedidos}

      Tabela Orders: Id, Cliente, Produto, Valor, Status (Pendente, Processando, Finalizado), DataCriacao.
      Tabela OrderStatusHistory: Id, OrderId, Status, DataAlteracao.

      Exemplos de perguntas que você pode receber:
      - Quantos pedidos temos hoje?
      - Qual o tempo médio para aprovar os pedidos?
      - Quantos pedidos estão pendentes?
      - Qual o valor total de pedidos finalizados este mês?

      Nunca invente dados, se eles não existirem explique que os dados pedidos não estão disponíveis.
      Sempre explique sua resposta de forma simples e objetiva e amigável, é o principal.
      ";
    var promptFinal = contexto + "\nPergunta: " + dto.Question;
    var payload = new
    {
        model = "llama3",
        prompt = promptFinal
    };

    using var client = new HttpClient();
    var response = await client.PostAsJsonAsync(ollamaUrl, payload);

    if (!response.IsSuccessStatusCode)
        return StatusCode((int)response.StatusCode, "Erro ao consultar IA");

    var result = await response.Content.ReadAsStringAsync();

    var lines = result.Split('\n');
    var respostaFinal = string.Join("", lines
        .Where(l => !string.IsNullOrWhiteSpace(l))
        .Select(l => JsonDocument.Parse(l).RootElement.GetProperty("response").GetString()));

    return Ok(new { answer = respostaFinal });
  }
}

public class AnalyticsQuestionDto
{
    public string Question { get; set; }
}