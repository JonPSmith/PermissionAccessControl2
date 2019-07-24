using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ServiceLayer.CompanyServices;
using ServiceLayer.CompanyServices.Concrete;

namespace PermissionAccessControl2.Controllers
{
    public class CompanyController : Controller
    {
        public IActionResult Index([FromServices] IListCompaniesService service)
        {
            return View(service.BuildViewOfHierarchy());
        }
    }
}