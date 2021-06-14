using Jint.Native.Object;

namespace Jint.Native.DataView
{
    /// <summary>
    /// https://tc39.es/ecma262/multipage/structured-data.html#sec-properties-of-dataview-instances
    /// </summary>
    public class DataViewInstance : ObjectInstance
    {
        internal ArrayBuffer.ArrayBufferInstance _buffer;
        internal ulong _byteLength;
        internal ulong _byteOffset;

        public DataViewInstance(Engine engine) : base(engine)
        {
        }
    }
}
