using ClosedXML.Excel;
using FinalProject.Helpers;
using FinalProject.Models;
using FinalProject.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using System.Text;

namespace FinalProject.Controllers
{
    public class ProvinceController(ConnectDb db, ILogger<ProvinceController> logger) : Controller
    {
        private readonly ConnectDb _db = db;
        private readonly ILogger<ProvinceController> _logger = logger;
        public async Task<IActionResult> Index(string searchString, string currentFilter, string sortOrder, int? pageSize, int currentPage = 1)
        {
            try
            {
                ViewData["CurrentSort"] = sortOrder;
                ViewData["NameSort"] = sortOrder == "name" ? "name_desc" : "name";

                searchString ??= currentFilter;

                ViewData["CurrentFilter"] = searchString;

                var query = (from x in _db.Province
                             select new ProvinceViewModel
                             {
                                 ProvinceId = x.ProvinceId,
                                 ProvinceName = x.ProvinceName
                             }).AsNoTracking();

                if (!query.Any())
                {
                    TempData["Warning"] = "ไม่มีข้อมูลในตาราง";
                    _logger.LogWarning("ไม่มีข้อมูลในตาราง");
                    return View();
                }

                if (!string.IsNullOrEmpty(searchString))
                {
                    searchString = searchString.Trim();

                    query = query.Where(x => x.ProvinceName.ToLower().Contains(searchString.ToLower()));
                }

                query = sortOrder switch
                {
                    "name" => query.OrderBy(u => u.ProvinceName),
                    "name_desc" => query.OrderByDescending(u => u.ProvinceName),
                    _ => query.OrderBy(u => u.ProvinceName),
                };

                ViewData["PageSize"] = pageSize;

                int pageSizeValue = pageSize ?? 10;
                if (pageSizeValue <= 0)
                    pageSizeValue = 10;

                int itemcount = await query.CountAsync();
                if (currentPage <= 0)
                    currentPage = 1;

                if (currentPage < 0)
                    currentPage = 1;

                var pager = new DataPager(itemcount, currentPage, pageSizeValue);
                int skipRows = (currentPage - 1) * pageSizeValue;

                ViewData["Pager"] = pager;

                var items = await query.Skip(skipRows).Take(pageSizeValue).ToListAsync();

                if (items.Count == 0)
                {
                    TempData["Warning"] = "ไม่มีข้อมูลในตาราง";
                    _logger.LogWarning("ไม่มีข้อมูลในตาราง");
                    return View();
                }

                _logger.LogInformation("ดึงข้อมูลสำเร็จ");
                return View(items);
            }
            catch (SqlException ex)
            {
                TempData["Error"] = "เกิดข้อผิดพลาดในการเชื่อมต่อกับฐานข้อมูล กรุณาลองอีกครั้งหรือติดต่อผู้ดูแลระบบ";
                _logger.LogError(ex, "เกิดข้อผิดพลาดในการเชื่อมต่อกับฐานข้อมูล");
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "เกิดข้อผิดพลาดในการดึงข้อมูล กรุณาลองอีกครั้งหรือติดต่อผู้ดูแลระบบ";
                _logger.LogError(ex, "เกิดข้อผิดพลาดในการดึงข้อมูล");
                return RedirectToAction("Index");
            }
        }

