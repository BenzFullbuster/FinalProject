using Microsoft.EntityFrameworkCore;

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

            // ดึงข้อมูลทั้งหมดและทำการประเมินผลฝั่งไคลเอนต์
            var items = await dbSet.AsNoTracking().ToListAsync();

            // ประเมินผลฝั่งไคลเอนต์เพื่อกรองข้อมูล
            var filteredItems = items.Where(item => codeSelector(item)
                .StartsWith(invoiceNumber)).ToList();

            //หาเลขต่ำสุด
            int minExistingNumber = filteredItems
                .Select(item => int.Parse(codeSelector(item)
                .Substring(invoiceNumber.Length)))
                .DefaultIfEmpty(0).Max();

            // เพิ่มเลขต่ำสุด
            int nextNumber = minExistingNumber + 1;

            string completeInvoiceNumber = $"{invoiceNumber}{nextNumber:D5}";

            return completeInvoiceNumber;
        }
    }
}
