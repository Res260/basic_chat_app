using TP2.Modeles;
using System.ComponentModel;

namespace TP2.VueModeles
{
    /// <summary>
    /// Vue-modèle pour la vue principale
    /// </summary>
    internal class vmMainWindow : ViewModelBase
    {
        #region Private Fields

        private ViewModelBase currentViewModel;

        private Profil profil;

        private vmChat vmChat;

        private vmConnexion vmConnexion;

        public ConnectionManager ConnectionManager;

        #endregion Private Fields

        #region Public Constructors

        /// <summary>
        /// Constructeur de la classe, initialise les vues-modèles de l'application et spécifie le
        /// premier vue-modèle à afficher
        /// </summary>
        public vmMainWindow()
        {
            this.profil = new Profil();
            this.ConnectionManager = new ConnectionManager(profil, vmChat);
            vmConnexion = new vmConnexion(profil, ConnectionManager);
            vmChat = new vmChat(profil, ConnectionManager);
            this.ConnectionManager.VmChat = vmChat;
            this.ConnectionManager.InitiateUdpSocket();

            CurrentViewModel = vmConnexion;

            profil.PropertyChanged += Profil_PropertyChanged;
        }

        #endregion Public Constructors

        #region Public Properties

        /// <summary>
        /// Vue-modèle présentement affiché dans la vue principale (les vues-modèles sont liés à leur
        /// vue dans le fichier App.xml)
        /// </summary>
        public ViewModelBase CurrentViewModel
        {
            get { return currentViewModel; }
            set
            {
                if (currentViewModel != value)
                {
                    currentViewModel = value;
                    NotifyPropertyChanged();
                }
            }
        }

        #endregion Public Properties

        #region Private Methods

        /// <summary>
        /// Permet de changer d'interface lorsque l'utilisateur est connecté
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Profil_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Connecte")
            {
                if (profil.Connecte)
                {
                    CurrentViewModel = vmChat;
                }
                else
                {
                    CurrentViewModel = vmConnexion;
                }
            }
        }

        #endregion Private Methods
    }
}