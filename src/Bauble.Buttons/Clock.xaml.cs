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
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Xml.Linq;
using Bauble.Common.Events;
using Bauble.Common.Interfaces;
using Bauble.Common.Themes;

namespace Bauble.Buttons
{
    /// <summary>
    /// Interaction logic for Clock.xaml
    /// </summary>
    public partial class Clock : UserControl, IBaubleButton
    {
        public Clock()
        {
            InitializeComponent();

            _canvas.DataContext = this;

            TimeInfo = new Data.TimeInfo(TimeZoneInfo.Local);

            _timer = new DispatcherTimer();
            _timer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            _timer.Tick += new EventHandler(_timer_Tick);
            _timer.Start();
        }

        static Clock()
        {
            TimeInfoProperty = DependencyProperty.Register
                ("TimeInfo", typeof(Data.TimeInfo), typeof(Clock)
                , new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

            HourAngleProperty = DependencyProperty.Register
                ("HourAngle", typeof(double), typeof(Clock)
                , new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender));
            MinuteAngleProperty = DependencyProperty.Register
                ("MinuteAngle", typeof(double), typeof(Clock)
                , new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender));
            SecondAngleProperty = DependencyProperty.Register
                ("SecondAngle", typeof(double), typeof(Clock)
                , new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender));
        }

        public Data.TimeInfo TimeInfo
        {
            get
            {
                return (Data.TimeInfo)GetValue(TimeInfoProperty);
            }
            set
            {
                SetValue(TimeInfoProperty, value);
            }
        }

        public double HourAngle
        {
            get
            {
                return (double)GetValue(HourAngleProperty);
            }
            set
            {
                SetValue(HourAngleProperty, value);
            }
        }

        public double MinuteAngle
        {
            get
            {
                return (double)GetValue(MinuteAngleProperty);
            }
            set
            {
                SetValue(MinuteAngleProperty, value);
            }
        }

        public double SecondAngle
        {
            get
            {
                return (double)GetValue(SecondAngleProperty);
            }
            set
            {
                SetValue(SecondAngleProperty, value);
            }
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            for (int i = 0; i < 60; ++i)
            {
                Rectangle marker = new Rectangle();

                if ((i % 5) == 0)
                {
                    marker.Width = 3;
                    marker.Height = 8;
                    marker.Fill = new SolidColorBrush(Color.FromArgb(0xe0, 0xff, 0xff, 0xff));
                    marker.Stroke = new SolidColorBrush(Color.FromArgb(0x80, 0x33, 0x33, 0x33));
                    marker.StrokeThickness = 0.5;
                }
                else
                {
                    marker.Width = 0.5;
                    marker.Height = 3;
                    marker.Fill = new SolidColorBrush(Color.FromArgb(0x80, 0xff, 0xff, 0xff));
                    marker.Stroke = null;
                    marker.StrokeThickness = 0;
                }

                TransformGroup transforms = new TransformGroup();

                transforms.Children.Add(new TranslateTransform(-(marker.Width / 2), marker.Width / 2 - 40 - marker.Height));
                transforms.Children.Add(new RotateTransform(i * 6));
                transforms.Children.Add(new TranslateTransform(50, 50));

                marker.RenderTransform = transforms;

                _markersCanvas.Children.Add(marker);
            }

            for (int i = 1; i <= 12; ++i)
            {
                TextBlock tb = new TextBlock();

                tb.Text = i.ToString();
                tb.TextAlignment = TextAlignment.Center;
                tb.RenderTransformOrigin = new Point(1, 1);
                tb.Foreground = Brushes.White;
                tb.FontSize = 4;

                tb.RenderTransform = new ScaleTransform(2, 2);

                double r = 34;
                double angle = Math.PI * i * 30.0 / 180.0;
                double x = Math.Sin(angle) * r + 50, y = -Math.Cos(angle) * r + 50;

                Canvas.SetLeft(tb, x);
                Canvas.SetTop(tb, y);

                _markersCanvas.Children.Add(tb);
            }
        }

        private void _timer_Tick(object sender, EventArgs e)
        {
            Data.TimeInfo ti = TimeInfo;

            if (ti == null)
            {
                HourAngle = MinuteAngle = SecondAngle = 0;
            }
            else
            {
                double hour = ti.GetAdjusted(DateTime.UtcNow).Hour;
                double minute = ti.GetAdjusted(DateTime.UtcNow).Minute;
                double second = ti.GetAdjusted(DateTime.UtcNow).Second;

                double hourAngle = 30 * hour + minute / 2 + second / 120;
                double minuteAngle = 6 * minute + second / 10;
                double secondAngle = 6 * second;

                HourAngle = hourAngle;
                MinuteAngle = minuteAngle;
                SecondAngle = secondAngle;
            }
        }

        public static DependencyProperty TimeInfoProperty, HourAngleProperty, MinuteAngleProperty, SecondAngleProperty;

        private DispatcherTimer _timer = new DispatcherTimer();

        public string Text { get; set; }

        bool IBaubleButton.Configurable
        {
            get { return false; }
        }

        void IBaubleButton.Configure()
        {
            MessageBox.Show("Configure me!");
        }

        void IBaubleButton.Initialize(Theme theme)
        {
            Width = theme.MinimumIconSize;
            Height = theme.MinimumIconSize;
            MinWidth = theme.MinimumIconSize;
            MaxWidth = theme.MaximumIconSize;
            MinHeight = theme.MinimumIconSize;
            MaxHeight = theme.MaximumIconSize;
            Margin = theme.Margin;
        }

        void IBaubleButton.LoadFromXml(string Xml)
        {
            XElement element = XElement.Parse(Xml);

            Text = element.Element("Text").Value;
        }

        string IBaubleButton.Text
        {
            get
            {
                return "Clock";
            }
            set { }
        }

        XElement IBaubleButton.ToXml()
        {
            return new XElement("Icon", new XAttribute("type", this.GetType().FullName),
                        new XElement("Text", Text));
        }

        private RoutedEventHandler _activated;
        event RoutedEventHandler IBaubleButton.Activated
        {
            add { _activated += value; }
            remove { _activated -= value; }
        }

        protected void OnActivated()
        {
            if (_activated != null)
                _activated(this, new RoutedEventArgs(SharedEvents.ActivatedEvent, this));
        }
    }
}

