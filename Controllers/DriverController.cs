using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using RideShareFrontend.DTOs;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Security.Claims;
using System.Text;
using System.Net;
using RideShareConnect.Models;
using RideShareFrontend.Models.DTOs;

namespace RideShareFrontend.Controllers
{
    [Authorize(Roles = "Driver")]
    public class DriverController : Controller
    {

        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<DriverController> _logger;

        private readonly IHttpClientFactory _httpClientFactory;


        public DriverController(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<DriverController> logger)
        {
            _httpClient = httpClientFactory.CreateClient();
            _configuration = configuration;
            _logger = logger;
            _httpClientFactory = httpClientFactory;

            // Set base address for API
            var apiBaseUrl = _configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5157/";
            _httpClient.BaseAddress = new Uri(apiBaseUrl);
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
            // Ensure cookies are sent with requests
            _httpClient.DefaultRequestHeaders.ConnectionClose = false; // Keep connection alive for cookie handling
        }



        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult PostRide()
        {
            ViewBag.Message = TempData["Message"];
            ViewBag.Msg = TempData["Msg"];
            ViewBag.Success = TempData["Success"];
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> VehicleManagement()
        {
            var model = new VehicleRegistrationViewModel();

            try
            {
                var jwtCookie = HttpContext.Request.Cookies["jwt"];
                if (string.IsNullOrEmpty(jwtCookie))
                {
                    TempData["ErrorMessage"] = "Unauthorized: JWT missing.";
                    return RedirectToAction("Index");
                }

                // Request to get existing vehicles for logged-in driver
                var request = new HttpRequestMessage(HttpMethod.Get, "/api/vehicle/my-vehicles");
                request.Headers.Add("Accept", "application/json");
                request.Headers.Add("Cookie", $"jwt={jwtCookie}");

                var response = await _httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var vehicles = JsonSerializer.Deserialize<List<VehicleRegistrationViewModel>>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    // If driver has at least one vehicle, prefill the form with first vehicle’s data
                    if (vehicles != null && vehicles.Count > 0)
                    {
                        var firstVehicle = vehicles[0];
                        model.VehicleType = firstVehicle.VehicleType;
                        model.InsuranceNumber = firstVehicle.InsuranceNumber;
                        model.RegistrationExpiry = firstVehicle.RegistrationExpiry;
                        model.RCDocumentBase64 = firstVehicle.RCDocumentBase64 ?? "";
                        model.InsuranceDocumentBase64 = firstVehicle.InsuranceDocumentBase64 ?? " ";
                        model.LicensePlate = firstVehicle.LicensePlate;
                        // model.ExistingVehicles = vehicles; // For displaying a list in the view
                    }
                }
                else
                {
                    _logger.LogWarning("Failed to load existing vehicles: {StatusCode}", response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception while loading existing vehicles for VehicleManagement.");
            }

            return View(model);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VehicleManagement(VehicleRegistrationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid Vehicle Registration model: {Errors}",
                    string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
                return View(model);
            }

            try
            {
                var jwtCookie = HttpContext.Request.Cookies["jwt"];
                if (string.IsNullOrEmpty(jwtCookie))
                {
                    TempData["ErrorMessage"] = "Unauthorized: JWT missing.";
                    return RedirectToAction("Index");
                }



                var dto = new
                {
                    model.VehicleType,
                    model.LicensePlate,
                    model.InsuranceNumber,
                    model.RegistrationExpiry,
                    model.RCDocumentBase64,
                    model.InsuranceDocumentBase64,
                };

                var json = JsonSerializer.Serialize(dto);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var request = new HttpRequestMessage(HttpMethod.Post, "/api/vehicle/register")
                {
                    Content = content
                };
                request.Headers.Add("Accept", "application/json");
                request.Headers.Add("Cookie", $"jwt={jwtCookie}");

                var response = await _httpClient.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Vehicle registered successfully!";
                    return RedirectToAction("VehicleManagement");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Vehicle registration API failed: {StatusCode} - {Error}", response.StatusCode, errorContent);
                    TempData["ErrorMessage"] = "Failed to register vehicle.";
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception during vehicle registration.");
                TempData["ErrorMessage"] = "An error occurred while registering the vehicle.";
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> DriverProfile()
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
        public async Task<IActionResult> DriverProfile(UserProfileViewModel model)
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
                    return RedirectToAction("DriverProfile");
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


        [HttpGet]
        public async Task<IActionResult> BookingView()
        {
            var token = HttpContext.Request.Cookies["jwt"];

            if (string.IsNullOrEmpty(token))
            {
                TempData["Error"] = "Unauthorized. Please log in.";
                Console.WriteLine("JWT cookie not found.");
                return RedirectToAction("Login", "Account"); // adjust route if needed
            }

            try
            {
                var client = _httpClientFactory.CreateClient("ApiClient");
                // client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                client.DefaultRequestHeaders.Add("Cookie", $"jwt={token}");

                var response = await client.GetAsync("api/ride/bookings");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var bookings = JsonSerializer.Deserialize<List<RideBookingDto>>(json);
                    return View(bookings);
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"API Error: {response.StatusCode} - {errorContent}");
                TempData["Error"] = "Failed to fetch bookings.";
                return View(new List<RideBookingDto>());
            }
            catch (HttpRequestException httpEx)
            {
                TempData["Error"] = $"Service unavailable: {httpEx.Message}";
                Console.WriteLine($"HTTP Exception: {httpEx}");
                return View(new List<RideBookingDto>());
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An unexpected error occurred.";
                Console.WriteLine($"Unexpected error: {ex}");
                return View(new List<RideBookingDto>());
            }
        }

        [HttpPost("approve-booking")]
        public async Task<IActionResult> ApproveBooking(int bookingId, bool isApproved)
        {
            var token = HttpContext.Request.Cookies["jwt"];
            if (string.IsNullOrEmpty(token))
            {
                TempData["Error"] = "Unauthorized. Please log in.";
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var client = _httpClientFactory.CreateClient("ApiClient");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                client.DefaultRequestHeaders.Add("Cookie", $"jwt={token}");

                var response = await client.PostAsJsonAsync("api/ride/booking/approve", new
                {
                    bookingId = bookingId,
                    isApproved = isApproved
                });

                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = $"Booking {(isApproved ? "approved" : "rejected")} successfully!";
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    TempData["Error"] = $"Failed to update booking status: {errorContent}";
                }
            }
            catch (HttpRequestException httpEx)
            {
                TempData["Error"] = $"Service unavailable: {httpEx.Message}";
                Console.WriteLine($"HTTP Exception: {httpEx}");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An unexpected error occurred.";
                Console.WriteLine($"Unexpected error: {ex}");
            }

            return RedirectToAction("BookingView");
        }

        [HttpPost]
        public async Task<IActionResult> PostRide(RideCreateDto ride)
        {
            if (!ModelState.IsValid)
            {
                TempData["Msg"] = "Model invalid";
                return View(ride);
            }

            var token = HttpContext.Request.Cookies["jwt"];
            if (string.IsNullOrEmpty(token))
            {
                TempData["Msg"] = "Unauthorized. Please log in.";
                Console.WriteLine("JWT cookie not found. in frontend");
                return RedirectToAction("PostRide");
            }

            try
            {
                var client = _httpClientFactory.CreateClient("ApiClient");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                client.DefaultRequestHeaders.Add("Cookie", $"jwt={token}");

                var response = await client.PostAsJsonAsync("api/Ride", ride);

                if (response.IsSuccessStatusCode)
                {
                    TempData["Success"] = "Ride posted successfully!";
                    return RedirectToAction("PostRide");
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                TempData["Error"] = "Failed to post ride. Please try again.";
                Console.WriteLine($"API Error: {response.StatusCode} - {errorContent}");
                ModelState.AddModelError("", $"API Error: {response.StatusCode} - {errorContent}");
                return View(ride);
            }
            catch (HttpRequestException httpEx)
            {
                TempData["Error"] = $"Service unavailable: {httpEx.Message}";
                Console.WriteLine($"HTTP Request Exception: {httpEx}");
                ModelState.AddModelError("", "Service unavailable. Please try again later.");
                return View(ride);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "A system error occurred. Please contact support.";
                Console.WriteLine($"Unexpected Error: {ex}");
                return RedirectToAction("PostRide");
            }
        }

        [HttpGet("geocode")]
        [AllowAnonymous]
        public async Task<IActionResult> Geocode([FromQuery(Name = "q")] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return BadRequest("Query parameter is required.");

            try
            {
                using var client = new HttpClient();
                client.DefaultRequestHeaders.UserAgent.ParseAdd("RideShareApp/1.0");

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

        [HttpGet("reverse-geocode")]
        [AllowAnonymous]
        public async Task<IActionResult> ReverseGeocode([FromQuery] double lat, [FromQuery] double lng)
        {
            try
            {
                using var client = new HttpClient();
                client.DefaultRequestHeaders.UserAgent.ParseAdd("RideShareApp/1.0");

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
        public async Task<IActionResult> UserProfile()
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

                var request = new HttpRequestMessage(HttpMethod.Get, "/api/driver-profile/getdrverprofile");
                request.Headers.Add("Accept", "application/json");
                request.Headers.Add("Cookie", $"jwt={jwtCookie}");


                var response = await _httpClient.SendAsync(request);
                _logger.LogInformation("Response Status: {StatusCode}", response.StatusCode);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();

                    var profile = JsonSerializer.Deserialize<DriverProfileDto>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (profile != null)
                    {
                        profile.IsNewProfile = false;
                        ViewData["IsVerified"] = profile.isverfied;

                        return View(profile);
                    }
                }
                else if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    return View(new DriverProfileDto { IsNewProfile = true });
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("API Error - Status: {StatusCode}, Content: {Content}", response.StatusCode, errorContent);
                    TempData["ErrorMessage"] = "Failed to load profile.";
                }

                return View(new DriverProfileDto { IsNewProfile = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception while loading profile.");
                TempData["ErrorMessage"] = "An error occurred.";
                return View(new DriverProfileDto { IsNewProfile = true });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UserProfile(DriverProfileDto model)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("ModelState invalid: {Errors}",
                    string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
                Console.WriteLine("ModelState is invalid");
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
                var checkRequest = new HttpRequestMessage(HttpMethod.Get, "/api/driver-profile/create-or-update");
                checkRequest.Headers.Add("Accept", "application/json");
                checkRequest.Headers.Add("Cookie", $"jwt={jwtCookie}");

                var checkResponse = await _httpClient.SendAsync(checkRequest);
                var isNewProfile = checkResponse.StatusCode == HttpStatusCode.NotFound;

                // 🔁 Step 2: Prepare content
                var json = JsonSerializer.Serialize(model);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // 🔁 Step 3: Call appropriate API
                var endpoint = isNewProfile ? "api/UserProfile" : "/api/driver-profile/create-or-update";
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

                    return RedirectToAction("UserProfile");
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

        [HttpGet]
        public async Task<IActionResult> Myrides()
        {
            var token = HttpContext.Request.Cookies["jwt"];

            if (string.IsNullOrEmpty(token))
            {
                TempData["Error"] = "Unauthorized. Please log in.";
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var client = _httpClientFactory.CreateClient("ApiClient");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                client.DefaultRequestHeaders.Add("Cookie", $"jwt={token}");

                var response = await client.GetAsync("api/Ride/user");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var rides = JsonSerializer.Deserialize<List<RideDto>>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    return View(rides);
                }

                TempData["Error"] = $"Failed to fetch rides: {response.StatusCode}";
                return View(new List<RideDto>());
            }
            catch (HttpRequestException httpEx)
            {
                TempData["Error"] = $"Service unavailable: {httpEx.Message}";
                return View(new List<RideDto>());
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Unexpected error: {ex.Message}";
                return View(new List<RideDto>());
            }
        }




    }
}