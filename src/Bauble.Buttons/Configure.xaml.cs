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
using System.Diagnostics;
using System.Drawing;
using System.IO;
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
using System.Xml.Linq;
using Bauble.Common.Events;
using Bauble.Common.Interfaces;
using Bauble.Common.Themes;
using System.Xml;

namespace Bauble.Buttons
{
    /// <summary>
    /// Interaction logic for Shortcut.xaml
    /// </summary>
    public partial class Configure : UserControl, IBaubleButton
    {
        #region Constructors

        public Configure()
        {
            InitializeComponent();
            ShortcutButton.Click += new RoutedEventHandler(ShortcutButton_Click);
        }

        public Configure(string text) : this()
        {
            Text = text;
        }

        #endregion Constructors

        #region Properties
       private RoutedEventHandler _activated;
        
        public string Text { get; set; }

        event RoutedEventHandler IBaubleButton.Activated
        {
            add { _activated += value; }
            remove { _activated -= value; }
        }
        
        bool IBaubleButton.Configurable { get { return true; } }
        
        #endregion Properties

        #region Methods

        protected void OnActivated()
        {
            if (_activated != null)
                _activated(this, new RoutedEventArgs(SharedEvents.ActivatedEvent, this));
        }
       
        void IBaubleButton.Initialize(Theme theme)
        {
            ConfigureImage.Source = theme.Configure;
            if (theme != null)
            {
                Width = theme.MinimumIconSize;
                Height = theme.MinimumIconSize;
                MinWidth = theme.MinimumIconSize;
                MaxWidth = theme.MaximumIconSize;
                MinHeight = theme.MinimumIconSize;
                MaxHeight = theme.MaximumIconSize;
                Margin = theme.Margin;
            }
        }

        #region Persistence

        /// <summary>
        /// Converts the Shortcut to xml and stores it in the xml file.
        /// </summary>
        public XElement ToXml()
        {
            return
                new XElement("Icon", 
                    // attributes
                    new XAttribute("type", this.GetType()),
                    // child elements
                    new XElement("Text", Text));
        }

        Object IBaubleButton.ToConfigurationObject()
        {
            return new
            {
                Type = this.GetType().FullName,
                Text = Text
            };
        }

        void IBaubleButton.LoadFromXml(string Xml)
        {
            XElement element = XElement.Parse(Xml);
            Text = element.Element("Text").Value;
        }

        void IBaubleButton.LoadFromJson(System.Text.Json.JsonElement json)
        {
            Text = json.GetProperty("text").GetString();
        }

        #endregion Persistence

        #region Configuration

        void IBaubleButton.Configure()
        {
            MessageBox.Show("Configure me!");
        }

        #endregion

        #endregion Methods

        #region Events

        /// <summary>
        /// Handles the event of clicking on a shortcut
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void ShortcutButton_Click(object sender, RoutedEventArgs e)
        {
            OnActivated();

            // do stuff
            Bauble.Common.Settings.BaubleSettings.Instance.ShowConfiguration();
        }
        


        #endregion Events                               
        
		}
    }

