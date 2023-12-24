using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace NETSpeedMonitor.CoreZ.Windows.NetInfo
{
    internal unsafe class NetInfoWrapper
    {
        //参考网站：https://learn.microsoft.com/en-us/openspecs/windows_protocols/ms-rrasm/882bec9c-2fb6-4acd-a9b6-dabcab1ac0d6
        private const int NO_ERROR = 0;
        private const int MIB_TCP_STATE_CLOSED = 1;
        private const int MIB_TCP_STATE_LISTEN = 2;
        private const int MIB_TCP_STATE_SYN_SENT = 3;
        private const int MIB_TCP_STATE_SYN_RCVD = 4;
        private const int MIB_TCP_STATE_ESTAB = 5;
        private const int MIB_TCP_STATE_FIN_WAIT1 = 6;
        private const int MIB_TCP_STATE_FIN_WAIT2 = 7;
        private const int MIB_TCP_STATE_CLOSE_WAIT = 8;
        private const int MIB_TCP_STATE_CLOSING = 9;
        private const int MIB_TCP_STATE_LAST_ACK = 10;
        private const int MIB_TCP_STATE_TIME_WAIT = 11;
        private const int MIB_TCP_STATE_DELETE_TCB = 12;

        #region TCP
        [DllImport("iphlpapi.dll", SetLastError = true)]
        private extern static int GetTcpStatistics(ref MIB_TCPSTATS pStats);

        [DllImport("iphlpapi.dll", SetLastError = true)]
        private extern static int GetTcpTable(byte[] pTcpTable, out int pdwSize, bool Order);

        [DllImport("iphlpapi.dll", SetLastError = true)]
        private extern static int GetExtendedTcpTable(IntPtr pTable, ref UInt32 dwOutBuffSize, bool sort,
                                int ipVersion, TCP_TABLE_CLASS tbClass, int reserved);
        #endregion

        #region UDP
        [DllImport("iphlpapi.dll", SetLastError = true)]
        private extern static int GetUdpStatistics(ref MIB_UDPSTATS pStats);

        [DllImport("iphlpapi.dll", SetLastError = true)]
        private static extern int GetUdpTable(byte[] UcpTable, out int pdwSize, bool bOrder);

        [DllImport("iphlpapi.dll", SetLastError = true)]
        private static extern int GetExtendedUdpTable(IntPtr pTable, ref UInt32 dwOutBufLen, bool sort,
                                int ipVersion, UDP_TABLE_CLASS tblClass, int reserved);
        #endregion


        public static MIB_TCPSTATS GetTcpStats()
        {
            MIB_TCPSTATS tcpStats = new MIB_TCPSTATS();
            GetTcpStatistics(ref tcpStats);
            Console.WriteLine("{0} --.", tcpStats.dwActiveOpens);
            return tcpStats;
        }

        private static string ConvertToIPv4Srting(UInt32 ipaddress)
        {
            //将UInt32 转换成为字节数
            byte[] bytes = BitConverter.GetBytes(ipaddress);

            //if (BitConverter.IsLittleEndian)
            //{
            //    Array.Reverse(bytes);
            //}

            string ipv4String = $"{bytes[0]}.{bytes[1]}.{bytes[2]}.{bytes[3]}";

            return ipv4String;
        }

        private static UInt16 convert_Port(UInt32 dwPort)
        {
            byte[] b = new byte[2];
            // 二进制数据向右移动8位，获取高位字节
            b[0] = byte.Parse((dwPort >> 8).ToString());
            // 二进制数据与 0xFF 的二进制表示进行按位与操作，获取低位字节
            b[1] = byte.Parse((dwPort & 0xFF).ToString());
            //
            //使用BitConverter将字节数组转换为UInt16（16位无符号整数）
            return BitConverter.ToUInt16(b, 0);
        }

        public static string convert_state(int state)
        {
            string strg_state = "";
            switch (state)
            {
                case NO_ERROR: strg_state = "NULL"; break;
                case MIB_TCP_STATE_CLOSED: strg_state = "CLOSED"; break;
                case MIB_TCP_STATE_LISTEN: strg_state = "LISTEN"; break;
                case MIB_TCP_STATE_SYN_SENT: strg_state = "SYN_SENT"; break;
                case MIB_TCP_STATE_SYN_RCVD: strg_state = "SYN_RCVD"; break;
                case MIB_TCP_STATE_ESTAB: strg_state = "ESTAB"; break;
                case MIB_TCP_STATE_FIN_WAIT1: strg_state = "FIN_WAIT1"; break;
                case MIB_TCP_STATE_FIN_WAIT2: strg_state = "FIN_WAIT2"; break;
                case MIB_TCP_STATE_CLOSE_WAIT: strg_state = "CLOSE_WAIT"; break;
                case MIB_TCP_STATE_CLOSING: strg_state = "CLOSING"; break;
                case MIB_TCP_STATE_LAST_ACK: strg_state = "LAST_ACK"; break;
                case MIB_TCP_STATE_TIME_WAIT: strg_state = "TIME_WAIT"; break;
                case MIB_TCP_STATE_DELETE_TCB: strg_state = "DELETE_TCB"; break;
            }
            return strg_state;
        }

        public static bool GetProcessUdpConns(ref MIB_UDPTABLE_OWNER_PID UdpConns)
        {

            UInt32* ptable = (UInt32*)IntPtr.Zero;
            UInt32 dwSize = 0;
            GetExtendedUdpTable((IntPtr)ptable, ref dwSize, true,
                            AFType.AF_INET, UDP_TABLE_CLASS.UDP_TABLE_OWNER_PID, 0);
            char* tmp = stackalloc char[(int)dwSize];
            ptable = (UInt32*)tmp;

            if (GetExtendedUdpTable((IntPtr)ptable, ref dwSize, true,
                            AFType.AF_INET, UDP_TABLE_CLASS.UDP_TABLE_OWNER_PID, 0) != NO_ERROR)
                return false;

            UdpConns.dwNumEntries = ptable[0];
            UdpConns.table = new MIB_UDPROW_OWNER_PID[UdpConns.dwNumEntries];
            MIB_UDPROW_OWNER_PID* row = (MIB_UDPROW_OWNER_PID*)&ptable[1];
            UInt32 J = 0;
            for (int i = 0; i < UdpConns.dwNumEntries; i++)
            {
                //Console.WriteLine("pid:{0}==>{1}:{2} -- upd", row[i].dwOwningPid,
                //    ConvertToIPv4Srting(row[i].dwLocalAddr), convert_Port(row[i].dwLocalPort));

                //if (!IP.isSpecialIP(row[i].dwLocalAddr))
                UdpConns.table[J++] = row[i];
            }
            UdpConns.dwNumEntries = J;
            return true;
        }

        public static bool GetProcessTcpConns(ref MIB_TCPTABLE_OWNER_PID ExConns)
        {
            UInt32* ptable = (UInt32*)IntPtr.Zero;
            UInt32 dwSize = 0;
            GetExtendedTcpTable((IntPtr)ptable, ref dwSize, true,
                            AFType.AF_INET, TCP_TABLE_CLASS.TCP_TABLE_OWNER_PID_ALL, 0);

            char* tmp = stackalloc char[(int)dwSize];
            ptable = (UInt32*)tmp;

            if (GetExtendedTcpTable((IntPtr)ptable, ref dwSize, true,
                            AFType.AF_INET, TCP_TABLE_CLASS.TCP_TABLE_OWNER_PID_ALL, 0) != NO_ERROR)
            {
                return false;
            }

            ExConns.dwNumEntries = (UInt32)ptable[0];
            ExConns.table = new MIB_TCPROW_OWNER_PID[ExConns.dwNumEntries];
            MIB_TCPROW_OWNER_PID* row = (MIB_TCPROW_OWNER_PID*)(&ptable[1]);
            UInt32 J = 0;
            for (int i = 0; i < ExConns.dwNumEntries; i++)
            {
                //Console.WriteLine("pid:{0}==>{1}:{2} <---> {3}:{4}--->{5}", row[i].dwOwningPid,
                //    ConvertToIPv4Srting(row[i].dwLocalAddr), convert_Port(row[i].dwLocalPort),
                //    ConvertToIPv4Srting(row[i].dwRemoteAddr), convert_Port(row[i].dwRemotePort),
                //    row[i].dwState);

                // 1. localaddr = 127.0.0.1 or 0.0.0.0 => False
                // 2. remoteaddr = 127.0.0.1 or 0.0.0.0 ==> False
                // 3. localaddr = 127.0.0.1 and remoteaddr = 127.0.0.1 ==> Ture
                // 4. Other ==> True
                //if ((!IP.isSpecialIP(row[i].dwLocalAddr) && !IP.isSpecialIP(row[i].dwRemoteAddr))
                //    //||(IP.isLookupIP(row[i].dwLocalAddr) && IP.isLookupIP(row[i].dwRemoteAddr))
                //    )
                var state = Convert.ToInt32(row[i].dwState);
                if (state == MIB_TCP_STATE_CLOSED || state == MIB_TCP_STATE_TIME_WAIT || state == MIB_TCP_STATE_DELETE_TCB)
                    continue;
                //{
                ExConns.table[J++] = row[i];
                //}
            }
            ExConns.dwNumEntries = J;

            return true;
        }


        public static bool GetUdpRowList(ref List<UDP_INFO_PID> udpinfolist)
        {
            MIB_UDPTABLE_OWNER_PID ttable = new();
            GetProcessUdpConns(ref ttable);
            if (ttable.dwNumEntries <= 0)
                return false;

            for (int i = 0; i < ttable.dwNumEntries; i++)
            {
                //if (ttable.table[i].dwOwningPid == 0)
                //    continue;

                UDP_INFO_PID tmp = new();
                tmp.dwpid = Convert.ToInt32(ttable.table[i].dwOwningPid);
                tmp.dwlocaladdr = ConvertToIPv4Srting(ttable.table[i].dwLocalAddr);
                tmp.dwlocalport = Convert.ToInt32(convert_Port(ttable.table[i].dwLocalPort));

                udpinfolist.Add(tmp);
            }

            //udpinfolist.Sort((s1, s2) => s2.dwlocaladdr.CompareTo(s1.dwlocaladdr));

            return true;
        }

        public static bool GetTcpRowList(ref List<TCP_INFO_PID> tcpinfolist)
        {
            MIB_TCPTABLE_OWNER_PID ttable = new MIB_TCPTABLE_OWNER_PID();
            GetProcessTcpConns(ref ttable);
            if (ttable.dwNumEntries <= 0)
                return false;


            for (int i = 0; i < ttable.dwNumEntries; i++)
            {
                //if (ttable.table[i].dwOwningPid == 0)
                //    continue;

                TCP_INFO_PID tmp = new();
                tmp.dwpid = Convert.ToInt32(ttable.table[i].dwOwningPid);
                tmp.dwlocaladdr = ConvertToIPv4Srting(ttable.table[i].dwLocalAddr);
                tmp.dwlocalport = Convert.ToInt32(convert_Port(ttable.table[i].dwLocalPort));
                tmp.dwremoteaddr = ConvertToIPv4Srting(ttable.table[i].dwRemoteAddr);
                tmp.dwremoteport = Convert.ToInt32(convert_Port(ttable.table[i].dwRemotePort));
                tmp.dwstate = Convert.ToInt32(ttable.table[i].dwState);

                tcpinfolist.Add(tmp);
            }

            //tcpinfolist.Sort((s1, s2) => s1.dwlocalport.CompareTo(s2.dwlocalport));
            return true;
        }
    }
}
