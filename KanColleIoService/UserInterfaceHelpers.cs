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
            // My skill of WPF sucks.
            ContentControl contentControl = ((mainWindow.Content as Grid).Children[1] as Grid).Children[1] as ContentControl;
            return VisualTreeHelper.GetChild(contentControl, 0) as ContentPresenter;
        }

        /// <summary>
        /// Adds the settings tab to KCV's start content settings menu.
        /// </summary>
        /// <param name="mainWindow">Main window of KCV</param>
        public static void AddSettingsTabStart(Window mainWindow)
        {
            ContentPresenter contentPresenter = GetContentPresenter(mainWindow);
            StartContent startContent = VisualTreeHelper.GetChild(contentPresenter, 0) as StartContent;
        }

        /// <summary>
        /// Adds the settings tab to KCV's main content settings menu.
        /// </summary>
        /// <param name="mainWindow">Main window of KCV</param>
        public static void AddSettingsTabMain(Window mainWindow)
        {
            ContentPresenter contentPresenter = GetContentPresenter(mainWindow);
            MainContent mainContent = VisualTreeHelper.GetChild(contentPresenter, 1) as MainContent;
        }
    }
}
