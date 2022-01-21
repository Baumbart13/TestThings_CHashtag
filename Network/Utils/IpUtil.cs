using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace Network.Utils;

public static class IpUtil
{
    public static IEnumerable<IPAddress> GetAllLocalIPv4(NetworkInterfaceType type)
    {
        if (!NetworkInterface.GetIsNetworkAvailable())
        {
            yield return IPAddress.None;
        }
        foreach (var item in NetworkInterface.GetAllNetworkInterfaces())
        {
            if (item.NetworkInterfaceType != type || item.OperationalStatus != OperationalStatus.Up)
            {
                continue;
            }

            foreach (var ip in item.GetIPProperties().UnicastAddresses)
            {
                if (ip.Address.AddressFamily != AddressFamily.InterNetwork)
                {
                    continue;
                }

                yield return ip.Address;
            }
        }
    }

    public static IEnumerable<IEnumerable<IPAddress>> GetAllLocalIPv4()
    {
        if (!NetworkInterface.GetIsNetworkAvailable())
        {
            yield return new List<IPAddress>();
        }
        foreach (var type in Enum.GetValues<NetworkInterfaceType>())
        {
            yield return GetAllLocalIPv4(type);
        }
    }

    public static IPAddress GetLocalIPv4(NetworkInterfaceType type)
    {
        foreach (var item in NetworkInterface.GetAllNetworkInterfaces())
        {
            if (item.NetworkInterfaceType != type || item.OperationalStatus != OperationalStatus.Up)
            {
                continue;
            }

            foreach (var ip in item.GetIPProperties().UnicastAddresses)
            {
                if (ip.Address.AddressFamily != AddressFamily.InterNetwork)
                {
                    continue;
                }

                return ip.Address;
            }
        }
        return IPAddress.None;
    }
}