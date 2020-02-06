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
using System.Windows.Media.Imaging;
using Bauble.Common.Themes;
using System.Collections.ObjectModel;
using System.Xml.Linq;
using Bauble.Common.Interfaces;
using Bauble.Common.Animation;
using Bauble.Common.Events;
using System.Reflection;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Bauble.Common.Properties;

namespace Bauble.Common.Settings
{
    public class BaubleSettings
    {

        #region Singleton
        private static BaubleSettings _instance = null;
        public static BaubleSettings Instance { get { if (_instance == null) _instance = new BaubleSettings(); return _instance; } }
        #endregion

        #region Private Constructor

        #region BaubleSettings()
        private BaubleSettings()
        {
            // check if the current settings need to be upgraded
            if (Properties.Settings.Default.UpgradeSettings)
            {
                // upgrade the settings
                Properties.Settings.Default.Upgrade();
                // mark the current settings as not needing to be upgraded
                Properties.Settings.Default.UpgradeSettings = false;
                // save the new settings
                Properties.Settings.Default.Save();
            }

            LoadAddIns();
            LoadThemes();
            //LoadButtonsFromXml();
            LoadButtonsFromJson();

            IconFlows = new List<IIconFlow>();
            foreach (var iconFlow in IconFlowAddIns)
            {
                // get an instance of the icon flow
                var current = GetIconFlow(iconFlow);
                // add it to the list
                IconFlows.Add(current);
                // if it is the selected icon flow, set the theme property
                if (current.TypeName == Properties.Settings.Default.IconFlow)
                    IconFlow = current;
            }

            if (IconFlow == null)
            {
                IconFlow = IconFlows[0];
                Properties.Settings.Default.IconFlow = IconFlow.TypeName;
                Properties.Settings.Default.Save();
            }
        }
        #endregion BaubleSettings()

        #endregion Private Constructors

        #region Properties
        public IIconFlow IconFlow { get; set; }

        public List<Theme> Themes { get; set; }
        public Theme Theme { get; set; }

        public List<IBaubleButton> Buttons;

        public List<IIconFlow> IconFlows { get; set; }
        public List<Type> IconFlowAddIns;
        public List<Type> ButtonAddIns;


        // extend the following (internal) settings so that other assemblies can read/write them
        // if you know a way to make VS generate the settings object public I would love to get
        // rid of this code!
        #region Extender Properties
        public int Screen
        {
            get { return Properties.Settings.Default.Screen; }
            set { Properties.Settings.Default.Screen = value; }
        }
        public string ThemeFile
        {
            get { return Properties.Settings.Default.ThemeFile.ToLower().Trim(); }
            set { Properties.Settings.Default.ThemeFile = value.ToLower().Trim(); }
        }
        public string IconFlowTypeName
        {
            get { return Properties.Settings.Default.IconFlow; }
            set { Properties.Settings.Default.IconFlow = value; }
        }
        public double HideTimerMilliseconds 
        { 
            get { return Properties.Settings.Default.HideTimerMilliseconds; } 
            set { Properties.Settings.Default.HideTimerMilliseconds = value;  } 
        }
        public double ShowTimerMilliseconds 
        {
            get { return Properties.Settings.Default.ShowTimerMilliseconds; }
            set { Properties.Settings.Default.ShowTimerMilliseconds = value; } 
        }
        public double HideAnimationDuration 
        {
            get { return Properties.Settings.Default.HideAnimationDuration; }
            set { Properties.Settings.Default.HideAnimationDuration = value; } 
        }
        public double ShowAnimationDuration 
        {
            get { return Properties.Settings.Default.ShowAnimationDuration; }
            set { Properties.Settings.Default.ShowAnimationDuration = value; } 
        }

        public bool AutoHideDock
        {
            get { return Properties.Settings.Default.AutoHideDock; }
            set { Properties.Settings.Default.AutoHideDock = value; }
        }
        #endregion

        #endregion Properties

        #region Public Methods

