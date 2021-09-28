namespace Skynet.Wiki.API.Models
{
    public class PdfRequest
    {
        public string[] TagNames { get; set; }
        public int[] TagIds { get; set; }
    }
}