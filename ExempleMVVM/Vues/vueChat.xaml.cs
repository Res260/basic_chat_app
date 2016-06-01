using TP2.VueModeles;
using System.Windows.Controls;
using System.Windows.Input;

namespace TP2.Vues
{
    /// <summary>
    /// Logique d'interaction pour vueChat.xaml
    /// </summary>
    public partial class vueChat : UserControl
    {
        #region Public Constructors

        public vueChat()
        {
            InitializeComponent();
        }

        #endregion Public Constructors

        #region Private Methods

        /// <summary>
        /// Permet de gérer l'appuie du DoubleClick sur un nom d'utilisateur pour ouvrir une
        /// conversation avec cet utilisateur.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lvUtilisateursConnectesItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            vmChat vm = DataContext as vmChat;
            if (vm.OuvrirConversation.CanExecute(lvUtilisateursConnectes.SelectedItem))
                vm.OuvrirConversation.Execute(lvUtilisateursConnectes.SelectedItem);
        }

        /// <summary>
        /// Permet de gérer l'appuie de la touche "Enter" sur le TextBox de message à envoyer pour
        /// appeler la commande d'envoie de message
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtMessageAEnvoyer_KeyDown(object sender, KeyEventArgs e)
        {
            vmChat vm = DataContext as vmChat;
            if (e.Key == Key.Enter && vm.EnvoyerMessage.CanExecute(null))
                vm.EnvoyerMessage.Execute(null);
        }

        #endregion Private Methods
    }
}