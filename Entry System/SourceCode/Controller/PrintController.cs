using CodeScanner.Model;
using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using ZXing;


namespace CodeScanner.Controller
{
    internal class PrintController
    {
        XFont fontBold = new XFont("Courier New", 16, XFontStyle.Bold);
        XFont fontNormal = new XFont("Courier New", 14);

        public void GenerateTicketPDF(string ticketPath, string ticketCode, string eventName, DateTime eventDate, float ticketPrice)
        {
            // function for generating PDF files of tickets

            // Document setup
            int xOffset = 10, yOffset = 25, lineHeight = 20;
            PdfDocument document = new PdfDocument();
            PdfPage page = document.AddPage();
            page.Height = XUnit.FromInch(5.63);
            page.Width = XUnit.FromInch(1.97);
            page.Orientation = PdfSharp.PageOrientation.Landscape;
            XGraphics gfx = XGraphics.FromPdfPage(page);

            // Add event details to the document
            gfx.DrawString($"Collosus -> {eventName}", fontBold, XBrushes.Black, xOffset, yOffset);
            yOffset += lineHeight * 2;
            gfx.DrawString($"Čas: {eventDate.Hour.ToString("00")}:{eventDate.Minute.ToString("00")}", fontNormal, XBrushes.Black, xOffset, yOffset);
            yOffset += lineHeight;
            gfx.DrawString($"Datum: {eventDate.Day}.{eventDate.Month} {eventDate.Year}", fontNormal, XBrushes.Black, xOffset, yOffset);
            yOffset += lineHeight;
            gfx.DrawString($"Cena za lístek: {ticketPrice} Kč", fontNormal, XBrushes.Black, xOffset, yOffset);
            yOffset += lineHeight;

            // Add the generated barcode to the document
            gfx.DrawImage(GenerateBarcode(ticketCode), 200, 40);

            // Save the document
            document.Save(ticketPath);
        }

        static XImage GenerateBarcode(string text)
        {
            // function for generating a XImage object with the ticket code

            BarcodeWriter barcodeWriter = new BarcodeWriter();
            barcodeWriter.Format = BarcodeFormat.CODE_128;
            barcodeWriter.Options = new ZXing.Common.EncodingOptions { Width = 300, Height = 100 };

            // Convert barcode to XImage
            Bitmap barcodeBitmap = barcodeWriter.Write(text);
            MemoryStream memoryStream = new MemoryStream();
            barcodeBitmap.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
            return XImage.FromStream(memoryStream);
        }
    }
}
