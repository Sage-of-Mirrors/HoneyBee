using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HoneyBee.Archive;
using HoneyBee.model;

namespace HouseCat
{
    class Program
    {
        static void Main(string[] args)
        {
            Archive arc = new Archive(args[0]);

            Model m = new Model(arc.m_Files[4].Data);
            m.DumpToOBJ(@"D:\Mario Party\full_model_test.obj");
        }
    }
}
