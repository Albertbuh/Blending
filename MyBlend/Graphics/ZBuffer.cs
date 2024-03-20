using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MyBlend.Graphics
{
    internal class ZBuffer
    {
        private float[,] buffer;
        private readonly int width;
        private readonly int height;
        private object[,] lockers;
        public ZBuffer(int width, int height)
        {
            this.width = width;
            this.height = height;
            buffer = new float[height, width];
            lockers = new object[height, width];
            Clear();
        }

        public float GetZ(int x, int y)
        {
            if (!IsInBuffer(x, y))
                throw new IndexOutOfRangeException($"No such coordinate on screen: ({x}, {y})");
            return buffer[y, x];
        }

        public void TryToUpdateZ(int x, int y, float z, Action? action)
        {
            lock (lockers[y, x])
            {
                if (GetZ(x, y) >= z)
                {
                    buffer[y, x] = z;
                    action?.Invoke();
                }
            }
        }

        public void SetZ(int x, int y, float value)
        {
            if (!IsInBuffer(x, y))
                return;
            buffer[y, x] = value;
            //Interlocked.Exchange(ref buffer[y, x], value);
        } 

        private bool IsInBuffer(int x, int y)
        {
            return  x >= 0 && y >= 0 && x < width && y < height;
        }

        public void Clear()
        {
            for (int i = 0; i < height; i++)
                for (int j = 0; j < width; j++)
                    buffer[i, j] = Int32.MaxValue;

            if (lockers[0,0] == null)
                for (int i = 0; i < height; i++)
                    for (int j = 0; j < width; j++)
                        lockers[i, j] = new object();

        }
    }
}
