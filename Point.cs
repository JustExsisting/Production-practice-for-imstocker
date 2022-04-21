using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPGtoPNG
{
    class Point
    {
        public Point(float y,float x) 
        {
            X = x;
            Y = y;
        }
        public float X { get; }
        public float Y { get; }
    }
}
