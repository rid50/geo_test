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

        static Dictionary<byte[], int> cities = null;

        // GET city/location
        public JsonResult GetCity(string city)
        {
            if (cities == null)
            {
                cities = new Dictionary<byte[], int>();
                unsafe
                {
                    byte[] bytes = new byte[24];
                    fixed (Location* p = Cache.locations)
                    {
                        for (int i = 0; i < Cache.records; i++)
                        {
                            string str = new string((sbyte*)p[i].city);
                            cities.Add(System.Text.Encoding.ASCII.GetBytes(str.Substring(4)), i);
                        }
                    }
                }
            }

            //Dictionary<byte[], int>.KeyCollection keyColl = Cache.cities.Keys;
            Dictionary<byte[], int>.KeyCollection keyColl = cities.Keys;

            byte[] key = System.Text.Encoding.ASCII.GetBytes(city.Substring(4));

            //Array.Resize(ref key, 20);

            int index = -1;
            foreach (byte[] k in keyColl)
            {
                if (Compare(key, k)) {
                    if (cities.ContainsKey(k))
                        index = cities[k];

                    break;
                }
            }

            object data;
            if (index != -1)
                data = Helper.GenerateJsonResult(Cache.locations[index]);
            else
                data = new { error = "Данные отсутствуют" };

            return new JsonResult { Data = data, ContentType = "application/json; charset=UTF-8" };
        }
    }
}