        public async Task<IActionResult> Detail(int id)
        {
            if (id < 0)
            {
                TempData["Warning"] = "ไม่มีข้อมูลในตาราง";
                _logger.LogWarning("ไม่มีข้อมูลในตาราง");
                return RedirectToAction("Index");
            }

            try
            {
                var item = await (from x in _db.Province
                                  where x.ProvinceId == id
                                  select new ProvinceViewModel
                                  {
                                      ProvinceId = x.ProvinceId,
                                      ProvinceName = x.ProvinceName,
                                      CreatedDate = x.CreatedDate,
                                      ModifiedDate = x.ModifiedDate
                                  }).AsNoTracking().FirstOrDefaultAsync();

                if (item == null)
                {
                    TempData["Warning"] = "ไม่มีข้อมูลในตาราง";
                    _logger.LogWarning("ไม่มีข้อมูลในตาราง");
                    return RedirectToAction("Index");
                }

                _logger.LogInformation("ดึงข้อมูลสำเร็จ");
                return View(item);
            }
            catch (SqlException ex)
            {
                TempData["Error"] = "เกิดข้อผิดพลาดในการเชื่อมต่อกับฐานข้อมูล กรุณาลองอีกครั้งหรือติดต่อผู้ดูแลระบบ";
                _logger.LogError(ex, "เกิดข้อผิดพลาดในการเชื่อมต่อกับฐานข้อมูล");
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "เกิดข้อผิดพลาดในการดึงข้อมูล กรุณาลองอีกครั้งหรือติดต่อผู้ดูแลระบบ";
                _logger.LogError(ex, "เกิดข้อผิดพลาดในการดึงข้อมูล");
                return RedirectToAction("Index");
            }
        }

        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProvinceCreateViewModel model)
        {
            using var transaction = await _db.Database.BeginTransactionAsync();

            try
            {
                if (ModelState.IsValid)
                {
                    model.ProvinceName = model.ProvinceName.Trim();

                    if (await NameIsExists(0, model.ProvinceName))
                    {
                        ModelState.AddModelError("ProvinceName", "ชื่อนี้มีอยู่แล้ว");
                        return View(model);
                    }

                    var item = new Province
                    {
                        ProvinceName = model.ProvinceName,
                        CreatedDate = DateTimeOffset.Now
                    };

                    await _db.Province.AddAsync(item);
                    await _db.SaveChangesAsync();
                    await transaction.CommitAsync();
                    TempData["Success"] = "เพิ่มข้อมูลสำเร็จ";
                    _logger.LogInformation("เพิ่มข้อมูลสำเร็จ");
                    return RedirectToAction("Index");
                }

                TempData["Warning"] = "กรุณากรอกข้อมูลให้ถูกต้องและครบถ้วน";
                _logger.LogWarning("กรุณากรอกข้อมูลให้ถูกต้องและครบถ้วน");
                return View(model);
            }
            catch (DbUpdateException ex)
            {
                TempData["Warning"] = "เกิดข้อผิดพลาดในการเพิ่มข้อมูล กรุณาลองอีกครั้งหรือติดต่อผู้ดูแลระบบ";
                _logger.LogWarning(ex, "เกิดข้อผิดพลาดในการเพิ่มข้อมูล");
                await transaction.RollbackAsync();
                return RedirectToAction("Index");
            }
            catch (SqlException ex)
            {
                TempData["Error"] = "เกิดข้อผิดพลาดในการเชื่อมต่อกับฐานข้อมูล กรุณาลองอีกครั้งหรือติดต่อผู้ดูแลระบบ";
                _logger.LogError(ex, "เกิดข้อผิดพลาดในการเชื่อมต่อกับฐานข้อมูล");
                await transaction.RollbackAsync();
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "เกิดข้อผิดพลาดที่ไม่คาดคิด กรุณาลองอีกครั้งหรือติดต่อผู้ดูแลระบบ";
                _logger.LogError(ex, "เกิดข้อผิดพลาดที่ไม่คาดคิด");
                await transaction.RollbackAsync();
                return RedirectToAction("Index");
            }
        }

