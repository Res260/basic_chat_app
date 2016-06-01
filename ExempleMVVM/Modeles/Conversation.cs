using System.Collections.ObjectModel;
using TP2.VueModeles;

namespace TP2.Modeles
{
    /// <summary>
    /// Permet de gérer une conversation. Une conversation peut être la conversation globale (avec
    /// tous les utilisateurs) ou une conversation privée (avec un utilisateur seulement).
    /// </summary>
    public class Conversation : ModelBase
    {
        #region Private Fields

        private bool estGlobale;
        private MTObservableCollection<LigneConversation> lignes;

        private Utilisateur utilisateur;

        #endregion Private Fields

        #region Public Properties

        /// <summary>
        /// Spécifie si la conversation est globale, c'est-à-dire, à tous les usagers du système
        /// </summary>
        public bool EstGlobale
        {
            get { return estGlobale; }
            set
            {
                if (estGlobale != value)
                {
                    estGlobale = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Spécifie si la conversation est privée, c'est-à-dire, liée à un seul utilisateur.
        /// </summary>
        public bool EstPrivee
        {
            get
            {
                return !EstGlobale;
            }
        }

        /// <summary>
        /// Liste des lignes de la conversation contenant le nom d'utilisateur, l'adresse IP et le message.
        /// </summary>
        public MTObservableCollection<LigneConversation> Lignes
        {
            get
            {
                if (lignes == null)
                    lignes = new MTObservableCollection<LigneConversation>();
                return lignes;
            }
            set
            {
                if (lignes != value)
                {
                    lignes = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Spécifie l'utilisateur auquel la conversation est liée. Utile pour une conversation
        /// privée. Dans le cas d'une conversation globale, on peut mettre un utilisateur bidon pour
        /// avoir le nom de l'utilisateur de la conversation.
        /// </summary>
        public Utilisateur Utilisateur
        {
            get { return utilisateur; }
            set
            {
                if (utilisateur != value)
                {
                    utilisateur = value;
                    NotifyPropertyChanged();
                }
            }
        }

        #endregion Public Properties
    }
}