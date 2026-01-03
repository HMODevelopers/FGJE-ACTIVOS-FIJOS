using System.Drawing;
using QRCoder;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;
using Image = iTextSharp.text.Image;
using System.Windows;


namespace ActivosFijos.Helpers
{
    public class QrCodeHelper
    {
        public static Image GenerateQRCodeImage(string qrText)
        {
            // Crear el generador de código QR
            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(qrText, QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(qrCodeData);

            // Crear la imagen del código QR
            Bitmap qrCodeBitmap = qrCode.GetGraphic(50, Color.Black, Color.White, true); // Fondo blanco

            // Convertir Bitmap a Image de iTextSharp
            Image qrCodeImage = Image.GetInstance(qrCodeBitmap, BaseColor.BLACK);
            qrCodeImage.ScaleToFit(30, 30); // Escalar la imagen al tamaño deseado

            return qrCodeImage;
        }
    }
}