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
    public class SupplierController(ConnectDb db, ILogger<SupplierController> logger) : Controller
    {
        private readonly ConnectDb _db = db;
        private readonly ILogger<SupplierController> _logger = logger;
        public async Task<IActionResult> Index(string searchBy, string searchString, string currentFilter, string sortOrder, int? pageSize, int currentPage = 1)
        {
            try
            {
                ViewData["CurrentSort"] = sortOrder;
                ViewData["NameSort"] = sortOrder == "name" ? "name_desc" : "name";
                ViewData["FullAddressSort"] = sortOrder == "fullAddress" ? "fullAddress_desc" : "fullAddress";
                ViewData["PhoneNumberSort"] = sortOrder == "phoneNumber" ? "phoneNumber_desc" : "phoneNumber";
                ViewData["EmailSort"] = sortOrder == "email" ? "email_desc" : "email";

                searchString ??= currentFilter;

                ViewData["SearchBy"] = searchBy;
                ViewData["CurrentFilter"] = searchString;

                var query = (from s in _db.Supplier
                                   join p in _db.Province on s.ProvinceId equals p.ProvinceId into pJoin
                                   from p in pJoin.DefaultIfEmpty()
                                   join d in _db.District on s.DistrictId equals d.DistrictId into dJoin
                                   from d in dJoin.DefaultIfEmpty()
                                   join sd in _db.Subdistrict on s.SubdistrictId equals sd.SubdistrictId into sdJoin
                                   from sd in sdJoin.DefaultIfEmpty()
                                   select new SupplierViewModel
                                   {
                                       SupplierId = s.SupplierId,
                                       SupplierName = s.SupplierName,
                                       Address = s.Address,
                                       ProvinceName = p.ProvinceName,
                                       DistrictName = d.DistrictName,
                                       SubdistrictName = sd.SubdistrictName,
                                       Zipcode = s.Zipcode,
                                       PhoneNumber = s.PhoneNumber,
                                       Email = s.Email
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
                        "SupplierName" => query = query.Where(x => x.SupplierName.ToLower().Contains(searchString.ToLower())),
                        "FullAddress" => query = query.Where(x => x.Address!.ToLower().Contains(searchString.ToLower())
                            || x.SubdistrictName!.ToLower().Contains(searchString.ToLower())
                            || x.DistrictName!.ToLower().Contains(searchString.ToLower())
                            || x.ProvinceName!.ToLower().Contains(searchString.ToLower())
                            || x.Zipcode!.ToLower().Contains(searchString.ToLower())),
                        "Email" => query = query.Where(x => x.Email!.ToLower().Contains(searchString.ToLower())),
                        "PhoneNumber" => query = query.Where(x => x.PhoneNumber!.ToLower().Contains(searchString.ToLower())),
                        _ => query
                    };
                }

                query = sortOrder switch
                {
                    "name" => query.OrderBy(x => x.SupplierName),
                    "name_desc" => query.OrderByDescending(x => x.SupplierName),
                    "FullAddressSort" => query.OrderBy(x => x.FullAddress),
                    "FullAddressSort_desc" => query.OrderByDescending(x => x.FullAddress),
                    "phoneNumber" => query.OrderBy(x => x.PhoneNumber),
                    "phoneNumber_desc" => query.OrderByDescending(x => x.PhoneNumber),
                    "email" => query.OrderBy(x => x.Email),
                    "email_desc" => query.OrderByDescending(x => x.Email),
                    _ => query.OrderBy(x => x.SupplierName)
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
                var item = await (from s in _db.Supplier
                                 join p in _db.Province on s.ProvinceId equals p.ProvinceId into pJoin
                                 from p in pJoin.DefaultIfEmpty()
                                 join d in _db.District on s.DistrictId equals d.DistrictId into dJoin
                                 from d in dJoin.DefaultIfEmpty()
                                 join sd in _db.Subdistrict on s.SubdistrictId equals sd.SubdistrictId into sdJoin
                                 from sd in sdJoin.DefaultIfEmpty()
                                 where s.SupplierId == id
                                 select new SupplierViewModel
                                 {
                                     SupplierId = s.SupplierId,
                                     SupplierName = s.SupplierName,
                                     Address = s.Address,
                                     ProvinceName = p.ProvinceName,
                                     DistrictName = d.DistrictName,
                                     SubdistrictName = sd.SubdistrictName,
                                     Zipcode = s.Zipcode,
                                     PhoneNumber = s.PhoneNumber,
                                     Email = s.Email,
                                     Taxnumber = s.Taxnumber,
                                     Description = s.Description,
                                     CreatedDate = s.CreatedDate,
                                     ModifiedDate = s.ModifiedDate
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
        public async Task<IActionResult> Create(SupplierCreateViewModel model)
        {
            using var transaction = await _db.Database.BeginTransactionAsync();

            try
            {
                if (ModelState.IsValid)
                {
                    model.SupplierName = model.SupplierName.Trim();

                    if (await NameIsExists(Guid.Empty, model.SupplierName))
                    {
                        ModelState.AddModelError("SupplierName", "ชื่อนี้มีอยู่แล้ว");
                        return View(model);
                    }

                    var item = new Supplier
                    { 
                        SupplierId = Guid.NewGuid(),
                        SupplierName = model.SupplierName,
                        Address = model.Address,
                        ProvinceId = model.ProvinceId,
                        DistrictId = model.DistrictId,
                        SubdistrictId = model.SubdistrictId,
                        Zipcode = model.Zipcode,
                        PhoneNumber = model.PhoneNumber,
                        Email = model.Email,
                        Taxnumber = model.Taxnumber.Replace(" ", ""),
                        Description = model.Description?.Trim(),
                        CreatedDate = DateTimeOffset.Now
                    };

                    await _db.Supplier.AddAsync(item);
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
                var item = await (from s in _db.Supplier
                                  join p in _db.Province on s.ProvinceId equals p.ProvinceId into pJoin
                                  from p in pJoin.DefaultIfEmpty()
                                  join d in _db.District on s.DistrictId equals d.DistrictId into dJoin
                                  from d in dJoin.DefaultIfEmpty()
                                  join sd in _db.Subdistrict on s.SubdistrictId equals sd.SubdistrictId into sdJoin
                                  from sd in sdJoin.DefaultIfEmpty()
                                  where s.SupplierId == id
                                  select new SupplierEditViewModel
                                  {
                                      SupplierId = s.SupplierId,
                                      SupplierName = s.SupplierName,
                                      Address = s.Address,
                                      ProvinceId = s.ProvinceId,
                                      ProvinceName = p.ProvinceName,
                                      DistrictId = s.DistrictId,
                                      DistrictName = d.DistrictName,
                                      SubdistrictId = s.SubdistrictId,
                                      SubdistrictName = sd.SubdistrictName,
                                      Zipcode = s.Zipcode,
                                      PhoneNumber = s.PhoneNumber,
                                      Email = s.Email,
                                      Taxnumber = s.Taxnumber,
                                      Description = s.Description,
                                      RowVersion = s.RowVersion
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
        public async Task<IActionResult> Edit(Guid id, SupplierEditViewModel model)
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
                    model.SupplierName = model.SupplierName.Trim();

                    if (await NameIsExists(model.SupplierId, model.SupplierName))
                    {
                        ModelState.AddModelError("SupplierName", "ชื่อนี้มีอยู่แล้ว");
                        return View(model);
                    }

                    var oldItem = await _db.Supplier.FirstOrDefaultAsync(x => x.SupplierId == id);

                    if (oldItem != null)
                    {
                        if (!oldItem.RowVersion!.SequenceEqual(model.RowVersion!))
                        {
                            TempData["Warning"] = "ข้อมูลถูกแก้ไขโดยผู้อื่นในขณะนี้ กรุณาลองอีกครั้งหรือติดต่อผู้ดูแลระบบ";
                            _logger.LogWarning("ข้อมูลถูกแก้ไขโดยผู้อื่นในขณะนี้");
                            return RedirectToAction("Edit", new { id });
                        }

                        var isUpdated = false;

                        if (oldItem.SupplierName != model.SupplierName)
                        {
                            oldItem.SupplierName = model.SupplierName;
                            isUpdated = true;
                        }

                        if (oldItem.Address != model.Address)
                        {
                            oldItem.Address = model.Address;
                            isUpdated = true;
                        }

                        if (oldItem.ProvinceId != model.ProvinceId)
                        {
                            oldItem.ProvinceId = model.ProvinceId;
                            isUpdated = true;
                        }

                        if (oldItem.DistrictId != model.DistrictId)
                        {
                            oldItem.DistrictId = model.DistrictId;
                            isUpdated = true;
                        }

                        if (oldItem.SubdistrictId != model.SubdistrictId)
                        {
                            oldItem.SubdistrictId = model.SubdistrictId;
                            isUpdated = true;
                        }

                        if (oldItem.Zipcode != model.Zipcode)
                        {
                            oldItem.Zipcode = model.Zipcode;
                            isUpdated = true;
                        }

                        if (oldItem.PhoneNumber != model.PhoneNumber)
                        {
                            oldItem.PhoneNumber = model.PhoneNumber;
                            isUpdated = true;
                        }

                        if (oldItem.Email != model.Email)
                        {
                            oldItem.Email = model.Email;
                            isUpdated = true;
                        }

                        if (oldItem.Taxnumber != model.Taxnumber.Replace(" ", ""))
                        {
                            oldItem.Email = model.Taxnumber.Replace(" ", "");
                            isUpdated = true;
                        }

                        if (isUpdated)
                        {
                            oldItem.ModifiedDate = DateTimeOffset.Now;
                            _db.Supplier.Update(oldItem);
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
                var item = await (from s in _db.Supplier
                                  join p in _db.Province on s.ProvinceId equals p.ProvinceId into pJoin
                                  from p in pJoin.DefaultIfEmpty()
                                  join d in _db.District on s.DistrictId equals d.DistrictId into dJoin
                                  from d in dJoin.DefaultIfEmpty()
                                  join sd in _db.Subdistrict on s.SubdistrictId equals sd.SubdistrictId into sdJoin
                                  from sd in sdJoin.DefaultIfEmpty()
                                  where s.SupplierId == id
                                  select new SupplierViewModel
                                  {
                                      SupplierId = s.SupplierId,
                                      SupplierName = s.SupplierName,
                                      Address = s.Address,
                                      ProvinceName = p.ProvinceName,
                                      DistrictName = d.DistrictName,
                                      SubdistrictName = sd.SubdistrictName,
                                      Zipcode = s.Zipcode,
                                      PhoneNumber = s.PhoneNumber,
                                      Email = s.Email,
                                      Taxnumber = s.Taxnumber,
                                      Description = s.Description,
                                      CreatedDate = s.CreatedDate,
                                      ModifiedDate = s.ModifiedDate,
                                      RowVersion = s.RowVersion
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
                var item = await _db.Supplier.Where(x => x.SupplierId == id).FirstOrDefaultAsync();

                if (item != null)
                {
                    if (!item.RowVersion!.SequenceEqual(rowVersion))
                    {
                        TempData["Error"] = "ข้อมูลถูกแก้ไขโดยผู้อื่นในขณะนี้ กรุณาลองอีกครั้งหรือติดต่อผู้ดูแลระบบ";
                        _logger.LogWarning("ข้อมูลถูกแก้ไขโดยผู้อื่นในขณะนี้");
                        return RedirectToAction("Index");
                    }

                    if (await CheckSupplierRelatedToOperations(id))
                    {
                        TempData["Warning"] = $"ไม่สามารถลบ {item.SupplierName} ได้เนื่องจากมีการอ้างอิงอยู่";
                        _logger.LogWarning("ไม่สามารถลบได้เนื่องจากมีการอ้างอิงอยู่");
                        return RedirectToAction("Index");
                    }

                    _db.Supplier.Remove(item);
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
                var items = await (from e in _db.Supplier
                                   join p in _db.Province on e.ProvinceId equals p.ProvinceId into pJoin
                                   from p in pJoin.DefaultIfEmpty()
                                   join d in _db.District on e.DistrictId equals d.DistrictId into dJoin
                                   from d in dJoin.DefaultIfEmpty()
                                   join s in _db.Subdistrict on e.SubdistrictId equals s.SubdistrictId into sJoin
                                   from s in sJoin.DefaultIfEmpty()
                                   select new SupplierViewModel
                                   {
                                       SupplierId = e.SupplierId,
                                       SupplierName = e.SupplierName,
                                       Address = e.Address,
                                       SubdistrictName = s.SubdistrictName,
                                       DistrictName = d.DistrictName,
                                       ProvinceName = p.ProvinceName,
                                       Zipcode = e.Zipcode,
                                       PhoneNumber = e.PhoneNumber,
                                       Email = e.Email,
                                       Taxnumber = e.Taxnumber,
                                       Description = e.Description,
                                       CreatedDate = e.CreatedDate,
                                       ModifiedDate = e.ModifiedDate
                                   }).AsNoTracking().ToListAsync();

                if (items == null || items.Count == 0)
                {
                    TempData["Warning"] = "ไม่มีข้อมูลในตาราง";
                    _logger.LogWarning("ไม่มีข้อมูลในตาราง");
                    return View();
                }

                var builder = new StringBuilder();

                builder.AppendLine("SupplierId,SupplierName,Address," +
                    "SubdistrictName,DistrictName,ProvinceName,Zipcode," +
                    "PhoneNumber, Email, Taxnumber, Description, CreatedDate, ModifiedDate");

                foreach (var item in items)
                {
                    builder.AppendLine($"{item.SupplierId}, {item.SupplierName}, {item.Address}, " +
                        $"{item.SubdistrictName}, {item.DistrictName}, {item.ProvinceName}, " +
                        $"{item.Zipcode}, {item.PhoneNumber}, {item.Email}, {item.Taxnumber}, " +
                        $"{item.Description}, {item.CreatedDate}, {item.ModifiedDate}");
                }

                var csvData = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(builder.ToString())).ToArray();
                var contentType = "text/csv";
                var fileName = "รายการผู้จำหน่าย.csv";

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
                var items = await (from e in _db.Supplier
                                   join p in _db.Province on e.ProvinceId equals p.ProvinceId into pJoin
                                   from p in pJoin.DefaultIfEmpty()
                                   join d in _db.District on e.DistrictId equals d.DistrictId into dJoin
                                   from d in dJoin.DefaultIfEmpty()
                                   join s in _db.Subdistrict on e.SubdistrictId equals s.SubdistrictId into sJoin
                                   from s in sJoin.DefaultIfEmpty()
                                   select new SupplierViewModel
                                   {
                                       SupplierId = e.SupplierId,
                                       SupplierName = e.SupplierName,
                                       Address = e.Address,
                                       SubdistrictName = s.SubdistrictName,
                                       DistrictName = d.DistrictName,
                                       ProvinceName = p.ProvinceName,
                                       Zipcode = e.Zipcode,
                                       PhoneNumber = e.PhoneNumber,
                                       Email = e.Email,
                                       Taxnumber = e.Taxnumber,
                                       Description = e.Description,
                                       CreatedDate = e.CreatedDate,
                                       ModifiedDate = e.ModifiedDate
                                   }).AsNoTracking().ToListAsync();

                if (items == null || items.Count == 0)
                {
                    TempData["Warning"] = "ไม่มีข้อมูลในตาราง";
                    _logger.LogWarning("ไม่มีข้อมูลในตาราง");
                    return View();
                }

                using var workbook = new XLWorkbook();

                var worksheet = workbook.Worksheets.Add("ผู้จำหน่าย");

                var fontName = "Angsana New"; //กำหนด Font

                var headerRange = worksheet.Range("A1:M1");
                headerRange.Merge().Value = "รายการผู้จำหน่าย"; //ผลาสเซลล์
                headerRange.Style.Font.Bold = true; //ตัวหนา
                headerRange.Style.Font.FontSize = 20; //ขนาดตัวอักษร
                headerRange.Style.Font.FontName = fontName;
                headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center; //แนวตั้ง
                headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center; //แนวนอน

                worksheet.Cell("A2").Value = "รหัสผู้จำหน่าย";
                worksheet.Cell("B2").Value = "ชื่อผู้จำหน่าย";
                worksheet.Cell("C2").Value = "ที่อยู่";
                worksheet.Cell("D2").Value = "แขวง/ตำบล";
                worksheet.Cell("E2").Value = "เขต/อำเภอ";
                worksheet.Cell("F2").Value = "จังหวัด";
                worksheet.Cell("G2").Value = "รหัสไปรษณีย์";
                worksheet.Cell("H2").Value = "เบอร์โทรศัพท์";
                worksheet.Cell("I2").Value = "อีเมล";
                worksheet.Cell("J2").Value = "เลขประจำตัวผู้เสียภาษี";
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
                    worksheet.Cell($"A{rowNumber}").Value = item.SupplierId.ToString();
                    worksheet.Cell($"B{rowNumber}").Value = item.SupplierName.ToString();
                    worksheet.Cell($"C{rowNumber}").Value = item.Address?.ToString();
                    worksheet.Cell($"D{rowNumber}").Value = item.SubdistrictName?.ToString();
                    worksheet.Cell($"E{rowNumber}").Value = item.DistrictName?.ToString();
                    worksheet.Cell($"F{rowNumber}").Value = item.ProvinceName?.ToString();
                    worksheet.Cell($"G{rowNumber}").Value = item.Zipcode?.ToString();
                    worksheet.Cell($"H{rowNumber}").Value = item.PhoneNumber?.ToString();
                    worksheet.Cell($"I{rowNumber}").Value = item.Email?.ToString();
                    worksheet.Cell($"J{rowNumber}").Value = item.FormatTaxNumber!.ToString();
                    worksheet.Cell($"K{rowNumber}").Value = item.Description?.ToString();
                    worksheet.Cell($"L{rowNumber}").Value = item.CreatedDate.ToString();
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
                var fileName = "รายการผู้จำหน่าย.xlsx";

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
                var items = await (from e in _db.Supplier
                                   join p in _db.Province on e.ProvinceId equals p.ProvinceId into pJoin
                                   from p in pJoin.DefaultIfEmpty()
                                   join d in _db.District on e.DistrictId equals d.DistrictId into dJoin
                                   from d in dJoin.DefaultIfEmpty()
                                   join s in _db.Subdistrict on e.SubdistrictId equals s.SubdistrictId into sJoin
                                   from s in sJoin.DefaultIfEmpty()
                                   select new SupplierViewModel
                                   {
                                       SupplierName = e.SupplierName,
                                       Address = e.Address,
                                       SubdistrictName = s.SubdistrictName,
                                       DistrictName = d.DistrictName,
                                       ProvinceName = p.ProvinceName,
                                       Zipcode = e.Zipcode,
                                       PhoneNumber = e.PhoneNumber,
                                       Email = e.Email
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

                        page.Header().ShowOnce().Text("รายงานผู้จำหน่าย").AlignCenter()
                        .SemiBold().FontSize(30).FontColor(Colors.Blue.Medium);

                        page.Content().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(50);
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(3);
                                columns.ConstantColumn(90);
                                columns.RelativeColumn(2);
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
                                header.Cell().Element(thead).Text("ผู้จำหน่าย").SemiBold();
                                header.Cell().Element(thead).Text("ที่อยู่").SemiBold();
                                header.Cell().Element(thead).Text("เบอร์โทรศัพท์").SemiBold();
                                header.Cell().Element(thead).Text("อีเมล").SemiBold();
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
                                table.Cell().Element(tbody).Text(item.SupplierName);
                                table.Cell().Element(tbody).Text(item.FullAddress);
                                table.Cell().Element(tbody).Text(item.PhoneNumber).AlignCenter();
                                table.Cell().Element(tbody).Text(item.Email);
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
                var fileName = "รายงานผู้จำหน่าย.pdf";

                return File(pdfBytes, contentType, fileName);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "เกิดข้อผิดพลาดในดาวน์โหลดข้อมูล กรุณาลองอีกครั้งหรือติดต่อผู้ดูแลระบบ";
                _logger.LogError(ex, "เกิดข้อผิดพลาดในดาวน์โหลดข้อมูล");
                return RedirectToAction("Index");
            }
        }

        private async Task<bool> NameIsExists(Guid supplierId, string supplierName)
        {
            return await _db.Supplier.AsNoTracking().AnyAsync(x => x.SupplierId != supplierId && x.SupplierName == supplierName);
        }

        private async Task<bool> CheckSupplierRelatedToOperations(Guid supplierId)
        {
            return await _db.PurchaseOrder.AsNoTracking().AnyAsync(x => x.SupplierId == supplierId)
                || await _db.ReceiveGoods.AsNoTracking().AnyAsync(x => x.SupplierId == supplierId)
                || await _db.GoodsReturnToSupplier.AsNoTracking().AnyAsync(x => x.SupplierId == supplierId);
        }
    }
}
