using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ImportService.Models;
using OfficeOpenXml;

namespace ImportService
{
    public class ExcelImportService : IExcelImportService
    {
        public async Task<ImportResult> ImportAsync(IFormFile file)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            var result = new ImportResult();
            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);
            using var package = new ExcelPackage(stream);
            var worksheet = package.Workbook.Worksheets[0];

            for (int row = 2; row <= worksheet.Dimension.End.Row; row++)
            {
                string value1 = worksheet.Cells[row, 1].Text;
                string value2 = worksheet.Cells[row, 2].Text;

                // Validate + import vào DB tại đây
                result.SuccessCount++;
            }

            return result;
        }
    }
}
