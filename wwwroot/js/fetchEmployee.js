document.addEventListener("DOMContentLoaded", async function ()
{
    const selectEmployee = document.getElementById("EmployeeId");

    async function loadEmployee()
    {
        try
        {
            const response = await fetch(`/api/GetEmployee`);
            const data = await response.json();

            if (!data.length)
            {
                showAlert("ไม่มีข้อมูลพนักงาน", "warning");
                return;
            }

            if (!selectEmployee.value)
            {
                selectEmployee.innerHTML = "<option value=''>เลือกพนักงาน</option>";
            }

            data.forEach(employee =>
            {
                if (employee.employeeId !== selectEmployee.value)
                {
                    const option = document.createElement("option");
                    option.value = employee.employeeId;
                    option.textContent = employee.employeeName;

                    if (employee.employeeId === selectEmployee.value)
                    {
                        option.selected = true;
                    }

                    selectEmployee.appendChild(option);
                }
            });

            selectEmployee.disabled = false;
        }
        catch (error)
        {
            console.error("Error fetching employee:", error);
            showAlert("ดึงข้อมูลพนักงานไม่สำเร็จ กรุณาลองอีกครั้งหรือติดต่อผู้ดูแลระบบ", "danger");
        }
    }

    await loadEmployee();
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
