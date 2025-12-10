using Microsoft.AspNetCore.Mvc;
using OnePro.Front.Middleware;

namespace OnePro.Front.Controllers
{
    // wajib login
    [AuthRequired]
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            return View("Index");
        }
    }
}
