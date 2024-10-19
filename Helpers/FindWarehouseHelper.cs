using FinalProject.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FinalProject.Helpers
{
    public static class FindWarehouseHelper
    {
        public static string GetWarehouseCode(this ClaimsPrincipal user)
        {
            return user.FindFirstValue("WarehouseCode") ?? string.Empty;
        }

        public static async Task<Guid> GetWarehouseId(this DbContext _db, string warehouseCode)
        {
            return await _db.Set<Warehouse>().Where(x => x.WarehouseCode == warehouseCode)
                .Select(x => x.WarehouseId).SingleOrDefaultAsync();
        }
    }
}
