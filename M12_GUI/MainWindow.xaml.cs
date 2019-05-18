using GalaSoft.MvvmLight.Messaging;
using System;
using System.Windows;

namespace M12_GUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            Messenger.Default.Register<GenericMessage<Exception>>(this, OnExceptionMessageReceived);
        }

        /// <summary>
        /// Show error messagebox.
        /// </summary>
        /// <param name="Message"></param>
        private void OnExceptionMessageReceived(GenericMessage<Exception> Message)
        {
            var ex = Message.Content as Exception;
            MessageBox.Show($"{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
