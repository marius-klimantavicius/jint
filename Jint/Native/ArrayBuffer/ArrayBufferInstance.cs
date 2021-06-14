using System;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using Jint.Native.Object;

namespace Jint.Native.ArrayBuffer
{
    /// <summary>
    /// https://tc39.es/ecma262/multipage/structured-data.html#sec-properties-of-the-arraybuffer-instances
    /// </summary>
    public class ArrayBufferInstance : ObjectInstance
    {
        internal ulong _byteLength;
        internal byte[] _data;
        internal bool _isShared;

        public ArrayBufferInstance(Engine engine) : base(engine)
        {
        }
    }
}
