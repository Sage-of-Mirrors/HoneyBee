using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameFormatReader.Common;
using System.IO;
using System.IO.Compression;

namespace HoneyBee.Archive
{
    public enum CompressionType
    {
        None,
        Type1,
        
        Deflate = 7,
    }

    public static class Compression
    {
        public static void Compress(File uncompressed_file)
        {
            byte[] type1_compressed = CompressType1RLE(uncompressed_file.Data);
            byte[] deflate_compressed = CompressDeflate(uncompressed_file.Data);

            if (type1_compressed.Length < deflate_compressed.Length)
            {
                uncompressed_file.Data = type1_compressed;
                uncompressed_file.Compression = CompressionType.Type1;
            }
            else
            {
                uncompressed_file.Data = deflate_compressed;
                uncompressed_file.Compression = CompressionType.Deflate;
            }
        }

        public static void Decompress(File compressed_file)
        {
            switch (compressed_file.Compression)
            {
                case CompressionType.Type1:
                    compressed_file.Data = DecompressType1(compressed_file.Data, compressed_file.Size);
                    break;
                case CompressionType.Deflate:
                    compressed_file.Data = DecompressDeflate(compressed_file.Data, compressed_file.Size);
                    break;
                case CompressionType.None:
                default:
                    return;
            }
        }

        private static byte[] CompressType1RLE(byte[] data)
        {
            return new byte[1];
        }

        private static byte[] CompressDeflate(byte[] data)
        {
            byte[] output_data = new byte[1];

            using (MemoryStream uncompressed_data = new MemoryStream(data))
            {
                using (MemoryStream compressed_data = new MemoryStream())
                using (DeflateStream deflator = new DeflateStream(compressed_data, CompressionMode.Compress))
                {
                    uncompressed_data.CopyTo(deflator);
                    deflator.Close();
                    output_data = compressed_data.ToArray();
                }
            }

            return output_data;
        }

        private static byte[] DecompressType1(byte[] compressed_data, int uncompressed_size)
        {
            int WINDOW_SIZE = 1024;
            int WINDOW_START = 0x3BE;
            int MIN_MATCH_LEN = 3;

            int src_offset = 0;
            int dest_offset = 0;
            int window_offset = WINDOW_START;

            byte[] dest = new byte[uncompressed_size];
            byte[] window_buffer = new byte[WINDOW_SIZE];

            ushort cur_code_byte = 0;

            while(dest_offset < uncompressed_size)
            {
                if ((cur_code_byte & 0x100) == 0)
                {
                    cur_code_byte = compressed_data[src_offset++];
                    cur_code_byte |= 0xFF00;
                }

                if ((cur_code_byte & 0x001) == 1)
                {
                    dest[dest_offset] = compressed_data[src_offset];
                    window_buffer[window_offset] = compressed_data[src_offset];

                    src_offset++;
                    dest_offset++;

                    window_offset = (window_offset + 1) % WINDOW_SIZE;
                }

                else
                {
                    byte byte1 = compressed_data[src_offset++];
                    byte byte2 = compressed_data[src_offset++];

                    int offset = ((byte2 & 0xC0) << 2) | byte1;
                    int length = (byte2 & 0x3F) + MIN_MATCH_LEN;

                    byte val = 0;
                    for (int i = 0; i < length; i++)
                    {
                        val = window_buffer[offset % WINDOW_SIZE];
                        window_buffer[window_offset] = val;

                        window_offset = (window_offset + 1) % WINDOW_SIZE;
                        dest[dest_offset] = val;

                        dest_offset++;
                        offset++;
                    }
                }

                cur_code_byte >>= 1;
            }


            return dest;
        }

        private static byte[] DecompressDeflate(byte[] data, int uncompressed_size)
        {
            byte[] output_data = new byte[1];

            using (MemoryStream compressed_data = new MemoryStream(data))
            {
                compressed_data.Seek(10, SeekOrigin.Begin); // The compressed data starts at 0xA in the file data

                using (MemoryStream uncompressed_data = new MemoryStream(new byte[uncompressed_size]))
                using (DeflateStream deflator = new DeflateStream(compressed_data, CompressionMode.Decompress))
                {
                    deflator.CopyTo(uncompressed_data);
                    output_data = uncompressed_data.ToArray();
                }
            }

            return output_data;
        }
    }
}
