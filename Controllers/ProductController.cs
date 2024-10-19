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
    public class ProductController(ConnectDb db, ILogger<ProductController> logger) : Controller
    {
        private readonly ConnectDb _db = db;
        private readonly ILogger<ProductController> _logger = logger;

        protected string? warehouseCode { get; private set; }
        protected Guid warehouseId { get; private set; }

        protected async Task SetWarehouse()
        {
            warehouseCode = User.GetWarehouseCode();
            warehouseId = await _db.GetWarehouseId(warehouseCode);
        }

        public async Task<IActionResult> Index(string searchBy, string searchString, string currentFilter, string sortOrder, int? pageSize, int currentPage = 1/*, bool? inStock = false*/)
        {
            try
            {
                //ViewData["InStock"] = inStock;
                ViewData["CurrentSort"] = sortOrder;
                ViewData["NameSort"] = sortOrder == "name" ? "name_desc" : "name";
                ViewData["CategorySort"] = sortOrder == "category" ? "category_desc" : "category";
                ViewData["UnitSort"] = sortOrder == "unit" ? "unit_desc" : "unit";
                ViewData["SellpriceSort"] = sortOrder == "sellPrice" ? "sellPrice_desc" : "sellPrice";

                searchString ??= currentFilter;

                ViewData["SearchBy"] = searchBy;
                ViewData["CurrentFilter"] = searchString;

                await SetWarehouse();

                var query = (from i in _db.Inventory
                             join p in _db.Product on i.ProductId equals p.ProductId
                             join c in _db.Category on p.CategoryId equals c.CategoryId
                             join u in _db.Unit on p.UnitId equals u.UnitId
                             where i.WarehouseId == warehouseId
                             select new ProductViewModel
                             {
                                 ProductId = p.ProductId,
                                 ProductName = p.ProductName,
                                 CategoryName = c.CategoryName,
                                 UnitName = u.UnitName,
                                 Sellprice = p.Sellprice,
                                 Balance = i.Balance
                             }).AsNoTracking();

                if (!query.Any())
                {
                    TempData["Warning"] = "ไม่มีข้อมูลในตาราง";
                    _logger.LogWarning("ไม่มีข้อมูลในตาราง");
                    return View();
                }

                /*if (inStock == false)
                    query = query.Where(x => x.Balance > 0);*/

                if (!string.IsNullOrEmpty(searchString))
                {
                    searchString = searchString.Trim();

                    query = searchBy switch
                    {
                        "ProductName" => query = query.Where(x => x.ProductName.ToLower().Contains(searchString.ToLower())),
                        "CategoryName" => query = query.Where(x => x.CategoryName!.ToLower().Contains(searchString.ToLower())),
                        "UnitName" => query = query.Where(x => x.UnitName!.ToLower().Contains(searchString.ToLower())),
                        _ => query
                    };
                }

                query = sortOrder switch
                {
                    "name" => query.OrderBy(x => x.ProductName),
                    "name_desc" => query.OrderByDescending(x => x.ProductName),
                    "category" => query.OrderBy(x => x.CategoryName),
                    "category_desc" => query.OrderByDescending(x => x.CategoryName),
                    "unit" => query.OrderBy(x => x.UnitName),
                    "unit_desc" => query.OrderByDescending(x => x.UnitName),
                    "sellPrice" => query.OrderBy(x => x.Sellprice),
                    "sellPrice_desc" => query.OrderByDescending(x => x.Sellprice),
                    _ => query.OrderBy(x => x.ProductName)
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
                await SetWarehouse();

                var item = await (from i in _db.Inventory
                                  join p in _db.Product on i.ProductId equals p.ProductId
                                  join c in _db.Category on p.CategoryId equals c.CategoryId
                                  join u in _db.Unit on p.UnitId equals u.UnitId
                                  where i.WarehouseId == warehouseId && p.ProductId == id
                                  select new ProductViewModel
                                  {
                                      ProductId = p.ProductId,
                                      ProductName = p.ProductName,
                                      CategoryName = c.CategoryName,
                                      UnitName = u.UnitName,
                                      Sellprice = p.Sellprice,
                                      Balance = i.Balance,
                                      Description = p.Description,
                                      CreatedDate = p.CreatedDate,
                                      ModifiedDate = p.ModifiedDate
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
        public async Task<IActionResult> Create(ProductCreateViewModel model)
        {
            using var transaction = await _db.Database.BeginTransactionAsync();

            try
            {
                if (ModelState.IsValid)
                {
                    model.ProductName = model.ProductName.Trim();

                    if (await NameIsExists(Guid.Empty, model.ProductName))
                    {
                        ModelState.AddModelError("ProductName", "ชื่อนี้มีอยู่แล้ว");
                        return View(model);
                    }

                    var item = new Product
                    {
                        ProductId = Guid.NewGuid(),
                        ProductName = model.ProductName,
                        CategoryId = model.CategoryId,
                        UnitId = model.UnitId,
                        Sellprice = model.Sellprice,
                        Description = model.Description?.Trim(),
                        CreatedDate = DateTimeOffset.Now,
                    };

                    await _db.Product.AddAsync(item);

                    await SetWarehouse();

                    var inventory = new Inventory
                    {
                        ProductId = item.ProductId,
                        WarehouseId = warehouseId,
                        Balance = 0,
                        LastUpdated = DateTimeOffset.Now
                    };

                    await _db.Inventory.AddAsync(inventory);

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
                await SetWarehouse();

                var item = await (from p in _db.Product
                                  join c in _db.Category on p.CategoryId equals c.CategoryId
                                  join u in _db.Unit on p.UnitId equals u.UnitId
                                  where p.ProductId == id
                                  select new ProductEditViewModel
                                  {
                                      ProductId = p.ProductId,
                                      ProductName = p.ProductName,
                                      CategoryId = c.CategoryId,
                                      CategoryName = c.CategoryName,
                                      UnitId = u.UnitId,
                                      UnitName = u.UnitName,
                                      Sellprice = p.Sellprice,
                                      Description = p.Description,
                                      RowVersion = p.RowVersion
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
        public async Task<IActionResult> Edit(Guid id, ProductEditViewModel model)
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
                    model.ProductName = model.ProductName.Trim();

                    if (await NameIsExists(model.ProductId, model.ProductName))
                    {
                        ModelState.AddModelError("ProductName", "ชื่อนี้มีอยู่แล้ว");
                        return View(model);
                    }

                    var oldItem = await _db.Product.FirstOrDefaultAsync(x => x.ProductId == id);

                    if (oldItem != null)
                    {
                        if (!oldItem.RowVersion!.SequenceEqual(model.RowVersion!))
                        {
                            TempData["Warning"] = "ข้อมูลถูกแก้ไขโดยผู้อื่นในขณะนี้ กรุณาลองอีกครั้งหรือติดต่อผู้ดูแลระบบ";
                            _logger.LogWarning("ข้อมูลถูกแก้ไขโดยผู้อื่นในขณะนี้");
                            return RedirectToAction("Edit", new { id });

                        }

                        bool isUpdated = false;

                        if (oldItem.ProductName != model.ProductName)
                        {
                            oldItem.ProductName = model.ProductName;
                            isUpdated = true;
                        }

                        if (oldItem.CategoryId != model.CategoryId)
                        {
                            oldItem.CategoryId = model.CategoryId;
                            isUpdated = true;
                        }

                        if (oldItem.UnitId != model.UnitId)
                        {
                            oldItem.UnitId = model.UnitId;
                            isUpdated = true;
                        }

                        if (oldItem.Sellprice != model.Sellprice)
                        {
                            oldItem.Sellprice = model.Sellprice;
                            isUpdated = true;
                        }

                        if (oldItem.Description != model.Description)
                        {
                            oldItem.Description = model.Description;
                            isUpdated = true;
                        }

                        if (isUpdated)
                        {
                            oldItem.ModifiedDate = DateTimeOffset.Now;
                            _db.Product.Update(oldItem);
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
                await SetWarehouse();

                var item = await (from i in _db.Inventory
                                  join p in _db.Product on i.ProductId equals p.ProductId
                                  join c in _db.Category on p.CategoryId equals c.CategoryId
                                  join u in _db.Unit on p.UnitId equals u.UnitId
                                  where i.WarehouseId == warehouseId && p.ProductId == id
                                  select new ProductViewModel
                                  {
                                      ProductId = p.ProductId,
                                      ProductName = p.ProductName,
                                      CategoryName = c.CategoryName,
                                      UnitName = u.UnitName,
                                      Sellprice = p.Sellprice,
                                      Balance = i.Balance,
                                      Description = p.Description,
                                      CreatedDate = p.CreatedDate,
                                      ModifiedDate = p.ModifiedDate,
                                      RowVersion = p.RowVersion
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
        public async Task<ActionResult> DeleteConfirmed(Guid id, byte[] rowVersion)
        {
            using var transaction = await _db.Database.BeginTransactionAsync();

            try
            {
                var item = await _db.Product.FirstOrDefaultAsync(x => x.ProductId == id);

                

                if (item != null)
                {
                    if (!item.RowVersion!.SequenceEqual(rowVersion))
                    {
                        TempData["Error"] = "ข้อมูลถูกแก้ไขโดยผู้อื่นในขณะนี้ กรุณาลองอีกครั้งหรือติดต่อผู้ดูแลระบบ";
                        _logger.LogWarning("ข้อมูลถูกแก้ไขโดยผู้อื่นในขณะนี้");
                        return RedirectToAction("Index");
                    }

                    var inventory = await _db.Inventory.Where(x => x.ProductId == id).FirstOrDefaultAsync();

                    if (inventory != null)
                    {
                        _db.Inventory.Remove(inventory);
                    }

                    _db.Product.Remove(item);
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
                await SetWarehouse();

                var items = await (from i in _db.Inventory
                                   join p in _db.Product on i.ProductId equals p.ProductId
                                   join c in _db.Category on p.CategoryId equals c.CategoryId
                                   join u in _db.Unit on p.UnitId equals u.UnitId
                                   where i.WarehouseId == warehouseId
                                   orderby p.ProductName
                                   select new ProductViewModel
                                   {
                                       ProductId = p.ProductId,
                                       ProductName = p.ProductName,
                                       CategoryName = c.CategoryName,
                                       UnitName = u.UnitName,
                                       Sellprice = p.Sellprice,
                                       Balance = i.Balance,
                                       Description = p.Description,
                                       CreatedDate = p.CreatedDate,
                                       ModifiedDate = p.ModifiedDate
                                   }).AsNoTracking().ToListAsync();

                if (items == null || items.Count == 0)
                {
                    TempData["Warning"] = "ไม่มีข้อมูลในตาราง";
                    _logger.LogWarning("ไม่มีข้อมูลในตาราง");
                    return View();
                }

                var builder = new StringBuilder();

                builder.AppendLine("ProductId,ProductName,CategoryName,UnitName," +
                    "Sellprice,Balance,Description,CreatedDate,ModifiedDate");

                foreach (var item in items)
                {
                    builder.AppendLine($"{item.ProductId}, {item.ProductName}, " +
                        $"{item.CategoryName}, {item.UnitName}, {item.Sellprice}, {item.Balance}, " +
                        $"{item.Description}, {item.CreatedDate}, {item.ModifiedDate}");
                }

                var csvData = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(builder.ToString())).ToArray();
                var contentType = "text/csv";
                var fileName = "รายการสินค้า.csv";

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
                await SetWarehouse();

                var items = await (from i in _db.Inventory
                                   join p in _db.Product on i.ProductId equals p.ProductId
                                   join c in _db.Category on p.CategoryId equals c.CategoryId
                                   join u in _db.Unit on p.UnitId equals u.UnitId
                                   where i.WarehouseId == warehouseId
                                   orderby p.ProductName
                                   select new ProductViewModel
                                   {
                                       ProductId = p.ProductId,
                                       ProductName = p.ProductName,
                                       CategoryName = c.CategoryName,
                                       UnitName = u.UnitName,
                                       Sellprice = p.Sellprice,
                                       Balance = i.Balance,
                                       Description = p.Description,
                                       CreatedDate = p.CreatedDate,
                                       ModifiedDate = p.ModifiedDate
                                   }).AsNoTracking().ToListAsync();

                if (items == null || items.Count == 0)
                {
                    TempData["Warning"] = "ไม่มีข้อมูลในตาราง";
                    _logger.LogWarning("ไม่มีข้อมูลในตาราง");
                    return View();
                }

                using var workbook = new XLWorkbook();

                var worksheet = workbook.Worksheets.Add("สินค้า");

                var fontName = "Angsana New"; //กำหนด Font

                var headerRange = worksheet.Range("A1:I1");
                headerRange.Merge().Value = "รายการสินค้า"; //ผลาสเซลล์
                headerRange.Style.Font.Bold = true; //ตัวหนา
                headerRange.Style.Font.FontSize = 20; //ขนาดตัวอักษร
                headerRange.Style.Font.FontName = fontName;
                headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center; //แนวตั้ง
                headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center; //แนวนอน

                worksheet.Cell("A2").Value = "รหัสสินค้า";
                worksheet.Cell("B2").Value = "ชื่อสินค้า";
                worksheet.Cell("C2").Value = "หมวดสินค้า";
                worksheet.Cell("D2").Value = "หน่วยนับ";
                worksheet.Cell("E2").Value = "ราคาขาย";
                worksheet.Cell("F2").Value = "คงเหลือ";
                worksheet.Cell("G2").Value = "คำอธิบาย";
                worksheet.Cell("H2").Value = "วันที่สร้าง";
                worksheet.Cell("I2").Value = "วันที่แก้ไข";

                var headerColumn = worksheet.Range("A2:I2");
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
                    worksheet.Cell($"A{rowNumber}").Value = item.ProductId.ToString();
                    worksheet.Cell($"B{rowNumber}").Value = item.ProductName.ToString();
                    worksheet.Cell($"C{rowNumber}").Value = item.CategoryName.ToString();
                    worksheet.Cell($"D{rowNumber}").Value = item.UnitName.ToString();
                    worksheet.Cell($"E{rowNumber}").Value = item.Sellprice;
                    worksheet.Cell($"E{rowNumber}").Style.NumberFormat.Format = "#,##0.00";
                    worksheet.Cell($"F{rowNumber}").Value = item.Balance;
                    worksheet.Cell($"F{rowNumber}").Style.NumberFormat.Format = "#,##0.00";
                    worksheet.Cell($"G{rowNumber}").Value = item.Description?.ToString();
                    worksheet.Cell($"H{rowNumber}").Value = item.CreatedDate?.ToString();
                    worksheet.Cell($"I{rowNumber}").Value = item.ModifiedDate?.ToString();

                    var row = worksheet.Range($"A{rowNumber}:I{rowNumber}");
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
                var fileName = "รายการสินค้า.xlsx";

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
                await SetWarehouse();

                var items = await (from i in _db.Inventory
                                   join p in _db.Product on i.ProductId equals p.ProductId
                                   join c in _db.Category on p.CategoryId equals c.CategoryId
                                   join u in _db.Unit on p.UnitId equals u.UnitId
                                   where i.WarehouseId == warehouseId
                                   orderby p.ProductName
                                   select new ProductViewModel
                                   {
                                       ProductName = p.ProductName,
                                       CategoryName = c.CategoryName,
                                       UnitName = u.UnitName,
                                       Sellprice = p.Sellprice,
                                       Balance = i.Balance
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
                        page.Size(PageSizes.A4.Landscape());
                        page.Margin(1, QuestPDF.Infrastructure.Unit.Centimetre);
                        page.PageColor(Colors.White);

                        page.Header().ShowOnce().Text("รายงานสินค้า").AlignCenter()
                        .SemiBold().FontSize(30).FontColor(Colors.Blue.Medium);

                        page.Content().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(50);
                                columns.RelativeColumn(3);
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.ConstantColumn(90);
                                columns.ConstantColumn(90);
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
                                header.Cell().Element(thead).Text("สินค้า").SemiBold();
                                header.Cell().Element(thead).Text("หมวดสินค้า").SemiBold();
                                header.Cell().Element(thead).Text("หน่วยนับ").SemiBold();
                                header.Cell().Element(thead).Text("ราคาขาย").SemiBold();
                                header.Cell().Element(thead).Text("คงเหลือ").SemiBold();
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
                                table.Cell().Element(tbody).Text(item.ProductName);
                                table.Cell().Element(tbody).Text(item.CategoryName);
                                table.Cell().Element(tbody).Text(item.UnitName);
                                table.Cell().Element(tbody).Text(item.Sellprice.ToString("N2")).AlignRight();
                                table.Cell().Element(tbody).Text(item.Balance.ToString("N2")).AlignRight();
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
                var fileName = "รายงานสินค้า.pdf";

                return File(pdfBytes, contentType, fileName);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "เกิดข้อผิดพลาดในดาวน์โหลดข้อมูล กรุณาลองอีกครั้งหรือติดต่อผู้ดูแลระบบ";
                _logger.LogError(ex, "เกิดข้อผิดพลาดในดาวน์โหลดข้อมูล");
                return RedirectToAction("Index");
            }
        }

        private async Task<bool> NameIsExists(Guid productId, string productName)
        {
            return await _db.Product.AsNoTracking().AnyAsync(x => x.ProductId != productId && x.ProductName == productName);
        }

        /*private async Task<bool> CheckProductInInventory(Guid productId)
        {
            return await _db.Inventory.AsNoTracking().AnyAsync(x => x.ProductId == productId);
        }*/
    }
}
