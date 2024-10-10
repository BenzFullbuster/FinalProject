document.addEventListener("DOMContentLoaded", async function ()
{
    $("#PurchaseOrderDate").datepicker({
        format: "dd/mm/yyyy",
        weekStart: 1,
        language: "th",
        daysOfWeekDisabled: "0",
        autoclose: true,
        todayHighlight: true,
    });

    
});

document.getElementById("addDetailModal").addEventListener("shown.bs.modal", async function ()
{
    // เลือก input ทั้งหมดที่มี class inputNumber
    const inputNumbers = document.querySelectorAll('.inputNumber');

    inputNumbers.forEach(function (inputNumber)
    {
        new Cleave(inputNumber, {
            numeral: true,
            numeralDecimalScale: 2,
            numeralThousandsGroupStyle: 'thousand',
            numeralPositiveOnly: true
        });
    });

    const selectCategory = document.getElementById("CategoryId");
    const selectProduct = document.getElementById("ProductId");
    const inputQuantity = document.getElementById("Quantity");
    const inputUnitprice = document.getElementById("Unitprice");
    const selectVatTypeId = document.getElementById("VatTypeId");

    function formatNumber(number)
    {
        return Number(number).toLocaleString('en',
            {
                minimumFractionDigits: 2,
                maximumFractionDigits: 2
            });
    }

    function resetModal()
    {
        selectCategory.value = "";
        selectCategory.innerHTML = "<option value=''>เลือกหมวดสินค้า</option>";
        selectProduct.value = "";
        selectProduct.innerHTML = "<option value=''>เลือกสินค้า</option>";
        inputQuantity.value = "";
        inputUnitprice.value = "";
    }

    window.addRow = function ()
    {
        const podTable = document.getElementById("PODTable");
        const index = podTable.rows.length;
        const row = podTable.insertRow();

        let productName = selectProduct.options[selectProduct.selectedIndex].text;
        let quantity = Number(inputQuantity.value.replace(/,/g, ""));
        let unitprice = Number(inputUnitprice.value.replace(/,/g, ""));
        let amount = quantity * unitprice;

        let cell1 = row.insertCell(0);
        let cell2 = row.insertCell(1);
        let cell3 = row.insertCell(2);
        let cell4 = row.insertCell(3);
        let cell5 = row.insertCell(4);
        let cell6 = row.insertCell(5);

        cell1.innerHTML = `${index + 1}`;
        cell1.classList.add("text-center");

        cell2.innerHTML = `${productName}
        <input type="hidden" id="PurchaseOrderDetails_${index}__ProductId" 
        name="PurchaseOrderDetails[${index}].ProductId" value="${selectProduct.value}" />`;
        cell2.classList.add("text-start");

        cell3.innerHTML = `${formatNumber(quantity)}
        <input type="hidden" id="PurchaseOrderDetails_${index}__Quantity" 
        name="PurchaseOrderDetails[${index}].Quantity" value="${quantity.toFixed(2)}" />`;
        cell3.classList.add("text-end");

        cell4.innerHTML = `${formatNumber(unitprice)}
        <input type="hidden" id="PurchaseOrderDetails_${index}__Unitprice" 
        name="PurchaseOrderDetails[${index}].Unitprice" value="${unitprice.toFixed(2)}" />`;
        cell4.classList.add("text-end");

        cell5.innerHTML = `${formatNumber(amount)}
        <input type="hidden" id="PurchaseOrderDetails_${index}__Amount" 
        name="PurchaseOrderDetails[${index}].Amount" value="${amount.toFixed(2)}" />`;
        cell5.classList.add("text-end");

        cell6.innerHTML = `<button type="button" onclick="removeRow(this)" class="btn btn-sm btn-danger rounded-5"><i class="bi bi-trash3"></i> ลบ</button>`;
        cell6.classList.add("text-center");

        resetModal();
        $("#addDetailModal").modal("hide");
        calculateNettotal();
    }

    window.removeRow = function (button)
    {
        const row = button.closest("tr");
        row.remove();
        updateRowIndex();
        calculateNettotal();
    }

    selectVatTypeId.addEventListener("change", async function ()
    {
        calculateNettotal();
    });

    function calculateNettotal()
    {
        let subtotal = 0;
        const tax = 0.07;
        let vat = 0;
        let nettotal = 0;

        const amountInputs = document.querySelectorAll(`input[id^="PurchaseOrderDetails_"][id$="__Amount"]`);

        amountInputs.forEach(input =>
        {
            subtotal += Number(input.value) || 0;
        });

        switch (selectVatTypeId.value)
        {
            case "2":
                vat = subtotal * tax / (1 + tax);
                nettotal = subtotal;
                subtotal = nettotal - vat;
                break;
            case "3":
                vat = subtotal * tax;
                nettotal = subtotal + vat;
                break;
            default:
                vat = 0;
                nettotal = subtotal;
                break;
        }

        //let vat = Number((subtotal * tax));
        //let nettotal = Number((subtotal + vat));

        document.getElementById("Subtotal").value = subtotal.toFixed(2);
        document.getElementById("displaySubtotal").textContent = formatNumber(subtotal);

        document.getElementById("Vat").value = vat.toFixed(2);
        document.getElementById("displayVat").textContent = formatNumber(vat);

        document.getElementById("Nettotal").value = nettotal.toFixed(2);
        document.getElementById("displayNettotal").textContent = formatNumber(nettotal);
    }

    function updateRowIndex()
    {
        const rows = document.querySelectorAll("#PODTable tr");

        rows.forEach((row, index) =>
        {
            // อัปเดตลำดับในเซลล์แรก (index)
            const indexCell = row.querySelector("td.text-center");
            if (indexCell)
            {
                indexCell.textContent = index + 1; // อัปเดตลำดับใหม่ โดยเริ่มที่ 1
            }

            // อัปเดต productId, quantity, unitprice, และ amount สำหรับ hidden inputs
            const productIdInput = row.querySelector(`input[id^="PurchaseOrderDetails_"][id$="__ProductId"]`);
            if (productIdInput)
            {
                productIdInput.id = `PurchaseOrderDetails_${index}__ProductId`;
                productIdInput.name = `PurchaseOrderDetails[${index}].ProductId`;
            }

            const quantityInput = row.querySelector(`input[id^="PurchaseOrderDetails_"][id$="__Quantity"]`);
            if (quantityInput)
            {
                quantityInput.id = `PurchaseOrderDetails_${index}__Quantity`;
                quantityInput.name = `PurchaseOrderDetails[${index}].Quantity`;
            }

            const unitpriceInput = row.querySelector(`input[id^="PurchaseOrderDetails_"][id$="__Unitprice"]`);
            if (unitpriceInput)
            {
                unitpriceInput.id = `PurchaseOrderDetails_${index}__Unitprice`;
                unitpriceInput.name = `PurchaseOrderDetails[${index}].Unitprice`;
            }

            const amountInput = row.querySelector(`input[id^="PurchaseOrderDetails_"][id$="__Amount"]`);
            if (amountInput)
            {
                amountInput.id = `PurchaseOrderDetails_${index}__Amount`;
                amountInput.name = `PurchaseOrderDetails[${index}].Amount`;
            }
        });
    }

    await loadCategory();
    await loadProduct();

    selectProduct.addEventListener("change", async function ()
    {
        inputQuantity.value = "";
        inputUnitprice.value = "";
    });

    selectCategory.addEventListener("change", async function ()
    {
        selectProduct.innerHTML = "<option value=''>เลือกสินค้า</option>";
        await loadProduct(selectCategory.value);
    });

    async function loadProduct(categoryId)
    {
        const url = selectCategory.value ? `/api/GetProduct?categoryId=${categoryId}` : `/api/GetProduct`;
        const response = await fetch(url);
        const data = await response.json();
        console.info(data);

        if (!data.length)
        {
            showAlert("ไม่มีข้อมูลสินค้า ของหมวดสินค้านี้", "warning");
            return;
        }

        if (!selectProduct.value)
        {
            selectProduct.innerHTML = "<option value=''>เลือกสินค้า</option>";
        }

        data.forEach(product =>
        {
            if (product.productId !== selectProduct.value)
            {
                const option = document.createElement("option");
                option.value = product.productId;
                option.textContent = product.productName;

                if (product.productId === selectProduct.value)
                {
                    option.selected = true;
                }

                selectProduct.appendChild(option);
            }
        });

        selectProduct.disabled = false;
    }

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
});

function showAlert(message, type = "")
{
    const alertContainer = document.getElementById("alertModalContainer");
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