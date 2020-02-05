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
using Bauble.Common.Interfaces;
using Bauble.Common.Themes;

namespace Bauble.Common.Events
{
    public delegate void SettingsChangedRoutedEventHandler(object source, SettingsChangedEventArgs e);
    public class SettingsChangedEventArgs : RoutedEventArgs
    {
        public bool? AutoHideDock { get; set; }
        public double? HideAnimationDuration { get; set; }
        public double? HideTimerMilliseconds { get; set; }
        public double? ShowAnimationDuration { get; set; }
        public double? ShowTimerMilliseconds { get; set; }
        public int? Screen { get; set; }
        public IIconFlow OldIconFlow { get; set; }
        public IIconFlow NewIconFlow { get; set; }
        public Theme OldTheme { get; set; }
        public Theme NewTheme { get; set; }
        public SettingsChangedEventArgs(RoutedEvent routedEvent, object source)
            : base(routedEvent, source)
        {
        }

        public bool ShouldRaiseEvent
        {
            get
            {
                return AutoHideDock != null ||
                    HideAnimationDuration != null ||
                    HideTimerMilliseconds != null ||
                    ShowAnimationDuration != null ||
                    ShowTimerMilliseconds != null ||
                    Screen != null ||
                    NewIconFlow != null ||
                    NewTheme != null;

            }
        }
        
    }
}
