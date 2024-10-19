using ClosedXML.Excel;
using FinalProject.Helpers;
using FinalProject.Models;
using FinalProject.Models.ViewModels;
using GreatFriends.ThaiBahtText;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using Rotativa.AspNetCore;
using System.Security.Claims;
using System.Text;

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
                        .Select(x => x.StatusId).FirstOrDefaultAsync();

                    if (status == Guid.Empty)
                    {
                        TempData["Warning"] = "ไม่พบสถานะ 'สั่งซื้อแล้ว'";
                        ViewData["VatTypeId"] = await GetVatType();
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
                        StatusId = status,
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

                var item = await (from po in _db.PurchaseOrder
                                  join e in _db.Employee on po.EmployeeId equals e.EmployeeId
                                  join s in _db.Supplier on po.SupplierId equals s.SupplierId
                                  join sn in _db.Status on po.StatusId equals sn.StatusId
                                  join v in _db.VatType on po.VatTypeId equals v.VatTypeId
                                  where po.WarehouseId == warehouseId && po.PurchaseOrderId == id
                                  select new PurchaseOrderEditViewModel
                                  {
                                      PurchaseOrderId = po.PurchaseOrderId,
                                      PurchaseOrderCode = po.PurchaseOrderCode,
                                      PurchaseOrderDate = po.PurchaseOrderDate,
                                      WarehouseId = po.WarehouseId,
                                      EmployeeId = po.EmployeeId,
                                      EmployeeName = e.EmployeeName,
                                      SupplierId = po.SupplierId,
                                      SupplierName = s.SupplierName,
                                      Subtotal = po.Subtotal,
                                      Vat = po.Vat,
                                      Nettotal = po.Nettotal,
                                      Description = po.Description,
                                      StatusId = po.StatusId,
                                      StatusName = sn.StatusName,
                                      VatTypeId = po.VatTypeId,
                                      VatTypeName = v.VatTypeName,
                                      RowVersion = po.RowVersion,
                                      PurchaseOrderDetails = (from pod in _db.PurchaseOrderDetail
                                                              join p in _db.Product on pod.ProductId equals p.ProductId
                                                              where pod.PurchaseOrderId == po.PurchaseOrderId
                                                              select new PurchaseOrderDetailViewModel
                                                              {
                                                                  PurchaseOrderDetailId = pod.PurchaseOrderDetailId,
                                                                  ProductId = pod.ProductId,
                                                                  ProductName = p.ProductName,
                                                                  Quantity = pod.Quantity,
                                                                  Unitprice = pod.Unitprice,
                                                                  Amount = pod.Amount,
                                                                  PurchaseOrderId = pod.PurchaseOrderId
                                                              }).ToList()
                                  }).AsNoTracking().FirstOrDefaultAsync();
                                
                if (item == null)
                {
                    TempData["Warning"] = "ไม่มีข้อมูลในตาราง";
                    _logger.LogWarning("ไม่มีข้อมูลในตาราง");
                    return RedirectToAction("Index");
                }

                var status = await _db.Status.Where(x => x.StatusName == "รับสินค้าแล้ว")
                        .Select(x => x.StatusId).FirstOrDefaultAsync();

                if (item.StatusId == status)
                {
                    TempData["Warning"] = "ไม่สามารถแก้ไขได้ เพราะมีการรับสินค้าแล้ว";
                    _logger.LogWarning("ไม่สามารถแก้ไขได้ เพราะมีการรับสินค้าแล้ว");
                    return RedirectToAction(nameof(Index));
                }

                _logger.LogInformation("ดึงข้อมูลสำเร็จ");
                ViewData["VatTypeId"] = await GetVatType();
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
        public async Task<IActionResult> Edit(Guid id, PurchaseOrderEditViewModel model)
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
                    model.PurchaseOrderCode = model.PurchaseOrderCode!.Trim();

                    if (await POCodeIsExists(model.PurchaseOrderId, model.PurchaseOrderCode))
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

                    var status = await _db.Status.Where(x => x.StatusName == "รับสินค้าแล้ว")
                        .Select(x => x.StatusId).FirstOrDefaultAsync();

                    if (model.StatusId == status)
                    {
                        TempData["Warning"] = "ไม่สามารถแก้ไขได้ เพราะมีการรับสินค้าแล้ว";
                        _logger.LogWarning("ไม่สามารถแก้ไขได้ เพราะมีการรับสินค้าแล้ว");
                        return RedirectToAction(nameof(Index));
                    }

                    var oldItem = await _db.PurchaseOrder.Include(x => x.PurchaseOrderDetail).FirstOrDefaultAsync(x => x.PurchaseOrderId == id);

                    if (oldItem != null)
                    {
                        if (!oldItem.RowVersion!.SequenceEqual(model.RowVersion!))
                        {
                            TempData["Warning"] = "ข้อมูลถูกแก้ไขโดยผู้อื่นในขณะนี้ กรุณาลองอีกครั้งหรือติดต่อผู้ดูแลระบบ";
                            _logger.LogWarning("ข้อมูลถูกแก้ไขโดยผู้อื่นในขณะนี้");
                            ViewData["VatTypeId"] = await GetVatType();
                            return RedirectToAction("Edit", new { id });
                        }

                        bool isUpdated = false;

                        if (oldItem.PurchaseOrderCode != model.PurchaseOrderCode)
                        {
                            oldItem.PurchaseOrderCode = model.PurchaseOrderCode;
                            isUpdated = true;
                        }

                        if (oldItem.PurchaseOrderDate != model.PurchaseOrderDate)
                        {
                            oldItem.PurchaseOrderDate = model.PurchaseOrderDate;
                            isUpdated = true;
                        }

                        if (oldItem.EmployeeId != model.EmployeeId)
                        {
                            oldItem.EmployeeId = model.EmployeeId;
                            isUpdated = true;
                        }

                        if (oldItem.SupplierId != model.SupplierId)
                        {
                            oldItem.SupplierId = model.SupplierId;
                            isUpdated = true;
                        }

                        if (oldItem.Description != model.Description)
                        {
                            oldItem.Description = model.Description;
                            isUpdated = true;
                        }

                        if (model.PurchaseOrderDetails.Count != 0)
                        {
                            var removeDetails = oldItem.PurchaseOrderDetail
                                .Where(o => !model.PurchaseOrderDetails
                                .Any(m => m.PurchaseOrderDetailId == o.PurchaseOrderDetailId))
                                .ToList();

                            if (removeDetails.Count != 0)
                            {
                                _db.PurchaseOrderDetail.RemoveRange(removeDetails);
                            }

                            foreach (var detailModel in model.PurchaseOrderDetails)
                            {
                                var detail = oldItem.PurchaseOrderDetail
                                    .Where(x => x.PurchaseOrderDetailId == detailModel.PurchaseOrderDetailId)
                                    .Select(x => x.PurchaseOrderDetailId).FirstOrDefault();

                                if (detail == Guid.Empty)
                                {
                                    var newDetail = new PurchaseOrderDetail
                                    {
                                        PurchaseOrderDetailId = Guid.NewGuid(),
                                        ProductId = detailModel.ProductId,
                                        Quantity = detailModel.Quantity,
                                        Unitprice = detailModel.Unitprice,
                                        Amount = Math.Round(detailModel.Quantity * detailModel.Unitprice, 2),
                                        PurchaseOrderId = model.PurchaseOrderId
                                    };

                                    await _db.PurchaseOrderDetail.AddAsync(newDetail);
                                }
                            }
                        }

                        if (oldItem.VatTypeId != model.VatTypeId)
                        {
                            oldItem.VatTypeId = model.VatTypeId;
                            isUpdated = true;
                        }

                        if (oldItem.Subtotal != model.Subtotal || oldItem.Vat != model.Vat || oldItem.Nettotal != model.Nettotal)
                        {
                            switch (model.VatTypeId)
                            {
                                case 2: //รวมvat
                                    oldItem.Vat = Math.Round(model.Subtotal * tax / (1 * tax));
                                    oldItem.Nettotal = model.Subtotal;
                                    oldItem.Subtotal = model.Subtotal - model.Vat;
                                    break;
                                case 3: //แยกvat
                                    oldItem.Subtotal = model.Subtotal;
                                    oldItem.Vat = Math.Round(model.Subtotal * tax, 2);
                                    oldItem.Nettotal = Math.Round(model.Subtotal + model.Vat, 2);
                                    break;
                                default:
                                    oldItem.Subtotal = model.Subtotal;
                                    oldItem.Vat = 0;
                                    oldItem.Nettotal = model.Subtotal;
                                    break;
                            }
                            isUpdated = true;
                        }

                        if (isUpdated)
                        {
                            oldItem.ModifiedDate = DateTimeOffset.Now;
                            _db.PurchaseOrder.Update(oldItem);
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
                ViewData["VatTypeId"] = await GetVatType();
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
                                      StatusId = po.StatusId,
                                      StatusName = sn.StatusName,
                                      VatTypeName = v.VatTypeName,
                                      RowVersion = po.RowVersion,
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

                var status = await _db.Status.Where(x => x.StatusName == "รับสินค้าแล้ว")
                        .Select(x => x.StatusId).FirstOrDefaultAsync();

                if (item.StatusId == status)
                {
                    TempData["Warning"] = "ไม่สามารถลบได้ เพราะมีการรับสินค้าแล้ว";
                    _logger.LogWarning("ไม่สามารถลบได้ เพราะมีการรับสินค้าแล้ว");
                    return RedirectToAction(nameof(Index));
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
                var item = await _db.PurchaseOrder.Include(x => x.PurchaseOrderDetail)
                    .FirstOrDefaultAsync(x => x.PurchaseOrderId == id);

                if (item != null)
                {
                    if (!item.RowVersion!.SequenceEqual(rowVersion))
                    {
                        TempData["Error"] = "ข้อมูลถูกแก้ไขโดยผู้อื่นในขณะนี้ กรุณาลองอีกครั้งหรือติดต่อผู้ดูแลระบบ";
                        _logger.LogWarning("ข้อมูลถูกแก้ไขโดยผู้อื่นในขณะนี้");
                        return RedirectToAction("Index");
                    }

                    if (await CheckPurchaseOrderRelatedToOperations(id))
                    {
                        TempData["Warning"] = $"ไม่สามารถลบ {item.PurchaseOrderCode} ได้เนื่องจากมีการอ้างอิงอยู่";
                        _logger.LogWarning("ไม่สามารถลบได้เนื่องจากมีการอ้างอิงอยู่");
                        return RedirectToAction("Index");
                    }

                    _db.PurchaseOrderDetail.RemoveRange(item.PurchaseOrderDetail);
                    _db.PurchaseOrder.Remove(item);
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

                var items = await (from p in _db.PurchaseOrder
                                   join s in _db.Supplier on p.SupplierId equals s.SupplierId
                                   join e in _db.Employee on p.EmployeeId equals e.EmployeeId
                                   join st in _db.Status on p.StatusId equals st.StatusId
                                   join v in _db.VatType on p.VatTypeId equals v.VatTypeId
                                   where p.WarehouseId == warehouseId
                                   select new PurchaseOrderViewModel
                                   {
                                       PurchaseOrderId = p.PurchaseOrderId,
                                       PurchaseOrderCode = p.PurchaseOrderCode,
                                       PurchaseOrderDate = p.PurchaseOrderDate,
                                       SupplierName = s.SupplierName,
                                       EmployeeName = e.EmployeeName,
                                       Subtotal = p.Subtotal,
                                       Vat = p.Vat,
                                       Nettotal = p.Nettotal,
                                       StatusName = st.StatusName,
                                       VatTypeName = v.VatTypeName,
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

                builder.AppendLine("PurchaseOrderId,PurchaseOrderCode,PurchaseOrderDate,SupplierName," +
                    "EmployeeName,Subtotal,Vat,Nettotal,StatusName,VatTypeName,Description,CreatedDate,ModifiedDate");

                foreach (var item in items)
                {
                    builder.AppendLine($"{item.PurchaseOrderId}, {item.PurchaseOrderCode}, {item.PurchaseOrderDate}, " +
                        $"{item.SupplierName}, {item.EmployeeName}, {item.Subtotal}, {item.Vat}, {item.Nettotal}, " +
                        $"{item.StatusName}, {item.VatTypeName}, {item.Description}, {item.CreatedDate}, {item.ModifiedDate}");
                }

                var csvData = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(builder.ToString())).ToArray();
                var contentType = "text/csv";
                var fileName = "รายการใบสั่งซื้อ.csv";

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

                var items = await (from p in _db.PurchaseOrder
                                   join s in _db.Supplier on p.SupplierId equals s.SupplierId
                                   join e in _db.Employee on p.EmployeeId equals e.EmployeeId
                                   join st in _db.Status on p.StatusId equals st.StatusId
                                   join v in _db.VatType on p.VatTypeId equals v.VatTypeId
                                   where p.WarehouseId == warehouseId
                                   select new PurchaseOrderViewModel
                                   {
                                       PurchaseOrderId = p.PurchaseOrderId,
                                       PurchaseOrderCode = p.PurchaseOrderCode,
                                       PurchaseOrderDate = p.PurchaseOrderDate,
                                       SupplierName = s.SupplierName,
                                       EmployeeName = e.EmployeeName,
                                       Subtotal = p.Subtotal,
                                       Vat = p.Vat,
                                       Nettotal = p.Nettotal,
                                       StatusName = st.StatusName,
                                       VatTypeName = v.VatTypeName,
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

                var worksheet = workbook.Worksheets.Add("ใบสั่งซื้อ");

                var fontName = "Angsana New"; //กำหนด Font

                var headerRange = worksheet.Range("A1:M1");
                headerRange.Merge().Value = "รายการใบสั่งซื้อ"; //ผลาสเซลล์
                headerRange.Style.Font.Bold = true; //ตัวหนา
                headerRange.Style.Font.FontSize = 20; //ขนาดตัวอักษร
                headerRange.Style.Font.FontName = fontName;
                headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center; //แนวตั้ง
                headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center; //แนวนอน

                worksheet.Cell("A2").Value = "รหัสใบสั่งซื้อ";
                worksheet.Cell("B2").Value = "ว/ด/ป";
                worksheet.Cell("C2").Value = "เลขที่ใบสั่งซื้อ";
                worksheet.Cell("D2").Value = "ผู้จำหน่าย";
                worksheet.Cell("E2").Value = "พนักงาน";
                worksheet.Cell("F2").Value = "ยอดรวม";
                worksheet.Cell("G2").Value = "ภาษีมูลค่าเพิ่ม 7%";
                worksheet.Cell("H2").Value = "ยอดรวมสุทธิ";
                worksheet.Cell("I2").Value = "สถานะ";
                worksheet.Cell("J2").Value = "ประเภทภาษี";
                worksheet.Cell("K2").Value = "คำอธิบาย";
                worksheet.Cell("L2").Value = "วันที่สร้าง";
                worksheet.Cell("M2").Value = "วันที่แก้ไข";

                var headerColumn = worksheet.Range("A2:M2");
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
                    worksheet.Cell($"A{rowNumber}").Value = item.PurchaseOrderId.ToString();
                    worksheet.Cell($"B{rowNumber}").Value = item.PurchaseOrderDate.ToString();
                    worksheet.Cell($"C{rowNumber}").Value = item.PurchaseOrderCode.ToString();
                    worksheet.Cell($"D{rowNumber}").Value = item.SupplierName?.ToString();
                    worksheet.Cell($"E{rowNumber}").Value = item.EmployeeName?.ToString();
                    worksheet.Cell($"F{rowNumber}").Value = item.Subtotal;
                    worksheet.Cell($"F{rowNumber}").Style.NumberFormat.Format = "#,##0.00";
                    worksheet.Cell($"G{rowNumber}").Value = item.Vat;
                    worksheet.Cell($"G{rowNumber}").Style.NumberFormat.Format = "#,##0.00";
                    worksheet.Cell($"H{rowNumber}").Value = item.Nettotal;
                    worksheet.Cell($"H{rowNumber}").Style.NumberFormat.Format = "#,##0.00";
                    worksheet.Cell($"I{rowNumber}").Value = item.StatusName?.ToString();
                    worksheet.Cell($"J{rowNumber}").Value = item.VatTypeName?.ToString();
                    worksheet.Cell($"K{rowNumber}").Value = item.Description?.ToString();
                    worksheet.Cell($"L{rowNumber}").Value = item.CreatedDate?.ToString();
                    worksheet.Cell($"M{rowNumber}").Value = item.ModifiedDate?.ToString();

                    var row = worksheet.Range($"A{rowNumber}:M{rowNumber}");
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
                var fileName = "รายการใบสั่งซื้อ.xlsx";

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

                var items = await (from p in _db.PurchaseOrder
                                   join s in _db.Supplier on p.SupplierId equals s.SupplierId
                                   join st in _db.Status on p.StatusId equals st.StatusId
                                   where p.WarehouseId == warehouseId
                                   select new PurchaseOrderViewModel
                                   {
                                       PurchaseOrderDate = p.PurchaseOrderDate,
                                       PurchaseOrderCode = p.PurchaseOrderCode,
                                       SupplierName = s.SupplierName,
                                       Subtotal = p.Subtotal,
                                       Vat = p.Vat,
                                       Nettotal = p.Nettotal
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

                        page.Header().ShowOnce().Text("รายงานใบสั่งซื้อ").AlignCenter()
                        .SemiBold().FontSize(30).FontColor(Colors.Blue.Medium);

                        page.Content().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(40);
                                columns.ConstantColumn(80);
                                columns.ConstantColumn(150);
                                columns.RelativeColumn();
                                columns.ConstantColumn(85);
                                columns.ConstantColumn(85);
                                columns.ConstantColumn(85);
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
                                header.Cell().Element(thead).Text("ว/ด/ป").SemiBold();
                                header.Cell().Element(thead).Text("เลขทีใบสั่งซื้อ").SemiBold();
                                header.Cell().Element(thead).Text("ผู้จำหน่าย").SemiBold();
                                header.Cell().Element(thead).Text("ยอดรวม").SemiBold();
                                header.Cell().Element(thead).Text("ภาษี").SemiBold();
                                header.Cell().Element(thead).Text("ยอดรวมสุทธิ").SemiBold();
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
                                table.Cell().Element(tbody).Text(item.PurchaseOrderDate.ToString("dd/MM/yyyy")).AlignCenter();
                                table.Cell().Element(tbody).Text(item.PurchaseOrderCode).AlignCenter();
                                table.Cell().Element(tbody).Text(item.SupplierName);
                                table.Cell().Element(tbody).Text(item.Subtotal.ToString("N2")).AlignRight();
                                table.Cell().Element(tbody).Text(item.Vat.ToString("N2")).AlignRight();
                                table.Cell().Element(tbody).Text(item.Nettotal.ToString("N2")).AlignRight();
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
                var fileName = "รายงานใบสั่งซื้อ.pdf";

                return File(pdfBytes, contentType, fileName);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "เกิดข้อผิดพลาดในดาวน์โหลดข้อมูล กรุณาลองอีกครั้งหรือติดต่อผู้ดูแลระบบ";
                _logger.LogError(ex, "เกิดข้อผิดพลาดในดาวน์โหลดข้อมูล");
                return RedirectToAction("Index");
            }
        }

        public async Task<IActionResult> ExportPDFById(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    TempData["Warning"] = "ไม่มีข้อมูลในตาราง";
                    _logger.LogWarning("ไม่มีข้อมูลในตาราง");
                    return RedirectToAction(nameof(Index));
                }

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
                                      Subtotal = po.Subtotal,
                                      Vat = po.Vat,
                                      Nettotal = po.Nettotal,
                                      Description = po.Description,
                                      StatusName = sn.StatusName,
                                      VatTypeName = v.VatTypeName,
                                      PurchaseOrderDetails = (from pod in _db.PurchaseOrderDetail
                                                              join p in _db.Product on pod.ProductId equals p.ProductId
                                                              where pod.PurchaseOrderId == po.PurchaseOrderId
                                                              select new PurchaseOrderDetailViewModel
                                                              {
                                                                  PurchaseOrderDetailId = pod.PurchaseOrderDetailId,
                                                                  ProductId = pod.ProductId,
                                                                  ProductName = p.ProductName,
                                                                  Quantity = pod.Quantity,
                                                                  Unitprice = pod.Unitprice,
                                                                  Amount = pod.Amount,
                                                                  PurchaseOrderId = pod.PurchaseOrderId
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
                    return View();
                }

                var fileName = $"ใบสั่งซื้อ {item.PurchaseOrderCode}.pdf";
                fileName = Uri.EscapeDataString(fileName);

                return new ViewAsPdf("ReportById", item)
                {
                    FileName = fileName
                };
            }
            catch (Exception ex)
            {
                TempData["Error"] = "เกิดข้อผิดพลาดในดาวน์โหลดข้อมูล กรุณาลองอีกครั้งหรือติดต่อผู้ดูแลระบบ";
                _logger.LogError(ex, "เกิดข้อผิดพลาดในดาวน์โหลดข้อมูล");
                return RedirectToAction(nameof(Index));
            }
        }

        /*public async Task<IActionResult> ExportPDFById(Guid id)
        {
            if (id == Guid.Empty)
            {
                TempData["Warning"] = "ไม่มีข้อมูลในตาราง";
                _logger.LogWarning("ไม่มีข้อมูลในตาราง");
                return RedirectToAction(nameof(Index));
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

                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4.Portrait());
                        page.Margin(1, QuestPDF.Infrastructure.Unit.Centimetre);
                        page.PageColor(Colors.White);

                        // Header
                        page.Header().PaddingBottom(15).Row(row =>
                        {
                            row.RelativeItem().Column(col =>
                            {
                                col.Item().Text("บริษัท กคข จำกัด (สำนักงานใหญ่)").FontSize(20).SemiBold();
                                col.Item().Text("149 หมู่ที่ 1 คลองบางบอน บางบอน กรุงเทพฯ 10150");
                                col.Item().Text("โทร.0-2222-2222");
                                col.Item().Text("เลขประจำตัวผู้เสียภาษี 01234567890123");
                            });

                            row.ConstantItem(150).AlignCenter().AlignMiddle().Text("ใบสั่งซื้อ")
                            .FontSize(28).Bold().FontColor(Colors.Blue.Medium);
                        });

                        // Content
                        page.Content().Column(column =>
                        {
                            column.Item().Border(1).Row(row =>
                            {
                                column.Spacing(15);

                                row.RelativeItem().PaddingLeft(5).Column(col =>
                                {
                                    col.Item().Text(text =>
                                    {
                                        text.Span("ผู้จำหน่าย ").SemiBold();
                                        text.Span(item.SupplierName);
                                    });
                                    col.Item().Text(text =>
                                    {
                                        text.Span("ที่อยู่ ").SemiBold();
                                        text.Span(item.SupplierAddress);
                                    });
                                    col.Item().Text(text =>
                                    {
                                        text.Span("โทร.").SemiBold();
                                        text.Span(item.SupplierPhoneNumber);
                                    });
                                    col.Item().Text(text =>
                                    {
                                        text.Span("เลขประจำตัวผู้เสียภาษี ").SemiBold();
                                        text.Span(item.FormattedTaxNumber);
                                    });
                                });

                                row.ConstantItem(55).PaddingRight(5).Column(col =>
                                {
                                    col.Item().AlignRight().Text("เลขที่").SemiBold();
                                    col.Item().AlignRight().Text("วันที่").SemiBold();
                                    col.Item().AlignRight().Text("พนักงาน").SemiBold();
                                });

                                row.ConstantItem(145).PaddingLeft(5).Column(col =>
                                {
                                    col.Item().Text($"{item.PurchaseOrderCode}");
                                    col.Item().Text($"{item.PurchaseOrderDate:dd/MM/yyyy}");
                                    col.Item().Text($"{item.EmployeeName}");
                                });
                            });

                            // Product Table
                            column.Item().Border(1).Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(40); // ลำดับ
                                    columns.RelativeColumn(); // รายการสินค้า
                                    columns.ConstantColumn(85); // จำนวน
                                    columns.ConstantColumn(85); // ราคาต่อหน่วย
                                    columns.ConstantColumn(85); // จำนวนเงิน
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Border(1).Text("ลำดับ").SemiBold().AlignCenter();
                                    header.Cell().Border(1).Text("รายการสินค้า").SemiBold().AlignCenter();
                                    header.Cell().Border(1).Text("จำนวน").SemiBold().AlignCenter();
                                    header.Cell().Border(1).Text("ราคาต่อหน่วย").SemiBold().AlignCenter();
                                    header.Cell().Border(1).Text("จำนวนเงิน").SemiBold().AlignCenter();
                                });

                                for (int i = 0; i < item.PurchaseOrderDetails.Count; i++)
                                {
                                    table.Cell().Border(1).Text((i + 1).ToString()).AlignCenter();
                                    table.Cell().Border(1).PaddingLeft(5).Text(item.PurchaseOrderDetails[i].ProductName);
                                    table.Cell().Border(1).PaddingRight(5).Text(item.PurchaseOrderDetails[i].Quantity.ToString("N2")).AlignRight();
                                    table.Cell().Border(1).PaddingRight(5).Text(item.PurchaseOrderDetails[i].Unitprice.ToString("N2")).AlignRight();
                                    table.Cell().Border(1).PaddingRight(5).Text(item.PurchaseOrderDetails[i].Amount.ToString("N2")).AlignRight();
                                }

                                table.Footer(footer =>
                                {
                                    footer.Cell().RowSpan(2).ColumnSpan(2).Border(1).PaddingLeft(5).Text(text =>
                                    {
                                        text.Span("หมายเหตุ ");
                                        text.Span(item.Description);
                                    });
                                    footer.Cell().ColumnSpan(2).Border(1).PaddingRight(5).Text("ยอดรวม").AlignRight();
                                    footer.Cell().Border(1).PaddingRight(5).Text(item.Subtotal.ToString("N2")).AlignRight();

                                    footer.Cell().ColumnSpan(2).Border(1).PaddingRight(5).Text("ภาษี").AlignRight();
                                    footer.Cell().Border(1).PaddingRight(5).Text(item.Vat.ToString("N2")).AlignRight();

                                    footer.Cell().ColumnSpan(2).Border(1).Text(item.NettotalText).AlignCenter();
                                    footer.Cell().ColumnSpan(2).Border(1).PaddingRight(5).Text("ยอดรวมสุทธิ").AlignRight();
                                    footer.Cell().Border(1).PaddingRight(5).Text(item.Nettotal.ToString("N2")).AlignRight();
                                });
                            });

                            column.Item().Height(80).AlignCenter().Border(1).Row(row =>
                            {
                                row.RelativeItem().AlignBottom().BorderTop(1).Text("ผู้สั่งซื้อ");

                                row.RelativeItem().AlignBottom().BorderTop(1).Text("ผู้มีอำนาจลงนาม");
                            });

                        });

                        // Footer
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
                var fileName = "รายงานใบสั่งซื้อ.pdf";

                return File(pdfBytes, contentType, fileName);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "เกิดข้อผิดพลาดในดาวน์โหลดข้อมูล กรุณาลองอีกครั้งหรือติดต่อผู้ดูแลระบบ";
                _logger.LogError(ex, "เกิดข้อผิดพลาดในดาวน์โหลดข้อมูล");
                return RedirectToAction("Index");
            }
        }*/

        private async Task<bool> POCodeIsExists(Guid purchaseOrderId, string purchaseOrderCode)
        {
            return await _db.PurchaseOrder.AsNoTracking().AnyAsync(x => x.PurchaseOrderId != purchaseOrderId && x.PurchaseOrderCode == purchaseOrderCode);
        }

        private async Task<bool> CheckPurchaseOrderRelatedToOperations(Guid purchaseOrderId)
        {
            return await _db.ReceiveGoods.AsNoTracking().AnyAsync(x => x.PurchaseOrderId == purchaseOrderId);
        }
    }
}
