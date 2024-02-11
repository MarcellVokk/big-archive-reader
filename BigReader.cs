using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BFME_API_Client.Utilities
{
    public static class BigReader
    {
        public static Dictionary<string, BigFile> GetFiles(string bigArchive)
        {
            Dictionary<string, BigFile> files = new Dictionary<string, BigFile>();

            using (var reader = new BinaryReader(new FileStream(bigArchive, FileMode.Open, FileAccess.Read, FileShare.ReadWrite), Encoding.UTF8, false))
            {
                var header = reader.ReadFixedLengthString(4);
                var file_size = reader.ReadUInt32();
                var n_files = reader.ReadUInt32_BigEndian();
                var index_table_size = reader.ReadUInt32_BigEndian();

                for (int i = 0; i < n_files; i++)
                {
                    var embeded_file_offset = reader.ReadUInt32_BigEndian();
                    var embeded_file_size = reader.ReadUInt32_BigEndian();
                    var embeded_file_name = reader.ReadNullTerminatedString();

                    files.Add(embeded_file_name, new BigFile(embeded_file_name, bigArchive, (int)embeded_file_offset, (int)embeded_file_size));
                }
            }

            return files;
        }

        public static uint ReadUInt32_BigEndian(this BinaryReader reader)
        {
            return BitConverter.ToUInt32(reader.ReadBytes(4).Reverse().ToArray());
        }

        public static string ReadNullTerminatedString(this BinaryReader reader)
        {
            var sb = new StringBuilder();

            char c;
            while ((c = reader.ReadChar()) != '\0')
                sb.Append(c);

            return sb.ToString();
        }

        public static string ReadFixedLengthString(this BinaryReader reader, int count)
        {
            if (count == 0)
                return string.Empty;

            var chars = reader.ReadChars(count);
            var result = new string(chars);

            if (result.Contains('\0'))
                return result.Substring(0, result.IndexOf('\0'));
            else
                return result;
        }
    }

    public struct BigFile
    {
        public string Name { get; set; }
        public string Source { get; set; }
        public int Offset { get; set; }
        public int Size { get; set; }

        public BigFile(string name, string source, int offset, int size)
        {
            Name = name;
            Source = source;
            Offset = offset;
            Size = size;
        }

        public byte[] GetData()
        {
            byte[] data = File.ReadAllBytes(Source);
            return data.Skip(Offset).Take(Size).ToArray();
        }

        public string GetText() => Encoding.UTF8.GetString(GetData());
    }
}
