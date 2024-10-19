using FinalProject.Models;
using Microsoft.EntityFrameworkCore;

namespace FinalProject.Helpers
{
    public class ConnectDb(DbContextOptions<ConnectDb> options) : DbContext(options)
    {
        public DbSet<Unit> Unit { get; set; } //หน่วยนับ
        public DbSet<Category> Category { get; set; } //หมวดสินค้า
        public DbSet<Warehouse> Warehouse { get; set; } //คลังสินค้า
        public DbSet<Product> Product { get; set; } //สินค้า
        public DbSet<Inventory> Inventory { get; set; } //สินค้าคงเหลือแต่ละคลัง
        public DbSet<Province> Province { get; set; } //จังหวัด
        public DbSet<District> District { get; set; } //เขต/อำเภอ
        public DbSet<Subdistrict> Subdistrict { get; set; } //แขวง/ตำบล
        public DbSet<Position> Position { get; set; } //ตำแหน่ง
        public DbSet<Employee> Employee { get; set; } //พนักงาน
        public DbSet<EmployeeWarehouse> EmployeeWarehouse { get; set; } //สิทธิ์การเข้าถึงคลังพนักงาน
        public DbSet<Supplier> Supplier { get; set; } //ผู้จำหน่าย
        public DbSet<Customer> Customer { get; set; } //ลูกค้า
        public DbSet<Status> Status { get; set; } //สถานะ
        public DbSet<VatType> VatType { get; set; } //ประเภทภาษี
        public DbSet<PurchaseOrder> PurchaseOrder { get; set; } //ใบสั่งซื้อ
        public DbSet<PurchaseOrderDetail> PurchaseOrderDetail { get; set; }
        public DbSet<ReceiveGoods> ReceiveGoods { get; set; } //ใบรับสินค้า
        public DbSet<ReceiveGoodsDetail> ReceiveGoodsDetail { get; set; }
        public DbSet<Lot> Lot { get; set; } //Lotสินค้า
        public DbSet<MovementType> MovementType { get; set; } //ประเภทการเคลื่อนไหว
        public DbSet<MovementProduct> MovementProduct { get; set; } //การเคลื่อนไหวของสินค้า
        public DbSet<DeliveryGoods> DeliveryGoods { get; set; } //ใบส่งสินค้า
        public DbSet<DeliveryGoodsDetail> DeliveryGoodsDetail { get; set; }
        public DbSet<GoodsReturnFromCustomer> GoodsReturnFromCustomer { get; set; } //รับคืนสินค้า
        public DbSet<GoodsReturnFromCustomerDetail> GoodsReturnFromCustomerDetail { get; set; }
        public DbSet<GoodsReturnToSupplier> GoodsReturnToSupplier { get; set; } //ส่งคืนสินค้า
        public DbSet<GoodsReturnToSupplierDetail> GoodsReturnToSupplierDetail { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Product>(entity =>
            {
                entity.Property(x => x.Sellprice).HasPrecision(18, 2);
            });

            modelBuilder.Entity<Inventory>(entity =>
            {
                entity.HasKey(x => new { x.ProductId, x.WarehouseId });
                entity.Property(x => x.Balance).HasPrecision(18, 2);
            });

            modelBuilder.Entity<EmployeeWarehouse>(entity =>
            {
                entity.HasKey(x => new { x.EmployeeId, x.WarehouseId });
            });

            modelBuilder.Entity<PurchaseOrder>(entity =>
            {
                entity.Property(x => x.Subtotal).HasPrecision(18, 2);
                entity.Property(x => x.Vat).HasPrecision(18, 2);
                entity.Property(x => x.Nettotal).HasPrecision(18, 2);
            });

            modelBuilder.Entity<PurchaseOrderDetail>(entity =>
            {
                entity.Property(x => x.Quantity).HasPrecision(18, 2);
                entity.Property(x => x.Unitprice).HasPrecision(18, 2);
                entity.Property(x => x.Amount).HasPrecision(18, 2);
            });

            modelBuilder.Entity<ReceiveGoods>(entity =>
            {
                entity.Property(x => x.Subtotal).HasPrecision(18, 2);
                entity.Property(x => x.Vat).HasPrecision(18, 2);
                entity.Property(x => x.Nettotal).HasPrecision(18, 2);
            });

            modelBuilder.Entity<ReceiveGoodsDetail>(entity =>
            {
                entity.Property(x => x.Quantity).HasPrecision(18, 2);
                entity.Property(x => x.Unitprice).HasPrecision(18, 2);
                entity.Property(x => x.Amount).HasPrecision(18, 2);
            });

            modelBuilder.Entity<Lot>(entity =>
            {
                entity.Property(x => x.Quantity).HasPrecision(18, 2);
                entity.Property(x => x.Buyprice).HasPrecision(18, 2);
            });

            modelBuilder.Entity<MovementProduct>(entity =>
            {
                entity.Property(x => x.Quantity).HasPrecision(18, 2);
            });

            modelBuilder.Entity<DeliveryGoods>(entity =>
            {
                entity.Property(x => x.Subtotal).HasPrecision(18, 2);
                entity.Property(x => x.Vat).HasPrecision(18, 2);
                entity.Property(x => x.Nettotal).HasPrecision(18, 2);
            });

            modelBuilder.Entity<DeliveryGoodsDetail>(entity =>
            {
                entity.Property(x => x.Quantity).HasPrecision(18, 2);
                entity.Property(x => x.Unitprice).HasPrecision(18, 2);
                entity.Property(x => x.Amount).HasPrecision(18, 2);
            });

            modelBuilder.Entity<GoodsReturnFromCustomer>(entity =>
            {
                entity.Property(x => x.Subtotal).HasPrecision(18, 2);
                entity.Property(x => x.Vat).HasPrecision(18, 2);
                entity.Property(x => x.Nettotal).HasPrecision(18, 2);
            });

            modelBuilder.Entity<GoodsReturnFromCustomerDetail>(entity =>
            {
                entity.Property(x => x.Quantity).HasPrecision(18, 2);
                entity.Property(x => x.Unitprice).HasPrecision(18, 2);
                entity.Property(x => x.Amount).HasPrecision(18, 2);
            });

            modelBuilder.Entity<GoodsReturnToSupplier>(entity =>
            {
                entity.Property(x => x.Subtotal).HasPrecision(18, 2);
                entity.Property(x => x.Vat).HasPrecision(18, 2);
                entity.Property(x => x.Nettotal).HasPrecision(18, 2);
            });

            modelBuilder.Entity<GoodsReturnToSupplierDetail>(entity =>
            {
                entity.Property(x => x.Quantity).HasPrecision(18, 2);
                entity.Property(x => x.Unitprice).HasPrecision(18, 2);
                entity.Property(x => x.Amount).HasPrecision(18, 2);
            });
        }
    }
}
