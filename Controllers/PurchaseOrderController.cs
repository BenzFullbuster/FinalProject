using FinalProject.Helpers;
using FinalProject.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace FinalProject.Controllers
{
    public class PurchaseOrderController(ConnectDb db, ILogger<PurchaseOrderController> logger, GenerateInvoiceNumber generateInvoiceNumber) : Controller
    {
        private readonly ConnectDb _db = db;
        private readonly ILogger<PurchaseOrderController> _logger = logger;
        private readonly GenerateInvoiceNumber _generateInvoiceNumber = generateInvoiceNumber;
        const string warehouseId = "4cdbe460-9018-490e-8c65-bf8851868de3";
        public async Task<IActionResult> Index(string searchBy, string searchString, string currentFilter, string sortOrder, int? pageSize, int currentPage = 1)
        {
            try
            {
                ViewData["CurrentSort"] = sortOrder;
                ViewData["POCodeSort"] = sortOrder == "poCode" ? "poCode_desc" : "poCode";
                ViewData["PODateSort"] = sortOrder == "poDate" ? "poDate_desc" : "poDate";
                ViewData["SupplierSort"] = sortOrder == "supplier" ? "supplier_desc" : "supplier";
                ViewData["NettotalSort"] = sortOrder == "nettotal" ? "nettotal_desc" : "nettotal";
                ViewData["StatusSort"] = sortOrder == "status" ? "status_desc" : "status";

                searchString ??= currentFilter;

                ViewData["SearchBy"] = searchBy;
                ViewData["CurrentFilter"] = searchString;

                var query = (from po in _db.PurchaseOrder
                             join s in _db.Supplier on po.SupplierId equals s.SupplierId
                             join sn in _db.Status on po.StatusId equals sn.StatusId
                             select new PurchaseOrderViewModel
                             {
                                 PurchaseOrderId = po.PurchaseOrderId,
                                 PurchaseOrderCode = po.PurchaseOrderCode,
                                 PurchaseOrderDate = po.PurchaseOrderDate,
                                 SupplierName = s.SupplierName,
                                 Nettotal = po.Nettotal,
                                 StatusName = sn.StatusName
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

                    query = searchBy switch
                    {
                        "PurchaseOrderCode" => query = query.Where(x => x.PurchaseOrderCode.ToLower().Contains(searchString.ToLower())),
                        "PurchaseOrderDate" => query = query.Where(x => x.PurchaseOrderDate.ToString().ToLower().Contains(searchString.ToLower())),
                        "SupplierName" => query = query.Where(x => x.SupplierName!.ToLower().Contains(searchString.ToLower())),
                        _ => query
                    };
                }

                query = sortOrder switch
                {
                    "poCode" => query.OrderBy(x => x.PurchaseOrderCode),
                    "poCode_desc" => query.OrderByDescending(x => x.PurchaseOrderCode),
                    "poDate" => query.OrderBy(x => x.PurchaseOrderDate),
                    "poDate_desc" => query.OrderByDescending(x => x.PurchaseOrderDate),
                    "supplier" => query.OrderBy(x => x.SupplierName),
                    "supplier_desc" => query.OrderByDescending(x => x.SupplierName),
                    "nettotal" => query.OrderBy(x => x.Nettotal),
                    "nettotal_desc" => query.OrderByDescending(x => x.Nettotal),
                    "status" => query.OrderBy(x => x.StatusName),
                    "status_desc" => query.OrderByDescending(x => x.StatusName),
                    _ => query.OrderByDescending(x => x.PurchaseOrderCode)
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

        //public IActionResult Create()
        //{
        //    return View();
        //}
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Create(PurchaseOrderCreateViewModel model)
        //{
        //    using var transaction = await _db.Database.BeginTransactionAsync();

        //    if (ModelState.IsValid)
        //    {
        //        var item = new PurchaseOrder
        //        { 
        //            PurchaseOrderId = Guid.NewGuid(),
        //            PurchaseOrderCode= model.PurchaseOrderCode,
        //            PurchaseOrderDate = model.PurchaseOrderDate,
        //            WarehouseId = Guid.Parse(warehouseId),
        //            SupplierId = model.SupplierId,
        //            EmployeeId = model.EmployeeId,
        //            Subtotal = model.Subtotal,
        //            Vat = model.Vat,
        //            Nettotal = model.Nettotal
        //        };

        //        await _db.PurchaseOrder.AddAsync(item);
        //        await _db.SaveChangesAsync();
        //        await transaction.CommitAsync();
        //        TempData["Success"] = "เพิ่มข้อมูลสำเร็จ";
        //        _logger.LogInformation("เพิ่มข้อมูลสำเร็จ");
        //        return RedirectToAction("Index");
        //    }

        //    return View(model);

        //}
    }
}
