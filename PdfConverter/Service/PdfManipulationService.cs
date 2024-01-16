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
        MemoryStream mergedPdfStream = new MemoryStream();
        using (PdfWriter writer = new PdfWriter(mergedPdfStream))
        using (PdfDocument mergedPdf = new PdfDocument(writer))
        using (Document document = new Document(mergedPdf))
        {
            foreach (var pdfBytes in pdfs)
            {
                MergeSinglePdf(document, pdfBytes);
            }
        }

        return mergedPdfStream.ToArray();
    }
    private void MergeSinglePdf(Document document, byte[] pdfBytes)
    {
        MemoryStream pdfStream = new MemoryStream(pdfBytes);
        using (PdfDocument sourcePdf = new PdfDocument(new PdfReader(pdfStream)))
        {
            for (int pageNum = 1; pageNum <= sourcePdf.GetNumberOfPages(); pageNum++)
            {
                PdfPage page = sourcePdf.GetPage(pageNum);
                PdfFormXObject formXObject = page.CopyAsFormXObject(document.GetPdfDocument());
                document.Add(new Image(formXObject));
            }
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
        using (PdfDocument pdfDocument = new PdfDocument(new PdfReader(pdfStream)))
        {
            List<byte[]> splitPdfDocuments = new List<byte[]>();
            splitPdfDocuments.Add(CreateSplitPdf(pdfDocument, 1, splitAfterPage));
            splitPdfDocuments.Add(CreateSplitPdf(pdfDocument, splitAfterPage + 1, pdfDocument.GetNumberOfPages()));

            return splitPdfDocuments;
        }
    }
    private byte[] CreateSplitPdf(PdfDocument originalPdf, int startPage, int endPage)
    {
        MemoryStream splitStream = new MemoryStream();
        using (PdfWriter writer = new PdfWriter(splitStream))
        using (PdfDocument splitPdf = new PdfDocument(writer))
        {
            for (int pageNum = startPage; pageNum <= endPage; pageNum++)
            {
                splitPdf.AddPage(originalPdf.GetPage(pageNum).CopyTo(splitPdf));
            }
        }
        return splitStream.ToArray();
    }


 
    /// <summary>
    /// Adds a watermark to each page of a PDF.
    /// </summary>
    /// <param name="pdfBytes">Input PDF byte array.</param>
    /// <param name="watermarkText">Text for the watermark.</param>
    /// <returns>PDF byte array with watermark added.</returns>
    public byte[] AddWatermark(byte[] pdfBytes, string watermarkText)
    {
        MemoryStream inputStream = new MemoryStream(pdfBytes);
        MemoryStream outputStream = new MemoryStream();
        try
        {
            using (PdfReader pdfReader = new PdfReader(inputStream))
            using (PdfWriter pdfWriter = new PdfWriter(outputStream))
            using (PdfDocument pdfDocument = new PdfDocument(pdfReader, pdfWriter))
            {
                AddWatermarkToPdf(pdfDocument, watermarkText);
            }
            return outputStream.ToArray();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error while trying to add watermark: {ex.Message}");
            throw;
        }
        finally
        {
            inputStream.Close();
            outputStream.Close();
        }
    }

    private void AddWatermarkToPdf(PdfDocument pdfDocument, string watermarkText)
    {
        for (int i = 1; i <= pdfDocument.GetNumberOfPages(); i++)
        {
            PdfPage page = pdfDocument.GetPage(i);
            Canvas canvas = new Canvas(new PdfCanvas(page), page.GetPageSize());
            Paragraph watermark = new Paragraph(watermarkText)
                .SetFontColor(iText.Kernel.Colors.ColorConstants.RED)
                .SetFontSize(20)
                .SetRotationAngle(Math.PI / 4);

            canvas.Add(watermark.SetFixedPosition(140, 100, 500));
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
        MemoryStream inputStream = new MemoryStream(pdfBytes);
        MemoryStream outputStream = new MemoryStream();
        try
        {
            using (PdfReader pdfReader = new PdfReader(inputStream))
            using (PdfWriter pdfWriter = new PdfWriter(outputStream).SetCompressionLevel(compressionLevel))
            using (PdfDocument pdfDocument = new PdfDocument(pdfReader, pdfWriter))
            {
                // Дополнительные операции по сжатию могут быть добавлены здесь
            }
            return outputStream.ToArray();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error while trying to compress PDF: {ex.Message}");
            throw;
        }
        finally
        {
            inputStream.Close();
            outputStream.Close();
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
        ValidatePageRange(startPage, endPage);

        MemoryStream pdfStream = new MemoryStream(pdfBytes);
        MemoryStream extractedPdfStream = new MemoryStream();
        try
        {
            using (PdfReader pdfReader = new PdfReader(pdfStream))
            using (PdfDocument pdfDocument = new PdfDocument(pdfReader))
            using (PdfWriter writer = new PdfWriter(extractedPdfStream))
            using (PdfDocument extractedPdf = new PdfDocument(writer))
            {
                for (int pageNum = startPage; pageNum <= Math.Min(endPage, pdfDocument.GetNumberOfPages()); pageNum++)
                {
                    extractedPdf.AddPage(pdfDocument.GetPage(pageNum).CopyTo(extractedPdf));
                }
            }
            return extractedPdfStream.ToArray();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error extracting pages: {ex.Message}");
            throw;
        }
        finally
        {
            pdfStream.Close();
            extractedPdfStream.Close();
        }
    }

    private void ValidatePageRange(int startPage, int endPage)
    {
        if (startPage < 1 || endPage < startPage)
        {
            throw new ArgumentException("Invalid page parameters.");
        }
    }
    public byte[] ConvertToByteArray(IFormFile file)
    {
        using (var stream = new MemoryStream())
        {
            file.CopyTo(stream);
            return stream.ToArray();
        }
    }

}