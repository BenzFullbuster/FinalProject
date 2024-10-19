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
    public class UnitController(ConnectDb db, ILogger<UnitController> logger) : Controller
    {
        private readonly ConnectDb _db = db;
        private readonly ILogger<UnitController> _logger = logger;
        public async Task<IActionResult> Index(string searchString, string currentFilter, string sortOrder, int? pageSize, int currentPage = 1)
        {
            try
            {
                ViewData["CurrentSort"] = sortOrder;
                ViewData["NameSort"] = sortOrder == "name" ? "name_desc" : "name";
                ViewData["DescriptionSort"] = sortOrder == "description" ? "description_desc" : "description";

                searchString ??= currentFilter;

                ViewData["CurrentFilter"] = searchString;

                var query = (from x in _db.Unit
                             select new UnitViewModel
                             {
                                 UnitId = x.UnitId,
                                 UnitName = x.UnitName,
                                 Description = x.Description
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

                    query = query.Where(x => x.UnitName.ToLower().Contains(searchString.ToLower())
                        || x.Description!.ToLower().Contains(searchString.ToLower()));
                }

                query = sortOrder switch
                {
                    "name" => query.OrderBy(u => u.UnitName),
                    "name_desc" => query.OrderByDescending(u => u.UnitName),
                    "description" => query.OrderBy(u => u.Description),
                    "description_desc" => query.OrderByDescending(u => u.Description),
                    _ => query.OrderBy(u => u.UnitName),
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

        public async Task<IActionResult> Detail(Guid id)
        {
            if (id == Guid.Empty)
            {
                TempData["Warning"] = "ไม่มีข้อมูลในตาราง";
                _logger.LogWarning("ไม่มีข้อมูลในตาราง");
                return RedirectToAction("Index");
            }

            try
            {
                var item = await (from x in _db.Unit
                                  where x.UnitId == id
                                  select new UnitViewModel
                                  {
                                      UnitId = x.UnitId,
                                      UnitName = x.UnitName,
                                      Description = x.Description,
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
        public async Task<IActionResult> Create(UnitCreateViewModel model)
        {
            using var transaction = await _db.Database.BeginTransactionAsync();

            try
            {
                if (ModelState.IsValid)
                {
                    model.UnitName = model.UnitName.Trim();

                    if (await NameIsExists(Guid.Empty, model.UnitName))
                    {
                        ModelState.AddModelError("UnitName", "ชื่อนี้มีอยู่แล้ว");
                        return View(model);
                    }

                    var item = new Unit
                    {
                        UnitId = Guid.NewGuid(),
                        UnitName = model.UnitName,
                        Description = model.Description?.Trim(),
                        CreatedDate = DateTimeOffset.Now
                    };

                    await _db.Unit.AddAsync(item);
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

        public async Task<IActionResult> Edit(Guid id)
        {
            if (id == Guid.Empty)
            {
                TempData["Warning"] = "ไม่มีข้อมูลในตาราง";
                _logger.LogWarning("ไม่มีข้อมูลในตาราง");
                return RedirectToAction("Index");
            }

            try
            {
                var item = await (from x in _db.Unit
                                  where x.UnitId == id
                                  select new UnitEditViewModel
                                  {
                                      UnitId = x.UnitId,
                                      UnitName = x.UnitName,
                                      Description = x.Description,
                                      RowVersion = x.RowVersion
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
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, UnitEditViewModel model)
        {
            if (id == Guid.Empty)
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
                    model.UnitName = model.UnitName.Trim();

                    if (await NameIsExists(model.UnitId, model.UnitName))
                    {
                        ModelState.AddModelError("UnitName", "ชื่อนี้มีอยู่แล้ว");
                        return View(model);
                    }

                    var oldItem = await _db.Unit.FirstOrDefaultAsync(x => x.UnitId == id);

                    if (oldItem != null)
                    {
                        if (!oldItem.RowVersion!.SequenceEqual(model.RowVersion!))
                        {
                            TempData["Warning"] = "ข้อมูลถูกแก้ไขโดยผู้อื่นในขณะนี้ กรุณาลองอีกครั้งหรือติดต่อผู้ดูแลระบบ";
                            _logger.LogWarning("ข้อมูลถูกแก้ไขโดยผู้อื่นในขณะนี้");
                            return RedirectToAction("Edit", new { id });
                        }

                        bool isUpdated = false;

                        if (oldItem.UnitName != model.UnitName)
                        {
                            oldItem.UnitName = model.UnitName;
                            isUpdated = true;
                        }

                        if (oldItem.Description != model.Description)
                        {
                            oldItem.Description = model.Description?.Trim();
                            isUpdated = true;
                        }

                        if (isUpdated)
                        {
                            oldItem.ModifiedDate = DateTimeOffset.Now;
                            _db.Unit.Update(oldItem);
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

        public async Task<IActionResult> Delete(Guid id)
        {
            if (id == Guid.Empty)
            {
                TempData["Warning"] = "ไม่มีข้อมูลในตาราง";
                _logger.LogWarning("ไม่มีข้อมูลในตาราง");
                return RedirectToAction("Index");
            }

            try
            {
                var item = await (from x in _db.Unit
                                  where x.UnitId == id
                                  select new UnitViewModel
                                  {
                                      UnitId = x.UnitId,
                                      UnitName = x.UnitName,
                                      Description = x.Description,
                                      CreatedDate = x.CreatedDate,
                                      ModifiedDate = x.ModifiedDate,
                                      RowVersion = x.RowVersion
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
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id, byte[] rowVersion)
        {
            using var transaction = await _db.Database.BeginTransactionAsync();

            try
            {
                var item = await _db.Unit.FirstOrDefaultAsync(x => x.UnitId == id);

                if (item != null)
                {
                    if (!item.RowVersion!.SequenceEqual(rowVersion))
                    {
                        TempData["Error"] = "ข้อมูลถูกแก้ไขโดยผู้อื่นในขณะนี้ กรุณาลองอีกครั้งหรือติดต่อผู้ดูแลระบบ";
                        _logger.LogWarning("ข้อมูลถูกแก้ไขโดยผู้อื่นในขณะนี้");
                        return RedirectToAction("Index");
                    }

                    if (await CheckUnitInProduct(id))
                    {
                        TempData["Warning"] = $"ไม่สามารถลบ {item.UnitName} ได้เนื่องจากมีการอ้างอิงอยู่";
                        _logger.LogWarning("ไม่สามารถลบได้เนื่องจากมีการอ้างอิงอยู่");
                        return RedirectToAction("Index");
                    }

                    _db.Unit.Remove(item);
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
                var items = await (from x in _db.Unit
                                   orderby x.UnitName
                                   select new UnitViewModel
                                   {
                                       UnitId = x.UnitId,
                                       UnitName = x.UnitName,
                                       Description = x.Description,
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

                builder.AppendLine("UnitId,UnitName,Description,CreatedDate,ModifiedDate");

                foreach (var item in items)
                {
                    builder.AppendLine($"{item.UnitId}, {item.UnitName}, {item.Description}, " +
                        $"{item.CreatedDate}, {item.ModifiedDate}");
                }

                var csvData = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(builder.ToString())).ToArray();
                var contentType = "text/csv";
                var fileName = "รายการหน่วยนับ.csv";

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
                var items = await (from x in _db.Unit
                                   orderby x.UnitName
                                   select new UnitViewModel
                                   {
                                       UnitId = x.UnitId,
                                       UnitName = x.UnitName,
                                       Description = x.Description,
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

                var worksheet = workbook.Worksheets.Add("หน่วยนับ");

                var fontName = "Angsana New"; //กำหนด Font

                var headerRange = worksheet.Range("A1:E1");
                headerRange.Merge().Value = "รายการหน่วยนับ"; //ผลาสเซลล์
                headerRange.Style.Font.Bold = true; //ตัวหนา
                headerRange.Style.Font.FontSize = 20; //ขนาดตัวอักษร
                headerRange.Style.Font.FontName = fontName;
                headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center; //แนวตั้ง
                headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center; //แนวนอน

                worksheet.Cell("A2").Value = "รหัสหน่วยนับ";
                worksheet.Cell("B2").Value = "ชื่อหน่วยนับ";
                worksheet.Cell("C2").Value = "คำอธิบาย";
                worksheet.Cell("D2").Value = "วันที่สร้าง";
                worksheet.Cell("E2").Value = "วันที่แก้ไข";

                var headerColumn = worksheet.Range("A2:E2");
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
                    worksheet.Cell($"A{rowNumber}").Value = item.UnitId.ToString();
                    worksheet.Cell($"B{rowNumber}").Value = item.UnitName.ToString();
                    worksheet.Cell($"C{rowNumber}").Value = item.Description?.ToString();
                    worksheet.Cell($"D{rowNumber}").Value = item.CreatedDate?.ToString();
                    worksheet.Cell($"E{rowNumber}").Value = item.ModifiedDate?.ToString();

                    var row = worksheet.Range($"A{rowNumber}:E{rowNumber}");
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
                var fileName = "รายการหน่วยนับ.xlsx";

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
                var items = await (from x in _db.Unit
                                   orderby x.UnitName
                                   select new UnitViewModel
                                   {
                                       UnitName = x.UnitName,
                                       Description = x.Description,
                                       CreatedDate = x.CreatedDate,
                                       ModifiedDate = x.ModifiedDate
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

                        //page.Content().Row(row =>
                        //{
                        //    row.RelativeItem().AlignCenter().Text("รายงานหน่วยนับ")
                        //        .SemiBold().FontSize(30).FontColor(Colors.Blue.Medium);
                        //});

                        page.Header().ShowOnce().Text("รายงานหน่วยนับ").AlignCenter()
                        .SemiBold().FontSize(30).FontColor(Colors.Blue.Medium);

                        page.Content().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(50);
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.ConstantColumn(80);
                                columns.ConstantColumn(80);
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
                                header.Cell().Element(thead).Text("หน่วยนับ").SemiBold();
                                header.Cell().Element(thead).Text("คำอธิบาย").SemiBold();
                                header.Cell().Element(thead).Text("วันที่สร้าง").SemiBold();
                                header.Cell().Element(thead).Text("วันที่แก้ไข").SemiBold();
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
                                table.Cell().Element(tbody).Text(item.UnitName);
                                table.Cell().Element(tbody).Text(item.Description ?? "N/A");
                                table.Cell().Element(tbody).Text(item.CreatedDate?.ToString("dd/MM/yyyy")).AlignCenter();
                                table.Cell().Element(tbody).Text(item.ModifiedDate?.ToString("dd/MM/yyyy") ?? "N/A").AlignCenter();
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
                var fileName = "รายงานหน่วยนับ.pdf";

                return File(pdfBytes, contentType, fileName);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "เกิดข้อผิดพลาดในดาวน์โหลดข้อมูล กรุณาลองอีกครั้งหรือติดต่อผู้ดูแลระบบ";
                _logger.LogError(ex, "เกิดข้อผิดพลาดในดาวน์โหลดข้อมูล");
                return RedirectToAction("Index");
            }
        }

        private async Task<bool> NameIsExists(Guid unitId, string unitName)
        {
            return await _db.Unit.AsNoTracking().AnyAsync(x => x.UnitId != unitId && x.UnitName == unitName);
        }

        private async Task<bool> CheckUnitInProduct(Guid unitId)
        {
            return await _db.Product.AsNoTracking().AnyAsync(x => x.UnitId == unitId);
        }
    }
}
