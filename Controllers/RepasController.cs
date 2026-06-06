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
        try
        {
            if (file == null || file.Length == 0)
                return BadRequest("Fichier manquant");

            var client = _httpClientFactory.CreateClient("NutritionAPI");

            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);
            ms.Position = 0;

            using var form = new MultipartFormDataContent();
            var fileContent = new ByteArrayContent(ms.ToArray());
            fileContent.Headers.ContentType =
                new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType ?? "image/jpeg");

            form.Add(fileContent, "file", file.FileName ?? "photo.jpg");

            Console.WriteLine($"==> Envoi vers Python: POST /api/analyser-repas");
            Console.WriteLine($"==> Fichier: {file.FileName}, taille: {file.Length}, type: {file.ContentType}");

            var response = await client.PostAsync("/api/analyser-repas", form);
            var result = await response.Content.ReadAsStringAsync();

            Console.WriteLine($"==> Python status: {response.StatusCode}");
            Console.WriteLine($"==> Python response: {result}");

            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode, result);

            return Content(result, "application/json");
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"==> CONNEXION PYTHON IMPOSSIBLE: {ex.Message}");
            return StatusCode(500, $"Service Python inaccessible: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"==> ERREUR: {ex.Message}");
            return StatusCode(500, ex.Message);
        }
    }
}