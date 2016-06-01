namespace TP2
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Net.Sockets;
    using System.Net;
    using System.Security.Cryptography;
    using System.Text.RegularExpressions;
    using System.Text;
    using System.Threading.Tasks;
    using System.Threading;
    using System;
    using TP2.Modeles;
    using TP2.VueModeles;
    using System.Net.NetworkInformation;

    /// <summary>
    /// UDP and TCP connection manager that uses the xXGonzoChat420Xx protocol.
    /// </summary>
    public class ConnectionManager
    {
        #region General Fields
        private Socket udpSocket;

        private Profil profil;

        public List<Utilisateur> usersList;

        public List<Utilisateur> Users
        {
            get { return usersList; }
        }

        public vmChat vmChat;

        public vmChat VmChat
        {
            get { return vmChat; }
            set { vmChat = value; }
        }

        public Conversation GlobalConversation { get; set; }
        #endregion
        #region UDP Fields

        private Thread threadUdpListening;

        private Thread threadUdpProcessing;

        private Thread threadKeepUsersListUpdated;

        private bool continueListeningUdp;

        private bool continueProcessingUdp;

        private ConcurrentQueue<PacketInformations> udpPacketsQueue;

        private Utilisateur programme;

        #endregion
        #region TCP Fields

        private Socket tcpConnectionSocket;

        private bool continueListeningTcpConnection;

        private Thread threadTcpConnectionListening;

        #endregion        

        #region Public Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionManager" /> class. 
        /// </summary>
        /// <param name="profil">The local user's profile.</param>
        /// <param name="vmChat">The vmChat ViewModel.</param>
        public ConnectionManager(Profil profil, vmChat vmChat)
        {
            this.vmChat = vmChat;
            this.usersList = new List<Utilisateur>();
            this.udpPacketsQueue = new ConcurrentQueue<PacketInformations>();
            this.profil = profil;
            this.programme = new Utilisateur();
            programme.IP = "localhost";
            programme.Nom = "Système";
            this.GlobalConversation = GetGlobalConversation();
            InitiateTcpConnectionSocket();
        }
        #endregion

        #region UDP socket management
        /// <summary>
        /// Creates the UDP Socket and binds it to port 42069.
        /// </summary>
        public void InitiateUdpSocket() 
        {
            udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            udpSocket.EnableBroadcast = true;
            try
            {
                udpSocket.Bind(new IPEndPoint(IPAddress.Any, 42069));
            }
            catch (Exception e)
            {
                Debug.WriteLine("Impossible de se bind au port 42069.");
                Debug.WriteLine(e.ToString());
                Environment.Exit(1);
            }

            StartListeningUdp();
        }

        /// <summary>
        /// Starts the 3 UDP threads. One to listen for new packets, one to handle packets and one to keep the usersList updated.
        /// </summary>
        public void StartListeningUdp()
        {
            threadUdpListening = new Thread(ReceiveUdpPacket);
            threadUdpProcessing = new Thread(UdpPacketHandlingLoop);
            threadKeepUsersListUpdated = new Thread(KeepUsersListUpdated);
            continueListeningUdp = true;
            continueProcessingUdp = true;
            threadUdpListening.Start();
            threadUdpProcessing.Start();
            threadKeepUsersListUpdated.Start();
        }

        /// <summary>
        /// Stops the UDP listening and the UDP processing loop.
        /// </summary>
        public void StopListeningUdp()
        {
            continueListeningUdp = false;
            continueProcessingUdp = false;
            threadUdpListening.Join();
            threadUdpProcessing.Join();
        }

        /// <summary>
        /// Stops the threadUdpProcessing thread (when the application closes).
        /// </summary>
        public void StopProcessingUDP()
        {
            continueProcessingUdp = false;
            threadUdpProcessing.Join();
        }

        /// <summary>
        /// Closes the UDP Socket cleanly.
        /// </summary>
        /// <param name="index">Index of the UDP Socket.</param>
        public void TerminateUDPSocket()
        {
            SendGonzoCyaNerds();
            continueListeningUdp = false;
            threadUdpListening.Join();
            udpSocket.Shutdown(SocketShutdown.Both);
            udpSocket.Close();
        }
        #endregion

        #region TCP socket management
        /// <summary>
        /// Initializes the TCP Socket to accept incoming connections.
        /// </summary>
        public void InitiateTcpConnectionSocket()
        {
            try
            {
                tcpConnectionSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                tcpConnectionSocket.Bind(new IPEndPoint(IPAddress.Any, 42070));
                tcpConnectionSocket.Listen(10);
            }
            catch (Exception e)
            {
                Debug.WriteLine("Impossible de se bind au port 42070 en TCP.");
                Debug.WriteLine(e.ToString());
            }

            StartListeningTcpConnectionSocket();
        }

        /// <summary>
        /// Starts the thread to listen for incoming TCP connections.
        /// </summary>
        private void StartListeningTcpConnectionSocket()
        {
            continueListeningTcpConnection = true;
            threadTcpConnectionListening = new Thread(ReceiveTcpConnection);
            threadTcpConnectionListening.Start();
        }

        /// <summary>
        /// Connects to user using TCP, generates an AES key and sends GONZO_INITIATE_SECRET_COMMUNICATION{aesKey} to user.
        /// </summary>
        /// <param name="user">The user to connect to.</param>
        public void InitiatePrivateConversation(Utilisateur user)
        {
            try
            {
                user.SocketTcp = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                IAsyncResult result = user.SocketTcp.BeginConnect(new IPEndPoint(IPAddress.Parse(user.IP), 42070), null, null);
                bool success = result.AsyncWaitHandle.WaitOne(2000, true);
                if(success)
                {
                    user.ContinueListeningTCP = true;
                    Thread tempThread = new Thread(() => HandleTcpConversation(user));
                    tempThread.Start();
                    user.ThreadTcp = tempThread;
                    user.Key = GenerateAesKey();
                    SendGonzoInitiateSecretCommunication(user);
                }
                else
                {
                    System.Windows.Forms.MessageBox.Show("Impossible d'établir la connection avec cet utilisateur. Il ne doit plus être connecté.");
                    user.SocketTcp = null;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
            }
        }

        /// <summary>
        /// Calls TerminatePrivateConversation() for every connected user.
        /// </summary>
        public void TerminateAllPrivateConversations()
        {
            foreach (Utilisateur user in usersList)
            {
                TerminatePrivateConversation(user);
            }
        }

        /// <summary>
        /// If a private conversation was open with user, terminate it cleanly.
        /// </summary>
        /// <param name="user">The user that we close the conversation with.</param>
        public void TerminatePrivateConversation(Utilisateur user)
        {
            SendGonzoTerminateSecretCommunication(user);
            user.Conversation = null;
            TerminatePrivateConversationFromOther(user);
        }

        /// <summary>
        /// Stops listening for incoming TCP connections.
        /// </summary>
        public void StopListeningTcpConnection()
        {
            continueListeningTcpConnection = false;
            threadTcpConnectionListening.Join(); 
            tcpConnectionSocket.Close();
        }

        #endregion

        #region Send UDP communications
        /// <summary>
        /// Sends GONZO_SPOAM_GROUP_CHAT{message} to every users in usersList
        /// </summary>
        /// <param name="message">The content of the message.</param>
        public void SendGonzoSpamGroupChat(string message)
        {
            byte[] bytes = Encoding.UTF8.GetBytes("GONZO_SPAM_GROUP_CHAT{" + message + "}");
            foreach (Utilisateur user in usersList)
            {
                udpSocket.SendTo(bytes, bytes.Length, SocketFlags.None, new IPEndPoint(IPAddress.Parse(user.IP), 42069));
            }
        }

        /// <summary>
        /// Permet d'obtenir la liste des adresses Broadcast disponibles. La fonction élimine les adresses des cartes Loopback et des cartes qui ne sont pas branchées.
        /// </summary>
        /// <returns>La liste des adresses Broadcast disponibles. La fonction élimine les adresses des cartes Loopback et des cartes qui ne sont pas branchées.</returns>
        private static HashSet<IPAddress> GetBroadcastAddresses()
        {
            HashSet<IPAddress> broadcasts = new HashSet<IPAddress>();
            foreach (var i in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (i.OperationalStatus == OperationalStatus.Up && i.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                {
                    foreach (var ua in i.GetIPProperties().UnicastAddresses)
                    {
                        if (ua.Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            IPAddress broadcast = new IPAddress(BitConverter.ToUInt32(ua.Address.GetAddressBytes(), 0) | (BitConverter.ToUInt32(ua.IPv4Mask.GetAddressBytes(), 0) ^ BitConverter.ToUInt32(IPAddress.Broadcast.GetAddressBytes(), 0)));
                            broadcasts.Add(broadcast);
                        }
                    }
                }
            }

            return broadcasts;
        }

        /// <summary>
        /// Sends GONZO_WHOS_THERE{username} in broadcast.
        /// </summary>
        private void SendGonzoWhosThere()
        {
            byte[] bytes = Encoding.UTF8.GetBytes("GONZO_WHOS_THERE{" + profil.Nom + "}");
            try
            {
                foreach(IPAddress broadcastAddress in GetBroadcastAddresses())
                {
                    udpSocket.SendTo(bytes, bytes.Length, SocketFlags.None, new IPEndPoint(broadcastAddress, 42069));
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
            }
        }

        /// <summary>
        /// Sends GONZO_IM_THERE{username} to ip.
        /// </summary>
        /// <param name="ip">The IP which will receive the communication.</param>
        /// <param name="message">message of the GONZO_WHOS_THERE{} received so it does not send things to itself.</param>
        private void SendGonzoImThere(string ip, string message)
        {
            if (profil.Connecte && message != profil.Nom)
            {
                byte[] bytes = Encoding.UTF8.GetBytes("GONZO_IM_THERE{" + profil.Nom + "}");
                udpSocket.SendTo(bytes, bytes.Length, SocketFlags.None, new IPEndPoint(IPAddress.Parse(ip), 42069));
            }
        }

        /// <summary>
        /// Sends GONZO_CYA_NERDS{} (broadcast).
        /// </summary>
        private void SendGonzoCyaNerds()
        {
            if (profil.Connecte)
            {
                byte[] bytes = Encoding.UTF8.GetBytes("GONZO_CYA_NERDS{}");
                foreach (IPAddress broadcastAddress in GetBroadcastAddresses())
                {
                    udpSocket.SendTo(bytes, bytes.Length, SocketFlags.None, new IPEndPoint(broadcastAddress, 42069));
                }
            }
        }
        #endregion

        #region Send TCP communications

        /// <summary>
        /// Sends GONZO_INITIATE_SECRET_COMMUNICATION{aesKKey} using user.key as the AES key.
        /// </summary>
        /// <param name="user">The user to whom the message will be sent.</param>
        private void SendGonzoInitiateSecretCommunication(Utilisateur user)
        {
            byte[] buffer = Encoding.UTF8.GetBytes("GONZO_INITIATE_SECRET_COMMUNICATION{" + Convert.ToBase64String(user.Key) + "}");
            user.SocketTcp.Send(buffer, buffer.Length, SocketFlags.None);
            Debug.WriteLine("message envoyé TCP: " + Encoding.UTF8.GetString(buffer));
        }

        /// <summary>
        /// Sends GONZO_TERMINATE_SECRET_COMMUNICATION{} to user.
        /// </summary>
        /// <param name="user">The user to whom the message will be sent.</param>
        private void SendGonzoTerminateSecretCommunication(Utilisateur user)
        {
            byte[] buffer = Encoding.UTF8.GetBytes("GONZO_TERMINATE_SECRET_COMMUNICATION{}");
            try
            {
                user.SocketTcp.Send(buffer, buffer.Length, SocketFlags.None);
                Debug.WriteLine("message envoyé TCP: " + Encoding.UTF8.GetString(buffer));
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
            }
        }

        /// <summary>
        /// Sends an encrypted message to user.
        /// </summary>
        /// <param name="user">The user that will receive the encrypted message.</param>
        /// <param name="message">The message that will be encrypted and sent.</param>
        public void SendGonzoSendSecretMeme(Utilisateur user, string message)
        {
            if (!user.ContinueListeningTCP)
            {
                InitiatePrivateConversation(user);
            }

            byte[] cryptedMessage = EncryptStringToBytes_Aes(message, user.Key);
            byte[] buffer = Encoding.UTF8.GetBytes("GONZO_SEND_SECRET_MEME{" + Convert.ToBase64String(cryptedMessage) + "}");
            try
            {
                user.SocketTcp.Send(buffer, buffer.Length, SocketFlags.None);
                Debug.WriteLine("message envoyé TCP: " + Encoding.UTF8.GetString(buffer));
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
            }
        }
        #endregion

        #region UDP Thread Methods
        /// <summary>
        /// Method that consistently polls the socket to see if a packet was received
        /// and calls a method that puts it in a queue when one is received.
        /// </summary>
        private void ReceiveUdpPacket()
        {
            byte[] buffer = new byte[264];
            try
            {
                while (continueListeningUdp)
                {
                    if (udpSocket.Poll(100, SelectMode.SelectRead))
                    {
                        EndPoint quiMaEnvoyeCa = new IPEndPoint(IPAddress.Any, 42069);
                        udpSocket.ReceiveFrom(buffer, buffer.Length, SocketFlags.None, ref quiMaEnvoyeCa);
                        string address = ((IPEndPoint)quiMaEnvoyeCa).Address.ToString();
                        udpPacketsQueue.Enqueue(new PacketInformations(address, (byte[])buffer.Clone()));
                        ClearBuffer(ref buffer);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("Erreur UDP: " + e.ToString());
            }
        }

        /// <summary>
        /// Checks often if a new packet has been received. If so, it calls HandleUdpPacket() and magic happens.
        /// </summary>
        private void UdpPacketHandlingLoop()
        {
            while (continueProcessingUdp)
            {
                if (udpPacketsQueue.Count() > 0)
                {
                    int count;
                    bool dequeueSuccess;
                    PacketInformations packet;
                    count = udpPacketsQueue.Count();
                    dequeueSuccess = udpPacketsQueue.TryDequeue(out packet);
                    if (dequeueSuccess)
                    {
                        HandleUdpPacket(packet);
                    }
                }
            }
        }

        /// <summary>
        /// Threaded method that loops to update usersList and send GONZO_WHOS_THERE once every 2 secs.
        /// </summary>
        private void KeepUsersListUpdated()
        {
            while (continueListeningUdp)
            {
                if(profil.Connecte)
                {
                    lock (usersList)
                    {
                        for (int i = usersList.Count() - 1; i >= 0; i--)
                        {
                            if (getCurrentEpochTime() - usersList[i].DernierSigne > 3000)
                            {
                                AddProgramMessage(GlobalConversation, "L'utilisateur " + usersList[i].Nom + " @ " + usersList[i].IP + " est parti (raison: timed out).");
                                usersList.RemoveAt(i);
                            }
                        }
                    }
                }

                SendGonzoWhosThere();
                this.profil.UtilisateursConnectes.Clear();
                lock (usersList)
                {
                    foreach (Utilisateur user in usersList)
                    {
                        this.profil.UtilisateursConnectes.Add(user);
                    }
                }

                System.Threading.Thread.Sleep(500);
            }
        }
        #endregion

        #region TCP Thread Methods

        /// <summary>
        /// Method that constantly checks if a new TCP connection is incoming.
        /// When it happens, it creates a new thread to manage this new connection.
        /// </summary>
        private void ReceiveTcpConnection()
        {
            while (continueListeningTcpConnection)
            {
                if (tcpConnectionSocket.Poll(100, SelectMode.SelectRead))
                {
                    Socket tempSocket = tcpConnectionSocket.Accept();
                    string ip = ((IPEndPoint)tempSocket.RemoteEndPoint).Address.ToString();
                    Utilisateur user = usersList.Find(u => u.IP == ip);
                    if (user != null)
                    {
                        user.SocketTcp = tempSocket;
                        user.ContinueListeningTCP = true;
                        Thread tempThread = new Thread(() => HandleTcpConversation(user));
                        tempThread.Start();
                        user.ThreadTcp = tempThread;
                    }
                    else
                    {
                        tempSocket.Shutdown(SocketShutdown.Both);
                    }
                }
            }
        }

        /// <summary>
        /// The loop to get incoming communications from a private conversation with a user.
        /// </summary>
        /// <param name="user">The user with whom we have a private conversation.</param>
        private void HandleTcpConversation(Utilisateur user)
        {
            try
            {
                byte[] buffer = new byte[1000];
                while (user.ContinueListeningTCP)
                {
                    if (user.SocketTcp.Connected)
                    {
                        if (user.SocketTcp.Poll(100000, SelectMode.SelectRead))
                        {
                            user.SocketTcp.Receive(buffer, buffer.Length, SocketFlags.None);
                            Debug.WriteLine("Communication reçue de " + user.Nom + ": " + Encoding.UTF8.GetString(buffer));
                            HandleTcpCommunication(user, PacketInformations.FormatCommunication((byte[])buffer.Clone()));
                            ClearBuffer(ref buffer);
                        }
                    }
                    else
                    {
                        Debug.WriteLine("Pas connecté");
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
            }
        }

        #endregion

        #region UDP packet handling
        /// <summary>
        /// Calls the good method to treat a received UDP packet.
        /// If the packet is retarded (It is not a known command), it ignores it.
        /// </summary>
        /// <param name="packet">The packet to handle.</param>
        private void HandleUdpPacket(PacketInformations packet)
        {
            string message = GetMessageFromCommunication(packet.Communication);

            if (packet.Communication.StartsWith("GONZO_IM_THERE{"))
            {
                AddUserToListFromCommunication(message.Trim(), packet.IP);
            }
            else if (packet.Communication.StartsWith("GONZO_WHOS_THERE{"))
            {
                SendGonzoImThere(packet.IP, message);
            }
            else if (packet.Communication.StartsWith("GONZO_CYA_NERDS{}"))
            {
                RemoveUserFromListWithIP(packet.IP.ToString());
            }
            else if (packet.Communication.StartsWith("GONZO_SPAM_GROUP_CHAT{"))
            {
                AddReceivedUdpChatLine(packet.IP, message);
            }
            else
            {
                Debug.WriteLine("----------PACKET NON TRAITÉ:----------");
                Debug.WriteLine("FROM: " + packet.IP.ToString());
                Debug.WriteLine("Actual: " + packet.Communication);
            }
        }

        /// <summary>
        /// Adds a new entry in usersList if it does not exist. If it exists but it has a new username,
        /// it renames the current user and tells the client. It ignores the packet if the username is the same.
        /// </summary>
        /// <param name="utilisateur">The user's username.</param>
        /// <param name="ip">the user's IP.</param>
        private void AddUserToListFromCommunication(string utilisateur, string ip)
        {
            bool isNewUser = true;
            lock (usersList)
            {
                foreach (Utilisateur user in usersList)
                {
                    if (user.IP == ip)
                    {
                        isNewUser = false;
                        if (user.Nom != utilisateur)
                        {
                            AddProgramMessage(GlobalConversation, "L'utilisateur " + user.Nom + " @ " + user.IP + " a changé de nom pour " + utilisateur + ".");
                            Debug.WriteLine("user changé. " + user.Nom + " est devenu: " + utilisateur);
                            user.Nom = utilisateur;
                        }

                        user.DernierSigne = getCurrentEpochTime();
                        Debug.WriteLine("user: " + user.Nom + " a donné signe de vie.");
                    }
                }
            }

            if (isNewUser && utilisateur.Length > 0)
            {
                Utilisateur tempUser = new Utilisateur();
                tempUser.IP = ip.ToString();
                tempUser.Nom = utilisateur;
                tempUser.DernierSigne = getCurrentEpochTime();
                lock (usersList)
                {
                    usersList.Add(tempUser);
                    AddProgramMessage(GlobalConversation, "L'utilisateur " + tempUser.Nom + " @ " + tempUser.IP + " vient de se connecter!");
                    Debug.WriteLine("Utilisateur ajouté. New count:");
                    Debug.WriteLine(usersList.Count().ToString());
                }
            }
            else
            {
                Debug.WriteLine("Ajout d'utilisateur ignoré. Nom:" + utilisateur);
            }
        }

        /// <summary>
        /// Removes a user from usersList if ip matches with the user's IP.
        /// </summary>
        /// <param name="ip">The IP to look for.</param>
        private void RemoveUserFromListWithIP(string ip)
        {
            lock (usersList)
            {
                for (int i = 0; i < usersList.Count; i++)
                {
                    if (usersList[i].IP == ip)
                    {
                        AddProgramMessage(GlobalConversation, "L'utilisateur " + usersList[i].Nom + " est parti (raison: l'utilisateur a quitté l'application).");
                        usersList.RemoveAt(i);
                    }

                    Debug.WriteLine("Utilisateur supprimé. New count:");
                    Debug.WriteLine(usersList.Count().ToString());
                }
            }
        }

        /// <summary>
        /// Adds a new chat line to the GUI if the user is in usersList.
        /// </summary>
        /// <param name="ip">Source's IP</param>
        /// <param name="message">The message to show</param>
        private void AddReceivedUdpChatLine(string ip, string message)
        {
            Utilisateur userSource = null;
            lock (usersList)
            {
                foreach (Utilisateur user in usersList)
                {
                    if (user.IP == ip)
                    {
                        userSource = user;
                    }
                }
            }

            if (userSource != null)
            {
                lock (GlobalConversation.Lignes)
                {
                    GlobalConversation.Lignes.Add(new LigneConversation
                    {
                        Utilisateur = userSource,
                        Message = message
                    });
                }
            }
            else
            {
                Debug.WriteLine("---------PACKET IGNORÉ GONZO_SPAM_GROUP_CHAT---------");
                Debug.WriteLine("FROM: " + ip.ToString());
                Debug.WriteLine("CONTENU: " + message);
            }
        }

        /// <summary>
        /// Runs a regex to get the message from a communication.
        /// </summary>
        /// <param name="communication">the received packet's content (as a string).</param>
        /// <returns>the message (empty string if none).</returns>
        private string GetMessageFromCommunication(string communication)
        {

            Regex messageRegex = new Regex("{(.*)}");
            return messageRegex.Match(communication).Groups[1].ToString();
        }
        #endregion

        #region TCP packet handling

        /// <summary>
        /// Checks what communication is about and calls the good method to handle it, or ignores the communication.
        /// </summary>
        /// <param name="user">The user that sent the communication.</param>
        /// <param name="communication">The received communication.</param>
        private void HandleTcpCommunication(Utilisateur user, string communication)
        {
            string message = GetMessageFromCommunication(communication);

            if (communication.StartsWith("GONZO_INITIATE_SECRET_COMMUNICATION{"))
            {
                InitiatePrivateConversationFromOther(user, message);
            }
            else if (communication.StartsWith("GONZO_SEND_SECRET_MEME{"))
            {
                string decryptedMessage = DecryptStringFromBytes_Aes(Convert.FromBase64String(message), user.Key);
                AddPrivateMessageLine(user, decryptedMessage.Trim().TrimEnd('\r', '\n', '\0'));
            }
            else if (communication.StartsWith("GONZO_TERMINATE_SECRET_COMMUNICATION{}"))
            {
                TerminatePrivateConversationFromOther(user);
                AddProgramMessage(user.Conversation, user.Nom + " a quitté la conversation privée.");
            }
            else if(communication == "")
            {
                TerminatePrivateConversationFromOther(user);
                AddProgramMessage(user.Conversation, user.Nom + " a quitté de façon impropre (crash d'application?).");
            }
            else
            {
                Debug.WriteLine("Communication TCP de " + user.Nom + " ignorée. Communication: " + communication);
            }
        }

        /// <summary>
        /// Closes the connection with user and frees the memory.
        /// </summary>
        /// <param name="user">The user with whom we need to terminate the private conversation.</param>
        public void TerminatePrivateConversationFromOther(Utilisateur user)
        {
            if (user.SocketTcp != null)
            {
                user.ContinueListeningTCP = false;
                try
                {
                    user.SocketTcp.Shutdown(SocketShutdown.Both);
                }
                catch (Exception e) { }
                user.SocketTcp.Close();
                user.SocketTcp = null;
                user.Key = null;
                Debug.WriteLine("Terminate private conv.");
            }
        }

        /// <summary>
        /// Stocks key in users.key
        /// </summary>
        /// <param name="user">The user that initiated the private conversation</param>
        /// <param name="key">the AES key that will be used for secure communication.</param>
        private void InitiatePrivateConversationFromOther(Utilisateur user, string key)
        {
            user.Key = Convert.FromBase64String(key);
        }

        /// <summary>
        /// Adds a new chat line in the GUI. If the connection was closed, open it.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="message"></param>
        private void AddPrivateMessageLine(Utilisateur user, string message)
        {
            if (profil.Connecte)
                {
                    if (user.Conversation == null)
                    {
                        vmChat.CreateAddConversation(user);
                    }

                    lock (user.Conversation.Lignes)
                    {
                        user.Conversation.Lignes.Add(new LigneConversation
                        {
                            Utilisateur = user,
                            Message = message
                        });
                    }
                }
        }

        #endregion

        #region Not network-related methods

        /// <summary>
        /// Used to get the UNIX time (number of ms since jan. 1st, 1970.
        /// </summary>
        /// <returns>epoch time.</returns>
        private double getCurrentEpochTime()
        {
            return DateTime.UtcNow.ToUniversalTime().Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
        }

        /// <summary>
        /// Return true if username is not in any of usersList's username.
        /// </summary>
        /// <param name="username">the username to check.</param>
        /// <returns>true if it does not appear in usersList.</returns>
        public bool IsUsernameOkay(string username)
        {
            bool result = true;
            lock (usersList)
            {
                foreach (Utilisateur user in usersList)
                {
                    if (user.Nom == username.Trim())
                    {
                        result = false;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Returns the global conversation.
        /// </summary>
        /// <returns>The global conversation.</returns>
        private Conversation GetGlobalConversation()
        {
            Conversation globalConversation = null;
            foreach (Conversation conversation in profil.Conversations)
            {
                if (conversation.EstGlobale)
                {
                    globalConversation = conversation;
                }
            }

            return globalConversation;
        }

        /// <summary>
        /// Adds a chat line coming from the program to the interface.
        /// </summary>
        /// <param name="message">The message to display.</param>
        public void AddProgramMessage(Conversation conversation, string message)
        {
            lock (conversation.Lignes)
            {
                conversation.Lignes.Add(new LigneConversation
                {
                    Utilisateur = programme,
                    Message = message
                });
            }
        }

        /// <summary>
        ///  MODIFIED FROM: https://msdn.microsoft.com/en-us/library/system.security.cryptography.aesmanaged(v=vs.110).aspx
        ///  Encrypts a string using key and the AES 128bits encryption standard.
        /// </summary>
        /// <param name="plainText">The string to encrypt.</param>
        /// <param name="key">The key to use for the encryption.</param>
        /// <returns>An array of bytes representing the encrypted plainText.</returns>
        public static byte[] EncryptStringToBytes_Aes(string plainText, byte[] key)
        {
            // Check arguments.
            if (plainText == null || plainText.Length <= 0)
            {
                throw new ArgumentNullException("plainText");
            }
                
            if (key == null || key.Length <= 0)
            {
                throw new ArgumentNullException("key");
            }
                
            byte[] encrypted;

            // Create an AesManaged object
            // with the specified key and IV.
            using (AesManaged aesAlg = new AesManaged())
            {
                aesAlg.Key = key;
                aesAlg.Padding = PaddingMode.Zeros;
                aesAlg.Mode = CipherMode.ECB;

                // Create a decrytor to perform the stream transform.
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for encryption.
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            // Write all data to the stream.
                            swEncrypt.Write(plainText);
                        }

                        encrypted = msEncrypt.ToArray();
                    }
                }
            }

            // Return the encrypted bytes from the memory stream.
            return encrypted;
        }

        /// <summary>
        /// MODIFIED FROM: https://msdn.microsoft.com/en-us/library/system.security.cryptography.aesmanaged(v=vs.110).aspx
        /// Decrypts an array of bytes using key and the AES 128bits encryption standard.
        /// </summary>
        /// <param name="cipherText">The encrypted data</param>
        /// <param name="key">The key to use to decrypt.</param>
        /// <returns>A string representing the decrypted message (with '\0' chars at the end of it :/.</returns>
        public static string DecryptStringFromBytes_Aes(byte[] cipherText, byte[] key)
        {
            // Check arguments.
            if (cipherText == null || cipherText.Length <= 0)
            {
                throw new ArgumentNullException("cipherText");
            }
                
            if (key == null || key.Length <= 0)
            {
                throw new ArgumentNullException("key");
            }

            // Declare the string used to hold
            // the decrypted text.
            string plaintext = null;

            // Create an AesManaged object
            // with the specified key and IV.
            using (AesManaged aesAlg = new AesManaged())
            {
                aesAlg.Key = key;
                aesAlg.Padding = PaddingMode.Zeros;
                aesAlg.Mode = CipherMode.ECB;

                // Create a decrytor to perform the stream transform.
                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for decryption.
                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            // Read the decrypted bytes from the decrypting stream
                            // and place them in a string.
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }
            }

            return plaintext;
        }

        /// <summary>
        /// Generates an AES 128 bits (default) key.
        /// </summary>
        /// <returns>The AES key</returns>
        private byte[] GenerateAesKey()
        {
            AesManaged tempAes = new AesManaged();
            tempAes.GenerateKey();
            return tempAes.Key;
        }

        /// <summary>
        /// Clears (sets everything to 0) the received buffer.
        /// </summary>
        /// <param name="buffer">the buffer to clear.</param>
        public static void ClearBuffer(ref byte[] buffer)
        {
            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = 0;
            }
        }

        #endregion
    }
}