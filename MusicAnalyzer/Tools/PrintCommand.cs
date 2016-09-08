﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MusicAnalyzer.Tools
{
    public class PrintCommand : ICommand
    {
        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            if (parameter is FrameworkElement)
            {
                
                PrintDialog printDialog = new PrintDialog();
                if ((bool)printDialog.ShowDialog().GetValueOrDefault())
                {
                    Mouse.OverrideCursor = Cursors.Wait;
                    System.Printing.PrintCapabilities capabilities =
                      printDialog.PrintQueue.GetPrintCapabilities(printDialog.PrintTicket);
                    double maxPrintWidth = printDialog.PrintableAreaWidth;
                    double dpiScale = 300.0 / 96.0;
                    FixedDocument document = new FixedDocument();
                    //object grid = divideIncipitToRows(parameter, capabilities.PageImageableArea.ExtentWidth);
                    FrameworkElement objectToPrint = parameter as FrameworkElement;
                    try
                    {
                        // Convert the UI control into a bitmap at 300 dpi
                        double dpiX = 300;
                        double dpiY = 300;
                        RenderTargetBitmap bmp = new RenderTargetBitmap(Convert.ToInt32(
                          objectToPrint.Width * dpiScale),
                          Convert.ToInt32(objectToPrint.Height * dpiScale),
                          dpiX, dpiY, PixelFormats.Pbgra32);
                        bmp.Render(objectToPrint);

                        // Convert the RenderTargetBitmap into a bitmap we can more readily use
                        PngBitmapEncoder png = new PngBitmapEncoder();
                        png.Frames.Add(BitmapFrame.Create(bmp));
                        System.Drawing.Bitmap bmp2;
                        using (MemoryStream memoryStream = new MemoryStream())
                        {
                            png.Save(memoryStream);
                            bmp2 = new System.Drawing.Bitmap(memoryStream);
                        }
                        document.DocumentPaginator.PageSize =
                          new Size(printDialog.PrintableAreaWidth, printDialog.PrintableAreaHeight);
                        
                        
                        // break into rows
                        bmp2 = divideIncipitToRows(bmp2, maxPrintWidth, dpiScale);

                        // break the bitmap into pages
                        int pageBreak = 0;
                        int previousPageBreak = 0;
                        int pageHeight =
                            Convert.ToInt32(capabilities.PageImageableArea.ExtentHeight * dpiScale);
                        while (pageBreak < bmp2.Height - pageHeight)
                        {
                            pageBreak += pageHeight;  // Where we thing the end of the page should be

                            // Keep moving up a row until we find a good place to break the page
                            while (!IsRowGoodBreakingPoint(bmp2, pageBreak))
                                pageBreak--;

                            PageContent pageContent = generatePageContent(bmp2, previousPageBreak,
                              pageBreak, document.DocumentPaginator.PageSize.Width,
                              document.DocumentPaginator.PageSize.Height, capabilities);
                            document.Pages.Add(pageContent);
                            previousPageBreak = pageBreak;
                        }

                        // Last Page
                        PageContent lastPageContent = generatePageContent(bmp2, previousPageBreak,
                          bmp2.Height, document.DocumentPaginator.PageSize.Width,
                          document.DocumentPaginator.PageSize.Height, capabilities);
                        document.Pages.Add(lastPageContent);
                    }
                    finally
                    {
                        // Scale UI control back to the original so we don't effect what is on the screen 
                        objectToPrint.Width = double.NaN;
                        objectToPrint.UpdateLayout();
                        objectToPrint.LayoutTransform = new ScaleTransform(1, 1);
                        Size size = new Size(capabilities.PageImageableArea.ExtentWidth,
                                             capabilities.PageImageableArea.ExtentHeight);
                        objectToPrint.Measure(size);
                        objectToPrint.Arrange(new Rect(new Point(capabilities.PageImageableArea.OriginWidth,
                                              capabilities.PageImageableArea.OriginHeight), size));
                        Mouse.OverrideCursor = null;
                    }
                    printDialog.PrintDocument(document.DocumentPaginator, "Print Document Name");
                }
            }
        }

        private System.Drawing.Bitmap divideIncipitToRows(System.Drawing.Bitmap bmpInitial, double maxRowWidth, double dpiScale)
        {
            int rows = (int)Math.Ceiling(bmpInitial.Width / (maxRowWidth * dpiScale));
            System.Drawing.Bitmap newBMP = new System.Drawing.Bitmap((int)(maxRowWidth * dpiScale), bmpInitial.Height * (rows+1));
            for (int i = 0; i < rows; i++)
            {
                int tempWidth = Math.Min((int)(maxRowWidth * dpiScale), bmpInitial.Width - (i * (int)(maxRowWidth * dpiScale)));
                System.Drawing.Rectangle srcRect = new System.Drawing.Rectangle(i * (int)(maxRowWidth * dpiScale), 0, tempWidth, bmpInitial.Height);
                System.Drawing.Rectangle dstRect = new System.Drawing.Rectangle(0, (int)((i+1) * bmpInitial.Height), srcRect.Width, srcRect.Height);
                using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(newBMP)){
                    g.DrawImage(bmpInitial, dstRect, srcRect, System.Drawing.GraphicsUnit.Pixel);
                }
            }
           return newBMP;
        }

        private PageContent generatePageContent(System.Drawing.Bitmap bmp, int top, int bottom, double pageWidth, double PageHeight, System.Printing.PrintCapabilities capabilities)
        {
            FixedPage printDocumentPage = new FixedPage();
            printDocumentPage.Width = pageWidth;
            printDocumentPage.Height = PageHeight;

            int newImageHeight = bottom - top;
            System.Drawing.Bitmap bmpPage = bmp.Clone(new System.Drawing.Rectangle(0, top,
                   bmp.Width, newImageHeight), System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            // Create a new bitmap for the contents of this page
            Image pageImage = new Image();
            BitmapSource bmpSource =
                System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                    bmpPage.GetHbitmap(),
                    IntPtr.Zero,
                    System.Windows.Int32Rect.Empty,
                    BitmapSizeOptions.FromWidthAndHeight(bmp.Width, newImageHeight));

            pageImage.Source = bmpSource;
            pageImage.VerticalAlignment = VerticalAlignment.Top;

            // Place the bitmap on the page
            printDocumentPage.Children.Add(pageImage);

            PageContent pageContent = new PageContent();
            ((System.Windows.Markup.IAddChild)pageContent).AddChild(printDocumentPage);

            FixedPage.SetLeft(pageImage, capabilities.PageImageableArea.OriginWidth);
            FixedPage.SetTop(pageImage, capabilities.PageImageableArea.OriginHeight);

            pageImage.Width = capabilities.PageImageableArea.ExtentWidth;
            pageImage.Height = capabilities.PageImageableArea.ExtentHeight;
            return pageContent;
        }

        private bool IsRowGoodBreakingPoint(System.Drawing.Bitmap bmp, int row)
        {
            double maxDeviationForEmptyLine = 1627500;
            bool goodBreakingPoint = false;

            if (rowPixelDeviation(bmp, row) < maxDeviationForEmptyLine)
                goodBreakingPoint = true;

            return goodBreakingPoint;
        }

        private double rowPixelDeviation(System.Drawing.Bitmap bmp, int row)
        {
            int count = 0;
            double total = 0;
            double totalVariance = 0;
            double standardDeviation = 0;
            System.Drawing.Imaging.BitmapData bmpData = bmp.LockBits(new System.Drawing.Rectangle(0, 0, 
                   bmp.Width, bmp.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, bmp.PixelFormat);
            int stride = bmpData.Stride;
            IntPtr firstPixelInImage = bmpData.Scan0;

            unsafe
            {
                byte* p = (byte*)(void*)firstPixelInImage;
                p += stride * row;  // find starting pixel of the specified row
                for (int column = 0; column < bmp.Width; column++)
                {
                    count++;

                    byte blue = p[0];
                    byte green = p[1];
                    byte red = p[3];

                    int pixelValue = System.Drawing.Color.FromArgb(0, red, green, blue).ToArgb();
                    total += pixelValue;
                    double average = total / count;
                    totalVariance += Math.Pow(pixelValue - average, 2);
                    standardDeviation = Math.Sqrt(totalVariance / count);

                    // go to next pixel
                    p += 3;
                }
            }
            bmp.UnlockBits(bmpData);

            return standardDeviation;
        }


        public event EventHandler CanExecuteChanged;
    }
}
