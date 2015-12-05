using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SimpleSniffer.BaseClass
{
    
    
    public class Packet
    {
        private const int LineCount = 30; 

        /// <summary>
        /// use enumeration to identify the protocol type;
        /// </summary>
        enum ProtocolType
        {
            GGP = 3,
            ICMP = 1,
            IDP = 22,
            IGMP = 2,
            IP = 4,
            ND = 77,
            PUP = 12,
            TCP = 6,
            UDP = 17,
            OTHERS = -1
        }

        /// <summary>
        /// the original packet sniffed in the underlying layer;
        /// </summary>
        private byte[] raw_Packet;

        /// <summary>
        /// the sniffed time;
        /// </summary>
        private DateTime dateTime;

        /// <summary>
        /// protocol type of the packet;
        /// </summary>
        private ProtocolType protocolType;

        private IPAddress src_IPAddress;

        private IPAddress des_IPAddress;

        private int src_Port;

        private int des_Port;

        private int totalLength;

        private int headLength;

        //just for test
        public int HeadLength
        {
            get
            {
                return headLength;
            }
        }


        /// <summary>
        /// using the raw and current system time to initialize the packet;
        /// </summary>
        /// <param name="raw"></param>
        /// <param name="time"></param>
        public Packet(byte[] raw)
        {
            ///all the following exceptions should be caught when invoking this constructor;
            if (raw == null)
                throw new ArgumentNullException();

            ///when the orginal length is less than 20, it must be wrong;
            if (raw.Length < 20)
                throw new ArgumentException();
            raw_Packet = raw;
            dateTime = DateTime.Now;///the time sniffed the packet;
                                    
            ///get the headlength in the packet;
            headLength = (raw[0] & 0x0F) * 4;
            if ((raw[0] & 0x0F) < 5)
                throw new ArgumentException(); // header is wrong for the length is incorrect;

            ///the actual length is the same with the header length indicator?
            if ((raw[2] * 256 + raw[3]) != raw.Length)
                throw new ArgumentException(); // length is incorrect;

            ///get the type of the packet;
            if (Enum.IsDefined(typeof(ProtocolType), (int)raw[9]))
                protocolType = (ProtocolType)raw[9];
            else
                protocolType = ProtocolType.OTHERS;

            src_IPAddress = new IPAddress(BitConverter.ToUInt32(raw, 12));
            des_IPAddress = new IPAddress(BitConverter.ToUInt32(raw, 16));
            totalLength = raw[2] * 256 + raw[3];
            
            ///handle the TCP OR UDP in particular method;
            if (protocolType == ProtocolType.TCP || protocolType == ProtocolType.UDP)
            {
                src_Port = raw[headLength] * 256 + raw[headLength + 1];
                des_Port = raw[headLength + 2] * 256 + raw[headLength + 3];
                if (protocolType == ProtocolType.TCP)
                {
                    headLength += 20;
                }
                else if (protocolType == ProtocolType.UDP)
                {
                    headLength += 8;
                }
            }
            else
            {
                src_Port = -1;
                des_Port = -1;
            }
            
        }

        /// <summary>
        /// the accessible source IP address;
        /// </summary>
        public string Src_IP
        {
            get
            {
                return src_IPAddress.ToString();
                //return "127.0.0.1";
            }
        }

        /// <summary>
        /// the accessible source PORT;
        /// </summary>
        public string Src_PORT
        {
            get
            {
                if (src_Port != -1)
                    return src_Port.ToString();
                else
                    return "";
            }
        }

        /// <summary>
        /// the accessible destination IP address;
        /// </summary>
        public string Des_IP
        {
            get
            {
                return des_IPAddress.ToString();
            }
        }

        /// <summary>
        /// the accessible destination PORT;
        /// </summary>
        public string Des_PORT
        {
            get
            {
                if (des_Port != -1)
                    return des_Port.ToString();
                else
                    return "";
            }
        }

        /// <summary>
        /// the type of the packet;
        /// </summary>
        public string Type
        {
            get
            {
                return protocolType.ToString();
            }
        }

        /// <summary>
        /// the total length of the packet;
        /// </summary>
        public int TotalLength
        {
            get
            {
                return totalLength;
            }
        }

        /// <summary>
        /// the visibly formatted time;
        /// </summary>
        public string Time
        {
            get
            {
                return dateTime.ToLongTimeString();
            }
        }

        /// <summary>
        /// return the hexdecimal format of the packet ignoring the headers in the underlying layers;
        /// </summary>
        /// <returns></returns>
        public string getHexString()
        {
            StringBuilder sb = new StringBuilder(raw_Packet.Length);
            for (int i = headLength; i < TotalLength; i += LineCount)
            {
                for (int j = i; j < TotalLength && j < i + LineCount; j++)
                {
                    sb.Append(raw_Packet[j].ToString("X2") + " ");
                }
                sb.Append("\n");
            }
                return sb.ToString();
        }

        /// <summary>
        /// return the character format of the packet ignoring the headers in the underlying layers;
        /// </summary>
        /// <returns></returns>
        public string getCharString()
        {

            StringBuilder sb = new StringBuilder();
            
            for(int i = this.HeadLength; i < TotalLength; i += LineCount)
            {
                for (int j = i; j < TotalLength && j < i + LineCount; j++)
                {
                    if (raw_Packet[j] > 31 && raw_Packet[j] < 128)
                        sb.Append((char)raw_Packet[j]);
                    else
                        sb.Append(".");
                }
                sb.Append("\n");
            }
            return sb.ToString();
        }
    }
}
