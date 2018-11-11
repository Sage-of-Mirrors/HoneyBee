using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using GameFormatReader.Common;

namespace HoneyBee.model
{
    public sealed class MeshVertexHolder
    {
        public List<Vector3> Position = new List<Vector3>();
        public List<Vector3> Normal = new List<Vector3>();
        public List<Vector3> Color0 = new List<Vector3>();
        public List<Vector2> Tex0 = new List<Vector2>();
    }

    public class MeshVertexIndex
    {
        public int Position = -1;
        public int Normal = -1;
        public int Color0 = -1;
        public int Tex0 = -1;
    }

    public class Mesh
    {
        public string Name { get; private set; }
        public MeshVertexHolder VertexData { get; private set; }
        public List<MeshVertexIndex> Vertices { get; private set; }

        public Mesh()
        {
            Name = "";
            VertexData = new MeshVertexHolder();
            Vertices = new List<MeshVertexIndex>();
        }

        public Mesh(string name)
        {
            Name = name;
            VertexData = new MeshVertexHolder();
            Vertices = new List<MeshVertexIndex>();
        }

        public override string ToString()
        {
            return $"Mesh: { Name }";
        }
    }
}
