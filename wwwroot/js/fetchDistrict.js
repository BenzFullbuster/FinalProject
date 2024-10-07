document.addEventListener("DOMContentLoaded", async function ()
{
    const selectDistrict = document.getElementById("DistrictId");

    async function loadDistrict()
    {
        try
        {
            const response = await fetch(`/api/GetDistrict`);
            const data = await response.json();

            if (!data.length)
            {
                showAlert("ไม่มีข้อมูลเขต/อำเภอ", "warning");
                return;
            }

            if (!Number(selectDistrict.value))
            {
                selectDistrict.innerHTML = "<option value=''>เลือกเขต/อำเภอ</option>";
            }

            data.forEach(district =>
            {
                if (district.districtId !== Number(selectDistrict.value))
                {
                    const option = document.createElement("option");
                    option.value = district.districtId;
                    option.textContent = district.districtName;

                    if (district.districtId === Number(selectDistrict.value))
                    {
                        option.selected = true;
                    }

                    selectDistrict.appendChild(option);
                }
            });

            selectDistrict.disabled = false;
        }
        catch (error)
        {
            console.error("Error fetching district:", error);
            showAlert("ดึงข้อมูลเขต/อำเภอไม่สำเร็จ กรุณาลองอีกครั้งหรือติดต่อผู้ดูแลระบบ", "danger");
        }
    }

    await loadDistrict();
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
