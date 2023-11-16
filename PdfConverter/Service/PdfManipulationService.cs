using System.IO.Compression;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.Kernel.Pdf.Xobject;
using iText.Layout;
using iText.Layout.Element;

namespace PdfConverter.Service;
/// <summary>
/// Service for manipulating PDF files.
/// </summary>
public class PdfManipulationService
{
    /// <summary>
    /// Merges multiple PDFs into a single PDF.
    /// </summary>
    /// <param name="pdfs">List of PDF byte arrays to be merged.</param>
    /// <returns>Merged PDF byte array.</returns>
    public byte[] MergePdfs(List<byte[]> pdfs)
    {
        using (MemoryStream mergedPdfStream = new MemoryStream())
        {
            using (PdfWriter writer = new PdfWriter(mergedPdfStream))
            {
                using (PdfDocument mergedPdf = new PdfDocument(writer))
                {
                    using (Document document = new Document(mergedPdf))
                    {
                        foreach (var pdfBytes in pdfs)
                        {
                            using (MemoryStream pdfStream = new MemoryStream(pdfBytes))
                            {
                                using (PdfDocument sourcePdf = new PdfDocument(new PdfReader(pdfStream)))
                                {
                                    // Iterate through all pages in the source PDF
                                    for (int pageNum = 1; pageNum <= sourcePdf.GetNumberOfPages(); pageNum++)
                                    {
                                        // Import the page from the source PDF
                                        PdfPage page = sourcePdf.GetPage(pageNum);
                                        PdfFormXObject formXObject = page.CopyAsFormXObject(mergedPdf);

                                        // Add the imported page to the new document
                                        document.Add(new Image(formXObject));
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return mergedPdfStream.ToArray();
        }
    }
    /// <summary>
    /// Splits a PDF into two parts at the specified page.
    /// </summary>
    /// <param name="pdfBytes">Input PDF byte array.</param>
    /// <param name="splitAfterPage">Page number to split after.</param>
    /// <returns>List containing two split PDF byte arrays.</returns>
    public List<byte[]> SplitPdf(byte[] pdfBytes, int splitAfterPage)
    {

        using (MemoryStream pdfStream = new MemoryStream(pdfBytes))
        {
            using (PdfDocument pdfDocument = new PdfDocument(new PdfReader(pdfStream)))
            {
                List<byte[]> splitPdfDocuments = new List<byte[]>();

                // Добавим первый документ с страницами до splitAfterPage
                using (MemoryStream firstSplitStream = new MemoryStream())
                {
                    using (PdfWriter writer = new PdfWriter(firstSplitStream))
                    {
                        using (PdfDocument firstSplitPdf = new PdfDocument(writer))
                        {
                            for (int pageNum = 1; pageNum <= splitAfterPage; pageNum++)
                            {
                                firstSplitPdf.AddPage(pdfDocument.GetPage(pageNum).CopyTo(firstSplitPdf));
                            }
                        }
                    }
                    splitPdfDocuments.Add(firstSplitStream.ToArray());
                }

                // Добавим второй документ с оставшимися страницами
                using (MemoryStream secondSplitStream = new MemoryStream())
                {
                    using (PdfWriter writer = new PdfWriter(secondSplitStream))
                    {
                        using (PdfDocument secondSplitPdf = new PdfDocument(writer))
                        {
                            for (int pageNum = splitAfterPage + 1; pageNum <= pdfDocument.GetNumberOfPages(); pageNum++)
                            {
                                secondSplitPdf.AddPage(pdfDocument.GetPage(pageNum).CopyTo(secondSplitPdf));
                            }
                        }
                    }
                    splitPdfDocuments.Add(secondSplitStream.ToArray());
                }

                return splitPdfDocuments;
            }
        }
    }
    /// <summary>
    /// Adds a watermark to each page of a PDF.
    /// </summary>
    /// <param name="pdfBytes">Input PDF byte array.</param>
    /// <param name="watermarkText">Text for the watermark.</param>
    /// <returns>PDF byte array with watermark added.</returns>
    public byte[] AddWatermark(byte[] pdfBytes, string watermarkText)
    {
        try
        {
            using (MemoryStream inputStream = new MemoryStream(pdfBytes))
            {
                using (MemoryStream outputStream = new MemoryStream())
                {
                    using (PdfReader pdfReader = new PdfReader(inputStream))
                    {
                        using (PdfWriter pdfWriter = new PdfWriter(outputStream))
                        {
                            using (PdfDocument pdfDocument = new PdfDocument(pdfReader, pdfWriter))
                            {
                                // Iterate through all pages in the PDF
                                for (int i = 1; i <= pdfDocument.GetNumberOfPages(); i++)
                                {
                                    PdfPage page = pdfDocument.GetPage(i);

                                    // Create a Canvas object for drawing on the page
                                    Canvas canvas = new Canvas(new PdfCanvas(page), new iText.Kernel.Geom.Rectangle(page.GetPageSize()));

                                    // Add the watermark text
                                    Paragraph watermark = new Paragraph(watermarkText)
                                        .SetFontColor(iText.Kernel.Colors.ColorConstants.RED)
                                        .SetFontSize(20)
                                        .SetRotationAngle(Math.PI / 4);

                                    canvas.Add(watermark.SetFixedPosition(140, 100,500));
                                }
                            }
                        }
                    }

                    return outputStream.ToArray();
                }
            }
        }
        catch (Exception ex)
        {
            // Добавим обработку ошибок и выводим подробности об ошибке
            Console.WriteLine($"Error while trying add watermark: {ex.Message}");
            throw;
        }
    }
    /// <summary>
    /// Compresses a PDF using the specified compression level.
    /// </summary>
    /// <param name="pdfBytes">Input PDF byte array.</param>
    /// <param name="compressionLevel">Compression level (from 0-9) 9 high compression 0 without.</param>
    /// <returns>Compressed PDF byte array.</returns>
    public byte[] CompressPdf(byte[] pdfBytes, int compressionLevel)
    {
        using (MemoryStream inputStream = new MemoryStream(pdfBytes))
        {
            using (MemoryStream outputStream = new MemoryStream())
            {
                using (PdfReader pdfReader = new PdfReader(inputStream))
                {
                    using (PdfWriter pdfWriter = new PdfWriter(outputStream).SetCompressionLevel(compressionLevel))
                    {
                        using (PdfDocument pdfDocument = new PdfDocument(pdfReader, pdfWriter))
                        {
                            Document document = new Document(pdfDocument);

                            // Вы можете добавить здесь дополнительные операции по сжатию, если это необходимо

                            document.Close();
                        }
                    }
                }

                return outputStream.ToArray();
            }
        }
    }
    /// <summary>
    /// Extracts specific pages from a PDF.
    /// </summary>
    /// <param name="pdfBytes">Input PDF byte array.</param>
    /// <param name="startPage">Starting page number.</param>
    /// <param name="endPage">Ending page number.</param>
    /// <returns>Extracted PDF byte array.</returns>
    public byte[] ExtractPagesFromPdf(byte[] pdfBytes, int startPage, int endPage)
    {
        if (startPage < 1 || endPage < startPage)
        {
            throw new ArgumentException("Неверные параметры страниц.");
        }

        using (MemoryStream pdfStream = new MemoryStream(pdfBytes))
        {
            using (PdfDocument pdfDocument = new PdfDocument(new PdfReader(pdfStream)))
            {
                using (MemoryStream extractedPdfStream = new MemoryStream())
                {
                    using (PdfWriter writer = new PdfWriter(extractedPdfStream))
                    {
                        using (PdfDocument extractedPdf = new PdfDocument(writer))
                        {
                            for (int pageNum = startPage; pageNum <= Math.Min(endPage, pdfDocument.GetNumberOfPages()); pageNum++)
                            {
                                extractedPdf.AddPage(pdfDocument.GetPage(pageNum).CopyTo(extractedPdf));
                            }
                        }
                    }

                    return extractedPdfStream.ToArray();
                }
            }
        }
    }
}