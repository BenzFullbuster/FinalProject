using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace FinalProject.Helpers
{
    public class GenerateInvoiceNumber
    {
        public async Task<string> AutoInvoiceNumber<T>(string prefix, string warehouseCode, DbSet<T> dbSet, Func<T, string> codeSelector) where T : class
        {
            // ดึงวันที่ปัจจุบัน
            DateTime currentDate = DateTime.Now;

            // สร้างเลขจากวันที่
            string invoiceNumber = $"{prefix}-{warehouseCode}-{currentDate:yyMMdd}";

            var filteredItems = await dbSet.AsNoTracking()
                .Where(item => codeSelector(item).StartsWith(invoiceNumber))
                .ToListAsync();

            int maxExistingNumber = filteredItems
                .Select(item =>
                {
                    string numberPart = codeSelector(item)[invoiceNumber.Length..];
                    Match match = Regex.Match(numberPart, @"\d+");
                    return match.Success ? int.Parse(match.Value) : 0;
                }).DefaultIfEmpty(0).Max();

            // เพิ่มเลขต่ำสุด
            int nextNumber = maxExistingNumber + 1;

            string completeInvoiceNumber = $"{invoiceNumber}{nextNumber:D5}";

            return completeInvoiceNumber;
        }
    }
}
