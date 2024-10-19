document.addEventListener("DOMContentLoaded", async function ()
{
    const selectPosition = document.getElementById("PositionId");

    async function loadPosition()
    {
        try
        {
            const response = await fetch(`/api/GetPosition`);
            const data = await response.json();

            if (!data.length)
            {
                showAlert("ไม่มีข้อมูลตำแหน่ง", "warning");
                return;
            }

            if (!selectPosition.value)
            {
                selectPosition.innerHTML = "<option value=''>เลือกตำแหน่ง</option>";
            }

            data.forEach(position =>
            {
                if (position.positionId !== selectPosition.value)
                {
                    const option = document.createElement("option");
                    option.value = position.positionId;
                    option.textContent = position.positionName;

                    if (position.positionId === selectPosition.value)
                    {
                        option.selected = true;
                    }

                    selectPosition.appendChild(option);
                }
            });

            selectPosition.disabled = false;
        }
        catch (error)
        {
            console.error("Error fetching position:", error);
            showAlert("ดึงข้อมูลตำแหน่งไม่สำเร็จ กรุณาลองอีกครั้งหรือติดต่อผู้ดูแลระบบ", "danger");
        }
    }

    await loadPosition();
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
