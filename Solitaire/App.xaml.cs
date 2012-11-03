using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using Spider.Solitaire.ViewModel;
using System.Text;
using Spider.Engine.Core;

namespace Spider.Solitaire
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            Utils.SuppressConsole = true;

            MainWindow window = new MainWindow();

            // Create the ViewModel to which 
            // the main window binds.
            var viewModel = new SpiderViewModel();

            // When the ViewModel asks to be closed, 
            // close the window.
            EventHandler handler = null;
            handler = delegate
            {
                viewModel.RequestClose -= handler;
                window.Close();
            };
            viewModel.RequestClose += handler;

            // Allow all controls in the window to 
            // bind to the ViewModel by setting the 
            // DataContext, which propagates down 
            // the element tree.
            window.DataContext = viewModel;

            window.Show();
        }
    }
}
