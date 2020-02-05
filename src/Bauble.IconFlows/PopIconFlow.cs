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

using System.Windows.Media.Animation;
using System.Windows.Media;
using Bauble.Common.Interfaces;
using System.Windows.Input;
using Bauble.Common.Animation;

namespace Bauble.IconFlows
{
    /// <summary>
    /// PopIconFlow will enlarge the icon that the mouse is currently over.
    /// </summary>
    public class PopIconFlow : IIconFlow
    {
        #region IIconFlow Members
        public string Name { get { return "Pop Icon Flow"; } set { } }

        public float AreaOfEffect
        {
            get
            {
                return 1F;
            }
            set
            {
            }
        }

        public float AreaOfEffectMinumum
        {
            get { return 1F; }
        }

        public float AreaOfEffectMaximum
        {
            get { return 1F; }
        }

        public float AreaOfEffectIncrement
        {
            get { return 1F; }
        }
        public string TypeName
        {
            get { return this.GetType().FullName; }
            set { }
        }

        public Boolean MouseMove(MouseEventArgs mouse, IBaubleButton activeButton, List<IBaubleButton> buttons)
        {
            return false;
        }

        public bool Configurable { get { return false; } }
        public void Configure() { throw new NotImplementedException("Pop Icon Flow Is Not Configurable"); }

        public void Initialize(List<IBaubleButton> buttons)
        {
            foreach (var button in buttons)
            {
                Initialize(button);
            }
        }
        public void Initialize(IBaubleButton button)
        {
            var properties = new string[] { "Width", "Height" };
            Animator.SetButtonDoubleAnimation(button as FrameworkElement, EasingMode.EaseIn, FrameworkElement.MouseEnterEvent, button.MaxWidth, properties);
            Animator.SetButtonDoubleAnimation(button as FrameworkElement, EasingMode.EaseOut, FrameworkElement.MouseLeaveEvent, button.MinWidth, properties);
        }

        public void Uninitialize(List<IBaubleButton> buttons)
        {
            // remove any existing animations from the button
            buttons.ForEach(b =>
            {
                Animator.ClearButtonDoubleAnimations(b as FrameworkElement, FrameworkElement.MouseEnterEvent, FrameworkElement.MouseLeaveEvent);
            });
        }

        #endregion
    }
}
