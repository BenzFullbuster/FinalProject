document.addEventListener("DOMContentLoaded", async function ()
{
    const selectProvince = document.getElementById("ProvinceId");
    const selectDistrict = document.getElementById("DistrictId");
    const selectSubdistrict = document.getElementById("SubdistrictId");
    const inputZipcode = document.getElementById("Zipcode");

    await loadProvince();

    if (Number(selectProvince.value))
    {
        await loadDistrict(Number(selectProvince.value));
        if (Number(selectDistrict.value))
        {
            await loadSubdistrict(Number(selectDistrict.value))
            if (Number(selectSubdistrict.value))
            {
                await loadZipcode(Number(selectSubdistrict.value))
            }
            else
            {
                inputZipcode.value = "";
            }
        }
        else
        {
            selectSubdistrict.innerHTML = "<option value=''>เลือกแขวง/ตำบล</option>";
            selectSubdistrict.disabled = true;
            inputZipcode.value = "";
        }
    }
    else
    {
        selectDistrict.innerHTML = "<option value=''>เลือกเขต/อำเภอ</option>";
        selectDistrict.disabled = true;
        selectSubdistrict.innerHTML = "<option value=''>เลือกแขวง/ตำบล</option>";
        selectSubdistrict.disabled = true;
        inputZipcode.value = "";
    }

    selectProvince.addEventListener("change", async function ()
    {
        selectDistrict.innerHTML = "<option value=''>เลือกเขต/อำเภอ</option>";
        selectDistrict.disabled = false;
        selectSubdistrict.innerHTML = "<option value=''>เลือกแขวง/ตำบล</option>";
        selectSubdistrict.disabled = true;
        inputZipcode.value = "";

        await loadDistrict(Number(selectProvince.value));

    });

    selectDistrict.addEventListener("change", async function ()
    {
        selectSubdistrict.innerHTML = "<option value=''>เลือกแขวง/ตำบล</option>";
        selectSubdistrict.disabled = false;
        inputZipcode.value = "";
        await loadSubdistrict(Number(selectDistrict.value));
    });

    selectSubdistrict.addEventListener("change", async function ()
    {
        inputZipcode.value = "";
        await loadZipcode(Number(selectSubdistrict.value));
    });

    async function loadZipcode(subdistrictId)
    {
        try
        {
            const response = await fetch(`/api/GetZipcode?subdistrictId=${subdistrictId}`);
            const data = await response.json();

            if (data != null)
            {
                inputZipcode.value = data.zipcode;
            }
            else
            {
                showAlert("ไม่มีข้อมูลรหัสไปรษณีย์ ของแขวง/ตำบลนี้", "warning");
                return;
            }
            
        }
        catch (error)
        {
            console.error("Error fetching subdistrict:", error);
            showAlert("ดึงข้อมูลรหัสไปรษณีย์ไม่สำเร็จ กรุณาลองอีกครั้งหรือติดต่อผู้ดูแลระบบ", "danger");
        }
    }

    async function loadSubdistrict(districtId)
    {
        try
        {
            const response = await fetch(`/api/GetSubdistrict?districtId=${districtId}`);
            const data = await response.json();

            if (!data.length)
            {
                showAlert("ไม่มีข้อมูลแขวง/ตำบล ของเขต/อำเภอนี้", "warning");
                return;
            }

            if (!Number(selectSubdistrict.value))
            {
                selectSubdistrict.innerHTML = "<option value=''>เลือกแขวง/ตำบล</option>";
            }

            data.forEach(subdistrict =>
            {
                if (subdistrict.subdistrictId !== Number(selectSubdistrict.value))
                {
                    const option = document.createElement("option");
                    option.value = subdistrict.subdistrictId;
                    option.textContent = subdistrict.subdistrictName;

                    if (subdistrict.districtId === Number(selectSubdistrict.value))
                    {
                        option.selected = true;
                    }

                    selectSubdistrict.appendChild(option);
                }
            });
        }
        catch (error)
        {
            console.error("Error fetching subdistrict:", error);
            showAlert("ดึงข้อมูลแขวง/ตำบลไม่สำเร็จ กรุณาลองอีกครั้งหรือติดต่อผู้ดูแลระบบ", "danger");
        }
    }

    async function loadDistrict(provinceId)
    {
        try
        {
            const response = await fetch(`/api/GetDistrict?provinceId=${provinceId}`);
            const data = await response.json();

            if (!data.length)
            {
                showAlert("ไม่มีข้อมูลเขต/อำเภอ ของจังหวัดนี้", "warning");
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
        }
        catch (error)
        {
            console.error("Error fetching district:", error);
            showAlert("ดึงข้อมูลเขต/อำเภอไม่สำเร็จ กรุณาลองอีกครั้งหรือติดต่อผู้ดูแลระบบ", "danger");
        }
    }

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
        }
        catch (error)
        {
            console.error("Error fetching province:", error);
            showAlert("ดึงข้อมูลจังหวัดไม่สำเร็จ กรุณาลองอีกครั้งหรือติดต่อผู้ดูแลระบบ", "danger");
        }
    }
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
