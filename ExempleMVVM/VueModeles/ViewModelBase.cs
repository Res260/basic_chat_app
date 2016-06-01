using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TP2.VueModeles
{
    /// <summary>
    /// Classe mère des vues-modèles du système.
    /// </summary>
    public class ViewModelBase : INotifyPropertyChanged
    {
        #region Public Events

        /// <summary>
        /// Évènement permettant d'avertir les observateurs du changement d'une propriété
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion Public Events

        #region Protected Methods

        /// <summary>
        /// Méthode permettant d'appeler PropertyChanged sans avoir à spécifier ses paramètres
        /// </summary>
        protected void NotifyPropertyChanged([CallerMemberName] string nomPropriete = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(nomPropriete));
        }

        #endregion Protected Methods
    }
}