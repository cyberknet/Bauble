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
using Bauble.Common.Themes;
using System.Xml.Linq;

namespace Bauble.Common.Interfaces
{
    public interface IBaubleButton
    {        
        bool Configurable { get; }
        string Text { get; set; }
        double Width { get; set; }
        double Height { get; set; }
        double MinWidth { get; set; }
        double MaxWidth { get; set; }
        double MinHeight { get; set; }
        double MaxHeight { get; set; }
        event RoutedEventHandler Activated;
        
        void Configure();
        void Initialize(Theme theme);
        void LoadFromXml(string Xml);
        XElement ToXml();
        
    }
}
