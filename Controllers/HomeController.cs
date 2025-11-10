using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using RideShareFrontend.Models;

namespace RideShareFrontend.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly HttpClient _httpClient;

    public HomeController(ILogger<HomeController> logger, HttpClient httpClient)
    {
        _logger = logger;
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("http://localhost:5157/api/"); // Update with your API base URL
    }

    public IActionResult Index()
    {
        _logger.LogInformation("Index action called");
        return View();
    }

    [HttpGet("/VerifyOtp")]
    public IActionResult VerifyOtp()
    {
        _logger.LogInformation("VerifyOtp GET action called");
        return View(new SendOtpModel());
    }

    [HttpPost("/VerifyOtp")]
    public async Task<IActionResult> SendOtp(SendOtpModel model)
    {
        _logger.LogInformation("SendOtp action called with email: {Email}", model.Email);

        if (!ModelState.IsValid)
        {
            _logger.LogWarning("ModelState invalid for SendOtp: {Errors}", string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
            return View("VerifyOtp", model);
        }

        try
        {
            _logger.LogInformation("Preparing to send OTP request to API for email: {Email}", model.Email);
            var content = new StringContent(
                JsonSerializer.Serialize(new { Email = model.Email }),
                Encoding.UTF8,
                "application/json");

            _logger.LogInformation("Sending POST request to {Url}", $"{_httpClient.BaseAddress}ResetPassword/send-otp");
            var response = await _httpClient.PostAsync("ResetPassword/send-otp", content);
            _logger.LogInformation("Received response from send-otp API: StatusCode={StatusCode}", response.StatusCode);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("OTP sent successfully for email: {Email}", model.Email);
                return RedirectToAction("VerifySecond", new { email = model.Email });
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Failed to send OTP. StatusCode: {StatusCode}, Response: {Response}", response.StatusCode, errorContent);
            ModelState.AddModelError(string.Empty, $"Failed to send OTP: {errorContent}");
            return View("VerifyOtp", model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending OTP for email: {Email}", model.Email);
            ModelState.AddModelError(string.Empty, "An error occurred while sending OTP");
            return View("VerifyOtp", model);
        }
    }

    [HttpGet("/VerifySecond")]
    public IActionResult VerifySecond(string email)
    {
        _logger.LogInformation("VerifySecond GET action called with email: {Email}", email);
        return View(new VerifyOtpModel { Email = email });
    }

    [HttpPost("/VerifySecond")]
    public async Task<IActionResult> VerifySecond(VerifyOtpModel model)
    {
        _logger.LogInformation("VerifySecond POST action called with email: {Email}, OTP: {OtpCode}", model.Email, model.OtpCode);

        if (!ModelState.IsValid)
        {
            _logger.LogWarning("ModelState invalid for VerifySecond: {Errors}", string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
            return View("VerifySecond", model);
        }

        try
        {
            _logger.LogInformation("Preparing to verify OTP for email: {Email}", model.Email);
            var content = new StringContent(
                JsonSerializer.Serialize(new { Email = model.Email, OtpCode = model.OtpCode }),
                Encoding.UTF8,
                "application/json");

            _logger.LogInformation("Sending POST request to {Url}", $"{_httpClient.BaseAddress}ResetPassword/verify-otp");
            var response = await _httpClient.PostAsync("ResetPassword/verify-otp", content);
            _logger.LogInformation("Received response from verify-otp API: StatusCode={StatusCode}", response.StatusCode);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("OTP verified successfully for email: {Email}", model.Email);
                return RedirectToAction("ResetPassword", new { email = model.Email });
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Failed to verify OTP. StatusCode: {StatusCode}, Response: {Response}", response.StatusCode, errorContent);
            ModelState.AddModelError(string.Empty, $"Invalid or expired OTP: {errorContent}");
            return View("VerifySecond", model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying OTP for email: {Email}", model.Email);
            ModelState.AddModelError(string.Empty, "An error occurred while verifying OTP");
            return View("VerifySecond", model);
        }
    }

    [HttpGet("/ResetPassword")]
    public IActionResult ResetPassword(string email)
    {
        _logger.LogInformation("ResetPassword GET action called with email: {Email}", email);
        return View(new ResetPasswordModel { Email = email });
    }

    [HttpPost("/ResetPassword")]
    public async Task<IActionResult> ResetPassword(ResetPasswordModel model)
    {
        _logger.LogInformation("ResetPassword POST action called with email: {Email}", model.Email);

        if (!ModelState.IsValid)
        {
            _logger.LogWarning("ModelState invalid for ResetPassword: {Errors}", string.Join("; ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
            return View("ResetPassword", model);
        }

        try
        {
            _logger.LogInformation("Preparing to reset password for email: {Email}", model.Email);
            var content = new StringContent(
                JsonSerializer.Serialize(new { Email = model.Email, Password = model.Password, ConfirmPassword = model.ConfirmPassword }),
                Encoding.UTF8,
                "application/json");

            _logger.LogInformation("Sending POST request to {Url}", $"{_httpClient.BaseAddress}ResetPassword/reset-password");
            var response = await _httpClient.PostAsync("ResetPassword/reset-password", content);
            _logger.LogInformation("Received response from reset-password API: StatusCode={StatusCode}", response.StatusCode);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("Password reset successfully for email: {Email}", model.Email);
                TempData["SuccessMessage"] = "Password reset successfully";
                return RedirectToAction("Index");
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Failed to reset password. StatusCode: {StatusCode}, Response: {Response}", response.StatusCode, errorContent);
            ModelState.AddModelError(string.Empty, $"Failed to reset password: {errorContent}");
            return View("ResetPassword", model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting password for email: {Email}", model.Email);
            ModelState.AddModelError(string.Empty, "An error occurred while resetting password");
            return View("ResetPassword", model);
        }
    }

    [HttpPost("/Logout")]
    public IActionResult Logout()
    {
        _logger.LogInformation("Logout action called");
        return RedirectToAction("Index");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        _logger.LogError("Error action called with RequestId: {RequestId}", Activity.Current?.Id ?? HttpContext.TraceIdentifier);
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}