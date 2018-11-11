using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameFormatReader.Common;
using System.IO;
using OpenTK;
using System.Diagnostics;

namespace HoneyBee.model
{
    public class Model
    {
        private List<Mesh> m_Meshes;

        public Model(byte[] data)
        {
            m_Meshes = new List<Mesh>();

            using (EndianBinaryReader reader = new EndianBinaryReader(data, Endian.Big))
            {
                EndianBinaryReader string_table = GetStringTable(reader);

                ReadPositionData(reader, string_table);
                ReadColorData(reader, string_table);
                ReadTexCoordData(reader, string_table);
                ReadPrimitiveData(reader, string_table);
            }
        }

        private EndianBinaryReader GetStringTable(EndianBinaryReader reader)
        {
            reader.Skip(0xA8);

            int table_offset = reader.ReadInt32();
            int table_size = reader.ReadInt32();

            reader.BaseStream.Seek(8, SeekOrigin.Begin);

            return new EndianBinaryReader(reader.ReadBytesAt(table_offset, table_size), Endian.Big);
        }

        private void ReadPositionData(EndianBinaryReader reader, EndianBinaryReader string_table)
        {
            reader.Skip(0x20);

            int pos_offset = reader.ReadInt32();
            int pos_count = reader.ReadInt32();

            int data_base_offset = pos_offset + (pos_count * 12);

            reader.BaseStream.Seek(pos_offset, SeekOrigin.Begin);

            for (int i = 0; i < pos_count; i++)
            {
                int name_offset = reader.ReadInt32();
                int vec_count = reader.ReadInt32();
                int data_offset = reader.ReadInt32();

                long cur_pos = reader.BaseStream.Position;
                reader.BaseStream.Seek(data_base_offset + data_offset, SeekOrigin.Begin);

                string_table.BaseStream.Seek(name_offset, SeekOrigin.Begin);
                Mesh mesh = new Mesh(string_table.ReadStringUntil('\0'));
                m_Meshes.Add(mesh);

                for (int v = 0; v < vec_count; v++)
                {
                    mesh.VertexData.Position.Add(new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()));
                }

                reader.BaseStream.Seek(cur_pos, SeekOrigin.Begin);
            }

            reader.BaseStream.Seek(8, SeekOrigin.Begin);
        }

        private void ReadColorData(EndianBinaryReader reader, EndianBinaryReader string_table)
        {
            reader.Skip(0x28);

            int col_offset = reader.ReadInt32();
            int col_count = reader.ReadInt32();

            int data_base_offset = col_offset + (col_count * 12);

            reader.BaseStream.Seek(col_offset, SeekOrigin.Begin);

            for (int i = 0; i < col_count; i++)
            {
                int name_offset = reader.ReadInt32();
                int vec_count = reader.ReadInt32();
                int data_offset = reader.ReadInt32();

                string_table.BaseStream.Seek(name_offset, SeekOrigin.Begin);
                string mesh_name = string_table.ReadStringUntil('\0');
                Mesh mesh = m_Meshes.Find(x => x.Name == mesh_name);

                long cur_pos = reader.BaseStream.Position;
                reader.BaseStream.Seek(data_base_offset + data_offset, SeekOrigin.Begin);

                for (int v = 0; v < vec_count; v++)
                {
                    mesh.VertexData.Color0.Add(new Vector3(reader.ReadByte() / 255f, reader.ReadByte() / 255f, reader.ReadByte() / 255f));
                }

                reader.BaseStream.Seek(cur_pos, SeekOrigin.Begin);
            }

            reader.BaseStream.Seek(8, SeekOrigin.Begin);
        }

        private void ReadTexCoordData(EndianBinaryReader reader, EndianBinaryReader string_table)
        {
            reader.Skip(0x30);

            int tex_offset = reader.ReadInt32();
            int tex_count = reader.ReadInt32();

            int data_base_offset = tex_offset + (tex_count * 12);

            reader.BaseStream.Seek(tex_offset, SeekOrigin.Begin);

            for (int i = 0; i < tex_count; i++)
            {
                int name_offset = reader.ReadInt32();
                int vec_count = reader.ReadInt32();
                int data_offset = reader.ReadInt32();

                string_table.BaseStream.Seek(name_offset, SeekOrigin.Begin);
                string mesh_name = string_table.ReadStringUntil('\0');
                Mesh mesh = m_Meshes.Find(x => x.Name == mesh_name);

                long cur_pos = reader.BaseStream.Position;
                reader.BaseStream.Seek(data_base_offset + data_offset, SeekOrigin.Begin);

                for (int v = 0; v < vec_count; v++)
                {
                    mesh.VertexData.Tex0.Add(new Vector2(reader.ReadSingle(), reader.ReadSingle()));
                }

                reader.BaseStream.Seek(cur_pos, SeekOrigin.Begin);
            }

            reader.BaseStream.Seek(8, SeekOrigin.Begin);
        }

