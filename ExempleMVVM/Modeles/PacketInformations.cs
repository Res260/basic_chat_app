using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;

namespace TP2.Modeles
{

    /// <summary>
    /// Holds a packet's IP and its content.
    /// </summary>
    class PacketInformations
    {
        public String IP;
        public String Communication;

        /// <summary>
        /// Constructor for PacketInformations
        /// </summary>
        /// <param name="ip">Packet's source'sIP</param>
        /// <param name="communication">Packet's content.</param>
        public PacketInformations(String ip, byte[] communication)
        {
            this.IP = ip;
            //Debug.WriteLine("NEW PACKET FROM: " + ip);
            this.Communication = FormatCommunication(communication);
        }

        /// <summary>
        /// Transform packet's content to string and removes useless spaces and line feeds and zeros.
        /// </summary>
        /// <param name="communication"></param>
        /// <returns></returns>
        public static String FormatCommunication(byte[] communication)
        {
            String FormatedCommunication;
            FormatedCommunication = Encoding.UTF8.GetString(communication).Trim().TrimEnd('\r', '\n', '\0');
            return FormatedCommunication;
        }
    }
}
