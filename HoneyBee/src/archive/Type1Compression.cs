using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameFormatReader.Common;
using System.IO;

namespace HoneyBee
{
    public static class Type1Compression
    {
        private static int WINDOW_SIZE = 1024;
        private static int WINDOW_START = 0x3BE;
        private static int MIN_MATCH_LEN = 3;

        public static byte[] Compress(byte[] data)
        {
            return new byte[1];
        }

        public static byte[] Decompress(byte[] compressed_data, int uncompressed_size)
        {
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
    }
}
