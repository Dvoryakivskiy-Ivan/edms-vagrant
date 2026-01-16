using Microsoft.AspNetCore.Mvc;

namespace EDMS.MvcClient.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
       
        return View();
    }

    public IActionResult AccessDenied()
    {
        return View();
    }
}
