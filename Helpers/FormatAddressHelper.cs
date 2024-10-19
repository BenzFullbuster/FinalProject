namespace FinalProject.Helpers
{
    public static class FormatAddressHelper
    {
        public static string GetFormattedAddress(string? address, string? provinceName, string? districtName, string? subdistrictName, string? zipcode, bool includeAddress)
        {
            string mainAddress = includeAddress && !string.IsNullOrEmpty(address) ? $"{address}" : string.Empty;

            provinceName ??= string.Empty;
            string districtPrefix = provinceName == "กรุงเทพมหานคร" ? "เขต" : "อำเภอ";
            string subdistrictPrefix = provinceName == "กรุงเทพมหานคร" ? "แขวง" : "ตำบล";

            provinceName = !string.IsNullOrEmpty(provinceName) && provinceName != "กรุงเทพมหานคร" ? $"จังหวัด{provinceName}" : provinceName;

            districtName = !string.IsNullOrEmpty(districtName) ? $"{districtPrefix}{districtName}" : string.Empty;
            subdistrictName = !string.IsNullOrEmpty(subdistrictName) ? $"{subdistrictPrefix}{subdistrictName}" : string.Empty;
            zipcode ??= string.Empty;

            string fullAddress = $"{mainAddress} {subdistrictName} {districtName} {provinceName} {zipcode}".Trim();
            
            return !string.IsNullOrEmpty(fullAddress) ? fullAddress : string.Empty;
        }
    }
}
