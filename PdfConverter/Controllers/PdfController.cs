using Microsoft.AspNetCore.Mvc;
using PdfConverter.Service;
namespace PdfConverter.Controllers
{
    [ApiController]
    [Route("api/pdf")]
    public class PdfController : ControllerBase
    {
        private readonly IWebHostEnvironment _environment;
        private readonly PdfManipulationService _pdfManipulationService;

        public PdfController(PdfManipulationService pdfManipulationService, IWebHostEnvironment environment)
        {
            _pdfManipulationService = pdfManipulationService;
            _environment = environment;
        }

        [HttpPost("merge")]
        public IActionResult MergePdfs(List<IFormFile> pdfFiles)
        {
            try
            {
                List<byte[]> pdfs = new List<byte[]>();

                foreach (var pdfFile in pdfFiles)
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        pdfFile.CopyTo(ms);
                        pdfs.Add(ms.ToArray());
                    }
                }

                byte[] mergedPdf = _pdfManipulationService.MergePdfs(pdfs);

                return File(mergedPdf, "application/pdf", "merged.pdf");
            }
            catch (Exception ex)
            {
                return BadRequest($"Ошибка при объединении PDF: {ex.Message}");
            }
        }


        [HttpPost("split")]
        public IActionResult SplitPdf(IFormFile pdfFile, int splitAfterPage)
        {
            if (pdfFile == null || pdfFile.Length == 0)
            {
                return BadRequest("Пустой файл.");
            }

            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    pdfFile.CopyTo(ms);
                    byte[] pdfBytes = ms.ToArray();

                    List<byte[]> splitPdfDocuments = _pdfManipulationService.SplitPdf(pdfBytes, splitAfterPage);

                    if (splitPdfDocuments.Count != 2)
                    {
                        return BadRequest("Ошибка при разделении файла.");
                    }

                    string fileName1 = "FirstPart.pdf";
                    string fileName2 = "SecondPart.pdf";

                    // Сохранить первую часть во временный файл
                    string filePath1 = Path.Combine(Path.GetTempPath(), fileName1);
                    System.IO.File.WriteAllBytes(filePath1, splitPdfDocuments[0]);

                    // Сохранить вторую часть во временный файл
                    string filePath2 = Path.Combine(Path.GetTempPath(), fileName2);
                    System.IO.File.WriteAllBytes(filePath2, splitPdfDocuments[1]);

                    // Создать URL для скачивания
                    string downloadUrl1 =
                        Url.Action("DownloadFile", "Pdf", new { fileName = fileName1 }, Request.Scheme)!;
                    string downloadUrl2 =
                        Url.Action("DownloadFile", "Pdf", new { fileName = fileName2 }, Request.Scheme)!;

                    return Ok(new
                    {
                        FirstPartUrl = downloadUrl1,
                        SecondPartUrl = downloadUrl2
                    });
                }
            }
            catch (Exception ex)
            {
                return BadRequest($"Ошибка: {ex.Message}");
            }
        }
        [HttpPost("addWatermark")]
        public IActionResult AddWatermark(IFormFile pdfFile, string watermarkText)
        {
            if (pdfFile == null || pdfFile.Length == 0)
            {
                return BadRequest("Пустой файл.");
            }

            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    pdfFile.CopyTo(ms);
                    byte[] pdfBytes = ms.ToArray();

                    byte[] watermarkedPdf = _pdfManipulationService.AddWatermark(pdfBytes, watermarkText);

                    return File(watermarkedPdf, "application/pdf", "watermarked.pdf");
                }
            }
            catch (Exception ex)
            {
                return BadRequest($"Ошибка: {ex.Message}");
            }

            
        }

        [HttpGet("Download")]
        public IActionResult DownloadFile(string fileName)
        {
            var filePath = Path.Combine(Path.GetTempPath(), fileName);

            if (System.IO.File.Exists(filePath))
            {
                var fileBytes = System.IO.File.ReadAllBytes(filePath);

                // Удалите временный файл после отправки
                System.IO.File.Delete(filePath);

                return File(fileBytes, "application/pdf", fileName);
            }
            else
            {
                return NotFound();
            }
        }
        [HttpPost("compress")]
        public IActionResult CompressPdf(IFormFile pdfFile, int compressionLevel)
        {
            if (pdfFile == null || pdfFile.Length == 0)
            {
                return BadRequest("Пустой файл.");
            }

            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    pdfFile.CopyTo(ms);
                    byte[] pdfBytes = ms.ToArray();

                    byte[] compressedPdf = _pdfManipulationService.CompressPdf(pdfBytes, compressionLevel);

                    return File(compressedPdf, "application/pdf", "compressed.pdf");
                }
            }
            catch (Exception ex)
            {
                return BadRequest($"Ошибка: {ex.Message}");
            }
        }

        [HttpPost("extract")]
        public IActionResult ExtractPagesFromPdf(IFormFile pdfFile, int startPage, int endPage)
        {
            try
            {
                if (pdfFile == null || pdfFile.Length == 0)
                {
                    return BadRequest("Пустой файл.");
                }

                using (MemoryStream ms = new MemoryStream())
                {
                    pdfFile.CopyTo(ms);
                    byte[] pdfBytes = ms.ToArray();

                    byte[] extractedPdf = _pdfManipulationService.ExtractPagesFromPdf(pdfBytes, startPage, endPage);

                    return File(extractedPdf, "application/pdf", "extracted.pdf");
                }
            }
            catch (Exception ex)
            {
                return BadRequest($"Ошибка при извлечении страниц из PDF: {ex.Message}");
            }
        }
    }
}
