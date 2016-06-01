using TP2.VueModeles;
using System.Windows.Controls;
using System.Windows.Input;

namespace TP2.Vues
{
    /// <summary>
    /// Logique d'interaction pour vueConnexion.xaml
    /// </summary>
    public partial class vueConnexion : UserControl
    {
        #region Public Constructors

        public vueConnexion()
        {
            InitializeComponent();
        }

        #endregion Public Constructors

        #region Private Methods

        /// <summary>
        /// Permet de gérer l'appuie de la touche "Enter" sur le textbox du nom d'utilisateur pour
        /// activer la commande de connexion
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            vmConnexion vm = (vmConnexion)DataContext;
            if (e.Key == Key.Enter)
            {
                if (vm.ConnecterUtilisateur.CanExecute(null))
                    vm.ConnecterUtilisateur.Execute(null);
            }
        }

        #endregion Private Methods
    }
}