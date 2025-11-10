using Microsoft.AspNetCore.Mvc;

namespace RideShareConnect.Web.Controllers
{
    public class VerifyController : Controller
    {
        // GET: /verify
        [HttpGet]
        public IActionResult Index(string email)
        {
            // Pass the email to the view
            ViewData["Email"] = email;
            return View();
        }
    }
}