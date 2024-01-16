using System.IO.Compression;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.Kernel.Pdf.Extgstate;
using iText.Kernel.Pdf.Xobject;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;

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
    /// <param name="fileName">file name</param>
    /// <returns>Merged PDF byte array.</returns>
    public byte[] MergePdfs(List<byte[]> pdfs, string fileName)
    {
        MemoryStream mergedPdfStream = new MemoryStream();
        string outputFileName = $"{fileName}_Merged.pdf";

        using (PdfWriter writer = new PdfWriter(mergedPdfStream))
        using (PdfDocument mergedPdf = new PdfDocument(writer))
        using (Document document = new Document(mergedPdf))
        {
            foreach (var pdfBytes in pdfs)
            {
                MergeSinglePdf(document, pdfBytes);
            }

            // Save the merged file with the user's filename and operation name
            mergedPdfStream.Seek(0, SeekOrigin.Begin);
            using (FileStream fs = new FileStream(outputFileName, FileMode.Create))
            {
                mergedPdfStream.CopyTo(fs);
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
    public List<byte[]> SplitPdf(byte[] pdfBytes, int splitAfterPage, string fileName)
    {
        using (MemoryStream pdfStream = new MemoryStream(pdfBytes))
        using (PdfDocument pdfDocument = new PdfDocument(new PdfReader(pdfStream)))
        {
            List<byte[]> splitPdfDocuments = new List<byte[]>();
            splitPdfDocuments.Add(CreateSplitPdf(pdfDocument, 1, splitAfterPage, fileName + "_Split1"));
            splitPdfDocuments.Add(CreateSplitPdf(pdfDocument, splitAfterPage + 1, pdfDocument.GetNumberOfPages(), fileName + "_Split2"));

            return splitPdfDocuments;
        }
    }
    private byte[] CreateSplitPdf(PdfDocument originalPdf, int startPage, int endPage, string fileName)
    {
        MemoryStream splitStream = new MemoryStream();
        string outputFileName = $"{fileName}.pdf";

        using (PdfWriter writer = new PdfWriter(splitStream))
        using (PdfDocument splitPdf = new PdfDocument(writer))
        {
            for (int pageNum = startPage; pageNum <= endPage; pageNum++)
            {
                splitPdf.AddPage(originalPdf.GetPage(pageNum).CopyTo(splitPdf));
            }

            // Save the split file with the user's filename and operation name
            splitStream.Seek(0, SeekOrigin.Begin);
            using (FileStream fs = new FileStream(outputFileName, FileMode.Create))
            {
                splitStream.CopyTo(fs);
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
    public byte[] AddWatermark(byte[] pdfBytes, string watermarkText, string fileName)
    {
        MemoryStream inputStream = new MemoryStream(pdfBytes);
        MemoryStream outputStream = new MemoryStream();
        string outputFileName = $"{fileName}_Watermarked.pdf";

        try
        {
            using (PdfReader pdfReader = new PdfReader(inputStream))
            using (PdfWriter pdfWriter = new PdfWriter(outputStream))
            using (PdfDocument pdfDocument = new PdfDocument(pdfReader, pdfWriter))
            {
                AddWatermarkToPdf(pdfDocument, watermarkText);
            }

            // Save the watermarked file with the user's filename and operation name
            outputStream.Seek(0, SeekOrigin.Begin);
            using (FileStream fs = new FileStream(outputFileName, FileMode.Create))
            {
                outputStream.CopyTo(fs);
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
            Rectangle pageSize = page.GetPageSize();

            // Настройка параметров водяного знака
            float width = pageSize.GetWidth();
            float height = pageSize.GetHeight();
            float watermarkWidth = width / 2;
            float x = (width - watermarkWidth) / 2;
            float y = height / 2;

            // Создание водяного знака
            Paragraph watermark = new Paragraph(watermarkText)
                .SetFontColor(iText.Kernel.Colors.ColorConstants.RED, 0.5f) // 50% прозрачность
                .SetFontSize(40)
                .SetRotationAngle(Math.PI / 4);

            // Добавление водяного знака на страницу
            Canvas canvas = new Canvas(page, pageSize);
            canvas.Add(watermark.SetFixedPosition(x, y, watermarkWidth));
        }
    }

    /// <summary>
    /// Compresses a PDF using the specified compression level.
    /// </summary>
    /// <param name="pdfBytes">Input PDF byte array.</param>
    /// <param name="compressionLevel">Compression level (from 0-9) 9 high compression 0 without.</param>
    /// <returns>Compressed PDF byte array.</returns>
    public byte[] CompressPdf(byte[] pdfBytes, int compressionLevel, string fileName)
    {
        MemoryStream inputStream = new MemoryStream(pdfBytes);
        MemoryStream outputStream = new MemoryStream();
        string outputFileName = $"{fileName}_Compressed.pdf";

        try
        {
            using (PdfReader pdfReader = new PdfReader(inputStream))
            using (PdfWriter pdfWriter = new PdfWriter(outputStream).SetCompressionLevel(compressionLevel))
            using (PdfDocument pdfDocument = new PdfDocument(pdfReader, pdfWriter))
            {
                // Additional compression operations can be added here
            }

            // Save the compressed file with the user's filename and operation name
            outputStream.Seek(0, SeekOrigin.Begin);
            using (FileStream fs = new FileStream(outputFileName, FileMode.Create))
            {
                outputStream.CopyTo(fs);
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
    public byte[] ExtractPagesFromPdf(byte[] pdfBytes, int startPage, int endPage, string fileName)
    {
        ValidatePageRange(startPage, endPage);

        MemoryStream pdfStream = new MemoryStream(pdfBytes);
        MemoryStream extractedPdfStream = new MemoryStream();
        string outputFileName = $"{fileName}_Extracted.pdf";

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

            // Save the extracted file with the user's filename and operation name
            extractedPdfStream.Seek(0, SeekOrigin.Begin);
            using (FileStream fs = new FileStream(outputFileName, FileMode.Create))
            {
                extractedPdfStream.CopyTo(fs);
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