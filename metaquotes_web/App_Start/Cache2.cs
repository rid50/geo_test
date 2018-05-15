using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace metaquotes_web_BinaryReader
{
    struct GeoBaseHeader
    {
        public int version;
        public byte[] name;
        public ulong timestamp;
        public int records;
        public uint offset_ranges;
        public uint offset_cities;
        public uint offset_locations;
    }

    //unsafe struct Location
    public struct Location
    {
        // ************************************************** Experimental Unsafe marshaling **************************
        //[MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        //public byte[] country;
        //[MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
        //public byte[] region;
        //[MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
        //public byte[] postal;
        //[MarshalAs(UnmanagedType.ByValArray, SizeConst = 24)]
        //public byte[] city;
        //[MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        //public byte[] organization;

        //public fixed byte country[8];      // название страны (случайная строка с префиксом "cou_")
        //public fixed byte region[12];       // название области (случайная строка с префиксом "reg_")
        //public fixed byte postal[12];       // почтовый индекс (случайная строка с префиксом "pos_")
        //public fixed byte city[24];         // название города (случайная строка с префиксом "cit_")
        //public fixed byte organization[32];   // название организации (случайная строка с префиксом "org_")
        // ************************************************** Experimental Unsafe marshaling **************************

        public byte[] country;       // название страны (случайная строка с префиксом "cou_")
        public byte[] region;        // название области (случайная строка с префиксом "reg_")
        public byte[] postal;        // почтовый индекс (случайная строка с префиксом "pos_")
        public byte[] city;          // название города (случайная строка с префиксом "cit_")
        public byte[] organization;  // название организации (случайная строка с префиксом "org_")
        public float latitude;       // широта
        public float longitude;      // долгота
    }

    public struct Ip
    {
        public uint ip_from;           // начало диапазона IP адресов
        public uint ip_to;             // конец диапазона IP адресов
        public uint location_index;    // индекс записи о местоположении
    }

    public class ByteArrayComparer : EqualityComparer<byte[]>
    {
        public override bool Equals(byte[] x, byte[] y)
        {
            if (x == null || y == null)
            {
                return x == y;
            }
            // Linq extension method
            return x.SequenceEqual(y);
        }

        public override int GetHashCode(byte[] obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }

            return BitConverter.ToString(obj).GetHashCode();
        }
    }

    public class Cache
    {
        public static Ip[] ips;
        public static Location[] locations;
        public static Dictionary<byte[], int> cities;
        //public static uint[] cities;

        static T ToStruct<T>(byte[] data) where T : struct
        {
            var size = Marshal.SizeOf(typeof(T));
            IntPtr ptr = Marshal.AllocHGlobal(size);
            try
            {
                Marshal.Copy(data, 0, ptr, size);
                return (T)Marshal.PtrToStructure(ptr, typeof(T));
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
        }

        // ************************************************** Experimental Unsafe marshaling **************************
        //static T PtrToStruct<T>(byte[] data) where T : struct
        //{
        //    unsafe
        //    {
        //        fixed (byte* ptr = &data[0])
        //        {
        //            return (T)Marshal.PtrToStructure(new IntPtr(ptr), typeof(T));
        //        }
        //    }
        //}

        public static void LoadCache()
        {
            string path = AppDomain.CurrentDomain.GetData("DataDirectory").ToString();
            FileStream fileStream = File.OpenRead(path + "/geobase.dat");

            using (BinaryReader br = new BinaryReader(fileStream))
            {
                //Debug.WriteLine("Length of the stream: " + br.BaseStream.Length);

                // ************************************************** Performance report **************************
                //Stopwatch sw = new Stopwatch();
                //sw.Start();

                GeoBaseHeader hd;
                hd.version = br.ReadInt32();
                hd.name = br.ReadBytes(32);
                hd.timestamp = br.ReadUInt64();
                hd.records = br.ReadInt32();
                hd.offset_ranges = br.ReadUInt32();
                hd.offset_cities = br.ReadUInt32();
                hd.offset_locations = br.ReadUInt32();

                ips = new Ip[hd.records];

                for (int i = 0; i < hd.records; i++)
                {
                    ips[i].ip_from = br.ReadUInt32();
                    ips[i].ip_to = br.ReadUInt32();
                    ips[i].location_index = br.ReadUInt32();
                }

                cities = new Dictionary<byte[], int>();
                locations = new Location[hd.records];
                for (int i = 0; i < hd.records; i++)
                {
                    // ************************************************** Experimental Unsafe marshaling **************************
                    //byte[] data = br.ReadBytes(Marshal.SizeOf(typeof(Location)));
                    //unsafe
                    //{
                    //    fixed (byte* ptr = &data[0])
                    //    {
                    //        locations[i] = (Location)Marshal.PtrToStructure(new IntPtr(ptr), typeof(Location));
                    //    }

                    locations[i].country = br.ReadBytes(8);
                    locations[i].region = br.ReadBytes(12);
                    locations[i].postal = br.ReadBytes(12);
                    br.ReadBytes(4);
                    locations[i].city = br.ReadBytes(20);

                    // ************************************************** Experimental Unsafe marshaling **************************
                    //cities[i] = System.Text.Encoding.UTF8.GetString(Cache.locations[i].city);
                    //if (i < 110)
                    //{
                    //    Debug.WriteLine(System.Text.Encoding.UTF8.GetString(Cache.locations[i].city));
                    //    Debug.WriteLine("\n");
                    //}
                    cities.Add(locations[i].city, i);
                    locations[i].organization = br.ReadBytes(32);
                    locations[i].latitude = br.ReadSingle();
                    locations[i].longitude = br.ReadSingle();
                }

                // ************************************************** Parsing sorted index city byte array **************************
                //byte[][] cities = new byte[hd.records][];
                ////uint[] cities = new uint[hd.records];

                //for (int i = 0; i < hd.records; i++)
                //{
                //    //cities[i] = br.ReadUInt32();
                //    cities[i] = br.ReadBytes(4);
                //}

                //if (BitConverter.IsLittleEndian)
                //  Array.Reverse(cities[0]);

                // ************************************************** Performance report **************************
                //sw.Stop();
                //Debug.WriteLine("TotalMilliseconds: " + sw.Elapsed.TotalMilliseconds);
            }
        }
    }
}