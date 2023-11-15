using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Xobject;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;

namespace PdfConverter.Service;

public class PdfManipulationService
{
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
    /*public byte[] AddWatermark(byte[] pdfBytes, string watermarkText)
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
                            Document document = new Document(pdfDocument);

                            for (int i = 1; i <= pdfDocument.GetNumberOfPages(); i++)
                            {
                                document.ShowTextAligned(new Paragraph(watermarkText), 100, 100, i, TextAlignment.CENTER, VerticalAlignment.TOP, 0);
                            }
                        }
                    }
                }

                return outputStream.ToArray();
            }
        }
    }*/
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