using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.Compression;
using GameFormatReader.Common;

namespace HoneyBee
{
    public class File
    {
        public int Size;
        public int Flags;

        public byte[] Data;

        public override string ToString()
        {
            return $"{ Flags }";
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
                    file.Flags = reader.ReadInt32();
                    file.Data = reader.ReadBytes(compressed_size);

                    m_Files.Add(file);
                }
            }

            for (int i = 0; i < m_Files.Count; i++)
            {
                File f = m_Files[i];

                if ((f.Flags & 6) == 6)
                {
                    byte[] buffer = new byte[4096];
                    long idk = 0;

                    using (MemoryStream testaaaa = new MemoryStream(f.Data))
                    {
                        testaaaa.ReadByte();
                        testaaaa.ReadByte();
                        testaaaa.ReadByte();
                        testaaaa.ReadByte();

                        testaaaa.ReadByte();
                        testaaaa.ReadByte();
                        testaaaa.ReadByte();
                        testaaaa.ReadByte();

                        testaaaa.ReadByte();
                        testaaaa.ReadByte();

                        using (MemoryStream test = new MemoryStream(new byte[f.Size]))
                        using (DeflateStream def = new DeflateStream(testaaaa, CompressionMode.Decompress, true))
                        {
                            def.CopyTo(test);
                            f.Data = test.ToArray();
                        }
                    }
                }
                else if (f.Flags == 4)
                {

                }
                else
                {
                    f.Data = Type1Compression.Decompress(f.Data, f.Size);
                }
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
