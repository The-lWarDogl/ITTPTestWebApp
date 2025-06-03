using System.Net;

using Microsoft.AspNetCore.Http;

namespace ITTPTestWebApp.Network
{
    static partial class ServerCommon
    {
        public static bool CheckRemoteAddress(HttpContext context, List<string>? permittedAddresses)
        {
            string? headerIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
            IPAddress? address = IPAddress.TryParse(headerIp, out var parsedIp)
                ? parsedIp
                : context.Connection.RemoteIpAddress;

            if (address == null) { return false; }
            if (permittedAddresses == null) { return true; }
            if (permittedAddresses.Contains(address.ToString())) { return true; }

            return permittedAddresses
                .Where(a => a.Contains('/'))
                .Select(a => a.Split('/'))
                .Where(parts => parts.Length == 2 && IPAddress.TryParse(parts[0], out _) && int.TryParse(parts[1], out _))
                .Select(parts => (IPAddress.Parse(parts[0]), int.Parse(parts[1])))
                .Any(cidr => IsInCidrRange(address, cidr.Item1, cidr.Item2));
        }

        private static bool IsInCidrRange(IPAddress ip, IPAddress networkIP, int prefixLength)
        {
            byte[] ipBytes = ip.GetAddressBytes(), networkBytes = networkIP.GetAddressBytes();
            int fullBytes = prefixLength / 8, remainingBits = prefixLength % 8;
            return ipBytes.Take(fullBytes).SequenceEqual(networkBytes.Take(fullBytes)) &&
                   (remainingBits == 0 ||
                    (ipBytes[fullBytes] & (0xFF << (8 - remainingBits))) ==
                    (networkBytes[fullBytes] & (0xFF << (8 - remainingBits))));
        }
    }
}
