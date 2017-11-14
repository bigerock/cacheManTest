using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using cacheManTest.Models;

namespace cacheManTest.Controllers
{
    public class HomeController : Controller
    {
        private readonly ICacheHandler _cacheHandler;
        
        public HomeController(ICacheHandler cacheHandler)
        {
            _cacheHandler = cacheHandler;
        }

        public ActionResult Index()
        {
            var vm = new HomeViewmodel(_cacheHandler);
            return View("Index", vm);
        }

    }
}