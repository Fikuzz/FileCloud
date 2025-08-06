using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System.Diagnostics;


namespace FileCloud.Application.Services
{
    public class PreviewService
    {
        private readonly string _previewPath = Path.Combine("wwwroot/uploads", "previews");
        private readonly ILogger<PreviewService> _logger;

        public PreviewService(ILogger<PreviewService> logger)
        {
            _logger = logger;
        }

        public async Task GeneratePreviewAsync(string filePath, Guid fileId)
        {
            try
            {
                var ext = Path.GetExtension(filePath).ToLower();
                var previewFilePath = Path.Combine(_previewPath, $"{fileId}.jpg");

                if (System.IO.File.Exists(previewFilePath))
                    return;

                Directory.CreateDirectory(_previewPath);

                if (ext == ".jpg" || ext == ".jpeg" || ext == ".png")
                {
                    using var image = SixLabors.ImageSharp.Image.Load(filePath);
                    image.Mutate(x => x.Resize(new ResizeOptions
                    {
                        Mode = ResizeMode.Max,
                        Size = new Size(128, 128)
                    }));

                    await using var fs = File.Create(previewFilePath);
                    await image.SaveAsJpegAsync(fs);
                }
                else if (ext == ".mp4" || ext == ".mov" || ext == ".mkv")
                {
                    var ffmpeg = "ffmpeg";
                    var args = $"-i \"{filePath}\" -ss 00:00:01.000 -vframes 1 \"{previewFilePath}\" -y";

                    var startInfo = new ProcessStartInfo
                    {
                        FileName = ffmpeg,
                        Arguments = args,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    };

                    using var process = Process.Start(startInfo);
                    await process.WaitForExitAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Preview generation failed for file: {filePath}");
            }
        }
        public void DeletePreview(Guid id)
        {
            var previewFilePath = Path.Combine(_previewPath, $"{id}.jpg");
            if (File.Exists(previewFilePath))
            {
                File.Delete(previewFilePath);
            }
            return;
        }
        public string GetPreviewPath(Guid id)
        {
            return Path.Combine(_previewPath, $"{id}.jpg");
        }
    }
}
