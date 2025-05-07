namespace IIA_01.Models.LogModel
{
    public class LogViewModel
    {
        public int Id { get; set; }

        public string MaVach { get; set; }

        public string BarcodeImageUrl { get; set; }

        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
    }

}
