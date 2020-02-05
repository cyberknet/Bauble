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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Bauble.Common.Interfaces;
using Bauble.Common.Events;
using Bauble.Common.Themes;
using System.Xml.Linq;

namespace Bauble.Buttons
{
    /// <summary>
    /// Interaction logic for Separator.xaml
    /// </summary>
    public partial class Separator : UserControl, IBaubleButton
    {
        public Separator()
        {
            InitializeComponent();
        }

        #region IBaubleButton Members

        bool IBaubleButton.Configurable
        {
            get { return false; }
        }

        void IBaubleButton.Configure()
        {
            throw new NotImplementedException();
        }

        void IBaubleButton.Initialize(Theme theme)
        {
            SeparatorImage.Source = theme.Separator;
            if (theme.Separator != null)
            {
                Width = theme.Separator.Width;
                Height = theme.Separator.Height;
            }
            else
            {
                Width = theme.MinimumIconSize;
                Height = theme.MinimumIconSize;
            }
            MaxWidth = Width;
            MinWidth = Width;
            MinHeight = Height;
            MaxHeight = Height;
            //Margin = theme.Margin;
        }

        void IBaubleButton.LoadFromXml(string Xml)
        {
        }

        string IBaubleButton.Text
        {
            get
            {
                return string.Empty;
            }
            set
            {
            }
        }

        XElement IBaubleButton.ToXml()
        {
            return new XElement("Icon", new XAttribute("type", this.GetType().FullName));
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

        #endregion
    }
}
