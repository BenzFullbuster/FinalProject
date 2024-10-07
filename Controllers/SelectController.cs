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
