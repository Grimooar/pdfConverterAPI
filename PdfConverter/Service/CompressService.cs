namespace PdfConverter.Service;

/// <summary>
/// 
/// </summary>
public class CompressService
{
    private byte[] compressedPdf;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="inputFilePath"></param>
    /// <param name="compressionLevel"></param>
    /// <exception cref="ArgumentException"></exception>
    public void CompressPdf(string inputFilePath, int compressionLevel)
    {
        try
        {
            var pdf = new PdfDocument(inputFilePath);

            switch (compressionLevel)
            {
                case 1: // Light Compression
                    pdf.CompressImages(60);
                    break;
                case 2: // Strong Compression
                    pdf.CompressImages(90, ScaleToVisibleSize: true);
                    break;
                case 3: // Ultra Compression
                    pdf.CompressImages(50);
                    break;
                default:
                    throw new ArgumentException("Invalid compression level");
            }

            // Сохранение в поле compressedPdf
            compressedPdf = pdf.Stream.ToArray();

            Console.WriteLine($"PDF compressed successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error while trying to compress PDF: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public byte[] GetCompressedPdf()
    {
        // Возвращаем сжатый PDF
        return compressedPdf;
    }
}