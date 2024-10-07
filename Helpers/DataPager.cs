namespace FinalProject.Helpers
{
    public class DataPager
    {
        public DataPager() { }

        //totalrows = จำนวนแถวทั้งหมดในตาราง page = หน้าปัจจุบัน pagesize = จำนวนแถวในแต่ละหน้า
        public DataPager(int totalrows, int page, int pagesize = 10)
        {
            if (totalrows < 0)
                totalrows = 0;

            if (page < 1)
                page = 1;

            if (pagesize < 1)
                pagesize = 10;

            int totalpages = (int)Math.Ceiling((decimal)totalrows / pagesize); // หารจำนวนหน้าทั้งหมด เศษปัดขึ้น

            int currentpage = page > totalpages ? totalpages : page;

            // สร้างปุ่มลิงก์ 4 หน้า
            int startpage = currentpage - 2;
            int endpage = currentpage + 2;

            if (startpage <= 0)
            {
                endpage = endpage - (startpage - 1); //ถ้าหน้าปัจจุบันเป็นหน้าที่ 1 startpage = (-1) endpage = 3 [3 - [(-3) - 1]]
                startpage = 1;
            }

            if (endpage > totalpages)
            {
                endpage = totalpages;
            }

            StartPage = startpage;
            EndPage = endpage;
            TotalPages = totalpages;

            CurrentPage = currentpage;
            PageSize = pagesize;
            TotalRows = totalrows;
        }

        public int StartPage { get; set; }
        public int EndPage { get; set; }
        public int TotalPages { get; set; }

        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalRows { get; set; }
    }
}
