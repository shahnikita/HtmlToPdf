using HtmlAgilityPack;
using HTMLtoPDF;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.tool.xml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HtmlToPdf
{
    class Program
    {
        static void Main(string[] args)
        {
            byte[] pdf; // result will be here
        
            var cssText = File.ReadAllText("layout.css");
            var html = File.ReadAllText("demo.html");

            html = AutoCloseHtmlTags(html);


            using (var memoryStream = new MemoryStream())
            {
                var document = new Document(PageSize.A4, 0, 0, 0, 0);
                var writer = PdfWriter.GetInstance(document, memoryStream);
                document.Open();

                using (var cssMemoryStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(cssText)))
                {
                    using (var htmlMemoryStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(html)))
                    {
                        XMLWorkerHelper.GetInstance().ParseXHtml(writer, document, htmlMemoryStream, cssMemoryStream);
                    }
                }

                document.Close();

                pdf = memoryStream.ToArray();
                File.WriteAllBytes("Foo.pdf", pdf);

            }
        }


        public static void CreatePDFFromHTMLFile(string HtmlStream, string FileName)
        {
            try
            {
                object TargetFile = FileName;
                string ModifiedFileName = string.Empty;
                string FinalFileName = string.Empty;


                GeneratePDF.HtmlToPdfBuilder builder = new GeneratePDF.HtmlToPdfBuilder(iTextSharp.text.PageSize.A4);
                GeneratePDF.HtmlPdfPage first = builder.AddPage();
                first.AppendHtml(HtmlStream);
                byte[] file = builder.RenderPdf();
                File.WriteAllBytes(TargetFile.ToString(), file);

                iTextSharp.text.pdf.PdfReader reader = new iTextSharp.text.pdf.PdfReader(TargetFile.ToString());
                ModifiedFileName = TargetFile.ToString();
                ModifiedFileName = ModifiedFileName.Insert(ModifiedFileName.Length - 4, "1");

                iTextSharp.text.pdf.PdfEncryptor.Encrypt(reader, new FileStream(ModifiedFileName, FileMode.Append), iTextSharp.text.pdf.PdfWriter.STRENGTH128BITS, "", "", iTextSharp.text.pdf.PdfWriter.AllowPrinting);
                reader.Close();
                if (File.Exists(TargetFile.ToString()))
                    File.Delete(TargetFile.ToString());
                FinalFileName = ModifiedFileName.Remove(ModifiedFileName.Length - 5, 1);
                //File.Copy(ModifiedFileName, FinalFileName);
                //if (File.Exists(ModifiedFileName))
                //    File.Delete(ModifiedFileName);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static string AutoCloseHtmlTags(string inputHtml)
        {
            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.OptionWriteEmptyNodes = true;
            htmlDoc.LoadHtml(inputHtml);

            var inputTags = htmlDoc.DocumentNode.SelectNodes("//input");
            HtmlNode span = htmlDoc.CreateElement("span");
            HtmlNode node2;
            HtmlAttribute att;
            //foreach (HtmlNode input in inputTags)
            //{

            //    att = input.Attributes["value"];
            //    span.InnerHtml = att.Value.ToString();
            //    node2 = input.ParentNode;
            //    node2.AppendChild(span);
            //    node2.RemoveChild(input);

            //}
            return htmlDoc.DocumentNode.OuterHtml;
        }

    }
}

