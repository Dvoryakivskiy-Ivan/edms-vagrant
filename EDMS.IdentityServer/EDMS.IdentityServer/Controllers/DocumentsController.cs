using EDMS.MvcClient.Models;
using EDMS.MvcClient.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EDMS.MvcClient.Controllers;

[Authorize] //Needs Authorization
public class DocumentsController : Controller
{
    //View documents (User + Admin)
    [HttpGet]
    public IActionResult Browse(string? q = null, string? type = null, DocumentStatus? status = null)
    {
        var docs = InMemoryDocumentStore.GetAll().AsEnumerable();

        if (!string.IsNullOrWhiteSpace(q))
            docs = docs.Where(d =>
                d.Title.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                d.Author.Contains(q, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(type))
            docs = docs.Where(d => d.Type.Equals(type, StringComparison.OrdinalIgnoreCase));

        if (status.HasValue)
            docs = docs.Where(d => d.Status == status);

        ViewBag.Query = q;
        ViewBag.Type = type;
        ViewBag.Status = status;

        return View(docs.ToList());
    }

    //Document creation (Admin)
    [Authorize(Roles = "Admin")]
    [HttpGet]
    public IActionResult Create() => View();

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public IActionResult Create(string title, string type, string content)
    {
        if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(type))
        {
            ViewBag.Error = "Title and type are required.";
            return View();
        }

        InMemoryDocumentStore.Add(new Document
        {
            Title = title,
            Type = type,
            Content = content ?? "",
            Author = User.Identity?.Name ?? "Admin"
        });

        return RedirectToAction(nameof(Browse));
    }

    //Status change (Admin)
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public IActionResult ChangeStatus(int id, DocumentStatus status)
    {
        InMemoryDocumentStore.UpdateStatus(id, status);
        return RedirectToAction(nameof(Browse));
    }
}
