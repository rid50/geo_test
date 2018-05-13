using metaquotes_web.Controllers;
using System;
using System.Net;
using System.Web.Http;
using System.Web.Mvc;

namespace metaquotes_web
{
    public class IpController : ApiController
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

        // GET ip/location
        public JsonResult GetIP(string ip)
        {
            uint uip = IpStringToIpUint(ip);
            int index = -1;
            for (int i = 0; i < Cache.ips.Length; i++)
            {
                if (uip < Cache.ips[i].ip_from)
                    continue;
                if (uip > Cache.ips[i].ip_to)
                    continue;
                index = (int)Cache.ips[i].location_index;
            }

            var data = Helper.GenerateJsonResult(Cache.locations[index], index != -1);

            return new JsonResult { Data = data, ContentType = "application/json; charset=UTF-8" };
        }
    }
}
