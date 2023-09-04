﻿using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;
using Mcv.PluginV2;

namespace Mcv.MainViewPlugin
{
    /// <summary>
    /// Interaction logic for OptionsView.xaml
    /// </summary>
    public partial class OptionsView : Window
    {
        List<IOptionsTabPage> _pagePanels = new List<IOptionsTabPage>();
        public OptionsView()
        {
            InitializeComponent();
        }
        public void AddTabPage(IOptionsTabPage page)
        {
            var tabPage = new TabItem()
            {
                Header = page.HeaderText,
                Content = page.TabPagePanel,
            };
            tabControl.Items.Add(tabPage);
            _pagePanels.Add(page);
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _pagePanels.ForEach(panel => panel.Apply());
                Close();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _pagePanels.ForEach(panel => panel.Cancel());
                Close();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

    }
}
