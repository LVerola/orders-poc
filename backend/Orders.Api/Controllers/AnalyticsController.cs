using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

[ApiController]
[Route("analytics")]
public class AnalyticsController : ControllerBase
{
    [HttpPost("ask")]
public async Task<IActionResult> Ask([FromBody] AnalyticsQuestionDto dto)
  {
    var ollamaUrl = $"{Environment.GetEnvironmentVariable("OLLAMA_URL") ?? "http://localhost:11434"}/api/generate";
    var contexto = @"
      Você é um assistente para uma API de pedidos.
      Tabela Orders: Id, Cliente, Produto, Valor, Status (Pendente, Processando, Finalizado), DataCriacao.
      Tabela OrderStatusHistory: Id, OrderId, Status, DataAlteracao.
      Responda perguntas sobre pedidos usando esse contexto.
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