        private void ReadPrimitiveData(EndianBinaryReader reader, EndianBinaryReader string_table)
        {
            reader.Skip(0x38);

            int prim_offset = reader.ReadInt32();
            int prim_count = reader.ReadInt32();

            int data_base_offset = prim_offset + (prim_count * 12);

            reader.BaseStream.Seek(prim_offset, SeekOrigin.Begin);

            for (int i = 0; i < prim_count; i++)
            {
                int name_offset = reader.ReadInt32();
                int vec_count = reader.ReadInt32();
                int data_offset = reader.ReadInt32();

                string_table.BaseStream.Seek(name_offset, SeekOrigin.Begin);
                string mesh_name = string_table.ReadStringUntil('\0');
                Mesh mesh = m_Meshes.Find(x => x.Name == mesh_name);

                long cur_pos = reader.BaseStream.Position;
                reader.BaseStream.Seek(data_base_offset + data_offset, SeekOrigin.Begin);

                for (int v = 0; v < vec_count; v++)
                {
                    ushort unk_1 = reader.ReadUInt16();
                    byte unk_a = reader.ReadByte();
                    byte unk_2 = reader.ReadByte();

                    List<MeshVertexIndex> vert_indices = new List<MeshVertexIndex>();

                    for (int a = 0; a < 4; a++)
                    {
                        MeshVertexIndex index = new MeshVertexIndex();

                        index.Position = reader.ReadInt16();
                        index.Normal = reader.ReadInt16();
                        index.Color0 = reader.ReadInt16();
                        index.Tex0 = reader.ReadInt16();

                        vert_indices.Add(index);
                    }

                    mesh.Vertices.AddRange(new MeshVertexIndex[] { vert_indices[0], vert_indices[1], vert_indices[2] });

                    if (unk_1 != 2)
                        mesh.Vertices.AddRange(new MeshVertexIndex[] { vert_indices[1], vert_indices[3], vert_indices[2] });

                    Vector3 unk_3 = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                }

                reader.BaseStream.Seek(cur_pos, SeekOrigin.Begin);
            }

            reader.BaseStream.Seek(8, SeekOrigin.Begin);
        }

        public void DumpToOBJ(string file_name)
        {
            using (FileStream strm = new FileStream(file_name, FileMode.Create))
            {
                EndianBinaryWriter writer = new EndianBinaryWriter(strm, Endian.Big);

                List<Vector3> master_list = new List<Vector3>();

                for (int i = 0; i < m_Meshes.Count; i++)
                {
                    Mesh cur_mesh = m_Meshes[i];

                    for (int j = 0; j < cur_mesh.VertexData.Position.Count; j++)
                    {
                        if (!master_list.Contains(cur_mesh.VertexData.Position[j]))
                        {
                            master_list.Add(cur_mesh.VertexData.Position[j]);
                        }
                    }
                }

                for (int i = 0; i < master_list.Count; i++)
                {
                    writer.Write($"v { master_list[i].X } { master_list[i].Y } { master_list[i].Z }\n".ToCharArray());
                    writer.Flush();
                }

                for (int i = 0; i < m_Meshes.Count; i++)
                {
                    writer.Write($"o { m_Meshes[i].Name }\n");

                    for (int j = 0; j < m_Meshes[i].Vertices.Count; j += 3)
                    {
                        MeshVertexIndex index_1 = m_Meshes[i].Vertices[j];
                        MeshVertexIndex index_2 = m_Meshes[i].Vertices[j + 1];
                        MeshVertexIndex index_3 = m_Meshes[i].Vertices[j + 2];

                        if (index_1.Position >= m_Meshes[i].VertexData.Position.Count)
                            continue;
                        if (index_2.Position >= m_Meshes[i].VertexData.Position.Count)
                            continue;
                        if (index_3.Position >= m_Meshes[i].VertexData.Position.Count)
                            continue;

                        Vector3 vec_1 = m_Meshes[i].VertexData.Position[index_1.Position];
                        Vector3 vec_2 = m_Meshes[i].VertexData.Position[index_2.Position];
                        Vector3 vec_3 = m_Meshes[i].VertexData.Position[index_3.Position];

                        writer.Write($"f { master_list.IndexOf(vec_1) + 1 } { master_list.IndexOf(vec_2) + 1 } { master_list.IndexOf(vec_3) + 1 }\n".ToCharArray());
                        writer.Flush();
                    }
                }
            }
        }
    }
}
