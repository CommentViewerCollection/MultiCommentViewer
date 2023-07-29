﻿///*
//  In App.xaml:
//  <Application.Resources>
//      <vm:ViewModelLocator xmlns:vm="clr-namespace:MultiCommentViewer"
//                           x:Key="Locator" />
//  </Application.Resources>

//  In the View:
//  DataContext="{Binding Source={StaticResource Locator}, Path=ViewModelName}"

//  You can also use Blend to do all this with the tool's support.
//  See http://www.galasoft.ch/mvvm
//*/

//using CommonServiceLocator;
//using GalaSoft.MvvmLight;
//using GalaSoft.MvvmLight.Ioc;
//using System.ComponentModel;

//namespace MultiCommentViewer.ViewModels
//{
//    /// <summary>
//    /// This class contains static references to all the view models in the
//    /// application and provides an entry point for the bindings.
//    /// </summary>
//    public class ViewModelLocator
//    {
//        /// <summary>
//        /// Initializes a new instance of the ViewModelLocator class.
//        /// </summary>
//        public ViewModelLocator()
//        {
//            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

//            if ((bool)(DesignerProperties.IsInDesignModeProperty.GetMetadata(typeof(System.Windows.DependencyObject)).DefaultValue))
//            {
//                SimpleIoc.Default.Register<MainViewModel>();
//            }
//        }

//        public MainViewModel Main
//        {
//            get
//            {
//                return ServiceLocator.Current.GetInstance<MainViewModel>();
//            }
//            set
//            {
//                SimpleIoc.Default.Register(() => value);
//            }
//        }
//        public static void Cleanup()
//        {
//        }
//    }
//}