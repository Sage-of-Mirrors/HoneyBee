using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Compression;
using GameFormatReader.Common;

namespace HoneyBee.Archive
{
    public class File
    {
        public int Size;
        public CompressionType Compression;

        public byte[] Data;

        public override string ToString()
        {
            return $"{ Compression }";
        }
    }

    public class Archive
    {
        public string m_ArcName { get; private set; }
        public List<File> m_Files;

        public Archive(string file_name)
        {
            m_ArcName = Path.GetFileNameWithoutExtension(file_name);
            m_Files = new List<File>();

            using (FileStream stream = new FileStream(file_name, FileMode.Open))
            {
                EndianBinaryReader reader = new EndianBinaryReader(stream, Endian.Big);

                int num_files = reader.ReadInt32();
                int[] offsets = new int[num_files];

                for (int i = 0; i < num_files; i++)
                {
                    offsets[i] = reader.ReadInt32();
                }

                for (int i = 0; i < num_files; i++)
                {
                    reader.BaseStream.Seek(offsets[i], SeekOrigin.Begin);

                    int compressed_size = 0;

                    // Calculate the size of the file data as stored in the archive.
                    // We subtract 8 because offsets[i + 1] - offsets[i] includes the uncompressed size
                    // and the flag field, which are 8 bytes together.
                    if (offsets[i] != offsets.Last())
                    {
                        compressed_size = (offsets[i + 1] - offsets[i]) - 8;
                    }
                    else
                    {
                        compressed_size = ((int)reader.BaseStream.Length - offsets[i]) - 8;
                    }

                    File file = new File();

                    file.Size = reader.ReadInt32();
                    file.Compression = (CompressionType)reader.ReadInt32();
                    file.Data = reader.ReadBytes(compressed_size);

                    m_Files.Add(file);
                }
            }

            foreach (File f in m_Files)
            {
                Compression.Decompress(f);
            }
        }

        public void DumpToDisk(string directory_name)
        {
            if (!Directory.Exists(directory_name))
            {
                Directory.CreateDirectory(directory_name);
            }

            string full_dir_name = Path.Combine(directory_name, m_ArcName);
            Directory.CreateDirectory(full_dir_name);

            for (int i = 0; i < m_Files.Count; i++)
            {
                using (FileStream f = new FileStream(Path.Combine(full_dir_name, i.ToString()), FileMode.Create))
                {
                    EndianBinaryWriter writer = new EndianBinaryWriter(f, Endian.Big);
                    writer.Write(m_Files[i].Data);
                }
            }
        }
    }
}
