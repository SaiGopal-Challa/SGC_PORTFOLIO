using Microsoft.AspNetCore.Mvc;
using SGC_PORTFOLIO.Services;
using SGC_PORTFOLIO.Models;
using System.Linq;

namespace SGC_PORTFOLIO.Controllers
{
    public class BlogController : Controller
    {
        private readonly BlogService _blogService;

        public BlogController(BlogService blogService)
        {
            _blogService = blogService;
        }

        public IActionResult Index()
        {
            var blogEntries = _blogService.GetBlogEntries();
            return View(blogEntries);
        }

        public IActionResult Details(string title)
        {
            if (string.IsNullOrEmpty(title))
            {
                return BadRequest("Blog title is required.");
            }

            var blogEntries = _blogService.GetBlogEntries();
            var blogEntry = blogEntries.FirstOrDefault(b => string.Equals(b.Title, title, System.StringComparison.OrdinalIgnoreCase));

            if (blogEntry == null)
            {
                return NotFound($"Blog entry with title '{title}' not found.");
            }

            return View(blogEntry);
        }
    }
}
