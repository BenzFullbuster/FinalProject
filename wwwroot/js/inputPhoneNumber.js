document.addEventListener("DOMContentLoaded", function ()
{
    const inputPhone = document.getElementById("PhoneNumber");
    const phoneNumber = inputPhone.value.replace(/\D/g, "");
    const newBlocks = phoneNumber.length === 9 ? [1, 4, 4] : [2, 4, 4];

    let cleave = new Cleave(".inputPhone",
    {
        numericOnly: true,
        delimiter: "-",
        blocks: newBlocks
    });

    inputPhone.addEventListener("input", function ()
    {
        const phoneNumber = inputPhone.value.replace(/\D/g, "");
        const newBlocks = phoneNumber.length === 9 ? [1, 4, 4] : [2, 4, 4];

        if (cleave.properties.blocks.join() !== newBlocks.join())
        {
            cleave.destroy();
            cleave = new Cleave(".inputPhone",
            {
                numericOnly: true,
                delimiter: "-",
                blocks: newBlocks
            });
        }
    });
});
