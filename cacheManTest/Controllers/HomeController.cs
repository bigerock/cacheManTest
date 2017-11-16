using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using cacheManTest.Models;
using WebGrease.Css.Ast.Selectors;

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

        [HttpGet]
        [Route("home/removelocalstring/{num}")]
        public ActionResult RemoveLocalString(int num)
        {
            var vm = new HomeViewmodel(_cacheHandler);
            return vm.RemoveLocalString(num)
                ? new HttpStatusCodeResult(HttpStatusCode.OK)
                : new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
        }

        [HttpGet]
        [Route("home/removestring/{num}")]
        public ActionResult RemoveString(int num)
        {
            var vm = new HomeViewmodel(_cacheHandler);
            return vm.RemoveString(num)
                ? new HttpStatusCodeResult(HttpStatusCode.OK)
                : new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
        }


        [HttpGet]
        [Route("home/getstring/{num}")]
        public ActionResult GetString(int num)
        {
            var vm = new HomeViewmodel(_cacheHandler);
            var str = vm.GetString(num);
            return new ContentResult {Content = str};

        }

    }
}