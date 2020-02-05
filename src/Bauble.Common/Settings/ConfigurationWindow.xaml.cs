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
using System.Collections.ObjectModel;
using System.IO;
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
using System.Xml.Linq;
using Bauble.Common.Interfaces;
using Bauble.Common.Themes;
using System.Timers;
using System.Windows.Media.Animation;
using Bauble.Common.Interop;
using Bauble.Common.Settings;

namespace Bauble.Common.Settings
{
    /// <summary>
    /// Interaction logic for IconWindow.xaml
    /// </summary>  
    public partial class ConfigurationWindow : Window
    {        
        #region Constuructor

        public ConfigurationWindow()
        {
            InitializeComponent();
            _settings = BaubleSettings.Instance; // singleton
        }

        #endregion Constuructor

        #region Properties

        private Bauble.Common.Settings.BaubleSettings _settings;

        #endregion Properties

        #region Events
       
        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.Save();
            this.DialogResult = true;
            this.Hide();
            // theme and icon flow changing are handled in the BaubleSettings object that showed this dialog
        }
        
        #endregion Events

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.Reload();
        }
    }
}
        
      
        
		
    

