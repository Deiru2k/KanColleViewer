using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Grabacr07.KanColleViewer.Views;

namespace KanColleIoService
{
    static class UserInterfaceHelpers
    {
        private static ContentPresenter GetContentPresenter(Window mainWindow)
        {
            // My WPF skills suck.
            ContentControl contentControl = ((mainWindow.Content as Grid).Children[1] as Grid).Children[1] as ContentControl;
            return VisualTreeHelper.GetChild(contentControl, 0) as ContentPresenter;
        }

        private static TabItem ConstructTabItem()
        {
            TextBlock headerTextBlock = new TextBlock();
            headerTextBlock.Text = "KanColle.io";
            headerTextBlock.Foreground = Brushes.LightGray;
            headerTextBlock.Margin = new Thickness(12, 0, 12, 0);
            headerTextBlock.FontSize = 14;

            TabItem tabItem = new TabItem();
            tabItem.Header = headerTextBlock;
            tabItem.Content = new SyncServiceSettings();

            return tabItem;
        }

        /// <summary>
        /// Adds the settings tab to KCV's start content settings menu.
        /// </summary>
        /// <param name="mainWindow">Main window of KCV</param>
        public static void AddSettingsTabStart(Window mainWindow)
        {
            ContentPresenter contentPresenter = GetContentPresenter(mainWindow);
            StartContent startContent = VisualTreeHelper.GetChild(contentPresenter, 0) as StartContent;

            // And it should've been much easier if the controls were named. Welp.
            TabControl tabControl = ((((startContent.Content as Grid).Children[1] as Grid).Children[2] as Border).Child as ScrollViewer).Content as TabControl;
            tabControl.Items.Add(ConstructTabItem());
        }

        /// <summary>
        /// Adds the settings tab to KCV's main content settings menu.
        /// </summary>
        /// <param name="mainWindow">Main window of KCV</param>
        public static void AddSettingsTabMain(Window mainWindow)
        {
            ContentPresenter contentPresenter = GetContentPresenter(mainWindow);
            MainContent mainContent = VisualTreeHelper.GetChild(contentPresenter, 0) as MainContent;
        }
    }
}
