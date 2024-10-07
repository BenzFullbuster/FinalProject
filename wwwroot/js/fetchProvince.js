document.addEventListener("DOMContentLoaded", async function ()
{
    const selectProvince = document.getElementById("ProvinceId");

    async function loadProvince()
    {
        try
        {
            const response = await fetch(`/api/GetProvince`);
            const data = await response.json();

            if (!data.length)
            {
                showAlert("ไม่มีข้อมูลจังหวัด", "warning");
                return;
            }

            if (!Number(selectProvince.value))
            {
                selectProvince.innerHTML = "<option value=''>เลือกจังหวัด</option>";
            }

            data.forEach(province =>
            {
                if (province.provinceId !== Number(selectProvince.value))
                {
                    const option = document.createElement("option");
                    option.value = province.provinceId;
                    option.textContent = province.provinceName;

                    if (province.provinceId === Number(selectProvince.value))
                    {
                        option.selected = true;
                    }

                    selectProvince.appendChild(option);
                }
            });

            selectProvince.disabled = false;
        }
        catch (error)
        {
            console.error("Error fetching province:", error);
            showAlert("ดึงข้อมูลจังหวัดไม่สำเร็จ กรุณาลองอีกครั้งหรือติดต่อผู้ดูแลระบบ", "danger");
        }
    }

    await loadProvince();
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
