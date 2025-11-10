using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Net;
using RideShareFrontend.DTOs;
using System.Net.Http.Headers;
using RideShareConnect.Models;

namespace RideShareConnect.Controllers
{
    [Authorize(Roles = "Passenger")]
    public class PassengerController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<PassengerController> _logger;

        public PassengerController(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<PassengerController> logger)
        {
            _httpClient = httpClientFactory.CreateClient();
            _configuration = configuration;
            _logger = logger;

            // Set base address for API
            var apiBaseUrl = _configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5157/";
            _httpClient.BaseAddress = new Uri(apiBaseUrl);
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
            // Ensure cookies are sent with requests
            _httpClient.DefaultRequestHeaders.ConnectionClose = false; // Keep connection alive for cookie handling
        }

        // Set JWT token from cookies to Authorization header
        private void SetAuthorizationHeader()
        {
            var token = HttpContext.Request.Cookies["jwt"];
            Console.WriteLine(token, "This is token");
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                _logger.LogInformation("JWT token set in Authorization header");
            }
            else
            {
                _logger.LogWarning("No JWT token found in cookies");
            }
        }

        public async Task<IActionResult> Index()
        {
            var token = HttpContext.Request.Cookies["jwt"];
            _logger.LogInformation("Authenticated: {IsAuthenticated}, Role: {Role}, JWT: {Token}",
                User.Identity.IsAuthenticated, User.FindFirst(ClaimTypes.Role)?.Value, token);

            if (string.IsNullOrEmpty(token))
            {
                _logger.LogWarning("No JWT token found in cookies");
                ViewBag.Bookings = new List<PassengerBookingDto>();
                ViewBag.TotalRides = 0;
                ViewBag.TotalSpent = 0;
                return View();
            }

            try
            {
                var client = _httpClient;
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                client.DefaultRequestHeaders.Add("Cookie", $"jwt={token}");

                // Fetch passenger's bookings
                var bookingsResponse = await client.GetAsync("/api/Ride/bookingspassenger");

                if (bookingsResponse.IsSuccessStatusCode)
                {
                    var bookingsJson = await bookingsResponse.Content.ReadAsStringAsync();
                    var bookings = JsonSerializer.Deserialize<List<PassengerBookingDto>>(bookingsJson,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    ViewBag.Bookings = bookings ?? new List<PassengerBookingDto>();

                    // Calculate stats for dashboard
                    ViewBag.TotalRides = bookings?.Count ?? 0;
                    ViewBag.TotalSpent = bookings?
                        .Where(b => b.BookingStatus == "Confirmed")
                        .Sum(b => b.TotalAmount) ?? 0;
                }
                else
                {
                    var errorContent = await bookingsResponse.Content.ReadAsStringAsync();
                    _logger.LogError("Failed to fetch bookings: {StatusCode} - {Error}",
                        bookingsResponse.StatusCode, errorContent);
                    ViewBag.Bookings = new List<PassengerBookingDto>();
                    ViewBag.TotalRides = 0;
                    ViewBag.TotalSpent = 0;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching passenger bookings");
                ViewBag.Bookings = new List<PassengerBookingDto>();
                ViewBag.TotalRides = 0;
                ViewBag.TotalSpent = 0;
            }

            return View();
        }




        public IActionResult Wallet()
        {
            return View();
        }

        public IActionResult History()
        {
            return View();
        }

        public IActionResult Payments()
        {
            return View();
        }

        public IActionResult AcceptRide()
        {
            return View();
        }

        [HttpGet]
        public IActionResult SearchRide()
        {
            ViewBag.Msg = TempData["Message"];
            ViewBag.Msg = TempData["Msg"];
            ViewBag.Success = TempData["Success"];

            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BookRide(RideBookingCreateViewModel bookDto)
        {


            if (!ModelState.IsValid)
            {
                Console.WriteLine("ModelState is invalid.");
                foreach (var key in ModelState.Keys)
                {
                    var state = ModelState[key];
                    if (state.Errors.Count > 0)
                    {
                        Console.WriteLine($"Field: {key}");
                        foreach (var error in state.Errors)
                        {
                            Console.WriteLine($" - Error: {error.ErrorMessage}");
                        }
                    }
                }
                TempData["Error"] = "Invalid input. Please review the form.";
                return RedirectToAction("SearchRide");
            }

            var token = HttpContext.Request.Cookies["jwt"];
            if (string.IsNullOrEmpty(token))
            {
                TempData["Error"] = "Unauthorized. Please log in first.";
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var client = _httpClient;
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                client.DefaultRequestHeaders.Add("Cookie", $"jwt={token}");

                var response = await client.PostAsJsonAsync("api/ride/book", bookDto);

                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "Ride booking Request posted successfully, Please wait for driver to approved";
                    return RedirectToAction("SearchRide");
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError($"Booking failed: {response.StatusCode} - {errorContent}");
                TempData["Error"] = "Booking failed. Please try again.";
                return RedirectToAction("SearchRide");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during ride booking.");
                TempData["Error"] = "Unexpected error. Please try again.";
                Console.WriteLine(ex);
                return RedirectToAction("SearchRide");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SearchRide(RideSearchRequestDto searchDto)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Invalid input. Please check your fields.";
                return View(searchDto);
            }

            var token = HttpContext.Request.Cookies["jwt"];
            if (string.IsNullOrEmpty(token))
            {
                TempData["Error"] = "Unauthorized. Please log in.";
                return RedirectToAction("SearchRide");
            }

            try
            {
                var client = _httpClient;
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                client.DefaultRequestHeaders.Add("Cookie", $"jwt={token}");

                // 🔁 Map frontend DTO → backend DTO
                var backendSearchDto = new
                {
                    Origin = searchDto.Origin,
                    Destination = searchDto.Destination,
                    DepartureDate = searchDto.RideDate
                };

                var response = await client.PostAsJsonAsync("api/ride/search", backendSearchDto);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(json);

                    var rideResults = JsonSerializer.Deserialize<List<RideViewModel>>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (rideResults == null || !rideResults.Any())
                    {
                        TempData["Msg"] = "No rides found matching your criteria.";
                    }
                    else
                    {
                        TempData["Success"] = "Rides found successfully!";
                    }
                    // Console.WriteLine(rideResults);
                    ViewBag.RideResults = rideResults ?? new List<RideViewModel>();
                    ViewBag.Message = TempData["Message"];
                    ViewBag.Msg = TempData["Msg"];
                    ViewBag.Success = TempData["Success"];
                    return View(searchDto);
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                TempData["Error"] = "Failed to search rides. Please try again.";
                _logger.LogError($"API Error: {response.StatusCode} - {errorContent}");
                return View(searchDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during search.");
                TempData["Error"] = "An unexpected error occurred.";
                return View(searchDto);
            }
        }




        [HttpGet("pgeocode")]
        [AllowAnonymous]
        public async Task<IActionResult> Geocode([FromQuery(Name = "q")] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return BadRequest("Query parameter is required.");

            try
            {
                using var client = new HttpClient();
                client.DefaultRequestHeaders.UserAgent.ParseAdd("RideShareApp/1.0 (kamilmulani2433@gmail.com)");

                var url = $"https://nominatim.openstreetmap.org/search?q={Uri.EscapeDataString(query)}&format=json";
                var response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                return Content(content, "application/json");
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(503, $"Geocoding service unavailable: {ex.Message}");
            }
        }


        [HttpGet("preverse-geocode")]
        [AllowAnonymous]
        public async Task<IActionResult> ReverseGeocode([FromQuery] double lat, [FromQuery] double lng)
        {
            try
            {
                using var client = new HttpClient();
                client.DefaultRequestHeaders.UserAgent.ParseAdd("RideShareApp/1.0 (kamilmulani2433@gmail.com)");

                var url = $"https://nominatim.openstreetmap.org/reverse?format=json&lat={lat}&lon={lng}";
                var response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                return Content(content, "application/json");
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(503, $"Reverse geocoding service unavailable: {ex.Message}");
            }
        }

        [HttpGet]
        public async Task<IActionResult> PassengerProfile()
        {
            try
            {
                var jwtCookie = HttpContext.Request.Cookies["jwt"];
                if (string.IsNullOrEmpty(jwtCookie))
                {
                    _logger.LogWarning("JWT cookie not found in request");
                    TempData["ErrorMessage"] = "You are not logged in.";
                    return RedirectToAction("Index");
                }

                var request = new HttpRequestMessage(HttpMethod.Get, "api/UserProfile/me");
                request.Headers.Add("Accept", "application/json");
                request.Headers.Add("Cookie", $"jwt={jwtCookie}");

                var response = await _httpClient.SendAsync(request);
                _logger.LogInformation("Response Status: {StatusCode}", response.StatusCode);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var profile = JsonSerializer.Deserialize<UserProfileViewModel>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (profile != null)
                    {
                        profile.IsNewProfile = false;
                        return View(profile);
                    }
                }
                else if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    return View(new UserProfileViewModel { IsNewProfile = true });
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("API Error - Status: {StatusCode}, Content: {Content}", response.StatusCode, errorContent);
                    TempData["ErrorMessage"] = "Failed to load profile.";
                }

                return View(new UserProfileViewModel { IsNewProfile = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception while loading profile.");
                TempData["ErrorMessage"] = "An error occurred.";
                return View(new UserProfileViewModel { IsNewProfile = true });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PassengerProfile(UserProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("ModelState invalid: {Errors}",
                    string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));

                return View(model);
            }

            try
            {
                var jwtCookie = HttpContext.Request.Cookies["jwt"];
                if (string.IsNullOrEmpty(jwtCookie))
                {
                    TempData["ErrorMessage"] = "Unauthorized: JWT missing.";
                    return RedirectToAction("PassengerProfile");
                }

                // 🔍 Step 1: Check if profile exists
                var checkRequest = new HttpRequestMessage(HttpMethod.Get, "api/UserProfile/me");
                checkRequest.Headers.Add("Accept", "application/json");
                checkRequest.Headers.Add("Cookie", $"jwt={jwtCookie}");

                var checkResponse = await _httpClient.SendAsync(checkRequest);
                var isNewProfile = checkResponse.StatusCode == HttpStatusCode.NotFound;

                // 🔁 Step 2: Prepare content
                var json = JsonSerializer.Serialize(model);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // 🔁 Step 3: Call appropriate API
                var endpoint = isNewProfile ? "api/UserProfile" : "api/UserProfile/me";
                var saveRequest = new HttpRequestMessage(HttpMethod.Post, endpoint)
                {
                    Content = content
                };
                saveRequest.Headers.Add("Accept", "application/json");
                saveRequest.Headers.Add("Cookie", $"jwt={jwtCookie}");

                var saveResponse = await _httpClient.SendAsync(saveRequest);

                if (saveResponse.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = isNewProfile
                        ? "Profile created successfully!"
                        : "Profile updated successfully!";

                    return RedirectToAction("PassengerProfile");
                }
                else
                {
                    var errorContent = await saveResponse.Content.ReadAsStringAsync();
                    _logger.LogError("API save failed: {StatusCode} - {Error}", saveResponse.StatusCode, errorContent);
                    TempData["ErrorMessage"] = "Failed to save profile.";
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception while saving profile");
                TempData["ErrorMessage"] = "An error occurred while saving the profile.";
                return View(model);
            }
        }




        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProfile()
        {
            try
            {
                var jwtCookie = HttpContext.Request.Cookies["jwt"];
                if (string.IsNullOrEmpty(jwtCookie))
                {
                    TempData["ErrorMessage"] = "Unauthorized: JWT missing.";
                    return RedirectToAction("PassengerProfile");
                }

                var request = new HttpRequestMessage(HttpMethod.Delete, "api/UserProfile/me");
                request.Headers.Add("Accept", "application/json");
                request.Headers.Add("Cookie", $"jwt={jwtCookie}");

                var response = await _httpClient.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Profile deleted successfully!";
                    _logger.LogInformation("Profile deleted successfully");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Delete API call failed with status: {StatusCode}, Error: {Error}", response.StatusCode, errorContent);
                    TempData["ErrorMessage"] = "Failed to delete profile.";
                }

                return RedirectToAction("PassengerProfile");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting passenger profile");
                TempData["ErrorMessage"] = "An error occurred while deleting the profile.";
                return RedirectToAction("PassengerProfile");
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _httpClient?.Dispose();
            }
            base.Dispose(disposing);
        }




    }
}