using FinalProject.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinalProject.Controllers
{
    [Route("api")]
    [ApiController]
    public class SelectController(ConnectDb db) : ControllerBase
    {
        private readonly ConnectDb _db = db;

        [HttpGet("GetPODetail")]
        public async Task<IActionResult> ListPODetail(Guid purchaseOrderId)
        {
            var poDetail = await _db.PurchaseOrderDetail.Where(x => x.PurchaseOrderId == purchaseOrderId )
                .Select(x => new { x.ProductId, x.Product!.ProductName, x.Quantity, x.Unitprice, x.Amount })
                .OrderBy(x => x.ProductName).ToListAsync();
            return Ok(poDetail);
        }

        [HttpGet("GetPOCode")]
        public async Task<IActionResult> ListPOCode(Guid supplierId)
        {
            var poCode = await _db.PurchaseOrder.Where(x => x.SupplierId == supplierId && x.Status!.StatusName == "สั่งซื้อแล้ว")
                .Select(x => new { x.PurchaseOrderId, x.PurchaseOrderCode })
                .OrderBy(x => x.PurchaseOrderCode).AsNoTracking().ToListAsync();
            return Ok(poCode);
        }

        [HttpGet("GetSupplier")]
        public async Task<IActionResult> ListSupplier()
        {
            var supplier = await _db.Supplier.Select(x => new { x.SupplierId, x.SupplierName })
                .OrderBy(x => x.SupplierName).AsNoTracking().ToListAsync();
            return Ok(supplier);
        }

        [HttpGet("GetEmployee")]
        public async Task<IActionResult> ListEmployee()
        {
            var employee = await _db.Employee.Select(x => new { x.EmployeeId, x.EmployeeName })
                .OrderBy(x => x.EmployeeName).AsNoTracking().ToListAsync();
            return Ok(employee);
        }

        [HttpGet("GetPosition")]
        public async Task<IActionResult> ListPosition()
        {
            var position = await _db.Position.Select(x => new { x.PositionId, x.PositionName })
                .OrderBy(x => x.PositionName).AsNoTracking().ToListAsync();
            return Ok(position);
        }

        [HttpGet("GetZipcode")]
        public async Task<IActionResult> ListZipcode(int? subdistrictId)
        {
            var zipcode = await _db.Subdistrict.Where(x => x.SubdistrictId == subdistrictId)
                .Select(x => new { x.Zipcode }).FirstOrDefaultAsync();
            return Ok(zipcode);
        }

        [HttpGet("GetSubdistrict")]
        public async Task<IActionResult> ListSubdistrict(int? districtId)
        {
            if (districtId == null)
            {
                var subdistrict = await _db.Subdistrict.Select(x => new { x.SubdistrictId, x.SubdistrictName })
                    .OrderBy(x => x.SubdistrictName).AsNoTracking().ToListAsync();
                return Ok(subdistrict);
            }
            else
            {
                var subdistrict = await _db.Subdistrict.Where(x => x.DistrictId == districtId)
                    .Select(x => new { x.SubdistrictId, x.SubdistrictName })
                    .OrderBy(x => x.SubdistrictName).AsNoTracking().ToListAsync();
                return Ok(subdistrict);
            }
        }

        [HttpGet("GetDistrict")]
        public async Task<IActionResult> ListDistrict(int? provinceId)
        {
            if (provinceId == null)
            {
                var district = await _db.District.Select(x => new { x.DistrictId, x.DistrictName })
                    .OrderBy(x => x.DistrictName).AsNoTracking().ToListAsync();
                return Ok(district);
            }
            else
            {
                var district = await _db.District.Where(x => x.ProvinceId == provinceId)
                    .Select(x => new { x.DistrictId, x.DistrictName })
                    .OrderBy(x => x.DistrictName).AsNoTracking().ToListAsync();
                return Ok(district);
            }
        }

        [HttpGet("GetProvince")]
        public async Task<IActionResult> ListProvince()
        {
            var province = await _db.Province.Select(x => new { x.ProvinceId, x.ProvinceName })
                .OrderBy(x => x.ProvinceName).AsNoTracking().ToListAsync();
            return Ok(province);
        }

        [HttpGet("GetProduct")]
        public async Task<IActionResult> ListProduct(Guid? categoryId)
        {
            if (categoryId == null)
            {
                var product = await _db.Product.Select(x => new { x.ProductId, x.ProductName })
                    .OrderBy(x => x.ProductName).ToListAsync();
                return Ok(product);
            }
            else
            {
                var product = await _db.Product.Where(x => x.CategoryId == categoryId)
                    .Select(x => new { x.ProductId, x.ProductName }).OrderBy(x => x.ProductName).ToListAsync();
                return Ok(product);
            }
        }

        [HttpGet("GetCategory")]
        public async Task<IActionResult> ListCategory()
        {
            var category = await _db.Category.Select(x => new { x.CategoryId, x.CategoryName })
                .OrderBy(x => x.CategoryName).AsNoTracking().ToListAsync();
            return Ok(category);
        }

        [HttpGet("GetUnit")]
        public async Task<IActionResult> ListUnit()
        {
            var unit = await _db.Unit.Select(x => new { x.UnitId, x.UnitName })
                .OrderBy(x => x.UnitName).AsNoTracking().ToListAsync();
            return Ok(unit);
        }
    }
}
