using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Tractor.Com.QuantAsylum.Tractor.HTML
{
    class HtmlWriter
    {
        string Dir;
        const string ImagePath = @"\Images\";
        string ImageDir;

        StringBuilder Sb;

        const string MarkerStart = "<!-- TractorContentBegin -->";
        const string MarkerEnd = "<!-- TractorContentEnd -->";

        public HtmlWriter(string directory)
        {
            Dir = directory;
            ImageDir = Dir + ImagePath;

            if (Directory.Exists(Dir) == false)
            {
                Directory.CreateDirectory(Dir);
            }

            if (Directory.Exists(ImageDir) == false)
            {
                Directory.CreateDirectory(ImageDir);
            }

            Sb = new StringBuilder();
        }

        public void AddHeading1(string text)
        {
            Sb.Append("<h1>" + text + "</h1>");
        }

        public void AddHeading2(string text)
        {
            Sb.Append("<h2>" + text + "</h2>");
        }

        public void AddParagraph(string text)
        {
            Sb.Append("<p>"+text+"<p>");
        }

        public void AddLink(string linkText, string url)
        {
            Sb.Append( string.Format("<a href=\"{0}\">{1}</a>", url, linkText) );
        }

        public string ImageLink(string linkText, Bitmap bmp)
        {
            string fileName = string.Format(@"{0}.png", Guid.NewGuid());
            bmp.Save(ImageDir + fileName, ImageFormat.Png);
            return string.Format("<a href=\"{0}\">{1}</a>", @"." + ImagePath + fileName, linkText);
        }
        public void AddImageLink(string linkText, Bitmap bmp)
        {
            //string fileName = string.Format(@"{0}.png", Guid.NewGuid());
            //bmp.Save(ImageDir + fileName, ImageFormat.Png);
            //Sb.Append(string.Format("<a href=\"{0}\">{1}</a>", @"." + ImagePath + fileName, linkText));
            Sb.Append(ImageLink(linkText, bmp));
        }

        public void Render()
        {
            String existingPageContent = "";
            string file = Dir + "/Index.html";

            if (File.Exists(file))
            {
                string[] toks = File.ReadAllText(file).Split(new string[] { MarkerStart, MarkerEnd }, StringSplitOptions.None);
                existingPageContent = toks[1];
            }

            StringBuilder page = new StringBuilder();

            page.AppendLine("<!DOCTYPE html>");
            page.AppendLine("<html>");
            page.AppendLine("<body>");
            page.AppendLine(MarkerStart);
            page.AppendLine(existingPageContent);
            page.AppendLine(Sb.ToString());
            page.AppendLine(MarkerEnd);
            page.AppendLine("</body>");
            page.AppendLine("</html>");

            File.WriteAllText(file, page.ToString());
        }
    }
}
