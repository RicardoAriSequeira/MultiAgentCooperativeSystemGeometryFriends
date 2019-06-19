using System;
using System.Drawing;
using System.Xml;

namespace GeometryFriends.XNAStub
{
    internal class SpriteFont
    {
        public Font BaseFont { get; private set; }
        public string Name { get; set; }
        public int Size { get; set; }
        public int LineSpacing { get; set; }
        public FontStyle Style { get; set; }

        public SpriteFont(string name, int size, int spacing, FontStyle style)
        {
            Name = name;
            Size = size;
            LineSpacing = spacing;
            Style = style;

            //load the font
            BaseFont = new Font(name, size, style);
        }

        public SpriteFont(string spriteFontPath)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(spriteFontPath);

            //directly read all fields
            Name = doc.DocumentElement.SelectSingleNode("/XnaContent/Asset/FontName").FirstChild.InnerText;
            Size = Convert.ToInt32(doc.DocumentElement.SelectSingleNode("/XnaContent/Asset/Size").FirstChild.InnerText);
            LineSpacing = Convert.ToInt32(doc.DocumentElement.SelectSingleNode("/XnaContent/Asset/Spacing").FirstChild.InnerText);
            string styleID = doc.DocumentElement.SelectSingleNode("/XnaContent/Asset/FontName").FirstChild.InnerText;

            switch (styleID)
            {
                case "Bold":
                    Style = FontStyle.Bold;
                    break;
                case "Italic":
                    Style = FontStyle.Italic;
                    break;
                case "Regular":
                    Style = FontStyle.Regular;
                    break;
                case "Strikeout":
                    Style = FontStyle.Strikeout;
                    break;
                case "Underline":
                    Style = FontStyle.Underline;
                    break;
                default:
                    Log.LogWarning("No font style found in spritefont resource: " + spriteFontPath + " - using regular font style.");
                    Style = FontStyle.Regular;
                    break;
            }

            //load the font
            BaseFont = new Font(Name, Size, Style);
        }
    }
}
