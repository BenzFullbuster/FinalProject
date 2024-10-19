document.addEventListener("DOMContentLoaded", async function ()
{
    $("#ReceiveGoodsDate").datepicker({
        format: "dd/mm/yyyy",
        weekStart: 1,
        language: "th",
        daysOfWeekDisabled: "0",
        autoclose: true,
        todayHighlight: true,
    });

    const selectSupplier = document.getElementById("SupplierId");
    const selectPurchaseOrder = document.getElementById("PurchaseOrderId");

    await loadSupplier();

    selectPurchaseOrder.addEventListener("change", async function ()
    {
        await loadPODetail(selectPurchaseOrder.value);
    });

    selectSupplier.addEventListener("change", async function ()
    {
        resetPurchaseOrder();
        await loadPOCode(selectSupplier.value);
    });

    function resetPurchaseOrder()
    {
        selectPurchaseOrder.innerHTML = "<option value=''>เลือกใบสั่งซื้อ</option>";
        selectPurchaseOrder.value = "";
        selectPurchaseOrder.disabled = true;
    }

    async function loadPODetail(purchaseOrderId)
    {
        try
        {
            const poDetailsTable = document.getElementById("poDetailsTable");
            const response = await fetch(`/api/GetPODetail?purchaseOrderId=${purchaseOrderId}`);
            const data = await response.json();
            
            if (!data.length)
            {
                showAlert("ไม่มีข้อมูลรายละเอียดสินค้า ของใบสั่งซื้อนี้", "warning");
                return;
            }

            poDetailsTable.innerHTML = "";

            data.forEach((item, index) =>
            {
                const row = document.createElement("tr");

                row.innerHTML = `
                <td class="text-center">
                <input type="checkbox" class="form-check-input po-item" data-index="${item.index}" 
                data-productId="${item.productId}" data-productName="${item.productName}" 
                data-quantity="${item.quantity}" data-unitprice="${item.unitprice}" data-amount="${item.amount}" />
                </td>
                <td>${item.productName}</td >
                <td class="text-end">${formatNumber(item.quantity)}</td >
                <td class="text-end">${formatNumber(item.unitprice)}</td >
                <td class="text-end">${formatNumber(item.amount)}</td >`;

                poDetailsTable.appendChild(row);
            });
            console.info(data);
            const poDetailsModal = new bootstrap.Modal(document.getElementById("poDetailsModal"));
            poDetailsModal.show();
        }
        catch (error)
        {
            console.error("Error fetching purchaseOrder:", error);
            showAlert("ดึงข้อมูลรายละเอียดสินค้าในใบสั่งซื้อไม่สำเร็จ กรุณาลองอีกครั้งหรือติดต่อผู้ดูแลระบบ", "danger");
        }
    }

    async function loadPOCode(supplierId)
    {
        try
        {
            const response = await fetch(`/api/GetPOCode?supplierId=${supplierId}`);
            const data = await response.json();

            if (!data.length)
            {
                showAlert("ไม่มีข้อมูลใบสั่งซื้อ ของผู้จำหน่ายนี้", "warning");
                return;
            }

            if (!selectPurchaseOrder.value)
            {
                selectPurchaseOrder.innerHTML = "<option value=''>เลือกใบสั่งซื้อ</option>";
            }

            data.forEach(purchaseOrder =>
            {
                if (purchaseOrder.purchaseOrderId !== selectPurchaseOrder.value)
                {
                    const option = document.createElement("option");
                    option.value = purchaseOrder.purchaseOrderId;
                    option.textContent = purchaseOrder.purchaseOrderCode;

                    if (purchaseOrder.purchaseOrderId === selectPurchaseOrder.value)
                    {
                        option.selected = true;
                    }

                    selectPurchaseOrder.appendChild(option);
                }
            });

            selectPurchaseOrder.disabled = false;
        }
        catch (error)
        {
            console.error("Error fetching purchaseOrder:", error);
            showAlert("ดึงข้อมูลใบสั่งซื้อไม่สำเร็จ กรุณาลองอีกครั้งหรือติดต่อผู้ดูแลระบบ", "danger");
        }
    }

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

        const amountInputs = document.querySelectorAll(`input[id^="ReceiveGoodsDetails_"][id$="__Amount"]`);

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
                const podId = cells[0].querySelector("input[name*='ReceiveGoodsDetailId']").value;
                const poId = cells[0].querySelector("input[name*='ReceiveGoodsId']").value;
                const productId = cells[1].querySelector("input").value;
                const quantity = cells[2].querySelector("input").value;
                const unitprice = cells[3].querySelector("input").value;
                const amount = cells[4].querySelector("input").value;

                cells[0].innerHTML = `${index + 1}
                ${createHiddenInput(`ReceiveGoodsDetails_${index}__ReceiveGoodsDetailId`, `ReceiveGoodsDetails[${index}].ReceiveGoodsDetailId`, podId)}
                ${createHiddenInput(`ReceiveGoodsDetails_${index}__ReceiveGoodsId`, `ReceiveGoodsDetails[${index}].ReceiveGoodsId`, poId)}`;
                cells[0].classList.add("text-center");

                cells[1].innerHTML = `${cells[1].textContent}
                ${createHiddenInput(`ReceiveGoodsDetails_${index}__ProductId`, `ReceiveGoodsDetails[${index}].ProductId`, productId)}`;
                cells[1].classList.add("text-start");

                cells[2].innerHTML = `${cells[2].textContent}
                ${createHiddenInput(`ReceiveGoodsDetails_${index}__Quantity`, `ReceiveGoodsDetails[${index}].Quantity`, quantity)}`;
                cells[2].classList.add("text-end");

                cells[3].innerHTML = `${cells[3].textContent}
                ${createHiddenInput(`ReceiveGoodsDetails_${index}__Unitprice`, `ReceiveGoodsDetails[${index}].Unitprice`, unitprice)}`;
                cells[3].classList.add("text-end");

                cells[4].innerHTML = `${cells[4].textContent}
                ${createHiddenInput(`ReceiveGoodsDetails_${index}__Amount`, `ReceiveGoodsDetails[${index}].Amount`, amount)}`;
                cells[4].classList.add("text-end");
            }
        });
    }

    const selectAllItemsButton = document.getElementById('selectAllItems');
    const addSelectedItemsButton = document.getElementById('addSelectedItems');

    selectAllItemsButton.addEventListener("click", function ()
    {
        document.querySelectorAll(".po-item").forEach(item => item.checked = true);
    });

    addSelectedItemsButton.addEventListener("click", function ()
    {
        addSelectedItems();
    });

    window.addSelectedItems = function ()
    {
        const selectedItems = document.querySelectorAll('.po-item:checked');
        const podTable = document.getElementById("PODTable");
        let index = podTable.rows.length;

        selectedItems.forEach(item =>
        {
            let productId = item.dataset.productid;
            let productName = item.dataset.productname;
            let quantity = Number(item.dataset.quantity.replace(/,/g, ""));
            let unitprice = Number(item.dataset.unitprice.replace(/,/g, ""));
            let amount = quantity * unitprice;

            const row = podTable.insertRow();

            let cell1 = row.insertCell(0);
            let cell2 = row.insertCell(1);
            let cell3 = row.insertCell(2);
            let cell4 = row.insertCell(3);
            let cell5 = row.insertCell(4);
            let cell6 = row.insertCell(5);

            let value = "00000000-0000-0000-0000-000000000000";

            cell1.innerHTML = `${index + 1}
                ${createHiddenInput(`ReceiveGoodsDetails_${index}__ReceiveGoodsDetailId`, `ReceiveGoodsDetails[${index}].ReceiveGoodsDetailId`, value)}
                ${createHiddenInput(`ReceiveGoodsDetails_${index}__ReceiveGoodsId`, `ReceiveGoodsDetails[${index}].ReceiveGoodsId`, value)}`;
            cell1.classList.add("text-center");

            cell2.innerHTML = `${productName}
                ${createHiddenInput(`ReceiveGoodsDetails_${index}__ProductId`, `ReceiveGoodsDetails[${index}].ProductId`, productId)}`;
            cell2.classList.add("text-start");

            cell3.innerHTML = `${formatNumber(quantity)}
                ${createHiddenInput(`ReceiveGoodsDetails_${index}__Quantity`, `ReceiveGoodsDetails[${index}].Quantity`, quantity.toFixed(2))}`;
            cell3.classList.add("text-end");

            cell4.innerHTML = `${formatNumber(unitprice)}
                ${createHiddenInput(`ReceiveGoodsDetails_${index}__Unitprice`, `ReceiveGoodsDetails[${index}].Unitprice`, unitprice.toFixed(2))}`;
            cell4.classList.add("text-end");

            cell5.innerHTML = `${formatNumber(amount)}
                ${createHiddenInput(`ReceiveGoodsDetails_${index}__Amount`, `ReceiveGoodsDetails[${index}].Amount`, amount.toFixed(2))}`;
            cell5.classList.add("text-end");

            cell6.innerHTML = `<button type="button" onclick="removeRow(this)" class="btn btn-sm btn-danger rounded-5"><i class="bi bi-trash3"></i> ลบ</button>`;
            cell6.classList.add("text-center");

            index++
        });

        calculateNettotal();
    }

    /*document.getElementById("poDetailsModal").addEventListener("shown.bs.modal", async function ()
    {
        const selectAllItemsButton = document.getElementById('selectAllItems');
        const addSelectedItemsButton = document.getElementById('addSelectedItems');

        selectAllItemsButton.addEventListener("click", function ()
        {
            document.querySelectorAll(".po-item").forEach(item => item.checked = true);
        });

        addSelectedItemsButton.addEventListener("click", function ()
        {
            addSelectedItems();
        });

        window.addSelectedItems = function ()
        {
            const selectedItems = document.querySelectorAll('.po-item:checked');
            const podTable = document.getElementById("PODTable");
            let index = podTable.rows.length;

            selectedItems.forEach(item =>
            {
                let productId = item.dataset.productid;
                let productName = item.dataset.productname;
                let quantity = Number(item.dataset.quantity.replace(/,/g, ""));
                let unitprice = Number(item.dataset.unitprice.replace(/,/g, ""));
                let amount = quantity * unitprice;

                const row = podTable.insertRow();

                let cell1 = row.insertCell(0);
                let cell2 = row.insertCell(1);
                let cell3 = row.insertCell(2);
                let cell4 = row.insertCell(3);
                let cell5 = row.insertCell(4);
                let cell6 = row.insertCell(5);

                let value = "00000000-0000-0000-0000-000000000000";

                cell1.innerHTML = `${index + 1}
                ${createHiddenInput(`ReceiveGoodsDetails_${index}__ReceiveGoodsDetailId`, `ReceiveGoodsDetails[${index}].ReceiveGoodsDetailId`, value)}
                ${createHiddenInput(`ReceiveGoodsDetails_${index}__ReceiveGoodsId`, `ReceiveGoodsDetails[${index}].ReceiveGoodsId`, value)}`;
                cell1.classList.add("text-center");

                cell2.innerHTML = `${productName}
                ${createHiddenInput(`ReceiveGoodsDetails_${index}__ProductId`, `ReceiveGoodsDetails[${index}].ProductId`, productId)}`;
                cell2.classList.add("text-start");

                cell3.innerHTML = `${formatNumber(quantity)}
                ${createHiddenInput(`ReceiveGoodsDetails_${index}__Quantity`, `ReceiveGoodsDetails[${index}].Quantity`, quantity.toFixed(2))}`;
                cell3.classList.add("text-end");

                cell4.innerHTML = `${formatNumber(unitprice)}
                ${createHiddenInput(`ReceiveGoodsDetails_${index}__Unitprice`, `ReceiveGoodsDetails[${index}].Unitprice`, unitprice.toFixed(2))}`;
                cell4.classList.add("text-end");

                cell5.innerHTML = `${formatNumber(amount)}
                ${createHiddenInput(`ReceiveGoodsDetails_${index}__Amount`, `ReceiveGoodsDetails[${index}].Amount`, amount.toFixed(2))}`;
                cell5.classList.add("text-end");

                cell6.innerHTML = `<button type="button" onclick="removeRow(this)" class="btn btn-sm btn-danger rounded-5"><i class="bi bi-trash3"></i> ลบ</button>`;
                cell6.classList.add("text-center");

                index++
            });

            calculateNettotal();
        }
    });*/

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
            ${createHiddenInput(`ReceiveGoodsDetails_${index}__ReceiveGoodsDetailId`, `ReceiveGoodsDetails[${index}].ReceiveGoodsDetailId`, value)}
            ${createHiddenInput(`ReceiveGoodsDetails_${index}__ReceiveGoodsId`, `ReceiveGoodsDetails[${index}].ReceiveGoodsId`, value)}`;
            cell1.classList.add("text-center");

            cell2.innerHTML = `${productName}
            ${createHiddenInput(`ReceiveGoodsDetails_${index}__ProductId`, `ReceiveGoodsDetails[${index}].ProductId`, selectProduct.value)}`;
            cell2.classList.add("text-start");

            cell3.innerHTML = `${formatNumber(quantity)}
            ${createHiddenInput(`ReceiveGoodsDetails_${index}__Quantity`, `ReceiveGoodsDetails[${index}].Quantity`, quantity.toFixed(2))}`;
            cell3.classList.add("text-end");

            cell4.innerHTML = `${formatNumber(unitprice)}
            ${createHiddenInput(`ReceiveGoodsDetails_${index}__Unitprice`, `ReceiveGoodsDetails[${index}].Unitprice`, unitprice.toFixed(2))}`;
            cell4.classList.add("text-end");

            cell5.innerHTML = `${formatNumber(amount)}
            ${createHiddenInput(`ReceiveGoodsDetails_${index}__Amount`, `ReceiveGoodsDetails[${index}].Amount`, amount.toFixed(2))}`;
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