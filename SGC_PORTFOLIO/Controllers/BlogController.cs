using Microsoft.AspNetCore.Mvc;
using SGC_PORTFOLIO.Services;

namespace SGC_PORTFOLIO.Controllers
{
    public class BlogController : Controller
    {
        private readonly BlogService _blogService;

        public BlogController()
        {
            _blogService = new BlogService();
        }

        public IActionResult Index()
        {
            var blogEntries = _blogService.GetBlogEntries();
            return View(blogEntries);
        }

        public IActionResult Details(string title)
        {
            var blogEntries = _blogService.GetBlogEntries();
            var blogEntry = blogEntries.FirstOrDefault(b => b.Title == title);
            if (blogEntry == null)
            {
                return NotFound();
            }
            return View(blogEntry);
        }
    }
}
