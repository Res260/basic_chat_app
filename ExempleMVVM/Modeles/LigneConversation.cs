namespace TP2.Modeles
{
    /// <summary>
    /// Spécifie une ligne dans une conversation. Il y a une ligne par message envoyé ou reçu.
    /// </summary>
    public class LigneConversation : ModelBase
    {
        #region Private Fields

        private string message;
        private Utilisateur utilisateur;

        #endregion Private Fields

        #region Public Properties

        /// <summary>
        /// message que l'utilisateur a envoyé
        /// </summary>
        public string Message
        {
            get { return message; }
            set
            {
                if (message != value)
                {
                    message = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Utilisateur qui a envoyé le message.
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