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
    public partial class Shortcut : UserControl, IBaubleButton
    {
        #region Constructors

        public Shortcut()
        {
            InitializeComponent();
            ShortcutButton.Click += new RoutedEventHandler(ShortcutButton_Click);
        }

        public Shortcut(string imageFilePath, string text, string executablePath, string workingDirectory = null, string arguments = null)
        {
            _imageFilePath = imageFilePath;
            Text = text;
            _executablePath = executablePath;
            _workingDirectory = workingDirectory;
            _arguments = arguments;
            
            InitializeComponent();
        }

        #endregion Constructors

        #region Properties

        private BitmapImage _bitmapImage;
        private string _workingDirectory;
        private string _arguments;
        private string _imageFilePath;
        private string _executablePath;
        private RoutedEventHandler _activated;
        
        public string Text { get; set; }

        /// <summary>
        /// This Property is used as a DataSource, so that when you click on a shortcut the dock knows which app to launch
        /// </summary>
        public string ExecutablePath
        {
            get { return _executablePath; }

            set
            {
                if (_executablePath != value)
                {
                    _executablePath = value;
                }
            }
        }
        
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
       
        protected BitmapImage LoadBitmapImageFromFile(string FileName)
        {
            BitmapImage image = null;
            try
            {
                image = new BitmapImage(new Uri(System.IO.Path.GetFullPath(FileName)));
            }
            catch { }

            return image;
        }

        void IBaubleButton.Initialize(Theme theme)
        {
            // Initialize some default values here
            Width = theme.MinimumIconSize;
            Height = theme.MinimumIconSize;
            MinWidth = theme.MinimumIconSize;
            MaxWidth = theme.MaximumIconSize;
            MinHeight = theme.MinimumIconSize;
            MaxHeight = theme.MaximumIconSize;

            if (!string.IsNullOrEmpty(_imageFilePath))
                _bitmapImage = LoadBitmapImageFromFile(_imageFilePath);

            if (_bitmapImage != null)
            {
                //Image image = Image as Image;
                ShortcutImage.Source = _bitmapImage;
            }
            Margin = theme.Margin;
        }

        #region Persistence

        /// <summary>
        /// Converts the Shortcut to xml and stores it in the xml file.
        /// </summary>
        public XElement ToXml()
        {
            return new XElement("Icon", new XAttribute("type", this.GetType()),
                        new XElement("Image", _imageFilePath),
                        new XElement("Text", Text),
                        new XElement("Executable", ExecutablePath),
                        new XElement("WorkingDirectory", _workingDirectory),
                        new XElement("Arguments", _arguments));
        }

        Object IBaubleButton.ToConfigurationObject()
        {
            return new { 
                Type = this.GetType().FullName,
                Image = _imageFilePath,
                Text = Text,
                Executable = ExecutablePath,
                WorkingDirectory = _workingDirectory,
                Arguments = _arguments
            };
        }

        void IBaubleButton.LoadFromXml(string Xml)
        {
            XElement element = XElement.Parse(Xml);
            
            _imageFilePath = element.Element("Image").Value;
            Text = element.Element("Text").Value;
            ExecutablePath = element.Element("Executable").Value;
            _workingDirectory = element.Element("WorkingDirectory").Value;
            _arguments = element.Element("Arguments").Value;
        }

        void IBaubleButton.LoadFromJson(System.Text.Json.JsonElement json)
        {
            _imageFilePath = json.GetProperty("image").GetString();
            Text = json.GetProperty("text").GetString();
            ExecutablePath = json.GetProperty("executable").GetString();
            _workingDirectory = json.GetProperty("workingDirectory").GetString();
            _arguments = json.GetProperty("arguments").GetString();

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

            // Question: pvpatarinski - Why do we need this instead of using this.ExecutablePath? That would also avoid
            // needing to add ExecutablePath to the IBaubleButton interface, as all buttons will not need to expose an ExecutablePath...
            string fileName = (e.Source as System.Windows.Controls.Button).DataContext.ToString();

            if (!String.IsNullOrEmpty(fileName))
            {
                Process application = new Process();
                // set the executable path read from the configuration, and expand any environment variables
                application.StartInfo.FileName = Environment.ExpandEnvironmentVariables(fileName);
                // set the working directory read from the configuration, and expand any environment variables
                application.StartInfo.WorkingDirectory = Environment.ExpandEnvironmentVariables(this._workingDirectory);
                // set any arguments read from the configuration file
                application.StartInfo.Arguments = this._arguments;

                try
                {
                    application.Start();
                }
                catch
                {
                    MessageBox.Show("The icon you have clicked on does not have a valid execution path.");
                }
            }

        }
        


        #endregion Events                               
        
		}
    }

