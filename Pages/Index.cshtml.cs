using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RetailEdge.Web.DataAccess;

namespace RetailEdge.web.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(ILogger<IndexModel> logger)
    {
        _logger = logger;
    }

    public void OnGet()
    {
        try
    {
        using var conn = OracleHelper.GetConnection();
        ViewData["Message"] = "✅ Oracle DB Connected!";
    }
    catch (Exception ex)
    {
        ViewData["Message"] = "❌ Failed: " + ex.Message;
    }

    }
}
