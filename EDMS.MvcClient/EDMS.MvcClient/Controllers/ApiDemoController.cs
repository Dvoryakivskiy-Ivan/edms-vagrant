using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EDMS.MvcClient.Controllers;

[Authorize]
public class ApiDemoController : Controller
{
    [HttpGet]
    public IActionResult OldUi() => View();

    [HttpGet]
    public IActionResult NewUi() => View();
}
