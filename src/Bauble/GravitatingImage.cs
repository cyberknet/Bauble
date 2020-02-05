using System;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace Bauble
{
    public class GravitatingImage : Image 
    {
        public string Text { get; set; }

        public GravitatingImage(TextBlock textBlock, string text)
        {
            TextBlock = textBlock;
            Text = text;
        }

        public TextBlock TextBlock { get; private set; }

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            TextBlock.Inlines.Clear();
            TextBlock.Inlines.Add(new Bold(new Run(Text)));
            TextBlock.Visibility = Visibility.Visible;

            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            TextBlock.Visibility = Visibility.Hidden;
            base.OnMouseLeave(e);
        }
    }
}
