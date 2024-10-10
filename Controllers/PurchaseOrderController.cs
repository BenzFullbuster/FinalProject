using FinalProject.Helpers;
using FinalProject.Models;
using FinalProject.Models.ViewModels;
using GreatFriends.ThaiBahtText;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FinalProject.Controllers
{
    public class PurchaseOrderController(ConnectDb db, ILogger<PurchaseOrderController> logger) : Controller
    {
        private readonly ConnectDb _db = db;
        private readonly ILogger<PurchaseOrderController> _logger = logger;
        private readonly GenerateInvoiceNumber _generateInvoiceNumber = new();
        private const decimal tax = 0.07m;

        protected string? warehouseCode { get; private set; }
        protected Guid warehouseId { get; private set; }

        protected async Task SetWarehouse()
        {
            warehouseCode = User.GetWarehouseCode();
            warehouseId = await _db.GetWarehouseId(warehouseCode);
        }

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

                await SetWarehouse();

                var query = (from po in _db.PurchaseOrder
                             join s in _db.Supplier on po.SupplierId equals s.SupplierId
                             join sn in _db.Status on po.StatusId equals sn.StatusId
                             where po.WarehouseId == warehouseId
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

                var item = await (from po in _db.PurchaseOrder
                                  join e in _db.Employee on po.EmployeeId equals e.EmployeeId
                                  join s in _db.Supplier on po.SupplierId equals s.SupplierId
                                  join sn in _db.Status on po.StatusId equals sn.StatusId
                                  join v in _db.VatType on po.VatTypeId equals v.VatTypeId
                                  where po.WarehouseId == warehouseId && po.PurchaseOrderId == id
                                  select new PurchaseOrderViewModel
                                  {
                                      PurchaseOrderId = po.PurchaseOrderId,
                                      PurchaseOrderCode = po.PurchaseOrderCode,
                                      PurchaseOrderDate = po.PurchaseOrderDate,
                                      EmployeeName = e.EmployeeName,
                                      SupplierName = s.SupplierName,
                                      SupplierAddress = FormatAddressHelper.GetFormattedAddress(
                                          s.Address,
                                          s.Province != null ? s.Province.ProvinceName : null,
                                          s.District != null ? s.District.DistrictName : null,
                                          s.Subdistrict != null ? s.Subdistrict.SubdistrictName : null,
                                          s.Subdistrict != null ? s.Subdistrict.Zipcode : null,
                                          true),
                                      SupplierPhoneNumber = s.PhoneNumber,
                                      SupplierTaxNumber = s.Taxnumber,
                                      Subtotal = po.Subtotal,
                                      Vat = po.Vat,
                                      Nettotal = po.Nettotal,
                                      Description = po.Description,
                                      CreatedDate = po.CreatedDate,
                                      ModifiedDate = po.ModifiedDate,
                                      StatusName = sn.StatusName,
                                      VatTypeName = v.VatTypeName,
                                      PurchaseOrderDetails = (from pod in _db.PurchaseOrderDetail
                                                              join p in _db.Product on pod.ProductId equals p.ProductId
                                                              where pod.PurchaseOrderId == po.PurchaseOrderId
                                                              select new PurchaseOrderDetailViewModel
                                                              {
                                                                  ProductName = p.ProductName,
                                                                  Quantity = pod.Quantity,
                                                                  Unitprice = pod.Unitprice,
                                                                  Amount = pod.Amount
                                                              }).ToList()
                                  }).AsNoTracking().FirstOrDefaultAsync();

                if (item != null)
                {
                    item.NettotalText = item.Nettotal.ThaiBahtText();
                }

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

        public async Task<SelectList> GetVatType()
        {
            var vatType = await _db.VatType.Select(x => new { x.VatTypeId, x.VatTypeName })
                .OrderBy(x => x.VatTypeId).ToListAsync();
            return new SelectList(vatType, "VatTypeId", "VatTypeName");
        }

        public async Task<IActionResult> Create()
        {
            await SetWarehouse();
            var employeeId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var employeeName = User.FindFirstValue(ClaimTypes.Name);

            ViewData["VatTypeId"] = await GetVatType();

            PurchaseOrderCreateViewModel model = new()
            {
                PurchaseOrderCode = await _generateInvoiceNumber.AutoInvoiceNumber("PO", warehouseCode!, _db.PurchaseOrder, x => x.PurchaseOrderCode),
                PurchaseOrderDate = DateTime.Now.Date,
                VatTypeId = 3,
                WarehouseId = warehouseId,
                EmployeeId = Guid.Parse(employeeId!),
                EmployeeName = employeeName,
            };
            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PurchaseOrderCreateViewModel model)
        {
            using var transaction = await _db.Database.BeginTransactionAsync();

            try
            {
                if (ModelState.IsValid)
                {
                    model.PurchaseOrderCode = model.PurchaseOrderCode!.Trim();

                    if (await POCodeIsExists(Guid.Empty, model.PurchaseOrderCode))
                    {
                        ModelState.AddModelError("PurchaseOrderCode", "เลขที่ใบสั่งซื้อมีอยู่แล้ว");
                        ViewData["VatTypeId"] = await GetVatType();
                        return View(model);
                    }

                    if (model.PurchaseOrderDetails.Count <= 0)
                    {
                        TempData["Warning"] = "กรุณาเพิ่มรายละเอียดใบสั่งซื้อ";
                        ViewData["VatTypeId"] = await GetVatType();
                        return View(model);
                    }

                    var status = await _db.Status.Where(x => x.StatusName == "สั่งซื้อแล้ว")
                        .Select(x => new { x.StatusId }).FirstOrDefaultAsync();

                    if (status == null)
                    {
                        TempData["Error"] = "ไม่พบสถานะ 'สั่งซื้อแล้ว'";
                        return View(model);
                    }

                    var purchaseOrder = new PurchaseOrder
                    {
                        PurchaseOrderId = Guid.NewGuid(),
                        PurchaseOrderCode = model.PurchaseOrderCode,
                        PurchaseOrderDate = model.PurchaseOrderDate,
                        WarehouseId = model.WarehouseId,
                        SupplierId = model.SupplierId,
                        EmployeeId = model.EmployeeId,
                        Description = model.Description?.Trim(),
                        StatusId = status.StatusId,
                        VatTypeId = model.VatTypeId,
                        CreatedDate = DateTimeOffset.Now
                    };

                    if (model.PurchaseOrderDetails != null && model.PurchaseOrderDetails.Count != 0)
                    {
                        foreach (var itemDetail in model.PurchaseOrderDetails)
                        {
                            var purchaseOrderDetail = new PurchaseOrderDetail
                            {
                                PurchaseOrderDetailId = Guid.NewGuid(),
                                ProductId = itemDetail.ProductId,
                                Quantity = itemDetail.Quantity,
                                Unitprice = itemDetail.Unitprice,
                                Amount = Math.Round(itemDetail.Quantity * itemDetail.Unitprice, 2),
                                PurchaseOrderId = purchaseOrder.PurchaseOrderId
                            };

                            await _db.PurchaseOrderDetail.AddAsync(purchaseOrderDetail);
                        }

                        purchaseOrder.Subtotal = Math.Round(model.PurchaseOrderDetails.Sum(x => x.Amount), 2);

                        switch (model.VatTypeId)
                        {
                            case 2:
                                purchaseOrder.Vat = Math.Round(purchaseOrder.Subtotal * tax / (1 * tax), 2);
                                purchaseOrder.Nettotal = purchaseOrder.Subtotal;
                                purchaseOrder.Subtotal = purchaseOrder.Nettotal - purchaseOrder.Vat;
                                break;
                            case 3:
                                purchaseOrder.Vat = Math.Round(purchaseOrder.Subtotal * tax, 2);
                                purchaseOrder.Nettotal = Math.Round(purchaseOrder.Subtotal + purchaseOrder.Vat, 2);
                                break;
                            default:
                                purchaseOrder.Vat = 0;
                                purchaseOrder.Nettotal = purchaseOrder.Subtotal;
                                break;
                        }
                    }

                    await _db.PurchaseOrder.AddAsync(purchaseOrder);
                    await _db.SaveChangesAsync();
                    await transaction.CommitAsync();
                    TempData["Success"] = "เพิ่มข้อมูลสำเร็จ";
                    _logger.LogInformation("เพิ่มข้อมูลสำเร็จ");
                    return RedirectToAction("Index");
                }

                TempData["Warning"] = "กรุณากรอกข้อมูลให้ถูกต้องและครบถ้วน";
                _logger.LogWarning("กรุณากรอกข้อมูลให้ถูกต้องและครบถ้วน");
                ViewData["VatTypeId"] = await GetVatType();
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

        private async Task<bool> POCodeIsExists(Guid purchaseOrderId, string purchaseOrderCode)
        {
            return await _db.PurchaseOrder.AsNoTracking().AnyAsync(x => x.PurchaseOrderId != purchaseOrderId && x.PurchaseOrderCode == purchaseOrderCode);
        }
    }
}
