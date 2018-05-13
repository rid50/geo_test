using metaquotes_web.Controllers;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Web.Http;
using System.Web.Mvc;

namespace metaquotes_web
{
    public class CityController : ApiController
    {
        [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
        static extern int memcmp(byte[] b1, byte[] b2, long count);

        public bool Compare(byte[] b1, byte[] b2)
        {
            return b1.Length == b2.Length && memcmp(b1, b2, b1.Length) == 0;
        }

        // GET city/location
        public JsonResult GetCity(string city)
        {
            Dictionary<byte[], int>.KeyCollection keyColl = Cache.cities.Keys;

            byte[] key = System.Text.Encoding.ASCII.GetBytes(city.Substring(4));

            Array.Resize(ref key, 20);

            int index = -1;
            foreach (byte[] k in keyColl)
            {
                if (Compare(key, k)) {
                    if (Cache.cities.ContainsKey(k))
                        index = Cache.cities[k];

                    break;
                }
            }

            object data;
            if (index != -1)
                data = Helper.GenerateJsonResult(Cache.locations[index]);
            else
                data = new { error = "Данные отсутствуют" };

            return new JsonResult { Data = data, ContentType = "application/json; charset=UTF-8" };

            //if (index != -1)
            //{
            //    var data = new
            //    {
            //        city = System.Text.Encoding.UTF8.GetString(Cache.locations[index].city).Replace("\0", string.Empty),
            //        country = System.Text.Encoding.UTF8.GetString(Cache.locations[index].country).Replace("\0", string.Empty).Substring(4),
            //        region = System.Text.Encoding.UTF8.GetString(Cache.locations[index].region).Replace("\0", string.Empty).Substring(4),
            //        postal = System.Text.Encoding.UTF8.GetString(Cache.locations[index].postal).Replace("\0", string.Empty).Substring(4),
            //        organization = System.Text.Encoding.UTF8.GetString(Cache.locations[index].organization).Replace("\0", string.Empty).Substring(4),
            //        latitude = Cache.locations[index].latitude,
            //        longitude = Cache.locations[index].longitude
            //    };

            //    return new JsonResult { Data = data, ContentType = "application/json; charset=UTF-8" };
            //}
            //else
            //    return new JsonResult { Data = new { error = "Данные отсутствуют" }, ContentType = "application/json; charset=UTF-8" };
        }
    }
}
