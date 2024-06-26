﻿using DwaValidatorApp.Models;
using DwaValidatorApp.Repo;
using DwaValidatorApp.Services.Interface;
using DwaValidatorApp.Viewmodel;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace DwaValidatorApp
{
    /// <summary>
    /// Interaction logic for ProjectInSolutionWindow.xaml
    /// </summary>
    public partial class ProjectInSolutionWindow : Window
    {
        //public string TargetTable { get; set; }

        public ProjectInSolutionWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void DoneButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
