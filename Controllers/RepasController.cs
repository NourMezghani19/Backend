using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RepasController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;

    public RepasController(IHttpClientFactory httpClientFactory)
        => _httpClientFactory = httpClientFactory;

    [HttpPost("analyser")]
    public async Task<IActionResult> AnalyserRepas(IFormFile file)
    {
        var client = _httpClientFactory.CreateClient("NutritionAPI");

        using var form = new MultipartFormDataContent();
        using var stream = file.OpenReadStream();
        form.Add(new StreamContent(stream), "file", file.FileName);

        var response = await client.PostAsync("/api/analyser-repas", form);
        var result = await response.Content.ReadAsStringAsync();
        return Content(result, "application/json");
    }
}