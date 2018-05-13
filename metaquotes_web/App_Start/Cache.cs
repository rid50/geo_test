using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Web;
using System.Web.Hosting;

namespace metaquotes_web
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

        //public GeoBaseHeader(int i)
        //{
        //    version = 0;
        //    name = new byte[32];
        //    timestamp = 0;
        //    records = 0;
        //    offset_ranges = 0;
        //    offset_cities = 0;
        //    offset_locations = 0;
        //}
    }

    //unsafe struct Location
    public struct Location
    {
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

        public byte[] country;       // название страны (случайная строка с префиксом "cou_")
        public byte[] region;        // название области (случайная строка с префиксом "reg_")
        public byte[] postal;        // почтовый индекс (случайная строка с префиксом "pos_")
        public byte[] city;          // название города (случайная строка с префиксом "cit_")
        public byte[] organization;  // название организации (случайная строка с префиксом "org_")
        public float latitude;        // широта
        public float longitude;       // долгота
        //public byte[] dump;
        //public fixed byte dump[88];

        //public Location(int i)
        //{
        //    country = new byte[8];
        //    region = new byte[12];
        //    postal = new byte[12];
        //    city = new byte[24];
        //    organization = new byte[32];
        //    latitude = .0F;
        //    longitude = .0F;
        //}
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
            // Linq extension method is based on IEnumerable, must evaluate every item.
            return x.SequenceEqual(y);
        }

        public override int GetHashCode(byte[] obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }
            // quick and dirty, instantly identifies obviously different
            // arrays as being different
            //return obj.Length;
            int i = BitConverter.ToString(obj).GetHashCode();
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

        static T PtrToStruct<T>(byte[] data) where T : struct
        {
            unsafe
            {
                fixed (byte* ptr = &data[0])
                {
                    return (T)Marshal.PtrToStructure(new IntPtr(ptr), typeof(T));
                }
            }
        }

        public static void LoadCache()
        {
            string path = AppDomain.CurrentDomain.GetData("DataDirectory").ToString();
            //FileStream fileStream = File.OpenRead(Path.Combine(Directory.GetCurrentDirectory(), "geobase.dat"));
            //FileStream fileStream = File.OpenRead(HostingEnvironment.MapPath("~/App_Data/geobase.dat"));
            FileStream fileStream = File.OpenRead(path + "/geobase.dat");

            //using (FileStream fs = File.OpenRead(Path.Combine(Directory.GetCurrentDirectory(), "geobase.dat")))
            using (BinaryReader br = new BinaryReader(fileStream))
            {
                //Debug.WriteLine("Length of the stream: " + br.BaseStream.Length);

                //byte[] bt = new byte[br.BaseStream.Length];

                Stopwatch sw = new Stopwatch();
                sw.Start();

                //bt = br.ReadBytes(bt.Length);

                //sw.Stop();
                //Console.WriteLine("Elapsed={0}", sw.Elapsed);
                //Console.WriteLine("TotalMilliseconds: " + sw.Elapsed.TotalMilliseconds);

                //return;

                GeoBaseHeader hd;
                hd.version = br.ReadInt32();
                //Console.WriteLine("hd.version: " + hd.version);

                hd.name = br.ReadBytes(32);
                //Console.WriteLine(System.Text.Encoding.Default.GetString(hd.name));

                hd.timestamp = br.ReadUInt64();
                //var dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds((long)hd.timestamp).DateTime;
                //Console.WriteLine("hd.timestamp: " + dateTimeOffset);

                hd.records = br.ReadInt32();
                //Console.WriteLine("hd.records: " + hd.records);
                hd.offset_ranges = br.ReadUInt32();
                //Console.WriteLine("hd.offset_ranges: " + hd.offset_ranges);
                hd.offset_cities = br.ReadUInt32();
                Console.WriteLine("hd.offset_cities: " + hd.offset_cities);
                hd.offset_locations = br.ReadUInt32();
                Console.WriteLine("hd.offset_locations: " + hd.offset_locations);

                //sw.Stop();
                ////Console.WriteLine("Elapsed={0}", sw.Elapsed);
                //Console.WriteLine("TotalMilliseconds: " + sw.Elapsed.TotalMilliseconds);
                //sw.Start();

                //br.BaseStream.Position = hd.offset_ranges;

                //Console.WriteLine(br.BaseStream.Position);

                ips = new Ip[hd.records];

                for (int i = 0; i < hd.records; i++)
                {
                    ips[i].ip_from = br.ReadUInt32();
                    ips[i].ip_to = br.ReadUInt32();
                    ips[i].location_index = br.ReadUInt32();
                }

                //Console.WriteLine(br.BaseStream.Position);


                //Console.WriteLine("ip_from: " + IpUintToIpString(ips[0].ip_from));
                //Console.WriteLine("ip_to: " + IpUintToIpString(ips[0].ip_to));
                //Console.WriteLine("location_index: " + ips[10000].location_index);

                //Console.WriteLine("ip_from: " + ips[0].ip_from);
                //Console.WriteLine("ip_to: " + ips[0].ip_to);
                //Console.WriteLine("location_index: " + ips[0].location_index);

                //hd.version = br.ReadInt32();
                //Console.WriteLine("hd.version: " + hd.version);
                //br.BaseStream.Position = 0;
                //hd.version = br.ReadInt32();
                //Console.WriteLine("hd.version: " + hd.version);

                //br.BaseStream.Position = hd.offset_locations;

                //byte[] country = new byte[8];
                //country = br.ReadBytes(8);
                //Console.WriteLine(System.Text.Encoding.Default.GetString(country));
                //byte[] reg = new byte[8];
                //reg = br.ReadBytes(8);
                //Console.WriteLine(System.Text.Encoding.Default.GetString(reg));

                //Location loc = default(Location);
                //int structSize;
                //unsafe
                //{
                //    structSize = Marshal.SizeOf(loc);
                //}
                //Location loc = default(Location);

                //sw.Stop();
                ////Console.WriteLine("Elapsed={0}", sw.Elapsed);
                //Console.WriteLine("TotalMilliseconds: " + sw.Elapsed.TotalMilliseconds);
                //sw.Start();
                //int size = Marshal.SizeOf(typeof(Location));
                //Location[] locations = new Location[hd.records];
                //Dictionary<byte[], int> cities = new Dictionary<byte[], int>();
                //cities = new Dictionary<byte[], int>(new ByteArrayComparer());
                cities = new Dictionary<byte[], int>();
                locations = new Location[hd.records];
                for (int i = 0; i < hd.records; i++)
                {
                    //locations[i] = PtrToStruct<Location>(br.ReadBytes(size));

                    //Location location = default(Location);

                    //location.country = br.ReadBytes(8);
                    //location.region = br.ReadBytes(12);
                    //location.postal = br.ReadBytes(12);
                    //location.city = br.ReadBytes(24);
                    //location.organization = br.ReadBytes(32);
                    //location.latitude = br.ReadSingle();
                    //location.longitude = br.ReadSingle();

                    //locations[i] = location;

                    //byte[] data = br.ReadBytes(Marshal.SizeOf(typeof(Location)));
                    //unsafe
                    //{

                    //    //fixed (byte* ptr = locations[i].dump )
                    //    fixed (byte* ptr = &data[0])
                    //    {
                    //        locations[i] = (Location)Marshal.PtrToStructure(new IntPtr(ptr), typeof(Location));
                    //        //ptr[1] = 0x02;
                    //        //*ptr = 0x01;
                    //        //locations[i].dump = ptr;

                    //    }
                    //    //fixed { locations[i].dump[0] = 0x00; }

                    //}

                    //locations[i].dump = br.ReadBytes(88);

                    locations[i].country = br.ReadBytes(8);
                    locations[i].region = br.ReadBytes(12);
                    locations[i].postal = br.ReadBytes(12);
                    br.ReadBytes(4);
                    locations[i].city = br.ReadBytes(20);
                    //br.Read(locations[i].city, 4, 20);

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

                    //locations[i] = loc;
                    //loc = ReadUsingMarshalUnsafe<Location>(br.ReadBytes(8));
                    //Console.WriteLine(System.Text.Encoding.Default.GetString(locations[0].country));
                    //Console.WriteLine(System.Text.Encoding.Default.GetString(loc.region));
                    //Console.WriteLine(System.Text.Encoding.Default.GetString(loc.postal));
                    //Console.WriteLine(System.Text.Encoding.Default.GetString(loc.city));
                    //Console.WriteLine(System.Text.Encoding.Default.GetString(loc.organization));
                    //Console.WriteLine(loc.latitude);
                    //Console.WriteLine(loc.longitude);

                }

                Debug.WriteLine(System.Text.Encoding.Default.GetString(locations[0].city));
                //Debug.WriteLine("\n");
                //Console.WriteLine(br.BaseStream.Position);

                //unsafe
                //{
                //    fixed(byte* ptr = locations[0].country) {
                //        //int* pp = locations[0].country;
                //        //Console.WriteLine("country: " + System.Text.Encoding.Default.GetString(ptr, ));
                //    }
                //}

                //br.BaseStream.Position = hd.offset_cities;
                //Console.WriteLine(br.BaseStream.Position);

                //byte[][] cities = new byte[hd.records][];
                //sw.Stop();
                ////Console.WriteLine("Elapsed={0}", sw.Elapsed);
                //Console.WriteLine("TotalMilliseconds: " + sw.Elapsed.TotalMilliseconds);
                //sw.Start();

                //byte[][] cities = new byte[hd.records][];
                ////uint[] cities = new uint[hd.records];

                //for (int i = 0; i < hd.records; i++)
                //{
                //    //cities[i] = br.ReadUInt32();
                //    cities[i] = br.ReadBytes(4);
                //}

                //if (BitConverter.IsLittleEndian)
                  //  Array.Reverse(cities[0]);

                //uint value = BitConverter.ToUInt32(cities[0], 0);

                //Debug.WriteLine("city: " + value);

                //Debug.WriteLine("cities: " + System.Text.Encoding.Default.GetString(cities[0]));

                //Debug.WriteLine("\n");

                //Console.WriteLine(br.BaseStream.Position);

                //br.BaseStream.Position = cities[0];

                //Console.WriteLine("cities: " + cities[0]);
                //if (BitConverter.IsLittleEndian)
                //Array.Reverse(cities[0]);

                //int indx = BitConverter.ToInt32(cities[0], 0);
                //int indx = cities[0] - hd.offset_locations;
                //Console.WriteLine(indx);
                //Console.WriteLine(System.Text.Encoding.Default.GetString(locations[indx].country));

                //br.BaseStream.Position = cities[0];

                //Console.WriteLine(System.Text.Encoding.Default.GetString(br.ReadBytes(8)));
                //Console.WriteLine(cities[0]);

                //Console.WriteLine(cities.Length);
                //Console.WriteLine(br.BaseStream.Position);

                //br.BaseStream.Position = hd.offset_cities;


                //Location[] locations = new Location[hd.records];
                //byte[] byt = new Byte[10];
                ////br.Read((byte[])locations, 0, locations.Length);
                ////Location loc = new Location();
                //int structSize;
                //unsafe {
                //    structSize = sizeof(Location);
                //}

                //byte[] bt = new Byte[structSize];
                //bt = br.ReadBytes(structSize);
                //bt.CopyTo(locations, 0);

                ////Console.WriteLine(((Location)locations[0]).
                ////int size = System.Runtime.InteropServices.Marshal.SizeOf(typeof(Location));
                //Console.WriteLine(structSize);

                sw.Stop();
                //Console.WriteLine("Elapsed={0}", sw.Elapsed);
                Debug.WriteLine("TotalMilliseconds: " + sw.Elapsed.TotalMilliseconds);

            }



            //Console.WriteLine("Hello World!");
        }
    }
}