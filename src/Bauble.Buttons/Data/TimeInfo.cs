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
using System.Xml.Linq;

namespace Bauble.Buttons.Data
{
    public sealed class TimeInfo
    {
        public TimeInfo(TimeZoneInfo tz)
        {
            _timezone = tz;
        }

        public static string Write(TimeInfo ti)
        {
            XDocument xDoc = new XDocument(
                                     new XElement("TimeInfo",
                                         new XAttribute("TimeZoneInfo", ti._timezone.ToSerializedString()),
                                         new XAttribute("DisplayName", ti._displayName ?? string.Empty)));

            return xDoc.ToString();
        }

        public static TimeInfo Read(string s)
        {
            try
            {
                XDocument xDoc = XDocument.Parse(s);

                TimeZoneInfo tzi = TimeZoneInfo.FromSerializedString(xDoc.Element("TimeInfo").Attribute("TimeZoneInfo").Value);

                TimeInfo ti = new TimeInfo(tzi);

                string displayName = xDoc.Element("TimeInfo").Attribute("DisplayName").Value.Trim();

                if (displayName != string.Empty)
                {
                    ti.DisplayName = displayName;
                }

                return ti;
            }
            catch
            {
                return null;
            }
        }

        public static TimeInfo Copy(TimeInfo ti)
        {
            TimeInfo copy = new TimeInfo(ti.TimeZoneInfo);

            if (ti.IsDisplayNameOverridden)
            {
                copy.DisplayName = ti.DisplayName;
            }

            return copy;
        }

        public bool IsDisplayNameOverridden
        {
            get
            {
                return _displayName != null;
            }
        }

        public string OffsetName
        {
            get
            {
                if (_timezone.BaseUtcOffset == TimeSpan.Zero)
                {
                    return "GMT";
                }
                else if (_timezone.BaseUtcOffset.Hours < 0)
                {
                    return string.Format("GMT-{0}:{1:00}", -_timezone.BaseUtcOffset.Hours, -_timezone.BaseUtcOffset.Minutes);
                }
                else
                {
                    return string.Format("GMT+{0}:{1:00}", _timezone.BaseUtcOffset.Hours, _timezone.BaseUtcOffset.Minutes);
                }
            }
        }

        public string DisplayName
        {
            get
            {
                return _displayName ?? _timezone.DisplayName;
            }
            set
            {
                if (value == _timezone.DisplayName)
                {
                    _displayName = null;
                }
                else
                {
                    _displayName = value;
                }
            }
        }

        public TimeZoneInfo TimeZoneInfo
        {
            get
            {
                return _timezone;
            }
        }

        public DateTime GetAdjusted(DateTime start)
        {
            return start.Add(_timezone.GetUtcOffset(start));
        }

        private TimeZoneInfo _timezone;
        private string _displayName;
    }
}
