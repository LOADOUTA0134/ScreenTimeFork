﻿using FontAwesome.WPF;
using ScreenTime.classes;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ScreenTime
{
    public partial class MainWindow : Window
    {
        private readonly string dateFormat = "dd.MM.yyyy";
        private string dateString;
        public MainWindow()
        {
            InitializeComponent();

            dateString = DateTime.Now.ToString(dateFormat);
            SetScreenTimeAppsForMainScreen();
        }

        /*
         * Events
         */
        private void Image_MouseDown_Back(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            DateTime date = DateTime.ParseExact(dateString, dateFormat, CultureInfo.InvariantCulture);
            date = date.AddDays(-1);
            dateString = date.ToString(dateFormat);

            SetScreenTimeAppsForMainScreen();
        }

        private void Image_MouseDown_Forward(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            DateTime date = DateTime.ParseExact(dateString, dateFormat, CultureInfo.InvariantCulture);
            date = date.AddDays(1);
            dateString = date.ToString(dateFormat);

            SetScreenTimeAppsForMainScreen();
        }

        /*
         * Logic
         */
        private void SetScreenTimeAppsForMainScreen()
        {
            StackPanelDynamic.Children.Clear();

            List<ScreenTimeApp> focusedAppsToday = ScreenTimeApp.GetScreenTimeAppsByDateSorted(dateString, SortMode.SECONDS_IN_FOCUS, true);
            uint totalScreenTimeSeconds = (uint)focusedAppsToday.Sum(app => app.SecondsInFocus.GetValueOrDefault(dateString));

            foreach (ScreenTimeApp screenTimeApp in focusedAppsToday)
            {
                AddScreenTimeAppToMainScreen(screenTimeApp, dateString, totalScreenTimeSeconds);
            }

            TextBlockDate.Text = dateString;
        }

        private void AddScreenTimeAppToMainScreen(ScreenTimeApp screenTimeApp, string todayDate, uint totalScreenTimeSeconds)
        {
            string screenTimeAppName = screenTimeApp.Name;
            if (screenTimeAppName.Length > 16) screenTimeAppName = screenTimeAppName[..16] + "...";

            uint screenTimeAppSecondsInFocus = screenTimeApp.SecondsInFocus.GetValueOrDefault(todayDate);

            uint secondsInFocus = screenTimeAppSecondsInFocus % 60;
            uint minutesInFocus = screenTimeAppSecondsInFocus / 60;
            uint hoursInFocus = minutesInFocus / 60;
            string screenTimeAppTimeInFocus;
            if (hoursInFocus > 0)
                screenTimeAppTimeInFocus = string.Format("{0}h {1}m", hoursInFocus, minutesInFocus);
            else if (minutesInFocus > 0)
                screenTimeAppTimeInFocus = string.Format("{0}m {1}s", minutesInFocus, secondsInFocus);
            else
                screenTimeAppTimeInFocus = string.Format("{0}s", screenTimeAppSecondsInFocus);

            Grid grid = new();
            grid.RowDefinitions.Add(new RowDefinition());
            grid.RowDefinitions.Add(new RowDefinition());
            grid.RowDefinitions.Add(new RowDefinition());

            grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = GridLength.Auto }); // Add this line for the arrow column

            // Create the new TextBlocks
            TextBlock textBlockScreenTimeAppName = new()
            {
                Text = screenTimeAppName,
                Foreground = Brushes.White,
                FontSize = 30
            };

            TextBlock textBlockScreenTimeAppSecondsInFocus = new()
            {
                Text = screenTimeAppTimeInFocus,
                Foreground = Brushes.White,
                FontSize = 26
            };

            // Create the new Rectangle
            Rectangle progressRect = new Rectangle
            {
                Style = (Style)FindResource("CustomProgressBarStyle"),
                Width = CalculateProgressWidth(screenTimeAppSecondsInFocus, totalScreenTimeSeconds)
            };

            // Create the new arrow icon
            FontAwesome.WPF.FontAwesome arrowIcon = new FontAwesome.WPF.FontAwesome
            {
                Icon = FontAwesomeIcon.AngleRight,
                Foreground = Brushes.White,
                FontSize = 50,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            // Add elements to the grid
            Grid.SetRow(textBlockScreenTimeAppName, 0);
            Grid.SetColumn(textBlockScreenTimeAppName, 0);
            Grid.SetColumnSpan(textBlockScreenTimeAppName, 3); // Span across all columns

            Grid.SetColumn(textBlockScreenTimeAppSecondsInFocus, 1);
            Grid.SetRow(textBlockScreenTimeAppSecondsInFocus, 1);

            Grid.SetRow(progressRect, 1);
            Grid.SetColumnSpan(progressRect, 1);
            Grid.SetColumn(progressRect, 0);

            Grid.SetRow(arrowIcon, 0);
            Grid.SetRowSpan(arrowIcon, 2);
            Grid.SetColumn(arrowIcon, 3);


            grid.Children.Add(progressRect);
            grid.Children.Add(textBlockScreenTimeAppName);
            grid.Children.Add(textBlockScreenTimeAppSecondsInFocus);
            grid.Children.Add(arrowIcon);

            // Create and configure border for the entire item
            Border border = new Border
            {
                Width = 700,
                Style = (Style)FindResource("CustomBorderStyle"),
                Child = grid
            };

            // Add border to the dynamicStackPanel
            StackPanelDynamic.Children.Add(border);
        }

        private double CalculateProgressWidth(uint currentProgress, uint totalProgress)
        {
            double progressPercentage = (double)currentProgress / totalProgress;
            double maxWidth = 700;
            return progressPercentage * maxWidth;
        }
    }
}
