using System.ComponentModel.DataAnnotations;

namespace FileCloud.Contracts
{
    public class UploadFileRequest
    {
        [Required]
        public IFormFile File { get; set; }

        public string? Path { get; set; }
    }
}
