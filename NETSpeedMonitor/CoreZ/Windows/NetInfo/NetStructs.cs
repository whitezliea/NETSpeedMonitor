using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace NETSpeedMonitor.CoreZ.Windows.NetInfo
{

    public class AFType
    {
        public static int AF_INET = 2;
        public static int AF_INET6 = 23;
    }

    public class IP
    {
        //0b(二进制)|00000001 00000000 00000000 01111111 ==> 十进制 16777343 点十进制1.0.0.127
        public static UInt32 localhost = 16777343;  // =>127.0.0.1
        public static UInt32 systemmsg = 0; //0.0.0.0

        public static bool isSpecialIP(UInt32 ip)
        {
            return (localhost == ip || systemmsg == ip);
        }

        public static bool isLookupIP(UInt32 ip)
        {
            return (localhost == ip);
        }
    }

    public struct TCP_INFO_PID
    {
        public int dwpid;
        public string dwlocaladdr;
        public int dwlocalport;
        public string dwremoteaddr;
        public int dwremoteport;
        public int dwstate;
    }

    #region UDP Structure
    [StructLayout(LayoutKind.Sequential)]
    public struct MIB_UDPSTATS
    {
        public int dwInDatagrams;
        public int dwNoPorts;
        public int dwInErrors;
        public int dwOutDatagrams;
        public int dwNumAddrs;
    }

    public struct MIB_UDPTABLE
    {
        public int dwNumEntries;
        public MIB_UDPROW[] table;
    }

    public struct MIB_UDPROW
    {
        public IPEndPoint Local;
    }

    public struct MIB_UDPTABLE_OWNER_PID
    {
        public UInt32 dwNumEntries;
        public MIB_UDPROW_OWNER_PID[] table;
    }

    public struct UDP_INFO_PID
    {
        public int dwpid;
        public string dwlocaladdr;
        public int dwlocalport;
    }

    public struct MIB_UDPROW_OWNER_PID
    {
        public UInt32 dwLocalAddr;
        public UInt32 dwLocalPort;
        public UInt32 dwOwningPid;
    }

    public enum UDP_TABLE_CLASS
    {
        UDP_TABLE_BASIC,
        UDP_TABLE_OWNER_PID,
        UDP_TABLE_OWNER_MODULE
    }
    #endregion


    #region TCP Structure
    [StructLayout(LayoutKind.Sequential)]
    public struct MIB_TCPSTATS
    {
        public int dwRtoAlgorithm;
        public int dwRtoMin;
        public int dwRtoMax;
        public int dwMaxConn;
        public int dwActiveOpens;
        public int dwPassiveOpens;
        public int dwAttemptFails;
        public int dwEstabResets;
        public int dwCurrEstab;
        public int dwInSegs;
        public int dwOutSegs;
        public int dwRetransSegs;
        public int dwInErrs;
        public int dwOutRsts;
        public int dwNumConns;
    }

    public struct MIB_TCPTABLE
    {
        public int dwNumEntries;
        public MIB_TCPROW[] table;
    }

    public struct MIB_TCPROW
    {
        public string StrgState;
        public int iState;
        public IPEndPoint Local;
        public IPEndPoint Remote;
    }

    public struct MIB_TCPTABLE_OWNER_PID
    {
        public UInt32 dwNumEntries;
        public MIB_TCPROW_OWNER_PID[] table;
    }

    public struct MIB_TCPROW_OWNER_PID
    {
        public UInt32 dwState;
        public UInt32 dwLocalAddr;
        public UInt32 dwLocalPort;
        public UInt32 dwRemoteAddr;
        public UInt32 dwRemotePort;
        public UInt32 dwOwningPid;
    }



    public enum TCP_TABLE_CLASS
    {
        TCP_TABLE_BASIC_LISTENER,
        TCP_TABLE_BASIC_CONNECTIONS,
        TCP_TABLE_BASIC_ALL,
        TCP_TABLE_OWNER_PID_LISTENER,
        TCP_TABLE_OWNER_PID_CONNECTIONS,
        TCP_TABLE_OWNER_PID_ALL,
        TCP_TABLE_OWNER_PID_MODULE_LISTENER,
        TCP_TABLE_OWNER_PID_MODULE_CONNECTIONS,
        TCP_TABLE_OWNER_PID_MODULE_ALL
    }

    #endregion
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct MIB_IFROW
    {
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x100)]
        public string wszName;
        public uint dwIndex;
        public uint dwType;
        public uint dwMtu;
        public uint dwSpeed;
        public uint dwPhysAddrLen;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public byte[] bPhysAddr;
        public uint dwAdminStatus;
        public uint dwOperStatus;
        public uint dwLastChange;
        public uint dwInOctets;
        public uint dwInUcastPkts;
        public uint dwInNUcastPkts;
        public uint dwInDiscards;
        public uint dwInErrors;
        public uint dwInUnknownProtos;
        public uint dwOutOctets;
        public uint dwOutUcastPkts;
        public uint dwOutNUcastPkts;
        public uint dwOutDiscards;
        public uint dwOutErrors;
        public uint dwOutQLen;
        public uint dwDescrLen;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x100)]
        public byte[] bDescr;
    }
}

