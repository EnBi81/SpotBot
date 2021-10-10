using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Victoria;

namespace SpotBot.Helpers
{
    class HashHelper
    {

        public static string GetHash(LavaTrack track)
        {
            var writer = new Writer();

            writer.Write(GetHeader());
            writer.Write((sbyte)2);
            writer.Write(track.Title);
            writer.Write(track.Author);
            writer.Write((long)track.Duration.TotalMilliseconds);
            writer.Write(track.Id);
            writer.Write(track.IsStream);
            writer.Write(true);
            writer.Write(track.Url);

            return GetHash(writer.GetBytes);
        }

        private static string GetHash(byte[] bytes) => Convert.ToBase64String(bytes);

        private static int GetHeader() => 1073741955;



        class Writer
        {

            public byte[] GetBytes { get => Bytes.ToArray(); }
            private List<byte> Bytes { get; } = new List<byte>();

            public void Write(string text)
            {
                var bytes = Encoding.UTF8.GetBytes(text);
                Bytes.AddRange(bytes);
            }

            public void Write(bool param)
            {
                Bytes.Add(param ? (byte)1 : (byte)0);
            }

            public void Write(long num)
            {
                var bytes = BitConverter.GetBytes(num);
                if (BitConverter.IsLittleEndian) bytes.Reverse();

                Bytes.AddRange(bytes);
            }
            public void Write(int num)
            {
                var bytes = BitConverter.GetBytes(num);
                if (BitConverter.IsLittleEndian) bytes.Reverse();

                Bytes.AddRange(bytes);
            }

            public void Write(sbyte cucc = 2)
            {
                var bytes = BitConverter.GetBytes(cucc);
                if (BitConverter.IsLittleEndian) bytes.Reverse();

                Bytes.AddRange(bytes);
            }

        }
    }
}
