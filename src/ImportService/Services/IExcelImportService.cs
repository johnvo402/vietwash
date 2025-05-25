using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ImportService.Models;

namespace ImportService.Service
{
    public interface IExcelImportService
    {
        Task<ImportResult> ImportAsync(IFormFile file);
    }
}
