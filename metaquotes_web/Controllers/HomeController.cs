using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace metaquotes_web.Controllers
{
    // Disable Authorization support
    //[Authorize]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }
    }
}
