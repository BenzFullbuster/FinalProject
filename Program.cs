using FinalProject.Helpers;
using FinalProject.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Infrastructure;
using Rotativa.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ConnectDb>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DbCon")));
builder.Services.AddControllersWithViews();

builder.Services.AddAuthentication(CookieAuthenticationDefaults
    .AuthenticationScheme).AddCookie(options =>
    {
        options.Cookie.Name = "CookieMyWebApp";
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.LoginPath = "/Home/Login";
        options.LogoutPath = "/Home/Logout";
        options.AccessDeniedPath = "/Home/AccessDenied";
    });

var app = builder.Build();

QuestPDF.Settings.License = LicenseType.Community;

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ConnectDb>();

    await BasicData(context);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseRotativa();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

async Task BasicData(ConnectDb db)
{
    if (!db.Position.Any())
    {
        using var transaction = await db.Database.BeginTransactionAsync();

        try
        {
            var positionId = Guid.NewGuid();
            var warehouseId = Guid.NewGuid();
            var employeeId = Guid.NewGuid();

            await db.Position.AddAsync(new Position
            {
                PositionId = positionId,
                PositionName = "ผู้ดูแลระบบ",
                CreatedDate = DateTimeOffset.Now
            });

            await db.Warehouse.AddAsync(new Warehouse
            {
                WarehouseId = warehouseId,
                WarehouseCode = "00000",
                WarehouseName = "กรุงเทพฯ",
                CreatedDate = DateTimeOffset.Now
            });

            byte[] salt = GeneratePassword.GenerateSalt();

            await db.Employee.AddAsync(new Employee
            {
                EmployeeId = employeeId,
                Username = "Admin",
                Salt = Convert.ToBase64String(salt),
                Password = GeneratePassword.HashPassword("Admin", salt),
                EmployeeName = "Administrator",
                Email = "full_bu_ster@hotmail.com",
                PositionId = positionId,
                Taxnumber = "0000000000000",
                CreatedDate = DateTimeOffset.Now,
            });

            await db.EmployeeWarehouse.AddAsync(new EmployeeWarehouse
            {
                EmployeeId = employeeId,
                WarehouseId = warehouseId,
                Approval = true,
                LastUpdated = DateTimeOffset.Now
            });

            await db.VatType.AddRangeAsync(new List<VatType>
            {
                new() 
                {
                    VatTypeName = "ไม่มี_VAT",
                    CreatedDate = DateTimeOffset.Now
                },
                new() 
                {
                    VatTypeName = "รวม_VAT",
                    CreatedDate = DateTimeOffset.Now
                },
                new() 
                {
                    VatTypeName = "แยก_VAT",
                    CreatedDate = DateTimeOffset.Now
                }
            });

            await db.Status.AddRangeAsync(new List<Status>
            {
                new()
                {
                    StatusId = Guid.NewGuid(),
                    StatusName = "สั่งซื้อแล้ว",
                    CreatedDate = DateTimeOffset.Now
                },
                new()
                {
                    StatusId = Guid.NewGuid(),
                    StatusName = "รับสินค้าแล้ว",
                    CreatedDate = DateTimeOffset.Now
                },
                new()
                {
                    StatusId = Guid.NewGuid(),
                    StatusName = "ยกเลิก",
                    CreatedDate = DateTimeOffset.Now
                }
            });

            await db.MovementType.AddRangeAsync(new List<MovementType>
            {
                new()
                {
                    MovementTypeId = Guid.NewGuid(),
                    MovementTypeName = "เข้า",
                    CreatedDate = DateTimeOffset.Now
                },
                new()
                {
                    MovementTypeId = Guid.NewGuid(),
                    MovementTypeName = "ออก",
                    CreatedDate = DateTimeOffset.Now
                },
                new()
                {
                    MovementTypeId = Guid.NewGuid(),
                    MovementTypeName = "ส่งคืน",
                    CreatedDate = DateTimeOffset.Now
                },
                new()
                {
                    MovementTypeId = Guid.NewGuid(),
                    MovementTypeName = "รับคืน",
                    CreatedDate = DateTimeOffset.Now
                }
            });


            await db.SaveChangesAsync();
            await transaction.CommitAsync();
            Console.WriteLine("เพิ่มข้อมูลสำเร็จ");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
            await transaction.RollbackAsync();
            throw;
        }
    }
}