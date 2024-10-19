document.addEventListener("DOMContentLoaded", async function ()
{
    const selectCategory = document.getElementById("CategoryId");

    async function loadCategory()
    {
        try
        {
            const response = await fetch(`/api/GetCategory`);
            const data = await response.json();

            if (!data.length)
            {
                showAlert("ไม่มีข้อมูลหมวดสินค้า", "warning");
                return;
            }

            if (!selectCategory.value)
            {
                selectCategory.innerHTML = "<option value=''>เลือกหมวดสินค้า</option>";
            }

            data.forEach(category =>
            {
                if (category.categoryId !== selectCategory.value)
                {
                    const option = document.createElement("option");
                    option.value = category.categoryId;
                    option.textContent = category.categoryName;

                    if (category.categoryId === selectCategory.value)
                    {
                        option.selected = true;
                    }

                    selectCategory.appendChild(option);
                }
            });

            selectCategory.disabled = false;
        }
        catch (error)
        {
            console.error("Error fetching category:", error);
            showAlert("ดึงข้อมูลหมวดสินค้าไม่สำเร็จ กรุณาลองอีกครั้งหรือติดต่อผู้ดูแลระบบ", "danger");
        }
    }

    await loadCategory();
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
