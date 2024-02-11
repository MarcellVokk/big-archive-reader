using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace replayReader
{
    public static class BigReader
    {
        public static Dictionary<string, BigFile> GetFiles(string bigArchive)
        {
            Dictionary<string, BigFile> files = new Dictionary<string, BigFile>();

            List<(uint size, string name)> index = new List<(uint size, string name)>();

            using (var stream = new FileStream(bigArchive, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var reader = new BinaryReader(stream, Encoding.UTF8, true))
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

                    index.Add((embeded_file_size, embeded_file_name));
                }

                while (true)
                {
                    var curent_file = index.First();
                    index.RemoveAt(0);

                    files.Add(curent_file.name, new BigFile(curent_file.name, reader.ReadBytes((int)curent_file.size)));

                    if (index.Count == 0)
                        break;
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
        public byte[] Data { get; set; }

        public BigFile(string name, byte[] data)
        {
            Name = name;
            Data = data;
        }

        public string GetText() => Encoding.UTF8.GetString(Data);
    }
}
