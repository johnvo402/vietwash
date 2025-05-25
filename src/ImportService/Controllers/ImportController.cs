using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using ImportService.Models;
using ImportService.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ImportService.Controllers
{
    [Route("[controller]")]
    public class ImportController : Controller
    {
        private readonly IExcelImportService _excelImportService;

        public ImportController(IExcelImportService excelImportService)
        {
            _excelImportService = excelImportService;
        }

        private bool IsLoggedIn() => HttpContext.Session.GetString("User") != null;

        [HttpGet]
        public IActionResult Upload()
        {
            if (!IsLoggedIn())
                return RedirectToAction("Login", "Account");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (!IsLoggedIn())
                return RedirectToAction("Login", "Account");

            if (file == null || file.Length == 0)
            {
                ModelState.AddModelError("", "Vui lòng chọn file Excel.");
                return View();
            }

            var result = await _excelImportService.ImportAsync(file);
            ViewBag.Result = result;
            return View();
        }
    }
}
