document.addEventListener("DOMContentLoaded", async function ()
{
    const selectUnit = document.getElementById("UnitId");

    async function loadUnit()
    {
        try
        {
            const response = await fetch(`/api/GetUnit`);
            const data = await response.json();

            if (!data.length)
            {
                showAlert("ไม่มีข้อมูลหน่วยนับ", "warning");
                return;
            }

            if (!selectUnit.value)
            {
                selectUnit.innerHTML = "<option value=''>เลือกหน่วยนับ</option>";
            }

            data.forEach(unit =>
            {
                if (unit.unitId !== selectUnit.value)
                {
                    const option = document.createElement("option");
                    option.value = unit.unitId;
                    option.textContent = unit.unitName;

                    if (unit.unitId === selectUnit.value)
                    {
                        option.selected = true;
                    }

                    selectUnit.appendChild(option);
                }
            });

            selectUnit.disabled = false;
        }
        catch (error)
        {
            console.error("Error fetching unit:", error);
            showAlert("ดึงข้อมูลหน่วยนับไม่สำเร็จ กรุณาลองอีกครั้งหรือติดต่อผู้ดูแลระบบ", "danger");
        }
    }

    await loadUnit();
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
