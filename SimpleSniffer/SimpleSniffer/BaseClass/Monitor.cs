using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SimpleSniffer.BaseClass
{
    class Monitor
    {
        private const int SECURITY_BUILTIN_DOMAIN_RID = 0x20;
        private const int DOMAIN_ALIAS_RID_ADMINS = 0x220;
        
        private const int IOC_VENDOR = 0x18000000;
        private const int IOC_IN = -2147483648; //0x80000000;
        private const int SIO_RCVALL = IOC_IN | IOC_VENDOR | 1;
        private const int BUF_SIZE = 1024 * 1024;


        private Socket monitor_Socket;
        private IPAddress ipAddress;
        private byte[] buffer;

        public Monitor(IPAddress ip) 
        {
            this.ipAddress = ip;
			this.buffer = new byte[BUF_SIZE];
		}
		
		~Monitor() {
			stop();
		}

        public void start()
        {
            if (monitor_Socket == null)
            {
                try
                {
                    
                    if (ipAddress.AddressFamily == AddressFamily.InterNetwork)
                    {
                        monitor_Socket = new Socket(AddressFamily.InterNetwork, SocketType.Raw, System.Net.Sockets.ProtocolType.IP);
                    }
                    else
                    {
                        monitor_Socket = new Socket(AddressFamily.InterNetworkV6, SocketType.Raw, System.Net.Sockets.ProtocolType.IP);
                    }
                    monitor_Socket.Bind(new IPEndPoint(ipAddress, 0));
                    monitor_Socket.IOControl(SIO_RCVALL, BitConverter.GetBytes((int)1), null);
                    monitor_Socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(this.OnReceive), null);
                }
                catch(Exception e)
                {
                    monitor_Socket.Close();
                    monitor_Socket = null;
                    Console.WriteLine(e.ToString());
                }
            }
        }

        public void stop()
        {
            if(monitor_Socket != null)
            {
                monitor_Socket.Close();
                monitor_Socket = null;
            }
        }

        private void OnReceive(IAsyncResult ar)
        {
            try
            {
                int len = monitor_Socket.EndReceive(ar);
                if (monitor_Socket != null)
                {
                    byte[] receivedBuffer = new byte[len];
                    Array.Copy(buffer, 0, receivedBuffer, 0, len);
                    try
                    {
                        Packet packet = new Packet(receivedBuffer);
                        OnNewPacket(packet);
                    }
                    catch (ArgumentNullException ane)
                    {
                        Console.WriteLine(ane.ToString());
                    }
                    catch (ArgumentException ae)
                    {
                        Console.WriteLine(ae.ToString());
                    }
                }
                monitor_Socket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(this.OnReceive), null);
            }
            catch
            {
                stop();
            }
        }

        protected void OnNewPacket(Packet p)
        {
            if (newPacketEventHandler != null)
            {
                newPacketEventHandler(this, p);
            }
        }

        public event NewPacketEventHandler newPacketEventHandler;

        public delegate void NewPacketEventHandler(Monitor monitor, Packet p);

        /// <summary>
        /// check whether the current user is an administrator or not;
        /// </summary>
        /// <returns></returns>
        private bool IsUserAnAdmin()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        [DllImport("advapi32.dll")]
        private extern static int AllocateAndInitializeSid(byte[] pIdentifierAuthority, byte nSubAuthorityCount, int dwSubAuthority0, int dwSubAuthority1, int dwSubAuthority2, int dwSubAuthority3, int dwSubAuthority4, int dwSubAuthority5, int dwSubAuthority6, int dwSubAuthority7, out IntPtr pSid);


        [DllImport("advapi32.dll")]
        private extern static int CheckTokenMembership(IntPtr TokenHandle, IntPtr SidToCheck, ref int IsMember);


        [DllImport("advapi32.dll")]
        private extern static IntPtr FreeSid(IntPtr pSid);
    }
}
