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
using Bauble.Common.Events;
using System.Diagnostics;

namespace Bauble
{
    /// <summary>
    /// Interaction logic for IconWindow.xaml
    /// </summary>  
    public partial class IconWindow : Window
    {        
        #region Constuructor
        public IconWindow()
        {
            InitializeComponent();
            _settings = BaubleSettings.Instance; // singleton
            _settings.SettingsChanged += new SettingsChangedRoutedEventHandler(_settings_SettingsChanged);
        }
        #endregion Constuructor

        #region Properties

        private Bauble.Common.Settings.BaubleSettings _settings;
        private IBaubleButton _lastActiveButton = null;
        private IIconFlow _iconFlow { get; set; }
        private Timer _showTimer { get; set; }
        private Timer _hideTimer { get; set; }
        private double separatorScale { get; set; }
        private bool mouseOver { get; set; }
        private System.Drawing.Rectangle ScreenBounds
        {
            get
            {
                return System.Windows.Forms.Screen.AllScreens[_settings.Screen].Bounds;
            }
        }
        
        #endregion Properties

        #region Events
       
        #region Grid Events

        #region DisplayGrid_Loaded
        private void DisplayGrid_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                LoadSettings();
            }
            catch (Exception ex)
            {
            }
        }
        #endregion

        #region DisplayGrid_Drop
        /// <summary>
        /// This event gets called when an item is dropped on the dock
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DisplayGrid_Drop(object sender, System.Windows.DragEventArgs e)
        {
            //TODO : We could make it so you are able to add multiple icons at the same time 
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
            FileInfo fileInfo = new FileInfo(files[0]);
            //Create the image of the Icon
            System.Drawing.Icon icon = System.Drawing.Icon.ExtractAssociatedIcon(files[0]);
            //Create the name that this icon image is going to be stored under
            var newIconImageName = fileInfo.Name.Replace(fileInfo.Extension.ToString(), "") + ".png";
            string startupPath = Environment.CurrentDirectory;
            
            if (!File.Exists(startupPath + "\\icons\\" + newIconImageName))
            {
                icon.ToBitmap().Save(startupPath + "\\icons\\" + newIconImageName);
            }

            // instantiate a new shortcut object temporarily with enough information to generate an XML fragment
            var button = new Bauble.Buttons.Shortcut("icons/" + newIconImageName, fileInfo.Name, files[0]) as IBaubleButton;
            // have the settings object create a fully instantiated button object
            button = _settings.AddShortcut(button);
            // load the button onto the current dock
            DisplayButton(DisplayGrid.ColumnDefinitions.Count + 1, button);
            // Allow the IconFlow to initialize the button for any animations it uses
            _settings.IconFlow.Initialize(button);
            // save the new button list out to the file system
            _settings.SaveButtonList();
        }
        #endregion DisplayGrid_Drop

        #region DisplayGrid_SizeChanged
        /// <summary>
        /// This event gets called when an icon is hover over to allow for the zoom in effect
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DisplayGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if ((_settings != null))
            {
                // sometimes the window width is wrong
                var width = (LeftCap.Source.Width * separatorScale) + (RightCap.Source.Width * separatorScale);
                width += _settings.Buttons.Sum(b => b.Width);
                Left = ScreenBounds.Left + (ScreenBounds.Width / 2) - (width / 2);
            }
        }
        #endregion DisplayGrid_SizeChanged

        #endregion Grid Events

        #region Window Events

        #region Window_MouseMove
        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
            FrameworkElement element = e.Source as FrameworkElement;
            IBaubleButton activeButton = _settings.Buttons.FirstOrDefault(b => b == element);

            if (activeButton == null)
            {
                if (_lastActiveButton != null)
                    activeButton = _lastActiveButton;
            }
            if (activeButton != null)
            {
                bool updated = _iconFlow.MouseMove(e, activeButton, _settings.Buttons);

                if (activeButton != null)
                    UpdateIconText(activeButton);
                if (updated)
                    DisplayGrid_SizeChanged(null, null);

                _lastActiveButton = activeButton;
            }
        }
        #endregion Window_MouseMove

        #region Window_MouseLeave
        private void Window_MouseLeave(object sender, MouseEventArgs e)
        {
            if (!e.Handled)
            {
                var position = e.GetPosition(DisplayGrid);
                Debug.WriteLine("Mouse Leave for {0}  = X: {1}/{3} Y: {2}/{4}",
                     sender.GetType(),
                     position.X, position.Y,
                     DisplayGrid.ActualWidth, DisplayGrid.ActualHeight);
                e.Handled = true;
                mouseOver = false;
                
                if (
                        (position.X < 10) ||
                        (position.X > DisplayGrid.ActualWidth - 10) ||
                        (position.Y < 10) ||
                        (position.Y > DisplayGrid.ActualHeight - 10)
                    )
                {
                    if (_settings.AutoHideDock)
                    {
                        //MouseOverTrap.Background = Brushes.Blue;
                        _hideTimer.Enabled = true;
                        _showTimer.Enabled = false;
                    }
                }
            }
        }
        #endregion Window_MouseLeave

        #region Window_MouseEnter
        private void Window_MouseEnter(object sender, MouseEventArgs e)
        {
            if (_settings.AutoHideDock)
            {
                if (!e.Handled)
                {
                    Debug.WriteLine("Mouse Enter for " + sender.GetType().ToString());
                    mouseOver = true;
                    e.Handled = true;
                    //MouseOverTrap.Background = Brushes.Yellow;
                    _hideTimer.Enabled = false;
                    _showTimer.Enabled = true;
                }
            }
        }
        #endregion Window_MouseEnter

        #region Window_Loaded
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // WPF doesn't provide us with a way to have a transparent window with no chrome, and also have the
            // window not show up in alt-tab. This routine below was found on Stack Overflow on 7/16/2012
            // (http://stackoverflow.com/questions/357076/best-way-to-hide-a-window-from-the-alt-tab-program-switcher)
            // and allows to modify the window style to include WS_EX_TOOLWINDOW - add via an extension method to
            // the Window class in Bauble.Common.Interop.WindowExtensions
            this.HideFromAltTab();
        }
        #endregion Window_Loaded

        #endregion Window Events

        #region Timer Events
        void _showTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            ShowFormAnimated();
        }

        void _hideTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            HideFormAnimated();
        }
        #endregion Timer Events

        #region Settings Events
        void _settings_SettingsChanged(object source, SettingsChangedEventArgs e)
        {
            if ((e.AutoHideDock != null) && (e.AutoHideDock.Value))
                HideFormAnimated();
            else if (e.AutoHideDock != null)
                ShowFormAnimated();

            if (e.NewIconFlow != null)
            {
                SwitchIconFlow(e.NewIconFlow);
            }
            if (e.NewTheme != null)
            {
                SwitchTheme(e.NewTheme);
            }
        }
        #endregion Settings Events

        #region Menu Events
        void ExitMenuItem_Click(object source, RoutedEventArgs e)
        {
            this.Close();
        }

        void ConfigureMenuItem_Click(object source, RoutedEventArgs e)
        {
            BaubleSettings.Instance.ShowConfiguration();
        }
        #endregion

        #endregion Events

        #region Methods

        #region LoadSettings
        private void LoadSettings()
        {
            _showTimer = new Timer(_settings.ShowTimerMilliseconds);
            _showTimer.Elapsed += new ElapsedEventHandler(_showTimer_Elapsed);
            _hideTimer = new Timer(_settings.HideTimerMilliseconds);
            _hideTimer.Elapsed += new ElapsedEventHandler(_hideTimer_Elapsed);

            SwitchIconFlow(_settings.IconFlow);

            if (System.Windows.Forms.Screen.AllScreens.Length <= _settings.Screen)
            {
                _settings.Screen = 0;
            }

            System.Drawing.Rectangle screenRect = System.Windows.Forms.Screen.AllScreens[_settings.Screen].Bounds;
            Top = screenRect.Bottom - this.Height;

            SwitchTheme(_settings.Theme);

            DisplayButtons();

            if (_settings.AutoHideDock)
            {
                _hideTimer.Enabled = true;
            }
        }
        #endregion

        #region DisplayButtons
        private void DisplayButtons()
        {
            int buttonNumber = 1;
            
            foreach (IBaubleButton button in _settings.Buttons)
            {
                DisplayButton(buttonNumber, button);
                buttonNumber++;
            }
        }
        #endregion

        #region DisplayButton
        private void DisplayButton(int buttonNumber, IBaubleButton button)
        {
            UserControl fe = button as UserControl;
            if (fe is Bauble.Buttons.Shortcut)
                fe.DataContext = (button as Bauble.Buttons.Shortcut).ExecutablePath;
            else if (fe is Bauble.Buttons.Separator)
            {
                fe.MaxWidth = fe.MinWidth = fe.Width = fe.Width * separatorScale;
                fe.MaxHeight = fe.MinHeight = fe.Height = fe.Height * separatorScale;
            }

            DropShadowEffect dse = new DropShadowEffect();
            dse.Color = Colors.Black;
            dse.BlurRadius = 10;
            dse.Opacity = 0.25;
            dse.Direction = 270;
            fe.Effect = dse;


            if (buttonNumber > DisplayGrid.ColumnDefinitions.Count)
            {
                ColumnDefinition column = new ColumnDefinition();
                DisplayGrid.ColumnDefinitions.Add(column);
                UpdateDisplayGridColumn(button, column);
            }

            fe.VerticalAlignment = _settings.Theme.VerticalAlignment;

            // add an almost transparent shape to our dock
            // to capture mouse events even when the icon has mostly transparent area
            Canvas alphaCanvas = new Canvas();
            var rect = new System.Windows.Shapes.Rectangle();
            rect.Fill = new SolidColorBrush(Color.FromArgb(1,1,1,1));
            rect.Stroke = rect.Fill;
            rect.StrokeThickness = 1;
            alphaCanvas.Width = rect.Width = fe.Width;
            alphaCanvas.Height = rect.Height = fe.Height;
            alphaCanvas.Children.Add(rect);
            //alphaCanvas.Margin = _settings.Theme.Margin;
            Grid.SetColumn(alphaCanvas, DisplayGrid.ColumnDefinitions.Count - 1);
            Grid.SetRow(alphaCanvas, 0);
            DisplayGrid.Children.Add(alphaCanvas);

            //fe.SnapsToDevicePixels = true;
            Grid.SetColumn(fe, DisplayGrid.ColumnDefinitions.Count - 1);
            Grid.SetRow(fe, 0);

            DisplayGrid.Children.Add(fe);
            //ButtonImages.Add(button, button);
        }
        #endregion DisplayButton

        #region UpdateDisplayGridColumn
        private void UpdateDisplayGridColumn(IBaubleButton button, ColumnDefinition column)
        {
            column.MinWidth = 
                _settings.Theme.MinimumIconSize > button.MinWidth ? button.MinWidth : 
                _settings.Theme.MinimumIconSize;
            column.MaxWidth = _settings.Theme.MaximumIconSize > button.MaxWidth ? button.MaxWidth : _settings.Theme.MaximumIconSize;
            Grid.SetColumnSpan(IconTextCanvas, DisplayGrid.ColumnDefinitions.Count);
        }
        #endregion UpdateDisplayGridColumn

        private void button_Activate()
        {
            MessageBox.Show("Animate!");
        }

        #region SwitchIconFlow
        private void SwitchIconFlow(IIconFlow iconFlow)
        {
            if (this._iconFlow != null)
            {
                this._iconFlow.Uninitialize(_settings.Buttons);
            }
            this._iconFlow = iconFlow;
            this._iconFlow.Initialize(_settings.Buttons);
        }
        #endregion

        #region SwitchTheme
        private void SwitchTheme(Bauble.Common.Themes.Theme theme)
        {
            _settings.Theme = theme;
            //this.Background = theme.BackgroundBrush;

            if (theme.LeftCap != null)
                this.LeftCap.Source = theme.LeftCap;
            //if (theme.Middle != null)
            //    this.Middle.Source = theme.Middle;
            if (theme.RightCap != null)
                this.RightCap.Source = theme.RightCap;


            ImageBrush background = theme.Middle != null ? new ImageBrush(theme.Middle) : null;
            DisplayGrid.Background = background;
            DisplayGrid.Height = theme.MaximumIconSize + theme.Margin.Top + theme.Margin.Bottom;
            //DisplayGrid.Height = theme.MaximumIconSize;
            LeftCap.Height = DisplayGrid.Height;
            RightCap.Height = DisplayGrid.Height;

            separatorScale = RightCap.Height / theme.RightCap.Height;

            this.Height = DisplayGrid.Height;
            double top = ScreenBounds.Bottom - this.Height;
            Top = top;

            for (int i = 0; i < DisplayGrid.ColumnDefinitions.Count; i++)
            {
                var fe = (_settings.Buttons[i] as FrameworkElement);
                if (fe.Parent != null)
                {
                    _settings.Buttons[i].Initialize(theme);
                    if (fe is Bauble.Buttons.Separator)
                    {
                        fe.MaxWidth = fe.MinWidth = fe.Width = fe.Width * separatorScale;
                        fe.MaxHeight = fe.MinHeight = fe.Height = fe.Height * separatorScale;
                        DisplayGrid.ColumnDefinitions[i].Width = new GridLength(fe.Width);
                    }
                    UpdateDisplayGridColumn(_settings.Buttons[i], DisplayGrid.ColumnDefinitions[i]);
                }
            }

            DisplayGrid_SizeChanged(DisplayGrid, null);

            if (_settings.AutoHideDock)
                HideFormAnimated();
            else
                ShowFormAnimated();
        }
        #endregion SwitchTheme

        #region UpdateIconText
        private void UpdateIconText(IBaubleButton button)
        {
            if (button != null)
            {
                if (IconTextBlock.Text != button.Text)
                {
                    IconTextBlock.Inlines.Clear();
                    IconTextBlock.Inlines.Add(new Bold(new Run(button.Text)));
                }

                Size available = new Size(double.PositiveInfinity, double.PositiveInfinity);
                IconTextBlock.Measure(available);
                double textWidth = IconTextBlock.DesiredSize.Width;
                double textMidpoint = textWidth / 2;
                GeneralTransform transform = (button as FrameworkElement).TransformToAncestor(DisplayGrid);
                Point rootPoint = transform.Transform(new Point(0, 0));

                double imageLeft = rootPoint.X;
                double imageMidpoint = imageLeft + (button.Width / 2);

                double textLeft = (imageMidpoint - textMidpoint);

                if (textLeft < 0) textLeft = 0;
                Canvas.SetLeft(IconTextBlock, textLeft);
            }
        }
        #endregion UpdateIconText

        #region Animation Functions
        #region ShowFormAnimated
        private void ShowFormAnimated()
        {
            if (this.Dispatcher.Thread != System.Threading.Thread.CurrentThread)
            {
                _showTimer.Enabled = false;
                this.Dispatcher.BeginInvoke(
                    System.Windows.Threading.DispatcherPriority.Normal,
                    new Action(ShowFormAnimated)
                );
            }
            else
            {
                this.Focus();
                System.Drawing.Rectangle screenRect = System.Windows.Forms.Screen.AllScreens[_settings.Screen].Bounds;
                double animateTo = screenRect.Bottom - DisplayGrid.Height;
                AnimateWindowMovement(animateTo, _settings.ShowAnimationDuration);
            }
        }
        #endregion ShowFormAnimated

        #region HideFormAnimated
        private void HideFormAnimated()
        {
            if (this.Dispatcher.Thread != System.Threading.Thread.CurrentThread)
            {
                _hideTimer.Enabled = false;
                this.Dispatcher.BeginInvoke(
                    System.Windows.Threading.DispatcherPriority.Normal,
                    new Action(HideFormAnimated)
                );

            }
            else
            {
                System.Drawing.Rectangle screenRect = System.Windows.Forms.Screen.AllScreens[_settings.Screen].Bounds;
                double animateTo = (screenRect.Top + screenRect.Height) - _settings.Theme.AutoHideAdjustment;

                AnimateWindowMovement(animateTo, _settings.HideAnimationDuration);
            }
        }
        #endregion HideFormAnimated

        #region AnimateWindowMovement
        private void AnimateWindowMovement(double moveTo, double animationDuration)
        {
            Storyboard storyboard = Bauble.Common.Animation.Animator.CreateStoryboard(EasingMode.EaseIn, moveTo, animationDuration, new string[] { "Top" });

            this.BeginStoryboard(storyboard);
        }
        #endregion

        private void DisplayGrid_MouseLeave(object sender, MouseEventArgs e)
        {
            var position = e.GetPosition(DisplayGrid);
            Debug.WriteLine("Mouse Leave for {0}  = X: {1}/{3} Y: {2}/{4}",
                 sender.GetType(),
                 position.X, position.Y,
                 DisplayGrid.ActualWidth, DisplayGrid.ActualHeight);
        }
        #endregion

        #endregion Methods
    }
}
        
      
        
		
    