        public async Task<IActionResult> Edit(int id)
        {
            if (id < 0)
            {
                TempData["Warning"] = "ไม่มีข้อมูลในตาราง";
                _logger.LogWarning("ไม่มีข้อมูลในตาราง");
                return RedirectToAction("Index");
            }

            try
            {
                var item = await (from x in _db.Province
                                  where x.ProvinceId == id
                                  select new ProvinceEditViewModel
                                  {
                                      ProvinceId = x.ProvinceId,
                                      ProvinceName = x.ProvinceName,
                                      RowVersion = x.RowVersion
                                  }).FirstOrDefaultAsync();

                if (item == null)
                {
                    TempData["Warning"] = "ไม่มีข้อมูลในตาราง";
                    _logger.LogWarning("ไม่มีข้อมูลในตาราง");
                    return RedirectToAction("Index");
                }

                _logger.LogInformation("ดึงข้อมูลสำเร็จ");
                return View(item);
            }
            catch (SqlException ex)
            {
                TempData["Error"] = "เกิดข้อผิดพลาดในการเชื่อมต่อกับฐานข้อมูล กรุณาลองอีกครั้งหรือติดต่อผู้ดูแลระบบ";
                _logger.LogError(ex, "เกิดข้อผิดพลาดในการเชื่อมต่อกับฐานข้อมูล");
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "เกิดข้อผิดพลาดในการดึงข้อมูล กรุณาลองอีกครั้งหรือติดต่อผู้ดูแลระบบ";
                _logger.LogError(ex, "เกิดข้อผิดพลาดในการดึงข้อมูล");
                return RedirectToAction("Index");
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProvinceEditViewModel model)
        {
            if (id < 0)
            {
                TempData["Warning"] = "ไม่มีข้อมูลในตาราง";
                _logger.LogWarning("ไม่มีข้อมูลในตาราง");
                return RedirectToAction("Index");
            }

            using var transaction = await _db.Database.BeginTransactionAsync();

            try
            {
                if (ModelState.IsValid)
                {
                    model.ProvinceName = model.ProvinceName.Trim();

                    if (await NameIsExists(model.ProvinceId, model.ProvinceName))
                    {
                        ModelState.AddModelError("ProvinceName", "ชื่อนี้มีอยู่แล้ว");
                        return View(model);
                    }

                    var oldItem = await _db.Province.FirstOrDefaultAsync(x => x.ProvinceId == id);

                    if (oldItem != null)
                    {
                        if (!oldItem.RowVersion!.SequenceEqual(model.RowVersion!))
                        {
                            TempData["Warning"] = "ข้อมูลถูกแก้ไขโดยผู้อื่นในขณะนี้ กรุณาลองอีกครั้งหรือติดต่อผู้ดูแลระบบ";
                            _logger.LogWarning("ข้อมูลถูกแก้ไขโดยผู้อื่นในขณะนี้");
                            return RedirectToAction("Edit", new { id });
                        }

                        bool isUpdated = false;

                        if (oldItem.ProvinceName != model.ProvinceName)
                        {
                            oldItem.ProvinceName = model.ProvinceName;
                            isUpdated = true;
                        }

                        if (isUpdated)
                        {
                            oldItem.ModifiedDate = DateTimeOffset.Now;
                            _db.Province.Update(oldItem);
                            await _db.SaveChangesAsync();
                            TempData["Success"] = "แก้ไขข้อมูลสำเร็จ";
                            _logger.LogInformation("แก้ไขข้อมูลสำเร็จ");
                            await transaction.CommitAsync();
                        }
                        return RedirectToAction("Index");
                    }
                }

                TempData["Warning"] = "กรุณากรอกข้อมูลให้ถูกต้องและครบถ้วน";
                _logger.LogWarning("กรุณากรอกข้อมูลให้ถูกต้องและครบถ้วน");
                return View(model);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                TempData["Error"] = "ข้อมูลถูกแก้ไขโดยผู้อื่นในขณะนี้ กรุณาลองอีกครั้งหรือติดต่อผู้ดูแลระบบ";
                _logger.LogError(ex, "ข้อมูลถูกแก้ไขโดยผู้อื่นในขณะนี้");
                await transaction.RollbackAsync();
                return RedirectToAction("Index");
            }
            catch (DbUpdateException ex)
            {
                TempData["Error"] = "เกิดข้อผิดพลาดในการแก้ไขข้อมูล กรุณาลองอีกครั้งหรือติดต่อผู้ดูแลระบบ";
                _logger.LogError(ex, "เกิดข้อผิดพลาดในการแก้ไขข้อมูล");
                await transaction.RollbackAsync();
                return RedirectToAction("Index");
            }
            catch (SqlException ex)
            {
                TempData["Error"] = "เกิดข้อผิดพลาดในการเชื่อมต่อกับฐานข้อมูล กรุณาลองอีกครั้งหรือติดต่อผู้ดูแลระบบ";
                _logger.LogError(ex, "เกิดข้อผิดพลาดในการเชื่อมต่อกับฐานข้อมูล");
                await transaction.RollbackAsync();
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "เกิดข้อผิดพลาดที่ไม่คาดคิด กรุณาลองอีกครั้งหรือติดต่อผู้ดูแลระบบ";
                _logger.LogError(ex, "เกิดข้อผิดพลาดที่ไม่คาดคิด");
                await transaction.RollbackAsync();
                return RedirectToAction("Index");
            }
        }

        public async Task<IActionResult> Delete(int id)
        {
            if (id < 0)
            {
                TempData["Warning"] = "ไม่มีข้อมูลในตาราง";
                _logger.LogWarning("ไม่มีข้อมูลในตาราง");
                return RedirectToAction("Index");
            }

            try
            {
                var item = await (from x in _db.Province
                                  where x.ProvinceId == id
                                  select new ProvinceViewModel
                                  {
                                      ProvinceId = x.ProvinceId,
                                      ProvinceName = x.ProvinceName,
                                      CreatedDate = x.CreatedDate,
                                      ModifiedDate = x.ModifiedDate,
                                      RowVersion = x.RowVersion
                                  }).FirstOrDefaultAsync();

                if (item == null)
                {
                    TempData["Warning"] = "ไม่มีข้อมูลในตาราง";
                    _logger.LogWarning("ไม่มีข้อมูลในตาราง");
                    return RedirectToAction("Index");
                }

                _logger.LogInformation("ดึงข้อมูลสำเร็จ");
                return View(item);
            }
            catch (SqlException ex)
            {
                TempData["Error"] = "เกิดข้อผิดพลาดในการเชื่อมต่อกับฐานข้อมูล กรุณาลองอีกครั้งหรือติดต่อผู้ดูแลระบบ";
                _logger.LogError(ex, "เกิดข้อผิดพลาดในการเชื่อมต่อกับฐานข้อมูล");
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "เกิดข้อผิดพลาดในการดึงข้อมูล กรุณาลองอีกครั้งหรือติดต่อผู้ดูแลระบบ";
                _logger.LogError(ex, "เกิดข้อผิดพลาดในการดึงข้อมูล");
                return RedirectToAction("Index");
            }
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, byte[] rowVersion)
        {
            using var transaction = await _db.Database.BeginTransactionAsync();

            try
            {
                var item = await _db.Province.FirstOrDefaultAsync(x => x.ProvinceId == id);

                if (item != null)
                {
                    if (!item.RowVersion!.SequenceEqual(rowVersion))
                    {
                        TempData["Error"] = "ข้อมูลถูกแก้ไขโดยผู้อื่นในขณะนี้ กรุณาลองอีกครั้งหรือติดต่อผู้ดูแลระบบ";
                        _logger.LogWarning("ข้อมูลถูกแก้ไขโดยผู้อื่นในขณะนี้");
                        return RedirectToAction("Index");
                    }

                    if (await CheckProvinceInDistrict(id))
                    {
                        TempData["Warning"] = $"ไม่สามารถลบ {item.ProvinceName} ได้เนื่องจากมีการอ้างอิงอยู่";
                        _logger.LogWarning("ไม่สามารถลบได้เนื่องจากมีการอ้างอิงอยู่");
                        return RedirectToAction("Index");
                    }

                    _db.Province.Remove(item);
                    await _db.SaveChangesAsync();
                    await transaction.CommitAsync();
                    TempData["Success"] = "ลบข้อมูลสำเร็จ";
                    _logger.LogInformation("ลบข้อมูลสำเร็จ");
                    return RedirectToAction("Index");
                }

                TempData["Warning"] = "ไม่พบข้อมูลที่ต้องการลบ";
                _logger.LogWarning("ไม่พบข้อมูลที่ต้องการลบ");
                return RedirectToAction("Index");
            }
            catch (DbUpdateException ex)
            {
                TempData["Error"] = "เกิดข้อผิดพลาดในการลบข้อมูล กรุณาลองอีกครั้งหรือติดต่อผู้ดูแลระบบ";
                _logger.LogError(ex, "เกิดข้อผิดพลาดในการลบข้อมูล");
                await transaction.RollbackAsync();
                return RedirectToAction("Index");
            }
            catch (SqlException ex)
            {
                TempData["Error"] = "เกิดข้อผิดพลาดในการเชื่อมต่อกับฐานข้อมูล กรุณาลองอีกครั้งหรือติดต่อผู้ดูแลระบบ";
                _logger.LogError(ex, "เกิดข้อผิดพลาดในการเชื่อมต่อกับฐานข้อมูล");
                await transaction.RollbackAsync();
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "เกิดข้อผิดพลาดที่ไม่คาดคิด กรุณาลองอีกครั้งหรือติดต่อผู้ดูแลระบบ";
                _logger.LogError(ex, "เกิดข้อผิดพลาดที่ไม่คาดคิด");
                await transaction.RollbackAsync();
                return RedirectToAction("Index");
            }
        }

        public async Task<IActionResult> ExportCsv()
        {
            try
            {
                var items = await (from x in _db.Province
                                   orderby x.ProvinceName
                                   select new ProvinceViewModel
                                   {
                                       ProvinceId = x.ProvinceId,
                                       ProvinceName = x.ProvinceName,
                                       CreatedDate = x.CreatedDate,
                                       ModifiedDate = x.ModifiedDate
                                   }).AsNoTracking().ToListAsync();

                if (items == null || items.Count == 0)
                {
                    TempData["Warning"] = "ไม่มีข้อมูลในตาราง";
                    _logger.LogWarning("ไม่มีข้อมูลในตาราง");
                    return View();
                }

                var builder = new StringBuilder();

                builder.AppendLine("ProvinceId,ProvinceName,CreatedDate,ModifiedDate");

                foreach (var item in items)
                {
                    builder.AppendLine($"{item.ProvinceId}, {item.ProvinceName}, {item.CreatedDate}, {item.ModifiedDate}");
                }

                var csvData = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(builder.ToString())).ToArray();
                var contentType = "text/csv";
                var fileName = "รายการจังหวัด.csv";

                return File(csvData, contentType, fileName);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "เกิดข้อผิดพลาดในดาวน์โหลดข้อมูล กรุณาลองอีกครั้งหรือติดต่อผู้ดูแลระบบ";
                _logger.LogError(ex, "เกิดข้อผิดพลาดในดาวน์โหลดข้อมูล");
                return RedirectToAction("Index");
            }
        }

