using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace UniversalWindowsExample {
    public class MemoryBuffer : IBuffer {
        public uint Capacity => throw new NotImplementedException();

        public uint Length { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    }
}
