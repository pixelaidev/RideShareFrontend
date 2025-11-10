// File: ViewComponents/DriverSidebarViewComponent.cs
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using RideShareConnect.Models;

public class DriverSidebarViewComponent : ViewComponent
{
    private readonly HttpClient _httpClient;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public DriverSidebarViewComponent(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
    {
        _httpClient = httpClientFactory.CreateClient();
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var jwtCookie = _httpContextAccessor.HttpContext.Request.Cookies["jwt"];
        if (string.IsNullOrEmpty(jwtCookie))
        {
            return View(new UserProfileViewModel { FirstName = "Guest", LastName = "", ProfilePicture = null });
        }

        var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost:5157/api/UserProfile/me");
        request.Headers.Add("Cookie", $"jwt={jwtCookie}");

        var response = await _httpClient.SendAsync(request);
        if (response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadAsStringAsync();
            var profile = JsonSerializer.Deserialize<UserProfileViewModel>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            return View(profile);
        }

        return View(new UserProfileViewModel { FirstName = "Driver", LastName = "", ProfilePicture = null });
    }
}
