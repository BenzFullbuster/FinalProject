//ฟังก์ชั่น topbtn กลับไปด้านบน
let topbtn = document.getElementById("topbtn");
window.onscroll = function ()
{
    scrollFunction();
};
function scrollFunction()
{
    if (document.body.scrollTop > 20 || document.documentElement.scrollTop > 20)
    {
        topbtn.style.display = "block";
    } else
    {
        topbtn.style.display = "none";
    }
}
function topFunction()
{
    document.body.scrollTop = 0;
    document.documentElement.scrollTop = 0;
}
