using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Web;

namespace WebApplication1
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct GeoBaseHeader
    {
        public int version;
        public fixed byte name[32];
        public ulong timestamp;
        public int records;
        public uint offset_ranges;
        public uint offset_cities;
        public uint offset_locations;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct Location
    {
        public fixed byte country[8];       // название страны (случайная строка с префиксом "cou_")
        public fixed byte region[12];       // название области (случайная строка с префиксом "reg_")
        public fixed byte postal[12];       // почтовый индекс (случайная строка с префиксом "pos_")
        //[MarshalAs(UnmanagedType.ByValArray, SizeConst = 24)]
        //public byte[] city;         // название города (случайная строка с префиксом "cit_")
        public fixed byte city[24];         // название города (случайная строка с префиксом "cit_")
        public fixed byte organization[32]; // название организации (случайная строка с префиксом "org_")
        public float latitude;              // широта
        public float longitude;             // долгота
    }

    struct Ip
    {
        public uint ip_from;           // начало диапазона IP адресов
        public uint ip_to;             // конец диапазона IP адресов
        public uint location_index;    // индекс записи о местоположении
    }

    class Cache
    {
        public static int records;
        public static Ip[] ips;
        public static Location[] locations;
        //public static Dictionary<byte[], int> cities;

        const uint GENERIC_READ = 0x80000000;
        const uint OPEN_EXISTING = 3;

        [DllImport("kernel32", SetLastError = true, ThrowOnUnmappableChar = true, CharSet = CharSet.Unicode)]
        static extern unsafe IntPtr CreateFile
        (
            string FileName,          // file name
            uint DesiredAccess,       // access mode
            uint ShareMode,           // share mode
            uint SecurityAttributes,  // Security Attributes
            uint CreationDisposition, // how to create
            uint FlagsAndAttributes,  // file attributes
            int hTemplateFile         // handle to template file
        );

        [DllImport("kernel32", SetLastError = true)]
        static extern unsafe bool ReadFile
        (
            IntPtr hFile,             // handle to file
            void* pBuffer,            // data buffer
            int NumberOfBytesToRead,  // number of bytes to read
            int* pNumberOfBytesRead,  // number of bytes read
            int Overlapped            // overlapped buffer
        );

        [DllImport("kernel32", SetLastError = true)]
        static extern unsafe bool CloseHandle
        (
            IntPtr hObject // handle to object
        );

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

        public static void LoadCache()
        {
            //string path = AppDomain.CurrentDomain.GetData("DataDirectory").ToString();
            string path = HttpContext.Current.Server.MapPath("~/App_Data/geobase.dat");
            if (!System.IO.File.Exists(path))
            {
                System.Console.WriteLine("File  not found.");
                return;
            }

            IntPtr handle = CreateFile(path, GENERIC_READ, 0, 0, OPEN_EXISTING, 0, 0);
            if (handle == new IntPtr(-1))
            {
                Debug.WriteLine("Can't open file geobase.dat");
                return;
            }

            Stopwatch sw = new Stopwatch();
            sw.Start();

            GeoBaseHeader[] hd = new GeoBaseHeader[1];
            int n = 0;
            unsafe {
                fixed (GeoBaseHeader* p = hd)
                {
                    bool b = ReadFile(handle, p, sizeof(GeoBaseHeader), &n, 0);
                    records = hd[0].records;
                }
            }

            //unsafe
            //{
            //    fixed (GeoBaseHeader* p = hd)
            //    {
            //        Console.WriteLine(System.Text.Encoding.Default.GetString(p->name, 32));
            //    }
            //}

            //Console.WriteLine("hd.records: " + hd[0].records);
            //var dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds((long)hd[0].timestamp).DateTime;
            //Console.WriteLine("hd.timestamp: " + dateTimeOffset);

            ips = new Ip[hd[0].records];
            n = 0;
            unsafe
            {
                fixed (Ip* p = ips)
                {
                    ReadFile(handle, p, sizeof(Ip) * hd[0].records, &n, 0);
                }
            }

            //Console.WriteLine("ip_from: " + IpUintToIpString(ips[1].ip_from));
            //Console.WriteLine("ip_to: " + IpUintToIpString(ips[1].ip_to));
            //Console.WriteLine("location_index: " + ips[1].location_index);

            locations = new Location[hd[0].records];
            n = 0;
            unsafe
            {
                fixed (Location* p = locations)
                {
                    ReadFile(handle, p, sizeof(Location) * hd[0].records, &n, 0);
                }
            }

            //unsafe
            //{
            //    fixed (Location* p = locations)
            //    {
            //        Console.WriteLine(System.Text.Encoding.Default.GetString(p[0].city, 24));
            //        Console.WriteLine(System.Text.Encoding.Default.GetString(p[1].city, 24));
            //        Console.WriteLine(System.Text.Encoding.Default.GetString(p[2].city, 24));
            //    }
            //}

            //sw.Stop();
            //Console.WriteLine("TotalMilliseconds: " + sw.Elapsed.TotalMilliseconds);



            //Dictionary<byte[], int>.KeyCollection keyColl = cities.Keys;
            //int kk = 0;
            //foreach (byte[] k in keyColl)
            //{
            //    Console.WriteLine(System.Text.Encoding.Default.GetString(k));
            //    kk++;

            //    if (kk > 5)
            //        break;
            //}


            sw.Stop();
            Console.WriteLine("TotalMilliseconds: " + sw.Elapsed.TotalMilliseconds);

            CloseHandle(handle);

        }
    }
}
