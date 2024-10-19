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

    window.removeRow = function (button)
    {
        const row = button.closest("tr");
        row.remove();
        updateRowIndex();
        calculateNettotal();
    }

    const selectVatTypeId = document.getElementById("VatTypeId");
    selectVatTypeId.addEventListener("change", calculateNettotal);

    function formatNumber(number)
    {
        return Number(number).toLocaleString('en',
            {
                minimumFractionDigits: 2,
                maximumFractionDigits: 2
            });
    }

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
                subtotal = subtotal;
                vat = subtotal * tax;
                nettotal = subtotal + vat;
                break;
            default:
                subtotal = subtotal;
                vat = 0;
                nettotal = subtotal;
                break;
        }

        document.getElementById("Subtotal").value = subtotal.toFixed(2);
        document.getElementById("displaySubtotal").textContent = formatNumber(subtotal);

        document.getElementById("Vat").value = vat.toFixed(2);
        document.getElementById("displayVat").textContent = formatNumber(vat);

        document.getElementById("Nettotal").value = nettotal.toFixed(2);
        document.getElementById("displayNettotal").textContent = formatNumber(nettotal);
    }

    function createHiddenInput(id, name, value)
    {
        return `<input type="hidden" id="${id}" name="${name}" value="${value}" />`;
    }

    function updateRowIndex()
    {
        const rows = document.querySelectorAll("#PODTable tr");

        rows.forEach((row, index) =>
        {
            const cells = row.querySelectorAll("td");

            if (cells.length > 0)
            {
                const podId = cells[0].querySelector("input[name*='PurchaseOrderDetailId']").value;
                const poId = cells[0].querySelector("input[name*='PurchaseOrderId']").value;
                const productId = cells[1].querySelector("input").value;
                const quantity = cells[2].querySelector("input").value;
                const unitprice = cells[3].querySelector("input").value;
                const amount = cells[4].querySelector("input").value;

                cells[0].innerHTML = `${index + 1}
                ${createHiddenInput(`PurchaseOrderDetails_${index}__PurchaseOrderDetailId`, `PurchaseOrderDetails[${index}].PurchaseOrderDetailId`, podId)}
                ${createHiddenInput(`PurchaseOrderDetails_${index}__PurchaseOrderId`, `PurchaseOrderDetails[${index}].PurchaseOrderId`, poId)}`;
                cells[0].classList.add("text-center");

                cells[1].innerHTML = `${cells[1].textContent}
                ${createHiddenInput(`PurchaseOrderDetails_${index}__ProductId`, `PurchaseOrderDetails[${index}].ProductId`, productId)}`;
                cells[1].classList.add("text-start");

                cells[2].innerHTML = `${cells[2].textContent}
                ${createHiddenInput(`PurchaseOrderDetails_${index}__Quantity`, `PurchaseOrderDetails[${index}].Quantity`, quantity)}`;
                cells[2].classList.add("text-end");

                cells[3].innerHTML = `${cells[3].textContent}
                ${createHiddenInput(`PurchaseOrderDetails_${index}__Unitprice`, `PurchaseOrderDetails[${index}].Unitprice`, unitprice)}`;
                cells[3].classList.add("text-end");

                cells[4].innerHTML = `${cells[4].textContent}
                ${createHiddenInput(`PurchaseOrderDetails_${index}__Amount`, `PurchaseOrderDetails[${index}].Amount`, amount)}`;
                cells[4].classList.add("text-end");
            }
        });
    }

    document.getElementById("addDetailModal").addEventListener("shown.bs.modal", async function ()
    {
        const selectCategory = document.getElementById("CategoryId");
        const selectProduct = document.getElementById("ProductId");
        const inputQuantity = document.getElementById("Quantity");
        const inputUnitprice = document.getElementById("Unitprice");

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

            let value = "00000000-0000-0000-0000-000000000000";

            cell1.innerHTML = `${index + 1}
            ${createHiddenInput(`PurchaseOrderDetails_${index}__PurchaseOrderDetailId`, `PurchaseOrderDetails[${index}].PurchaseOrderDetailId`, value)}
            ${createHiddenInput(`PurchaseOrderDetails_${index}__PurchaseOrderId`, `PurchaseOrderDetails[${index}].PurchaseOrderId`, value)}`;
            cell1.classList.add("text-center");

            cell2.innerHTML = `${productName}
            ${createHiddenInput(`PurchaseOrderDetails_${index}__ProductId`, `PurchaseOrderDetails[${index}].ProductId`, selectProduct.value)}`;
            cell2.classList.add("text-start");

            cell3.innerHTML = `${formatNumber(quantity)}
            ${createHiddenInput(`PurchaseOrderDetails_${index}__Quantity`, `PurchaseOrderDetails[${index}].Quantity`, quantity.toFixed(2))}`;
            cell3.classList.add("text-end");

            cell4.innerHTML = `${formatNumber(unitprice)}
            ${createHiddenInput(`PurchaseOrderDetails_${index}__Unitprice`, `PurchaseOrderDetails[${index}].Unitprice`, unitprice.toFixed(2))}`;
            cell4.classList.add("text-end");

            cell5.innerHTML = `${formatNumber(amount)}
            ${createHiddenInput(`PurchaseOrderDetails_${index}__Amount`, `PurchaseOrderDetails[${index}].Amount`, amount.toFixed(2))}`;
            cell5.classList.add("text-end");

            cell6.innerHTML = `<button type="button" onclick="removeRow(this)" class="btn btn-sm btn-danger rounded-5"><i class="bi bi-trash3"></i> ลบ</button>`;
            cell6.classList.add("text-center");

            resetModal();
            $("#addDetailModal").modal("hide");
            calculateNettotal();
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

        async function loadProduct(categoryId)
        {
            try
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
            catch (error)
            {
                console.error("Error fetching product:", error);
                showAlert("ดึงข้อมูลสินค้าไม่สำเร็จ กรุณาลองอีกครั้งหรือติดต่อผู้ดูแลระบบ", "danger");
            }
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