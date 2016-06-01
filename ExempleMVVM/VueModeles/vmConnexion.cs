using TP2.Modeles;
using System;
using System.Windows.Input;

namespace TP2.VueModeles
{
    /// <summary>
    /// Vue-modèle pour la vue vueConnexion
    /// </summary>
    internal class vmConnexion : ViewModelBase
    {
        #region Private Fields

        private ICommand connecterUtilisateur;

        private Profil profil;

        private ConnectionManager ConnectionManager;

        #endregion Private Fields

        #region Public Constructors

        /// <summary>
        /// Constructeur de la classe
        /// </summary>
        /// <param name="profil">
        /// profil de l'utilsateur à faire suivre à travers les vues-modèles de l'application pour
        /// avoir l'état de l'application
        /// </param>
        /// <param name="connexionManager">
        /// Gestionnaire de connexion pour l'application.
        /// </param>
        public vmConnexion(Profil profil, ConnectionManager connexionManager)
        {
            this.profil = profil;
            this.ConnectionManager = connexionManager;
        }

        #endregion Public Constructors

        #region Public Properties

        /// <summary>
        /// Permet de connecter l'utilisateur à l'application. Théoriquement, vous devriez mettre le
        /// code permettant de voir si une autre utilisateur possède le même nom d'utilisateur.
        /// </summary>
        public ICommand ConnecterUtilisateur
        {
            get
            {
                if (connecterUtilisateur == null)
                    connecterUtilisateur = new RelayCommand<object>(
                    (obj) =>
                    {
                        if(ConnectionManager.IsUsernameOkay(NomUtilisateur))
                        {
                            profil.Connecte = true;
                        }
                        else
                        {
                            System.Windows.Forms.MessageBox.Show("Un utilisateur utilise déjà ce nom. Veuillez en choisir un autre.");
                        }
                    },
                    (obj) =>
                    {
                        return !string.IsNullOrWhiteSpace(NomUtilisateur);
                    });
                return connecterUtilisateur;
            }
        }

        /// <summary>
        /// Nom de l'utilisateur local pour se connecter
        /// </summary>
        public string NomUtilisateur
        {
            get { return profil.Nom; }
            set
            {
                if (profil.Nom != value)
                {
                    if (value.Length > 10)
                    {
                        profil.Nom = "";
                        throw new ArgumentException("Le nom d'utilisateur à un maximum de 10 caractères!");
                    }
                    else
                    {
                        profil.Nom = value;
                        NotifyPropertyChanged();
                    }
                }
            }
        }

        #endregion Public Properties
    }
}