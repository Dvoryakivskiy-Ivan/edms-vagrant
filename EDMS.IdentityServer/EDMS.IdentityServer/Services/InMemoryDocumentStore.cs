using EDMS.MvcClient.Models;

namespace EDMS.MvcClient.Services;

public static class InMemoryDocumentStore
{
    private static readonly List<Document> _docs = new();
    private static int _nextId = 1;

    static InMemoryDocumentStore()
    {
        Add(new Document { Title = "Contract #1", Type = "Contract", Author = "Admin", Content = "Demo content..." });
        Add(new Document { Title = "Order #12", Type = "Order", Author = "User", Content = "Demo content..." });
    }

    public static IReadOnlyList<Document> GetAll() => _docs.OrderByDescending(d => d.Id).ToList();

    public static Document? GetById(int id) => _docs.FirstOrDefault(d => d.Id == id);

    public static Document Add(Document doc)
    {
        doc.Id = _nextId++;
        doc.CreatedAt = DateTime.UtcNow;
        _docs.Add(doc);
        return doc;
    }

    public static bool UpdateStatus(int id, DocumentStatus status)
    {
        var doc = GetById(id);
        if (doc == null) return false;
        doc.Status = status;
        return true;
    }
}