        public async Task<IActionResult> ExportExcel()
        {
            try
            {
                var items = await (from x in _db.Province
                                   orderby x.ProvinceName
                                   select new ProvinceViewModel
                                   {
                                       ProvinceId = x.ProvinceId,
                                       ProvinceName = x.ProvinceName,
                                       CreatedDate = x.CreatedDate,
                                       ModifiedDate = x.ModifiedDate
                                   }).AsNoTracking().ToListAsync();

                if (items == null || items.Count == 0)
                {
                    TempData["Warning"] = "ไม่มีข้อมูลในตาราง";
                    _logger.LogWarning("ไม่มีข้อมูลในตาราง");
                    return View();
                }

                using var workbook = new XLWorkbook();

                var worksheet = workbook.Worksheets.Add("จังหวัด");

                var fontName = "Angsana New"; //กำหนด Font

                var headerRange = worksheet.Range("A1:D1");
                headerRange.Merge().Value = "รายการจังหวัด"; //ผลาสเซลล์
                headerRange.Style.Font.Bold = true; //ตัวหนา
                headerRange.Style.Font.FontSize = 20; //ขนาดตัวอักษร
                headerRange.Style.Font.FontName = fontName;
                headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center; //แนวตั้ง
                headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center; //แนวนอน

                worksheet.Cell("A2").Value = "รหัสจังหวัด";
                worksheet.Cell("B2").Value = "ชื่อจังหวัด";
                worksheet.Cell("C2").Value = "วันที่สร้าง";
                worksheet.Cell("D2").Value = "วันที่แก้ไข";

                var headerColumn = worksheet.Range("A2:D2");
                headerColumn.Style.Font.Bold = true;
                headerColumn.Style.Font.FontSize = 16;
                headerColumn.Style.Font.FontName = fontName;
                headerColumn.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                headerColumn.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                headerColumn.Style.Border.OutsideBorder = XLBorderStyleValues.Thin; //เส้นตารางด้านนอก
                headerColumn.Style.Border.InsideBorder = XLBorderStyleValues.Thin; //เส้นตารางด้านใน

                int rowNumber = 3;
                foreach (var item in items)
                {
                    worksheet.Cell($"A{rowNumber}").Value = item.ProvinceId.ToString();
                    worksheet.Cell($"B{rowNumber}").Value = item.ProvinceName.ToString();
                    worksheet.Cell($"C{rowNumber}").Value = item.CreatedDate?.ToString();
                    worksheet.Cell($"D{rowNumber}").Value = item.ModifiedDate?.ToString();

                    var row = worksheet.Range($"A{rowNumber}:D{rowNumber}");
                    row.Style.Font.FontName = fontName;
                    row.Style.Font.FontSize = 16;
                    row.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    row.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                    rowNumber++;
                }

                worksheet.Columns().AdjustToContents(); // ปรับขนาดคอลัมน์ Auto

                using var stream = new MemoryStream();

                workbook.SaveAs(stream);

                var fileContent = stream.ToArray();
                var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                var fileName = "รายการจังหวัด.xlsx";

                return File(fileContent, contentType, fileName);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "เกิดข้อผิดพลาดในดาวน์โหลดข้อมูล กรุณาลองอีกครั้งหรือติดต่อผู้ดูแลระบบ";
                _logger.LogError(ex, "เกิดข้อผิดพลาดในดาวน์โหลดข้อมูล");
                return RedirectToAction("Index");
            }
        }

