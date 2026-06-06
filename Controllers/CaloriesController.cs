using backend.DTOs.Nutrition;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CaloriesController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;

    public CaloriesController(IHttpClientFactory httpClientFactory)
        => _httpClientFactory = httpClientFactory;

    [HttpPost]
    public async Task<IActionResult> GetCalories([FromBody] ProfilRequestDto request)
    {
        var client = _httpClientFactory.CreateClient("NutritionAPI");
        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await client.PostAsync("/api/calories", content);
        var result = await response.Content.ReadAsStringAsync();
        return Content(result, "application/json");
    }
}