        #region SaveButtonList
        public void SaveButtonList()
        {

            //XElement[] buttonList = Buttons.Select(b => b.ToXml()).ToArray();
            //XDocument doc = new XDocument(
            //    new XElement("buttons", buttonList)
            //    );
            //doc.Save("icons.xml");
            object[] buttons = Buttons.Select(b => b.ToConfigurationObject()).ToArray();
            var config = new { Icons = buttons };
            var json = JsonSerializer.Serialize(
                config, 
                new JsonSerializerOptions() { 
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase, 
                    WriteIndented = true
                }
            );
            System.IO.File.WriteAllText("icons.json", json);
        }
        #endregion SaveButtonList

        #region AddButton
        public IBaubleButton AddShortcut(IBaubleButton button)
        {
            var element = button.ToXml();
            return LoadButton(element);
        }
        #endregion

        #region ShowConfiguration
        public void ShowConfiguration()
        {
            #region LeftHand Variables
            bool? autoHideDock = AutoHideDock;
            double? hideAnimationDuration = this.HideAnimationDuration;
            double? hideTimerMilliseconds = this.HideTimerMilliseconds;
            double? showAnimationDuration = this.ShowAnimationDuration;
            double? showTimerMilliseconds = this.ShowTimerMilliseconds;
            int? screen = this.Screen;
            Theme changeToTheme = null;
            IIconFlow changeToIconFlow = null;
            #endregion

            ConfigurationWindow configWindow = new ConfigurationWindow();

            // save the current icon flow and theme before the window is shown
            bool? result = configWindow.ShowDialog();
            if (result.HasValue && result.Value)
            {
                #region Right Hand Checks
                // check and see if the current Icon Flow matches the type name
                // of the icon flow set in the settings
                var selectedIconFlow = (configWindow.IconFlowComboBox.SelectedItem as IIconFlow);
                if (selectedIconFlow != IconFlow)
                {
                    // if not, get a reference to the new icon flow object 
                    changeToIconFlow = selectedIconFlow;
                }

                var selectedTheme = (configWindow.ThemeComboBox.SelectedItem as Theme);

                // check and see if the current theme matches the theme filename
                // of the theme set in the settings
                if (selectedTheme != Theme)
                {
                    // if not, get a reference to the new theme object
                    changeToTheme = selectedTheme;
                }
                if (autoHideDock.Value == AutoHideDock)
                    autoHideDock = null;
                else
                    autoHideDock = AutoHideDock;
                
                if (hideAnimationDuration.Value == HideAnimationDuration)
                    hideAnimationDuration = null;
                else 
                    hideAnimationDuration = HideAnimationDuration;

                if (hideTimerMilliseconds.Value == HideTimerMilliseconds)
                    hideTimerMilliseconds = null;
                else
                    hideTimerMilliseconds = HideAnimationDuration;

                if (showAnimationDuration == ShowAnimationDuration)
                    showAnimationDuration = null;
                else
                    showAnimationDuration = ShowAnimationDuration;

                if (showTimerMilliseconds == ShowTimerMilliseconds)
                    showTimerMilliseconds = null;
                else
                    showTimerMilliseconds = ShowTimerMilliseconds;

                if (screen == this.Screen)
                    screen = null;
                else
                    screen = this.Screen;
                #endregion

                OnSettingsChanged(changeToTheme, changeToIconFlow, autoHideDock, 
                    hideAnimationDuration, hideTimerMilliseconds,
                    showAnimationDuration, showTimerMilliseconds,
                    screen);
            }
        }

        #endregion ShowConfiguration

        #endregion Public Methods

        #region Private Methods

        #region GetIconFlow
        private IIconFlow GetIconFlow(Type type)
        {
            return (IIconFlow)Activator.CreateInstance(type);
        }
        #endregion

        #region LoadAddIns()

        private void LoadAddIns()
        {
            // Ensure all button / iconflow assemblies are loaded... if you
            // know a better way, feel free to update this. 
            EnsureAssembliesLoaded();

            // Load icon flows and button types via reflection - so that we don't have to
            // update a hard coded list every time we add a new button type
            IconFlowAddIns = new List<Type>(GetAssignableTypes(typeof(IIconFlow)));
            ButtonAddIns = new List<Type>(GetAssignableTypes(typeof(IBaubleButton)));
        }
        #endregion

