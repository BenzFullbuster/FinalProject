document.addEventListener("DOMContentLoaded", async function ()
{
    const selectSupplier = document.getElementById("SupplierId");

    async function loadSupplier()
    {
        try
        {
            const response = await fetch(`/api/GetSupplier`);
            const data = await response.json();

            if (!data.length)
            {
                showAlert("ไม่มีข้อมูลผู้จำหน่าย", "warning");
                return;
            }

            if (!selectSupplier.value)
            {
                selectSupplier.innerHTML = "<option value=''>เลือกผู้จำหน่าย</option>";
            }

            data.forEach(supplier =>
            {
                if (supplier.supplierId !== selectSupplier.value)
                {
                    const option = document.createElement("option");
                    option.value = supplier.supplierId;
                    option.textContent = supplier.supplierName;

                    if (supplier.supplierId === selectSupplier.value)
                    {
                        option.selected = true;
                    }

                    selectSupplier.appendChild(option);
                }
            });

            selectSupplier.disabled = false;
        }
        catch (error)
        {
            console.error("Error fetching supplier:", error);
            showAlert("ดึงข้อมูลผู้จำหน่ายไม่สำเร็จ กรุณาลองอีกครั้งหรือติดต่อผู้ดูแลระบบ", "danger");
        }
    }

    await loadSupplier();
});

function showAlert(message, type = "")
{
    const alertContainer = document.getElementById("alertContainer");
    const alert = document.createElement("div");

    alert.id = `alert-${type}`;
    alert.className = `alert alert-${type} alert-dismissible fade show`;
    alert.role = "alert";
    alert.innerHTML = `
        <strong>ข้อผิดพลาด!!!</strong> ${message}
        <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
    `;
    alertContainer.appendChild(alert);
}
