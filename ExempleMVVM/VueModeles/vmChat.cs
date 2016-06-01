using TP2.Modeles;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using System.Diagnostics;

namespace TP2.VueModeles
{
    /// <summary>
    /// Vue-modèle pour la vue vueChat
    /// </summary>
    public class vmChat : ViewModelBase
    {
        #region Private Fields

        private Conversation conversationEnCours;

        private ICommand envoyerMessage;

        private ICommand fermerConversation;

        private string messageAEnvoyer;

        private ICommand ouvrirConversation;

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
        public vmChat(Profil profil, ConnectionManager connexionManager)
        {
            this.profil = profil;

            this.ConnectionManager = connexionManager;

            ConnectionManager.AddProgramMessage(connexionManager.GlobalConversation, "Bienvenue!");
        }

        #endregion Public Constructors

        #region Public Properties

        /// <summary>
        /// Conversation présentement sélectionnée, à qui les messages sont envoyés
        /// </summary>
        public Conversation ConversationEnCours
        {
            get
            {
                if (conversationEnCours == null && profil.Conversations.Count > 0)
                    conversationEnCours = profil.Conversations[0];
                return conversationEnCours;
            }
            set
            {
                if (conversationEnCours != value)
                {
                    conversationEnCours = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Commande permettant d'envoyer le message se trouvant dans MessageAEnvoyer dans la
        /// conversation en cours.
        /// </summary>
        public ICommand EnvoyerMessage
        {
            get
            {
                if (envoyerMessage == null)
                    envoyerMessage = new RelayCommand<object>((obj) =>
                   {
                       if(conversationEnCours.EstGlobale)
                       {
                           ConnectionManager.SendGonzoSpamGroupChat(MessageAEnvoyer);
                       }
                       else
                       {
                           ConnectionManager.SendGonzoSendSecretMeme(conversationEnCours.Utilisateur, MessageAEnvoyer);
                       }
                       lock (conversationEnCours.Lignes)
                       {
                           ConversationEnCours.Lignes.Add(new LigneConversation()
                           {
                               Utilisateur = profil.UtilisateurLocal,
                               Message = MessageAEnvoyer
                           });
                           MessageAEnvoyer = string.Empty;
                       }
                   }, (obj) =>
                   {
                       bool canSend;
                       if (conversationEnCours.EstGlobale)
                       {
                           canSend = !string.IsNullOrWhiteSpace(messageAEnvoyer);
                       }
                       else
                       {
                           canSend = !string.IsNullOrWhiteSpace(messageAEnvoyer) && (ConnectionManager.Users.Find(u => u.IP == conversationEnCours.Utilisateur.IP) != null);
                       }
                       return canSend;
                   });
                return envoyerMessage;
            }
        }

        /// <summary>
        /// Permet de fermer une conversation privée.
        /// </summary>
        public ICommand FermerConversation
        {
            get
            {
                if (fermerConversation == null)
                    fermerConversation = new RelayCommand<Conversation>((conversation) =>
                    {
                        ConnectionManager.TerminatePrivateConversation(conversation.Utilisateur);
                        ConversationEnCours = profil.Conversations[0];
                        profil.Conversations.Remove(conversation);
                    }, (conversation) =>
                    {
                        return conversation != null && profil.Conversations.Count > 0 && conversation.EstPrivee;
                    });
                return fermerConversation;
            }
        }

        /// <summary>
        /// Liste des conversations en cours incluant la conversation globale
        /// </summary>
        public ObservableCollection<Conversation> ListeConversations
        {
            get
            {
                return profil.Conversations;
            }
        }

        /// <summary>
        /// Liste des utilisateurs connectées qui utilise cette application sur le réseau présentement
        /// </summary>
        public ObservableCollection<Utilisateur> ListeUtilisateursConnectes
        {
            get
            {
                return profil.UtilisateursConnectes;
            }
        }

        /// <summary>
        /// message à envoyer à la conversation en cours lors de l'appel de la commande EnvoyerMessage
        /// </summary>
        public string MessageAEnvoyer
        {
            get { return messageAEnvoyer; }
            set
            {
                if (messageAEnvoyer != value)
                {
                    messageAEnvoyer = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Permet d'ouvrir une conversation entre l'utilisateur local et un utilisateur dans la
        /// liste des utilisateurs connectés.
        /// </summary>
        public ICommand OuvrirConversation
        {
            get
            {
                if (ouvrirConversation == null)
                    ouvrirConversation = new RelayCommand<Utilisateur>((utilisateur) =>
                    {
                        List<Conversation> conversationsExistantes = profil.Conversations.Where((c) =>
                        {
                            return c.Utilisateur.Nom == utilisateur.Nom && c.Utilisateur.IP == utilisateur.IP;
                        }).ToList();
                        if (conversationsExistantes.Count == 0)
                        {
                            this.ConnectionManager.InitiatePrivateConversation(utilisateur);
                            if(utilisateur.SocketTcp != null)
                            {
                                CreateAddConversation(utilisateur);
                                conversationsExistantes.Add(utilisateur.Conversation);
                                ConversationEnCours = conversationsExistantes[0];
                            }
                        }
                        else
                        {
                            ConversationEnCours = conversationsExistantes[0];
                        }
                    }, (utilisateur) =>
                    {
                        return utilisateur != null;
                    });
                return ouvrirConversation;
            }
        }

        public void CreateAddConversation(Utilisateur user)
        {
            Conversation nouvelleConversation = new Conversation()
            {
                EstGlobale = false,
                Utilisateur = user
            };
            user.Conversation = nouvelleConversation;
            lock(profil.Conversations)
            {
                profil.Conversations.Add(nouvelleConversation);
            }
        }

        #endregion Public Properties
    }
}