namespace PdfConverter.Models
{
    public class FileUploadModel
    {
        public IFormFile UploadedFile { get; set; }
        public bool ParseImages { get; set; }
    }
}
