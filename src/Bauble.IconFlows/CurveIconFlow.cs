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
using System.Windows.Input;
using System.Windows;

using System.Windows.Media;
using Bauble.Common.Interfaces;


namespace Bauble.IconFlows
{
    public class CurveIconFlow : IIconFlow
    {
        // number of icons
        public float AreaOfEffect { get; set; }
        public string TypeName
        {
            get { return this.GetType().FullName; }
            set { }
        }

        public CurveIconFlow()
        {
            AreaOfEffect = 3;
        }

        public string Name { get { return "Curve Icon Flow"; } set { } }

        public bool MouseMove(MouseEventArgs mouse, IBaubleButton activeButton, List<IBaubleButton> buttons)
        {
            bool updated = false;

            foreach (var b in buttons)
            {
                var bUpdated = false;
                var spread = b.MinWidth * AreaOfEffect;
                var m = mouse.GetPosition(b as FrameworkElement);
                var x = m.X - (b.Width / 2);

                if (Math.Abs(x) >= spread)
                {
                    if ((b.Width > b.MinWidth) || (b.Height > b.MinHeight))
                    {
                        b.Width = b.MinHeight;
                        b.Height = b.MinHeight;
                        bUpdated = true;
                    }
                }
                else
                {
                    var percentageOfSpread = (spread - Math.Abs(x)) / spread;
                    b.Width = b.MinWidth + ((b.MaxWidth - b.MinWidth) * percentageOfSpread);
                    b.Height = b.MinHeight + ((b.MaxHeight - b.MinHeight) * percentageOfSpread);
                    bUpdated = true;
                }

                if (bUpdated)
                {
                    b.Width = b.Width;
                    b.Height = b.Height;
                    updated = true;
                }
            }
            return updated;
        }

        #region GetAffectedButtons(BaubleButton activeButton, List<IBaubleButton> buttons, double deltaPercent)
        private List<IBaubleButton> GetAffectedButtons(IBaubleButton activeButton, List<IBaubleButton> buttons, double positionRelativeToButtonCenter)
        {
            // get the list of icons this affects
            int iconsBefore = (int)Math.Ceiling((float)AreaOfEffect / 2F);
            int iconsAfter = (int)Math.Ceiling((float)AreaOfEffect / 2F);

            if ((iconsBefore + iconsAfter) > AreaOfEffect)
            {
                
                if (positionRelativeToButtonCenter < 0)
                    // if mouse is before center of icon
                    iconsAfter--;
                else
                    // else mouse is after center of icon
                    iconsBefore--;
            }
            else
            {
                if (positionRelativeToButtonCenter < 0)
                {
                    // if mouse is before center of icon
                    iconsAfter--;
                    iconsBefore++;
                }
                else
                {
                    // else mouse is after center of icon
                    iconsBefore++;
                    iconsAfter--;
                }
            }

            
            List<IBaubleButton> affectedButtons = new List<IBaubleButton>();
           
            int activeButtonIndex = buttons.IndexOf(activeButton);
            int buttonFrom = activeButtonIndex - iconsBefore;
            if (buttonFrom < 0) buttonFrom = 0;
            int buttonTo = activeButtonIndex + iconsAfter;
            if (buttonTo >= buttons.Count) buttonTo = buttons.Count - 1;

            for (int currentButton = buttonFrom; currentButton < activeButtonIndex; currentButton++)
                affectedButtons.Add(buttons[currentButton]);
            affectedButtons.Add(activeButton);
            for (int currentButton = activeButtonIndex + 1; currentButton <= buttonTo; currentButton++)
                affectedButtons.Add(buttons[currentButton]);
            return affectedButtons;
        }
        #endregion

        public float AreaOfEffectMinumum
        {
            get { return 1; }
        }

        public float AreaOfEffectMaximum
        {
            get { return 3; }
        }

        public float AreaOfEffectIncrement
        {
            get { return 0.25F; }
        }

        public bool Configurable
        {
            get
            {
                return true;
            }
        }

        public void Initialize(List<IBaubleButton> buttons)
        {
        }
        public void Initialize(IBaubleButton button)
        {
        }

        public void Uninitialize(List<IBaubleButton> buttons)
        {
        }

        public void Configure()
        {
            MessageBox.Show("Configure!");
        }
    }
}
