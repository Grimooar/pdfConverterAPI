using Microsoft.AspNetCore.Mvc;
using PdfConverter.Service;
namespace PdfConverter.Controllers;

    /// <summary>
    /// Controller for handling PDF-related actions.
    /// </summary>
    [ApiController]
    [Route("api/pdf")]
    public class PdfController : ControllerBase
    {
        private readonly PdfManipulationService _pdfManipulationService;

        /// <summary>
        /// Initializes a new instance of the <see cref="PdfController"/> class.
        /// </summary>
        /// <param name="pdfManipulationService">Service for manipulating PDFs.</param>
        public PdfController(PdfManipulationService pdfManipulationService)
        {
            _pdfManipulationService = pdfManipulationService;
        }
        /// <summary>
        /// Merges multiple PDFs into a single PDF.
        /// </summary>
        /// <param name="pdfFiles">List of PDF files to be merged.</param>
        /// <returns>Action result containing the merged PDF.</returns>
        [HttpPost("merge")]
        public IActionResult MergePdfs(List<IFormFile> pdfFiles)
        {
            try
            {
                var pdfs = pdfFiles.Select(file => _pdfManipulationService.ConvertToByteArray(file)).ToList();
                byte[] mergedPdf = _pdfManipulationService.MergePdfs(pdfs);
                return File(mergedPdf, "application/pdf", "merged.pdf");
            }
            catch (Exception ex)
            {
                return BadRequest($"Ошибка при объединении PDF: {ex.Message}");
            }
        }


        /// <summary>
        /// Splits a PDF file at the specified page.
        /// </summary>
        /// <param name="pdfFile">Input PDF file.</param>
        /// <param name="splitAfterPage">Page number to split after.</param>
        /// <returns>Action result containing URLs for downloading split parts.</returns>
        [HttpPost("split")]
        public IActionResult SplitPdf(IFormFile pdfFile, int splitAfterPage)
        {
            if (pdfFile == null || pdfFile.Length == 0)
            {
                return BadRequest("Empty file.");
            }

            try
            {
                byte[] pdfBytes = _pdfManipulationService.ConvertToByteArray(pdfFile);
                var splitPdfDocuments = _pdfManipulationService.SplitPdf(pdfBytes, splitAfterPage);

             
                return Ok(splitPdfDocuments); // Пример
            }
            catch (Exception ex)
            {
                return BadRequest($"Error splitting PDF: {ex.Message}");
            }
        }

        /// <summary>
        /// Adds a watermark to a PDF file.
        /// </summary>
        /// <param name="pdfFile">Input PDF file.</param>
        /// <param name="watermarkText">Text for the watermark.</param>
        /// <returns>Action result containing the watermarked PDF.</returns>
        [HttpPost("addWatermark")]
        public IActionResult AddWatermark(IFormFile pdfFile, string watermarkText)
        {
            if (pdfFile == null || pdfFile.Length == 0)
            {
                return BadRequest("Empty file.");
            }

            try
            {
                byte[] pdfBytes = _pdfManipulationService.ConvertToByteArray(pdfFile);
                byte[] watermarkedPdf = _pdfManipulationService.AddWatermark(pdfBytes, watermarkText);
                return File(watermarkedPdf, "application/pdf", "watermarked.pdf");
            }
            catch (Exception ex)
            {
                return BadRequest($"Ошибка: {ex.Message}");
            }
        }

        /// <summary>
        /// Downloads a file with the specified file name.
        /// </summary>
        /// <param name="fileName">Name of the file to be downloaded.</param>
        /// <returns>Action result containing the file for download.</returns>
        [HttpGet("Download")]
        public IActionResult DownloadFile(string fileName)
        {
            var filePath = Path.Combine(Path.GetTempPath(), fileName);

            if (System.IO.File.Exists(filePath))
            {
                var fileBytes = System.IO.File.ReadAllBytes(filePath);
                
                System.IO.File.Delete(filePath);

                return File(fileBytes, "application/pdf", fileName);
            }
            else
            {
                return NotFound();
            }
        }
        /// <summary>
        /// Compresses a PDF file at the specified compression level.
        /// </summary>
        /// <param name="pdfFile">Input PDF file.</param>
        /// <param name="compressionLevel">Compression level (from 0-9) 9 high compression 0 without.</param>
        /// <returns>Action result containing the compressed PDF.</returns>
        [HttpPost("compress")]
        public IActionResult CompressPdf(IFormFile pdfFile, int compressionLevel)
        {
            if (pdfFile == null || pdfFile.Length == 0)
            {
                return BadRequest("Empty file.");
            }

            try
            {
                byte[] pdfBytes = _pdfManipulationService.ConvertToByteArray(pdfFile);
                byte[] compressedPdf = _pdfManipulationService.CompressPdf(pdfBytes, compressionLevel);

                return File(compressedPdf, "application/pdf", "compressed.pdf");
            }
            catch (Exception ex)
            {
                return BadRequest($"Ошибка: {ex.Message}");
            }
        }
        /// <summary>
        /// Extracts pages from a PDF file.
        /// </summary>
        /// <param name="pdfFile">Input PDF file.</param>
        /// <param name="startPage">Starting page number.</param>
        /// <param name="endPage">Ending page number.</param>
        /// <returns>Action result containing the extracted PDF.</returns>
        [HttpPost("extract")]
        public IActionResult ExtractPagesFromPdf(IFormFile pdfFile, int startPage, int endPage)
        {
            if (pdfFile == null || pdfFile.Length == 0)
            {
                return BadRequest("Empty file.");
            }

            try
            {
                byte[] pdfBytes = _pdfManipulationService.ConvertToByteArray(pdfFile);
                byte[] extractedPdf = _pdfManipulationService.ExtractPagesFromPdf(pdfBytes, startPage, endPage);

                return File(extractedPdf, "application/pdf", "extracted.pdf");
            }
            catch (Exception ex)
            {
                return BadRequest($"Ошибка при извлечении страниц из PDF: {ex.Message}");
            }
        }
    }
