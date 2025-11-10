using Microsoft.AspNetCore.Authentication.Cookies;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Net.Http.Headers; // Add this line
var builder = WebApplication.CreateBuilder(args);

// MVC + CORS
builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient();
builder.Services.AddHttpContextAccessor();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBackend", policy =>
  {
      policy.WithOrigins("http://localhost:5157")
      .AllowAnyHeader()
     .AllowAnyMethod()
     .AllowCredentials();
  });
});

// Cookie settings for JWT storage
// builder.Services.Configure<CookiePolicyOptions>(options =>
// {
//     options.MinimumSameSitePolicy = SameSiteMode.Lax;
//     options.Secure = CookieSecurePolicy.None;
// });



// builder.Services.AddHttpClient("ApiClient", client =>
// {
//     client.BaseAddress = new Uri("http://localhost:5157/"); // Your API base URL
//     client.Timeout = TimeSpan.FromSeconds(30);
// })
// .ConfigurePrimaryHttpMessageHandler(() =>
// {
//     var handler = new HttpClientHandler();

//     // Only for development - skip SSL certificate validation
//     if (builder.Environment.IsDevelopment())
//     {
//         handler.ServerCertificateCustomValidationCallback =
//             (message, cert, chain, sslPolicyErrors) => true;
//     }

//     return handler;
// });

builder.Services.AddHttpClient("ApiClient", client =>
{
    client.BaseAddress = new Uri("http://localhost:5157/");
    client.Timeout = TimeSpan.FromSeconds(30);
})
.ConfigurePrimaryHttpMessageHandler(() =>
{
    var handler = new HttpClientHandler()
    {
        UseCookies = true, // Enable cookies
        CookieContainer = new System.Net.CookieContainer()
    };
    
    if (builder.Environment.IsDevelopment())
    {
        handler.ServerCertificateCustomValidationCallback = 
            (message, cert, chain, sslPolicyErrors) => true;
    }
    
    return handler;
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
  .AddCookie(options =>
  {
      options.Cookie.Name = "jwt";
      options.Cookie.HttpOnly = true;
      options.Cookie.SameSite = SameSiteMode.Lax;
      options.Cookie.SecurePolicy = CookieSecurePolicy.None;

      options.Events.OnRedirectToLogin = context =>
      {
          context.Response.StatusCode = 401;
          return Task.CompletedTask;
      };

      options.Events.OnRedirectToAccessDenied = context =>
      {
          context.Response.StatusCode = 403;
          return Task.CompletedTask;
      };

      options.Events.OnValidatePrincipal = context =>
      {
          var token = context.Request.Cookies["jwt"];
          Console.WriteLine("⏺ Cookie Token (ValidatePrincipal): " + token);

          if (string.IsNullOrEmpty(token))
          {
              context.RejectPrincipal();
              return Task.CompletedTask;
          }

          try
          {
              var handler = new JwtSecurityTokenHandler();
              var jwt = handler.ReadJwtToken(token);
              var identity = new ClaimsIdentity(jwt.Claims, CookieAuthenticationDefaults.AuthenticationScheme);

              // Add a default Name claim if needed
              if (!identity.HasClaim(c => c.Type == ClaimTypes.Name))
              {
                  identity.AddClaim(new Claim(ClaimTypes.Name, jwt.Subject ?? ""));
              }

              var principal = new ClaimsPrincipal(identity);
              context.ReplacePrincipal(principal);
              context.ShouldRenew = false;
          }
          catch (Exception ex)
          {
              Console.WriteLine(" JWT Parse Failed in ValidatePrincipal: " + ex.Message);
              context.RejectPrincipal();
          }

          return Task.CompletedTask;
      };
  });

builder.Services.AddAuthorization();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// ------------------- PIPELINE -------------------
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseCookiePolicy();  // Before auth

app.UseCors("AllowBackend");

// 🔍 JWT Cookie Debug Middleware
app.Use(async (context, next) =>
{
    var token = context.Request.Cookies["jwt"];
//     Console.WriteLine("🔍 Middleware sees cookie: " + token);

    if (!string.IsNullOrEmpty(token))
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);

            var identity = new ClaimsIdentity(jwt.Claims, CookieAuthenticationDefaults.AuthenticationScheme, ClaimTypes.Name, ClaimTypes.Role);

            //  Ensure IsAuthenticated becomes true
            var principal = new ClaimsPrincipal(identity);
            context.User = principal;

            Console.WriteLine(" Set HttpContext.User:");
            foreach (var claim in jwt.Claims)
            {
//                 Console.WriteLine($" {claim.Type}: {claim.Value}");
            }
        }
        catch (Exception ex)
        {
//             Console.WriteLine(" Failed to parse JWT: " + ex.Message);
        }
    }

    await next();
});


app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
   name: "default",
   pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
