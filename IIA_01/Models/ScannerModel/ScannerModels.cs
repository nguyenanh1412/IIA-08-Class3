namespace IIA_02_Server_scanner.Models.ScannerModel
{
    public class BarcodeDataModel
    {
        public int Id { get; set; }
        public string MaVach { get; set; }
        public string BarcodeImageUrl { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        // Không lưu hình ảnh, chỉ nhận upload
        public IFormFile BarcodeImageFile { get; set; }
    }

}
