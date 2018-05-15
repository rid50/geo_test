using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;

namespace metaquotes_web.Controllers
{
    public class Helper
    {
        public static string IpUintToIpString(uint ipUint)
        {
            byte[] byteArray = BitConverter.GetBytes(ipUint);
            Array.Reverse(byteArray);
            return new IPAddress(byteArray).ToString();
        }

        public static uint IpStringToIpUint(string ipString)
        {
            var ipAddress = IPAddress.Parse(ipString);
            var ipBytes = ipAddress.GetAddressBytes();
            var ip = (uint)ipBytes[0] << 24;
            ip += (uint)ipBytes[1] << 16;
            ip += (uint)ipBytes[2] << 8;
            ip += (uint)ipBytes[3];
            return ip;
        }

        public static unsafe object GenerateJsonResult(Location location)
        {
            return new
            {
                city = System.Text.Encoding.UTF8.GetString(location.city, 24).Replace("\0", string.Empty),
                country = System.Text.Encoding.UTF8.GetString(location.country, 8).Replace("\0", string.Empty).Substring(4),
                region = System.Text.Encoding.UTF8.GetString(location.region, 12).Replace("\0", string.Empty).Substring(4),
                postal = System.Text.Encoding.UTF8.GetString(location.postal, 12).Replace("\0", string.Empty).Substring(4),
                organization = System.Text.Encoding.UTF8.GetString(location.organization, 32).Replace("\0", string.Empty).Substring(4),
                latitude = location.latitude,
                longitude = location.longitude
            };
        }
    }
}