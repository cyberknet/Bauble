/*  This file is part of Bauble - http://bauble.codeplex.com
 *  Copyright (C) 2010-2012 Scott Blomfield
 *
 *  This program is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with this program.  If not, see <http://www.gnu.org/licenses/>. 
 *
 *  Additional portions Copyright (C) (year) by (contributor)
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media.Imaging;
using System.IO;
using System.Xml.Linq;
using System.Windows.Media;

namespace Bauble.Common.Themes
{
    public class Theme
    {
        public string Name { get; set; }

        public string FileName { get; set; }
        public string Location { get; set; }

        public string PreviewFileName { get; set; }
        public BitmapSource Preview { get; set; } 
        
        public string BackgroundImage { get; set; }
        public SolidColorBrush BackgroundBrush { get; set; }

        public string IconBackgroundImage { get; set; }

        public double MaximumIconSize { get; set; }
        public double MinimumIconSize { get; set; }

        public double AutoHideAdjustment { get; set; }

        public VerticalAlignment VerticalAlignment { get; set; }

        public BitmapImage LeftCap { get; set; }
        public BitmapImage Middle { get; set; }
        public BitmapImage RightCap { get; set; }
        public BitmapImage Separator { get; set; }
        public BitmapImage Configure { get; set; }
        
        public Thickness Margin { get; set; }

        // if no themes are found, then these values will be used
        public Theme()
        {
            Name = "Default";
            Location = @"Themes\";
            FileName = @"themes\theme.xml";
            BackgroundBrush = new SolidColorBrush(Color.FromArgb(1, 0, 0, 0));
            MaximumIconSize = 128;
            MinimumIconSize = 48;
            VerticalAlignment = VerticalAlignment.Top;
            AutoHideAdjustment = 2.0d;
        }

        public static List<Theme> FindThemes()
        {
            List<Theme> themes = new List<Theme>();
            string directory = @"themes\";
            try
            {
                string[] themeXmlFiles = Directory.GetFiles(directory, "theme.xml", SearchOption.AllDirectories);

                foreach (String themeXml in themeXmlFiles)
                {
                    Theme theme = LoadTheme(themeXml);
                    themes.Add(theme);
                }
            }
            catch (DirectoryNotFoundException)
            {
                themes.Add(new Theme());
            }
            return themes;
        }

        private static Theme LoadTheme(string themeXml)
        {
            Theme theme = new Theme();
            XDocument document = XDocument.Load(themeXml);
            XElement root = document.Root;
            XElement element = null;
            BrushConverter brush = new BrushConverter();
            ThicknessConverter tc = new ThicknessConverter();
            string value = string.Empty;

            theme.Name = root.Element("Name").Value;
            theme.Location = Path.GetDirectoryName(themeXml);
            theme.FileName = themeXml.ToLower().Trim();

            element = root.Element("images");
            theme.LeftCap = GetBitmapFromElement(element, theme.Location, "LeftCap");
            theme.Middle = GetBitmapFromElement(element, theme.Location, "Middle");
            theme.RightCap = GetBitmapFromElement(element, theme.Location, "RightCap");
            theme.Separator = GetBitmapFromElement(element, theme.Location, "Separator");
            theme.Configure = GetBitmapFromElement(element, theme.Location, "Configure");

            theme.PreviewFileName = root.Element("Preview").Value;

            element = root.Element("Background");
            theme.BackgroundImage = element.Element("Image").Value;
            theme.BackgroundBrush = brush.ConvertFromString(element.Element("Color").Value) as SolidColorBrush;

            element = root.Element("Icon");
            theme.IconBackgroundImage = element.Element("BackgroundImage").Value;
            theme.MaximumIconSize = Convert.ToDouble(element.Element("MaximumSize").Value);
            theme.MinimumIconSize = Convert.ToDouble(element.Element("MinimumSize").Value);
            theme.AutoHideAdjustment = Convert.ToDouble(root.Element("AutoHideAdjustment").Value);

            theme.VerticalAlignment = (VerticalAlignment)Enum.Parse(typeof(VerticalAlignment), root.Element("VerticalAlignment").Value);
            theme.Margin = GetThicknessFromElement((XElement)root.Element("Margin"), new Thickness(0));

            return theme;
        }

        #region GetThicknessFromElement
        private static Thickness GetThicknessFromElement(XElement element, Thickness defaultValue)
        {
            Thickness margin = defaultValue;
            ThicknessConverter tc = new ThicknessConverter();
            if (element != null)
            {
                try
                {
                    margin = (Thickness)tc.ConvertFromString(element.Value);
                }
                catch (NotSupportedException)
                {
                }
            }
            return margin;
        }
        #endregion GetThicknessFromElement

        #region GetBitmapFromElement
        private static BitmapImage GetBitmapFromElement(XElement root, string themeLocation, string elementName)
        {
            BitmapImage image = null;
            try
            {
                XElement element = root.Element(elementName);
                if (element != null)
                {
                    if (!string.IsNullOrEmpty(element.Value))
                    {
                        string filename = Path.Combine(themeLocation, element.Value);
                        image = new BitmapImage(new Uri(System.IO.Path.GetFullPath(filename)));
                    }
                }
            }
            catch { }

            return image;
        }
        #endregion GetBitmapFromElement
    }
}
