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
    public class ReceiveGoodsController(ConnectDb db, ILogger<ReceiveGoodsController> logger) : Controller
    {
        private readonly ConnectDb _db = db;
        private readonly ILogger<ReceiveGoodsController> _logger = logger;
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
                ViewData["RGCodeSort"] = sortOrder == "rgCode" ? "rgCode_desc" : "rgCode";
                ViewData["RGDateSort"] = sortOrder == "rgDate" ? "rgDate_desc" : "rgDate";
                ViewData["SupplierSort"] = sortOrder == "supplier" ? "supplier_desc" : "supplier";
                ViewData["NettotalSort"] = sortOrder == "nettotal" ? "nettotal_desc" : "nettotal";
                ViewData["StatusSort"] = sortOrder == "status" ? "status_desc" : "status";

                searchString ??= currentFilter;

                ViewData["SearchBy"] = searchBy;
                ViewData["CurrentFilter"] = searchString;

                await SetWarehouse();

                var query = (from rg in _db.ReceiveGoods
                             join s in _db.Supplier on rg.SupplierId equals s.SupplierId
                             join e in _db.Employee on rg.EmployeeId equals e.EmployeeId
                             where rg.WarehouseId == warehouseId
                             select new ReceiveGoodsViewModel
                             {
                                 ReceiveGoodsId = rg.ReceiveGoodsId,
                                 ReceiveGoodsCode = rg.ReceiveGoodsCode,
                                 ReceiveGoodsDate = rg.ReceiveGoodsDate,
                                 SupplierName = s.SupplierName,
                                 Nettotal = rg.Nettotal
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
                        "ReceiveGoodsCode" => query = query.Where(x => x.ReceiveGoodsCode.ToLower().Contains(searchString.ToLower())),
                        "ReceiveGoodsDate" => query = query.Where(x => x.ReceiveGoodsDate.ToString().ToLower().Contains(searchString.ToLower())),
                        "SupplierName" => query = query.Where(x => x.SupplierName!.ToLower().Contains(searchString.ToLower())),
                        _ => query
                    };
                }

                query = sortOrder switch
                {
                    "rgCode" => query.OrderBy(x => x.ReceiveGoodsCode),
                    "rgCode_desc" => query.OrderByDescending(x => x.ReceiveGoodsCode),
                    "rgDate" => query.OrderBy(x => x.ReceiveGoodsDate),
                    "rgDate_desc" => query.OrderByDescending(x => x.ReceiveGoodsDate),
                    "supplier" => query.OrderBy(x => x.SupplierName),
                    "supplier_desc" => query.OrderByDescending(x => x.SupplierName),
                    "nettotal" => query.OrderBy(x => x.Nettotal),
                    "nettotal_desc" => query.OrderByDescending(x => x.Nettotal),
                    _ => query.OrderByDescending(x => x.ReceiveGoodsCode)
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

                var item = await (from rg in _db.ReceiveGoods
                                  join e in _db.Employee on rg.EmployeeId equals e.EmployeeId
                                  join s in _db.Supplier on rg.SupplierId equals s.SupplierId
                                  join po in _db.PurchaseOrder on rg.PurchaseOrderId equals po.PurchaseOrderId into poJoin
                                  from po in poJoin.DefaultIfEmpty()
                                  join st in _db.Status on rg.StatusId equals st.StatusId
                                  join v in _db.VatType on rg.VatTypeId equals v.VatTypeId
                                  where rg.WarehouseId == warehouseId && rg.ReceiveGoodsId == id
                                  select new ReceiveGoodsViewModel
                                  {
                                      ReceiveGoodsId = rg.ReceiveGoodsId,
                                      ReceiveGoodsCode = rg.ReceiveGoodsCode,
                                      ReceiveGoodsDate = rg.ReceiveGoodsDate,
                                      EmployeeName = e.EmployeeName,
                                      SupplierName = s.SupplierName,
                                      SupplierAddress = FormatAddressHelper.GetFormattedAddress(s.Address, s.Province!.ProvinceName, s.District!.DistrictName, s.Subdistrict!.SubdistrictName, s.Subdistrict!.Zipcode, true),
                                      SupplierPhoneNumber = s.PhoneNumber,
                                      SupplierTaxNumber = s.Taxnumber,
                                      PurchaseOrderCode = po.PurchaseOrderCode,
                                      Subtotal = rg.Subtotal,
                                      Vat = rg.Vat,
                                      Nettotal = rg.Nettotal,
                                      Description = rg.Description,
                                      CreatedDate = rg.CreatedDate,
                                      ModifiedDate = rg.ModifiedDate,
                                      StatusName = st.StatusName,
                                      VatTypeName = v.VatTypeName,
                                      ReceiveGoodsDetails = (from rgo in _db.ReceiveGoodsDetail
                                                             join p in _db.Product on rgo.ProductId equals p.ProductId
                                                             where rgo.ReceiveGoodsId == rg.ReceiveGoodsId
                                                             select new ReceiveGoodsDetailViewModel
                                                             {
                                                                 ProductName = p.ProductName,
                                                                 Quantity = rgo.Quantity,
                                                                 Unitprice = rgo.Unitprice,
                                                                 Amount = rgo.Amount
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

            ReceiveGoodsCreateViewModel model = new()
            {
                ReceiveGoodsCode = await _generateInvoiceNumber.AutoInvoiceNumber("RG", warehouseCode!, _db.ReceiveGoods, x => x.ReceiveGoodsCode),
                ReceiveGoodsDate = DateTime.Now.Date,
                VatTypeId = 3,
                WarehouseId = warehouseId,
                EmployeeId = Guid.Parse(employeeId!),
                EmployeeName = employeeName,
            };

            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ReceiveGoodsCreateViewModel model)
        {
            using var transaction = await _db.Database.BeginTransactionAsync();

            try
            {
                if (ModelState.IsValid)
                {
                    model.ReceiveGoodsCode = model.ReceiveGoodsCode!.Trim();

                    if (await RGCodeIsExists(Guid.Empty, model.ReceiveGoodsCode))
                    {
                        ModelState.AddModelError("ReceiveGoodsCode", "เลขที่ใบรับสินค้ามีอยู่แล้ว");
                        ViewData["VatTypeId"] = await GetVatType();
                        return View(model);
                    }

                    if (model.ReceiveGoodsDetails.Count <= 0)
                    {
                        TempData["Warning"] = "กรุณาเพิ่มรายละเอียดใบรับสินค้า";
                        ViewData["VatTypeId"] = await GetVatType();
                        return View(model);
                    }

                    var status = await _db.Status.Where(x => x.StatusName == "รับสินค้าแล้ว")
                        .Select(x => x.StatusId).FirstOrDefaultAsync();

                    if (status == Guid.Empty)
                    {
                        TempData["Warning"] = "ไม่พบสถานะ 'รับสินค้าแล้ว'";
                        ViewData["VatTypeId"] = await GetVatType();
                        return View(model);
                    }

                    var movementType = await _db.MovementType.Where(x => x.MovementTypeName == "เข้า")
                        .Select(x => x.MovementTypeId).FirstOrDefaultAsync();

                    if (movementType == Guid.Empty)
                    {
                        TempData["Warning"] = "ไม่พบประเภทการเคลื่อนไหว 'เข้า'";
                        ViewData["VatTypeId"] = await GetVatType();
                        return View(model);
                    }

                    await SetWarehouse();

                    var receiveGoods = new ReceiveGoods
                    {
                        ReceiveGoodsId = Guid.NewGuid(),
                        ReceiveGoodsCode = model.ReceiveGoodsCode,
                        ReceiveGoodsDate = model.ReceiveGoodsDate,
                        WarehouseId = warehouseId,
                        SupplierId = model.SupplierId,
                        PurchaseOrderId = model.PurchaseOrderId,
                        EmployeeId = model.EmployeeId,
                        Description = model.Description?.Trim(),
                        StatusId = status,
                        VatTypeId = model.VatTypeId,
                        CreatedDate = DateTimeOffset.Now
                    };

                    if (model.ReceiveGoodsDetails != null && model.ReceiveGoodsDetails.Count != 0)
                    {
                        foreach (var itemDetail in model.ReceiveGoodsDetails)
                        {
                            var receiveGoodsDetail = new ReceiveGoodsDetail
                            {
                                ReceiveGoodsDetailId = Guid.NewGuid(),
                                ProductId = itemDetail.ProductId,
                                Quantity = itemDetail.Quantity,
                                Unitprice = itemDetail.Unitprice,
                                Amount = Math.Round(itemDetail.Quantity * itemDetail.Unitprice, 2),
                                ReceiveGoodsId = receiveGoods.ReceiveGoodsId,
                            };

                            await _db.ReceiveGoodsDetail.AddAsync(receiveGoodsDetail);

                            await SetWarehouse();

                            var itemInventory = await _db.Inventory.FirstOrDefaultAsync(x => x.ProductId == itemDetail.ProductId && x.WarehouseId == warehouseId);

                            if (itemInventory != null)
                            {
                                itemInventory.Balance += receiveGoodsDetail.Quantity;
                                itemInventory.LastUpdated = DateTimeOffset.Now;

                                _db.Inventory.Update(itemInventory);
                            }
                            else
                            {
                                var inventory = new Inventory
                                {
                                    ProductId = receiveGoodsDetail.ProductId,
                                    WarehouseId = warehouseId,
                                    Balance = receiveGoodsDetail.Quantity,
                                    LastUpdated = DateTimeOffset.Now
                                };

                                await _db.Inventory.AddAsync(inventory);
                            }

                            var lotNumber = await _generateInvoiceNumber.AutoInvoiceNumber("LO", warehouseCode!, _db.Lot, x => x.LotNumber);

                            var lot = new Lot
                            {
                                LotId = Guid.NewGuid(),
                                LotNumber = lotNumber,
                                LotDate = DateTime.Now.Date,
                                WarehouseId = warehouseId,
                                ReceiveGoodsId = receiveGoods.ReceiveGoodsId,
                                ProductId = receiveGoodsDetail.ProductId,
                                Quantity = receiveGoodsDetail.Quantity,
                                Balance = receiveGoodsDetail.Quantity,
                                Buyprice = receiveGoodsDetail.Unitprice,
                                CreatedDate = DateTimeOffset.Now
                            };

                            await _db.Lot.AddAsync(lot);

                            var movementProduct = new MovementProduct
                            {
                                MovementProductId = Guid.NewGuid(),
                                MovementProductDate = DateTime.Now.Date,
                                ReceiveGoodsId = receiveGoods.ReceiveGoodsId,
                                MovementTypeId = movementType,
                                LotId = lot.LotId,
                                ProductId = receiveGoodsDetail.ProductId,
                                Quantity = receiveGoodsDetail.Quantity,
                                WarehouseId = warehouseId,
                                CreatedDate = DateTimeOffset.Now
                            };

                            await _db.MovementProduct.AddAsync(movementProduct);
                        }

                        receiveGoods.Subtotal = Math.Round(model.ReceiveGoodsDetails.Sum(x => x.Amount), 2);

                        switch (model.VatTypeId)
                        {
                            case 2:
                                receiveGoods.Vat = Math.Round(receiveGoods.Subtotal * tax / (1 * tax), 2);
                                receiveGoods.Nettotal = receiveGoods.Subtotal;
                                receiveGoods.Subtotal = receiveGoods.Nettotal - receiveGoods.Vat;
                                break;
                            case 3:
                                receiveGoods.Vat = Math.Round(receiveGoods.Subtotal * tax, 2);
                                receiveGoods.Nettotal = Math.Round(receiveGoods.Subtotal + receiveGoods.Vat, 2);
                                break;
                            default:
                                receiveGoods.Vat = 0;
                                receiveGoods.Nettotal = receiveGoods.Subtotal;
                                break;
                        }
                    }

                    await _db.ReceiveGoods.AddAsync(receiveGoods);

                    if (model.PurchaseOrderId != null)
                    {
                        var purchaseOrder = await _db.PurchaseOrder.FindAsync(model.PurchaseOrderId);

                        if (purchaseOrder != null)
                        {
                            purchaseOrder.StatusId = status;
                            _db.PurchaseOrder.Update(purchaseOrder);
                        }
                    }

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

                var item = await (from rg in _db.ReceiveGoods
                                  join e in _db.Employee on rg.EmployeeId equals e.EmployeeId
                                  join s in _db.Supplier on rg.SupplierId equals s.SupplierId
                                  join po in _db.PurchaseOrder on rg.PurchaseOrderId equals po.PurchaseOrderId into poJoin
                                  from po in poJoin.DefaultIfEmpty()
                                  join st in _db.Status on rg.StatusId equals st.StatusId
                                  join v in _db.VatType on rg.VatTypeId equals v.VatTypeId
                                  where rg.WarehouseId == warehouseId && rg.ReceiveGoodsId == id
                                  select new ReceiveGoodsEditViewModel
                                  {
                                      ReceiveGoodsId = rg.ReceiveGoodsId,
                                      ReceiveGoodsCode = rg.ReceiveGoodsCode,
                                      ReceiveGoodsDate = rg.ReceiveGoodsDate,
                                      WarehouseId = rg.WarehouseId,
                                      EmployeeId = rg.EmployeeId,
                                      EmployeeName = e.EmployeeName,
                                      SupplierId = rg.SupplierId,
                                      SupplierName = s.SupplierName,
                                      PurchaseOrderId = rg.PurchaseOrderId,
                                      PurchaseOrderCode = po.PurchaseOrderCode,
                                      Subtotal = rg.Subtotal,
                                      Vat = rg.Vat,
                                      Nettotal = rg.Nettotal,
                                      Description = rg.Description,
                                      StatusId = rg.StatusId,
                                      StatusName = st.StatusName,
                                      VatTypeId = rg.VatTypeId,
                                      VatTypeName = v.VatTypeName,
                                      RowVersion = rg.RowVersion,
                                      ReceiveGoodsDetails = (from rgd in _db.ReceiveGoodsDetail
                                                              join p in _db.Product on rgd.ProductId equals p.ProductId
                                                              where rgd.ReceiveGoodsId == rg.ReceiveGoodsId
                                                              select new ReceiveGoodsDetailViewModel
                                                              {
                                                                  ReceiveGoodsDetailId = rgd.ReceiveGoodsDetailId,
                                                                  ProductId = rgd.ProductId,
                                                                  ProductName = p.ProductName,
                                                                  Quantity = rgd.Quantity,
                                                                  Unitprice = rgd.Unitprice,
                                                                  Amount = rgd.Amount,
                                                                  ReceiveGoodsId = rgd.ReceiveGoodsId
                                                              }).ToList()
                                  }).AsNoTracking().FirstOrDefaultAsync();

                if (item == null)
                {
                    TempData["Warning"] = "ไม่มีข้อมูลในตาราง";
                    _logger.LogWarning("ไม่มีข้อมูลในตาราง");
                    return RedirectToAction("Index");
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
        public async Task<IActionResult> Edit(Guid id, ReceiveGoodsEditViewModel model)
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
                    model.ReceiveGoodsCode = model.ReceiveGoodsCode!.Trim();

                    if (await RGCodeIsExists(model.ReceiveGoodsId, model.ReceiveGoodsCode))
                    {
                        ModelState.AddModelError("ReceiveGoodsCode", "เลขที่ใบรับสินค้ามีอยู่แล้ว");
                        ViewData["VatTypeId"] = await GetVatType();
                        return View(model);
                    }

                    if (model.ReceiveGoodsDetails.Count <= 0)
                    {
                        TempData["Warning"] = "กรุณาเพิ่มรายละเอียดใบรับสินค้า";
                        ViewData["VatTypeId"] = await GetVatType();
                        return View(model);
                    }

                    var oldItem = await _db.ReceiveGoods.Include(x => x.ReceiveGoodsDetail).FirstOrDefaultAsync(x => x.ReceiveGoodsId == id);

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

                        if (oldItem.ReceiveGoodsCode != model.ReceiveGoodsCode)
                        {
                            oldItem.ReceiveGoodsCode = model.ReceiveGoodsCode;
                            isUpdated = true;
                        }

                        if (oldItem.ReceiveGoodsDate != model.ReceiveGoodsDate)
                        {
                            oldItem.ReceiveGoodsDate = model.ReceiveGoodsDate;
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

                        if (oldItem.PurchaseOrderId != model.PurchaseOrderId)
                        {
                            oldItem.PurchaseOrderId = model.PurchaseOrderId;
                            isUpdated = true;
                        }

                        if (oldItem.Description != model.Description)
                        {
                            oldItem.Description = model.Description;
                            isUpdated = true;
                        }

                        if (model.ReceiveGoodsDetails.Count != 0)
                        {
                            await SetWarehouse();

                            var removeDetails = oldItem.ReceiveGoodsDetail
                                .Where(o => !model.ReceiveGoodsDetails
                                .Any(m => m.ReceiveGoodsDetailId == o.ReceiveGoodsDetailId))
                                .ToList();

                            foreach (var remove in removeDetails)
                            {
                                var inventory = await _db.Inventory.Where(x => x.ProductId == remove.ProductId && x.WarehouseId == warehouseId).ToListAsync();

                                if (inventory != null)
                                {
                                    foreach (var item in inventory)
                                    {
                                        if (item.Balance >= remove.Quantity)
                                        {
                                            item.Balance -= remove.Quantity;
                                            item.LastUpdated = DateTimeOffset.Now;

                                            _db.Inventory.Update(item);
                                        }
                                    }
                                }

                                var movementProducts = await _db.MovementProduct.Where(x => x.ReceiveGoodsId == model.ReceiveGoodsId 
                                && x.ProductId == remove.ProductId && x.WarehouseId == warehouseId).ToListAsync();

                                if (movementProducts != null)
                                {
                                    foreach (var movementProduct in movementProducts)
                                    {
                                        _db.MovementProduct.Remove(movementProduct);
                                    }
                                }

                                var lots = await _db.Lot.Where(x => x.ReceiveGoodsId == model.ReceiveGoodsId 
                                && x.ProductId == remove.ProductId && x.WarehouseId == warehouseId).ToListAsync();

                                if (lots != null)
                                {
                                    foreach (var lot in lots)
                                    {
                                        _db.Lot.Remove(lot);
                                    }
                                }

                                _db.ReceiveGoodsDetail.Remove(remove);
                            }

                            var movementType = await _db.MovementType.Where(x => x.MovementTypeName == "เข้า")
                                .Select(x => x.MovementTypeId).FirstOrDefaultAsync();

                            if (movementType == Guid.Empty)
                            {
                                TempData["Warning"] = "ไม่พบประเภทการเคลื่อนไหว 'เข้า'";
                                ViewData["VatTypeId"] = await GetVatType();
                                return View(model);
                            }

                            foreach (var newDetail in model.ReceiveGoodsDetails)
                            {
                                if (oldItem.ReceiveGoodsDetail.All(x => x.ReceiveGoodsDetailId != newDetail.ReceiveGoodsDetailId))
                                {
                                    var receiveGoodsDetail = new ReceiveGoodsDetail
                                    {
                                        ReceiveGoodsDetailId = Guid.NewGuid(),
                                        ProductId = newDetail.ProductId,
                                        Quantity = newDetail.Quantity,
                                        Unitprice = newDetail.Unitprice,
                                        Amount = Math.Round(newDetail.Quantity * newDetail.Unitprice, 2),
                                        ReceiveGoodsId = oldItem.ReceiveGoodsId,
                                    };

                                    await _db.ReceiveGoodsDetail.AddAsync(receiveGoodsDetail);

                                    var itemInventory = await _db.Inventory.FirstOrDefaultAsync(x => x.ProductId == newDetail.ProductId && x.WarehouseId == warehouseId);

                                    if (itemInventory != null)
                                    {
                                        itemInventory.Balance += receiveGoodsDetail.Quantity;
                                        itemInventory.LastUpdated = DateTimeOffset.Now;

                                        _db.Inventory.Update(itemInventory);
                                    }
                                    else
                                    {
                                        var inventory = new Inventory
                                        {
                                            ProductId = receiveGoodsDetail.ProductId,
                                            WarehouseId = warehouseId,
                                            Balance = receiveGoodsDetail.Quantity,
                                            LastUpdated = DateTimeOffset.Now
                                        };

                                        await _db.Inventory.AddAsync(inventory);
                                    }

                                    var lotId = await _db.Lot.FirstOrDefaultAsync(x => x.ReceiveGoodsId == model.ReceiveGoodsId);

                                    var lot = new Lot
                                    {
                                        LotId = Guid.NewGuid(),
                                        LotNumber = lotId!.LotNumber,
                                        LotDate = lotId.LotDate,
                                        WarehouseId = warehouseId,
                                        ReceiveGoodsId = model.ReceiveGoodsId,
                                        ProductId = receiveGoodsDetail.ProductId,
                                        Quantity = receiveGoodsDetail.Quantity,
                                        Balance = receiveGoodsDetail.Quantity,
                                        Buyprice = receiveGoodsDetail.Unitprice,
                                        CreatedDate = DateTimeOffset.Now
                                    };

                                    await _db.Lot.AddAsync(lot);

                                    var movementProduct = new MovementProduct
                                    {
                                        MovementProductId = Guid.NewGuid(),
                                        MovementProductDate = DateTime.Now.Date,
                                        ReceiveGoodsId = model.ReceiveGoodsId,
                                        MovementTypeId = movementType,
                                        LotId = lot.LotId,
                                        ProductId = receiveGoodsDetail.ProductId,
                                        Quantity = receiveGoodsDetail.Quantity,
                                        WarehouseId = warehouseId,
                                        CreatedDate = DateTimeOffset.Now
                                    };

                                    await _db.MovementProduct.AddAsync(movementProduct);

                                    isUpdated = true;
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
                            _db.ReceiveGoods.Update(oldItem);
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

                var item = await (from rg in _db.ReceiveGoods
                                  join e in _db.Employee on rg.EmployeeId equals e.EmployeeId
                                  join s in _db.Supplier on rg.SupplierId equals s.SupplierId
                                  join st in _db.Status on rg.StatusId equals st.StatusId
                                  join v in _db.VatType on rg.VatTypeId equals v.VatTypeId
                                  where rg.WarehouseId == warehouseId && rg.ReceiveGoodsId == id
                                  select new ReceiveGoodsViewModel
                                  {
                                      ReceiveGoodsId = rg.ReceiveGoodsId,
                                      ReceiveGoodsCode = rg.ReceiveGoodsCode,
                                      ReceiveGoodsDate = rg.ReceiveGoodsDate,
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
                                      Subtotal = rg.Subtotal,
                                      Vat = rg.Vat,
                                      Nettotal = rg.Nettotal,
                                      Description = rg.Description,
                                      CreatedDate = rg.CreatedDate,
                                      ModifiedDate = rg.ModifiedDate,
                                      StatusName = st.StatusName,
                                      VatTypeName = v.VatTypeName,
                                      RowVersion = rg.RowVersion,
                                      ReceiveGoodsDetails = (from rgo in _db.ReceiveGoodsDetail
                                                             join p in _db.Product on rgo.ProductId equals p.ProductId
                                                             where rgo.ReceiveGoodsId == rg.ReceiveGoodsId
                                                             select new ReceiveGoodsDetailViewModel
                                                             {
                                                                 ProductName = p.ProductName,
                                                                 Quantity = rgo.Quantity,
                                                                 Unitprice = rgo.Unitprice,
                                                                 Amount = rgo.Amount
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
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(Guid id, byte[] rowVersion)
        {
            using var transaction = await _db.Database.BeginTransactionAsync();

            try
            {
                var item = await _db.ReceiveGoods.Include(x => x.ReceiveGoodsDetail).FirstOrDefaultAsync(x => x.ReceiveGoodsId == id);

                if (item != null)
                {
                    if (!item.RowVersion!.SequenceEqual(rowVersion))
                    {
                        TempData["Error"] = "ข้อมูลถูกแก้ไขโดยผู้อื่นในขณะนี้ กรุณาลองอีกครั้งหรือติดต่อผู้ดูแลระบบ";
                        _logger.LogWarning("ข้อมูลถูกแก้ไขโดยผู้อื่นในขณะนี้");
                        return RedirectToAction("Index");
                    }

                    await SetWarehouse();

                    foreach (var detail in item.ReceiveGoodsDetail)
                    {
                        var inventory = await _db.Inventory.FirstOrDefaultAsync(x => x.ProductId == detail.ProductId && x.WarehouseId == warehouseId);

                        if (inventory != null)
                        {
                            inventory.Balance -= detail.Quantity;
                            inventory.LastUpdated = DateTimeOffset.Now;

                            _db.Inventory.Update(inventory);
                        }

                        var movementProduct = await _db.MovementProduct.FirstOrDefaultAsync(x => x.ProductId == detail.ProductId && x.ReceiveGoodsId == id && x.WarehouseId == warehouseId);

                        if (movementProduct != null)
                        {
                            _db.MovementProduct.Remove(movementProduct);
                        }

                        var lot = await _db.Lot.FirstOrDefaultAsync(x => x.ProductId == detail.ProductId && x.ReceiveGoodsId == id && x.WarehouseId == warehouseId);

                        if (lot != null)
                        {
                            _db.Lot.Remove(lot);
                        }
                    }

                    var statusId = await _db.Status.Where(x => x.StatusName == "ยกเลิก")
                        .Select(x => x.StatusId).FirstOrDefaultAsync();

                    var purchaseOrder = await _db.PurchaseOrder.FirstOrDefaultAsync(x => x.PurchaseOrderId == item.PurchaseOrderId);

                    if (purchaseOrder != null)
                    {
                        purchaseOrder.StatusId = statusId;
                        purchaseOrder.ModifiedDate = DateTimeOffset.Now;

                        _db.PurchaseOrder.Update(purchaseOrder);
                    }

                    _db.ReceiveGoodsDetail.RemoveRange(item.ReceiveGoodsDetail);
                    _db.ReceiveGoods.Remove(item);
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

                var items = await (from rg in _db.ReceiveGoods
                                   join e in _db.Employee on rg.EmployeeId equals e.EmployeeId
                                   join s in _db.Supplier on rg.SupplierId equals s.SupplierId
                                   join st in _db.Status on rg.StatusId equals st.StatusId
                                   join v in _db.VatType on rg.VatTypeId equals v.VatTypeId
                                   where rg.WarehouseId == warehouseId
                                   select new ReceiveGoodsViewModel
                                   {
                                       ReceiveGoodsId = rg.ReceiveGoodsId,
                                       ReceiveGoodsCode = rg.ReceiveGoodsCode,
                                       ReceiveGoodsDate = rg.ReceiveGoodsDate,
                                       SupplierName = s.SupplierName,
                                       EmployeeName = e.EmployeeName,
                                       Subtotal = rg.Subtotal,
                                       Vat = rg.Vat,
                                       Nettotal = rg.Nettotal,
                                       StatusName = st.StatusName,
                                       VatTypeName = v.VatTypeName,
                                       Description = rg.Description,
                                       CreatedDate = rg.CreatedDate,
                                       ModifiedDate = rg.ModifiedDate
                                   }).AsNoTracking().ToListAsync();

                if (items == null || items.Count == 0)
                {
                    TempData["Warning"] = "ไม่มีข้อมูลในตาราง";
                    _logger.LogWarning("ไม่มีข้อมูลในตาราง");
                    return View();
                }

                var builder = new StringBuilder();

                builder.AppendLine("ReceiveGoodsId,ReceiveGoodsCode,ReceiveGoodsDate,SupplierName," +
                    "EmployeeName,Subtotal,Vat,Nettotal,StatusName,VatTypeName,Description,CreatedDate,ModifiedDate");

                foreach (var item in items)
                {
                    builder.AppendLine($"{item.ReceiveGoodsId}, {item.ReceiveGoodsCode}, {item.ReceiveGoodsDate}, " +
                        $"{item.SupplierName}, {item.EmployeeName}, {item.Subtotal}, {item.Vat}, {item.Nettotal}, " +
                        $"{item.StatusName}, {item.VatTypeName}, {item.Description}, {item.CreatedDate}, {item.ModifiedDate}");
                }

                var csvData = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(builder.ToString())).ToArray();
                var contentType = "text/csv";
                var fileName = "รายการใบรับสินค้า.csv";

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

                var items = await (from rg in _db.ReceiveGoods
                                   join e in _db.Employee on rg.EmployeeId equals e.EmployeeId
                                   join s in _db.Supplier on rg.SupplierId equals s.SupplierId
                                   join st in _db.Status on rg.StatusId equals st.StatusId
                                   join v in _db.VatType on rg.VatTypeId equals v.VatTypeId
                                   where rg.WarehouseId == warehouseId
                                   select new ReceiveGoodsViewModel
                                   {
                                       ReceiveGoodsId = rg.ReceiveGoodsId,
                                       ReceiveGoodsCode = rg.ReceiveGoodsCode,
                                       ReceiveGoodsDate = rg.ReceiveGoodsDate,
                                       SupplierName = s.SupplierName,
                                       EmployeeName = e.EmployeeName,
                                       Subtotal = rg.Subtotal,
                                       Vat = rg.Vat,
                                       Nettotal = rg.Nettotal,
                                       StatusName = st.StatusName,
                                       VatTypeName = v.VatTypeName,
                                       Description = rg.Description,
                                       CreatedDate = rg.CreatedDate,
                                       ModifiedDate = rg.ModifiedDate
                                   }).AsNoTracking().ToListAsync();

                if (items == null || items.Count == 0)
                {
                    TempData["Warning"] = "ไม่มีข้อมูลในตาราง";
                    _logger.LogWarning("ไม่มีข้อมูลในตาราง");
                    return View();
                }

                using var workbook = new XLWorkbook();

                var worksheet = workbook.Worksheets.Add("ใบรับสินค้า");

                var fontName = "Angsana New"; //กำหนด Font

                var headerRange = worksheet.Range("A1:M1");
                headerRange.Merge().Value = "รายการใบรับสินค้า"; //ผลาสเซลล์
                headerRange.Style.Font.Bold = true; //ตัวหนา
                headerRange.Style.Font.FontSize = 20; //ขนาดตัวอักษร
                headerRange.Style.Font.FontName = fontName;
                headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center; //แนวตั้ง
                headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center; //แนวนอน

                worksheet.Cell("A2").Value = "รหัสใบรับสินค้า";
                worksheet.Cell("B2").Value = "ว/ด/ป";
                worksheet.Cell("C2").Value = "เลขที่ใบรับสินค้า";
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
                    worksheet.Cell($"A{rowNumber}").Value = item.ReceiveGoodsId.ToString();
                    worksheet.Cell($"B{rowNumber}").Value = item.ReceiveGoodsDate.ToString();
                    worksheet.Cell($"C{rowNumber}").Value = item.ReceiveGoodsCode.ToString();
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
                var fileName = "รายการใบรับสินค้า.xlsx";

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

                var items = await (from rg in _db.ReceiveGoods
                                   join e in _db.Employee on rg.EmployeeId equals e.EmployeeId
                                   join s in _db.Supplier on rg.SupplierId equals s.SupplierId
                                   join st in _db.Status on rg.StatusId equals st.StatusId
                                   join v in _db.VatType on rg.VatTypeId equals v.VatTypeId
                                   where rg.WarehouseId == warehouseId
                                   select new ReceiveGoodsViewModel
                                   {
                                       ReceiveGoodsCode = rg.ReceiveGoodsCode,
                                       ReceiveGoodsDate = rg.ReceiveGoodsDate,
                                       SupplierName = s.SupplierName,
                                       Subtotal = rg.Subtotal,
                                       Vat = rg.Vat,
                                       Nettotal = rg.Nettotal
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

                        page.Header().ShowOnce().Text("รายงานใบรับสินค้า").AlignCenter()
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
                                header.Cell().Element(thead).Text("เลขทีใบรับสินค้า").SemiBold();
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
                                table.Cell().Element(tbody).Text(item.ReceiveGoodsDate.ToString("dd/MM/yyyy")).AlignCenter();
                                table.Cell().Element(tbody).Text(item.ReceiveGoodsCode).AlignCenter();
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
                var fileName = "รายงานใบรับสินค้า.pdf";

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

                var item = await (from rg in _db.ReceiveGoods
                                  join e in _db.Employee on rg.EmployeeId equals e.EmployeeId
                                  join s in _db.Supplier on rg.SupplierId equals s.SupplierId
                                  join po in _db.PurchaseOrder on rg.PurchaseOrderId equals po.PurchaseOrderId into poJoin
                                  from po in poJoin.DefaultIfEmpty()
                                  join st in _db.Status on rg.StatusId equals st.StatusId
                                  join v in _db.VatType on rg.VatTypeId equals v.VatTypeId
                                  where rg.WarehouseId == warehouseId && rg.ReceiveGoodsId == id
                                  select new ReceiveGoodsViewModel
                                  {
                                      ReceiveGoodsId = rg.ReceiveGoodsId,
                                      ReceiveGoodsCode = rg.ReceiveGoodsCode,
                                      ReceiveGoodsDate = rg.ReceiveGoodsDate,
                                      EmployeeName = e.EmployeeName,
                                      SupplierName = s.SupplierName,
                                      SupplierAddress = FormatAddressHelper.GetFormattedAddress(s.Address, s.Province!.ProvinceName, s.District!.DistrictName, s.Subdistrict!.SubdistrictName, s.Subdistrict!.Zipcode, true),
                                      SupplierPhoneNumber = s.PhoneNumber,
                                      SupplierTaxNumber = s.Taxnumber,
                                      PurchaseOrderCode = po.PurchaseOrderCode,
                                      Subtotal = rg.Subtotal,
                                      Vat = rg.Vat,
                                      Nettotal = rg.Nettotal,
                                      Description = rg.Description,
                                      CreatedDate = rg.CreatedDate,
                                      ModifiedDate = rg.ModifiedDate,
                                      StatusName = st.StatusName,
                                      VatTypeName = v.VatTypeName,
                                      ReceiveGoodsDetails = (from rgo in _db.ReceiveGoodsDetail
                                                             join p in _db.Product on rgo.ProductId equals p.ProductId
                                                             where rgo.ReceiveGoodsId == rg.ReceiveGoodsId
                                                             select new ReceiveGoodsDetailViewModel
                                                             {
                                                                 ProductName = p.ProductName,
                                                                 Quantity = rgo.Quantity,
                                                                 Unitprice = rgo.Unitprice,
                                                                 Amount = rgo.Amount
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

                var fileName = $"ใบรับสินค้า {item.ReceiveGoodsCode}.pdf";
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

        private async Task<bool> RGCodeIsExists(Guid receiveGoodsId, string receiveGoodsCode)
        {
            return await _db.ReceiveGoods.AsNoTracking().AnyAsync(x => x.ReceiveGoodsId != receiveGoodsId && x.ReceiveGoodsCode == receiveGoodsCode);
        }
    }
}
