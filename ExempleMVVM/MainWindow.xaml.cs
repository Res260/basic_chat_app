using System.Windows;
using TP2.VueModeles;

namespace TP2
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Public Constructors

        public MainWindow()
        {
            InitializeComponent();
        }

        #endregion Public Constructors

        /// <summary>
        /// Closes the connexions and the sockets and closes the application
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            vmMainWindow vm = DataContext as vmMainWindow;
            vm.ConnectionManager.TerminateUDPSocket();
            vm.ConnectionManager.StopProcessingUDP();
            vm.ConnectionManager.StopListeningTcpConnection();
            vm.ConnectionManager.TerminateAllPrivateConversations();
        }
    }
}