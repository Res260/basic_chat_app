using System;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Threading;

namespace TP2.Modeles
{
    /// <summary>
    /// Utilisateur du système. Soit utilisateur local ou utilisateurs connectés utilisant cette application.
    /// </summary>
    public class Utilisateur : ModelBase
    {
        #region Private Fields

        private string ip;
        private string nom;
        /// <summary>
        /// Epoch time since that last GONZO_IM_THERE{nom} received.
        /// </summary>
        private double dernierSigne;
        public Socket socketTcp;
        public Socket SocketTcp
        {
            get { return socketTcp; }
            set { socketTcp = value; }
        }

        public bool continueListeningTCP;
        public bool ContinueListeningTCP
        {
            get { return continueListeningTCP; }
            set { continueListeningTCP = value; }
        }

        private byte[] key;
        public byte[] Key
        {
            get { return key; }
            set { key = value; }
        }

        private Conversation conversation;
        public Conversation Conversation
        {
            get { return conversation; }
            set { conversation = value; }
        }

        private Thread threadTcp;
        public Thread ThreadTcp
        {
            get { return threadTcp; }
            set { threadTcp = value; }
        }
        

        #endregion Private Fields

        #region Public Properties

        /// <summary>
        /// Adresse IP permettant d'envoyer un message à cet utilisateur.
        /// </summary>
        public string IP
        {
            get { return ip; }
            set
            {
                if (ip != value)
                {
                    ip = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Nom d'utilisateur
        /// </summary>
        public string Nom
        {
            get { return nom; }
            set
            {
                if (nom != value)
                {
                    nom = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public double DernierSigne
        {
            get { return dernierSigne; }
            set
            {
                if (dernierSigne != value)
                {
                    dernierSigne = value;
                    NotifyPropertyChanged();
                }
            }
        }

        #region Private Methods
        
        private void HandleTcpConnexion()
        {

        }

        #endregion

        #endregion Public Properties
    }
}