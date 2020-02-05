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
using System.Windows.Input;

namespace Bauble.Common.Animation
{
    public class Animator
    {
        public static void ClearButtonDoubleAnimations(FrameworkElement control, params RoutedEvent[] events)
        {
            for (int i = control.Triggers.Count - 1; i >= 0; i--)
            {
                TriggerBase trigger = control.Triggers[i];
                if (trigger is EventTrigger)
                {
                    EventTrigger eventTrigger = trigger as EventTrigger;
                    if (events.Contains(eventTrigger.RoutedEvent))
                    {
                        control.Triggers.Remove(trigger);
                    }
                }
                
            }
        }
        public static void SetButtonDoubleAnimation(FrameworkElement control, EasingMode easingMode, RoutedEvent mouseEvent, double animateTo, string[] properties)
        {
            EventTrigger trigger = new EventTrigger(mouseEvent);
            control.Triggers.Add(trigger);

            BeginStoryboard beginStoryBoard = new BeginStoryboard();
            trigger.Actions.Add(beginStoryBoard);

            Storyboard storyboard = new Storyboard();
            beginStoryBoard.Storyboard = storyboard;

            foreach (string property in properties)
            {
                DoubleAnimation da = CreateExponentialAnimation(easingMode, animateTo, 250, storyboard, false);
                storyboard.Children.Add(da);
                Storyboard.SetTargetProperty(da, new PropertyPath(property));
            }
        }

        private static DoubleAnimation CreateExponentialAnimation(EasingMode easingMode, double animateTo, double animationDuration, Storyboard enterStoryboard, bool autoReverse)
        {
            DoubleAnimation animation = new DoubleAnimation(animateTo, TimeSpan.FromMilliseconds(animationDuration));
            animation.AutoReverse = autoReverse;

            //ElasticEase easeIn = new ElasticEase();
            var easeIn = new System.Windows.Media.Animation.ExponentialEase();
            easeIn.EasingMode = easingMode;
            // ElasticEase
            //easeIn.Oscillations = 1;
            // BounceEase
            //easeIn.Bounciness = 10;
            //easeIn.Bounces = 1;
            // ExponentialEase
            easeIn.Exponent = 4;


            animation.EasingFunction = easeIn;
            return animation;
        }

        public static Storyboard CreateStoryboard(EasingMode easingMode, double animateTo, double animationDuration, string[] properties)
        {
            Storyboard storyboard = new Storyboard();

            foreach (string property in properties)
            {
                DoubleAnimation da = CreateExponentialAnimation(easingMode, animateTo, animationDuration, storyboard, false);
                storyboard.Children.Add(da);
                Storyboard.SetTargetProperty(da, new PropertyPath(property));
            }
            return storyboard;
        }
    }
}
