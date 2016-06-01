using System.Collections.ObjectModel;
using TP2.VueModeles;

namespace TP2.Modeles
{
    /// <summary>
    /// profil de l'utilisateur local.
    /// </summary>
    public class Profil : ModelBase
    {
        #region Private Fields

        private bool connecte = false;
        private MTObservableCollection<Conversation> conversations;
        private string nom = "";
        private Utilisateur utilisateurLocal;

        private MTObservableCollection<Utilisateur> utilisateursConnectes;

        #endregion Private Fields

        #region Public Properties

        /// <summary>
        /// Permet de savoir si l'utilisateur local a réussi à se connecter.
        /// </summary>
        public bool Connecte
        {
            get { return connecte; }
            set
            {
                if (connecte != value)
                {
                    connecte = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Conversations en cours. Une conversation globale existe par défaut pour parler à tous les
        /// utilisateurs connectés. Les autres conversations seront de type privé.
        /// </summary>
        public MTObservableCollection<Conversation> Conversations
        {
            get
            {
                if (conversations == null)
                {
                    conversations = new MTObservableCollection<Conversation>();
                    conversations.Add(new Conversation()
                    {
                        EstGlobale = true,
                        Utilisateur = new Utilisateur()
                        {
                            Nom = "Global"
                        }
                    });
                }
                return conversations;
            }
            set
            {
                if (conversations != value)
                {
                    conversations = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Nom de l'utilisateur local.
        /// </summary>
        public string Nom
        {
            get { return nom; }
            set
            {
                if (nom != value)
                {
                    nom = value;
                    utilisateurLocal = new Utilisateur()
                    {
                        Nom = Nom,
                        IP = "localhost"
                    };
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Permet d'obtenir l'objet de type Utilisateur représentant l'utilisateur local. Utile pour
        /// ajouter une ligne dans une conversation envoyé par l'utilisateur local.
        /// </summary>
        public Utilisateur UtilisateurLocal
        {
            get
            {
                if (utilisateurLocal == null)
                    utilisateurLocal = new Utilisateur()
                    {
                        Nom = Nom,
                        IP = "localhost"
                    };
                return utilisateurLocal;
            }
            set
            {
                if (utilisateurLocal != value)
                {
                    utilisateurLocal = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Liste des utilisateurs connectées qui utilise présentement cette application sur le réseau.
        /// </summary>
        public MTObservableCollection<Utilisateur> UtilisateursConnectes
        {
            get
            {
                if (utilisateursConnectes == null)
                    utilisateursConnectes = new MTObservableCollection<Utilisateur>();
                return utilisateursConnectes;
            }
            set
            {
                if (utilisateursConnectes != value)
                {
                    utilisateursConnectes = value;
                    NotifyPropertyChanged();
                }
            }
        }

        #endregion Public Properties
    }
}