        public async Task<IActionResult> ExportPDF()
        {
            try
            {
                var items = await (from x in _db.Province
                                   orderby x.ProvinceName
                                   select new ProvinceViewModel
                                   {
                                       ProvinceName = x.ProvinceName
                                   }).AsNoTracking().ToListAsync();

                if (items == null || items.Count == 0)
                {
                    TempData["Warning"] = "ไม่มีข้อมูลในตาราง";
                    _logger.LogWarning("ไม่มีข้อมูลในตาราง");
                    return RedirectToAction("Index");
                }

                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4.Portrait());
                        page.Margin(1, QuestPDF.Infrastructure.Unit.Centimetre);
                        page.PageColor(Colors.White);

                        page.Header().ShowOnce().Text("รายงานจังหวัด").AlignCenter()
                        .SemiBold().FontSize(30).FontColor(Colors.Blue.Medium);

                        page.Content().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(50);
                                columns.RelativeColumn();
                            });

                            QuestPDF.Infrastructure.IContainer thead(QuestPDF.Infrastructure.IContainer container)
                            {
                                return container.Border(1)
                                .BorderColor(Colors.Black)
                                .Padding(5).AlignMiddle().AlignCenter();
                            }

                            table.Header(header =>
                            {
                                header.Cell().Element(thead).Text("ลำดับ").SemiBold();
                                header.Cell().Element(thead).Text("จังหวัด").SemiBold();
                            });

                            QuestPDF.Infrastructure.IContainer tbody(QuestPDF.Infrastructure.IContainer container)
                            {
                                return container.Border(1)
                                .BorderColor(Colors.Black)
                                .Padding(5).AlignMiddle();
                            }


                            for (int i = 0; i < items.Count; i++)
                            {
                                var item = items[i];

                                table.Cell().Element(tbody).Text((i + 1).ToString()).AlignCenter();
                                table.Cell().Element(tbody).Text(item.ProvinceName);
                            }
                        });

                        page.Footer().AlignRight().Text(x =>
                        {
                            x.CurrentPageNumber().FontSize(8);
                            x.Span("/").FontSize(8);
                            x.TotalPages().FontSize(8);
                        });
                    });
                });

                byte[] pdfBytes = document.GeneratePdf();
                var contentType = "application/pdf";
                var fileName = "รายงานจังหวัด.pdf";

                return File(pdfBytes, contentType, fileName);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "เกิดข้อผิดพลาดในดาวน์โหลดข้อมูล กรุณาลองอีกครั้งหรือติดต่อผู้ดูแลระบบ";
                _logger.LogError(ex, "เกิดข้อผิดพลาดในดาวน์โหลดข้อมูล");
                return RedirectToAction("Index");
            }
        }

        private async Task<bool> NameIsExists(int provinceId, string provinceName)
        {
            return await _db.Province.AsNoTracking().AnyAsync(x => x.ProvinceId != provinceId && x.ProvinceName == provinceName);
        }

        private async Task<bool> CheckProvinceInDistrict(int provinceId)
        {
            return await _db.District.AsNoTracking().AnyAsync(x => x.ProvinceId == provinceId);
        }
    }
}