        #region EnsureAssembliesLoaded
        private void EnsureAssembliesLoaded()
        {
            var location = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var assemblies = Directory.GetFiles(location, "Bauble.*.dll");

            foreach (var assembly in assemblies)
            {
                Assembly.Load(AssemblyName.GetAssemblyName(assembly));
            }
        }
        #endregion

        #region GetTypesByInterface
        /// <summary>
        /// Locates all types in the current appdomain that are assignable from a base type
        /// </summary>
        /// <param name="baseType">Type that all required types will inherit from</param>
        /// <returns>A List{Type} containing all Types that inherit from baseType</returns>
        private List<Type> GetAssignableTypes(Type baseType)
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => a.FullName.IndexOf("Bauble.", StringComparison.OrdinalIgnoreCase) >= 0)
                .SelectMany(s => s.GetTypes())
                .Where(p => baseType.IsAssignableFrom(p)
                            && baseType != p // located type is not equal to the base class
                            && p.IsClass  // located is a class
                            && !p.IsAbstract // located class is not abstract
                ).ToList();
        }
        #endregion

        #region LoadButtonsFromXml

        /// <summary>
        /// loads the configured icons from the XML settings file
        /// </summary>
        private void LoadButtonsFromXml()
        {
            Buttons = new List<IBaubleButton>();
            try
            {
                XDocument doc = XDocument.Load("Icons.xml");

                var iconElements = from element
                                in doc.Descendants("Icon")
                                   select element;

                foreach (var element in iconElements)
                {
                    LoadButton(element);
                }
            }
            catch (FileNotFoundException)
            {
            }
            // if the icons.xml wasn't found or had no button definitions in it
            if (this.Buttons.Count == 0)
            {
                // add a configure button so that the dock has something
                LoadButton(
                    new XElement("button",
                        new Object[]
                        { 
                            new XAttribute("type", "Bauble.Buttons.Configure"), 
                            new XElement("Text", "Configure") 
                        }
                    )
                );
            }

        }
        #endregion LoadButtonsFromXml

        #region LoadButtonsFromJson

        /// <summary>
        /// loads the configured icons from the XML settings file
        /// </summary>
        private void LoadButtonsFromJson()
        {
            Buttons = new List<IBaubleButton>();
            try
            {
                using (var stream = System.IO.File.OpenRead("Icons.json"))
                {
                    JsonDocument doc = JsonDocument.Parse(stream);

                    var jsonRoot = doc.RootElement;

                    var icons = jsonRoot.GetProperty("icons");
                    foreach (JsonElement icon in icons.EnumerateArray())
                    {
                        LoadButton(icon);
                    }
                }
            }
            catch (FileNotFoundException)
            {
            }
            // if the icons.xml wasn't found or had no button definitions in it
            if (this.Buttons.Count == 0)
            {
                // add a configure button so that the dock has something
                LoadButton(
                    new XElement("button",
                        new Object[]
                        {
                            new XAttribute("type", "Bauble.Buttons.Configure"),
                            new XElement("Text", "Configure")
                        }
                    )
                );
            }

        }
        #endregion LoadButtonsFromJson

        #region LoadButton
        private IBaubleButton LoadButton(XElement element)
        {
            string typeName = element.Attribute("type").Value;

            IBaubleButton button = null;
            var buttonType = ButtonAddIns.FirstOrDefault(
                bt => bt.FullName == typeName
            );
            if (buttonType != null)
            {
                try
                {
                    button = CreateButton(buttonType);

                    button.LoadFromXml(element.ToString(SaveOptions.DisableFormatting));
                    
                    button.Initialize(Theme);

                    Animator.SetButtonDoubleAnimation(
                        button as FrameworkElement,
                        System.Windows.Media.Animation.EasingMode.EaseInOut,
                        SharedEvents.ActivatedEvent,
                        100,
                        new string[] { "Width", "Height" });


                    
                    Buttons.Add(button);
                }
                catch (Exception ex)
                {
                    string message = ex.Message;
                }
            }
            return button;
        }

        private IBaubleButton LoadButton(JsonElement element)
        {
            string typeName = element.GetProperty("type").GetString();

            IBaubleButton button = null;
            var buttonType = ButtonAddIns.FirstOrDefault(
                bt => bt.FullName == typeName
            );
            if (buttonType != null)
            {
                try
                {
                    button = CreateButton(buttonType);

                    button.LoadFromJson(element);

                    button.Initialize(Theme);

                    Animator.SetButtonDoubleAnimation(
                        button as FrameworkElement,
                        System.Windows.Media.Animation.EasingMode.EaseInOut,
                        SharedEvents.ActivatedEvent,
                        100,
                        new string[] { "Width", "Height" });



                    Buttons.Add(button);
                }
                catch (Exception ex)
                {
                    string message = ex.Message;
                }
            }
            return button;
        }

        #endregion LoadButton

        #region CreateButton()

        private static IBaubleButton CreateButton(Type buttonType)
        {
            return (IBaubleButton)Activator.CreateInstance(buttonType);
        }

        #endregion

        #region LoadThemes
        private void LoadThemes()
        {
            Themes = Theme.FindThemes();
            // if no themes were found, add the default theme
            if (Themes.Count == 0)
                Themes.Add(new Theme());
            // find the user's current theme in the themes list
            Theme activeTheme = Themes.FirstOrDefault(t => t.FileName.ToLower().Trim() == ThemeFile.ToLower().Trim());
            // if the user's current theme is unable to be located, then use the first theme.
            if (activeTheme == null)
                activeTheme = Themes[0];
            // set the current theme to the theme located
            Theme = activeTheme;
        }
        #endregion       

        #endregion Private Methods

        #region Event Definitions

        #region SettingsChanged
        public static readonly RoutedEvent SettingsChangedEvent = EventManager.RegisterRoutedEvent("SettingsChanged", RoutingStrategy.Bubble, typeof(SettingsChangedRoutedEventHandler), typeof(BaubleSettings));
        private SettingsChangedRoutedEventHandler _settingsChanged;
        public event SettingsChangedRoutedEventHandler SettingsChanged
        {
            add { _settingsChanged += value; }
            remove { _settingsChanged -= value; }
        }

        private void OnSettingsChanged(Common.Themes.Theme changeToTheme, IIconFlow changeToIconFlow, bool? autoHideDock, 
            double? hideAnimationDuration, double? hideTimerMilliseconds, 
            double? showAnimationDuration, double? showTimerMilliseconds, int? screen)
        {
            SettingsChangedEventArgs args = new SettingsChangedEventArgs(BaubleSettings.SettingsChangedEvent, this);
            args.OldTheme = Theme;
            args.NewTheme = changeToTheme;
            args.OldIconFlow = IconFlow;
            args.NewIconFlow = changeToIconFlow;
            args.AutoHideDock = autoHideDock;
            args.HideAnimationDuration = hideAnimationDuration;
            args.HideTimerMilliseconds = hideTimerMilliseconds;
            args.ShowAnimationDuration = showAnimationDuration;
            args.ShowTimerMilliseconds = showTimerMilliseconds;
            args.Screen = screen;

            if ((_settingsChanged != null) && (args.ShouldRaiseEvent))
            {
                // by default, don't re-save the config file
                bool save = false;
                _settingsChanged(this, args);

                // if the theme changed
                if (changeToTheme != null)
                {
                    // populate the settings variables
                    Theme = changeToTheme;
                    ThemeFile = Theme.FileName;
                    // and trigger a save
                    save = true;
                }
                // if the icon flow changed
                if (changeToIconFlow != null)
                {
                    // populated the settings variables
                    IconFlow = changeToIconFlow;
                    IconFlowTypeName = IconFlow.TypeName;
                    // and trigger a save
                    save = true;
                }
                // if a save was triggered
                if (save)
                {
                    // save the settings to file
                    Properties.Settings.Default.Save();
                }
                
            }
        }
        #endregion

        #endregion
    }
}
