using System;
using Jint.Collections;
using Jint.Native.Object;
using Jint.Native.Symbol;
using Jint.Runtime;
using Jint.Runtime.Descriptors;
using Jint.Runtime.Interop;

namespace Jint.Native.ArrayBuffer
{
    /// <summary>
    /// https://tc39.es/ecma262/multipage/structured-data.html#sec-properties-of-the-arraybuffer-prototype-object
    /// </summary>
    public class ArrayBufferPrototype : ObjectInstance
    {
        private readonly ArrayBufferConstructor _arrayBufferConstructor;
        private readonly string _toStringTag;

        private ArrayBufferPrototype(Engine engine, ArrayBufferConstructor arrayBufferConstructor) 
            : base(engine)
        {
            _arrayBufferConstructor = arrayBufferConstructor;
            _toStringTag = _arrayBufferConstructor._isShared ? "SharedArrayBuffer" : "ArrayBuffer";
        }

        public static ArrayBufferPrototype CreatePrototypeObject(Engine engine, ArrayBufferConstructor arrayBufferConstructor)
        {
            var obj = new ArrayBufferPrototype(engine, arrayBufferConstructor)
            {
                _prototype = engine.Object.PrototypeObject,
            };

            return obj;
        }

        protected override void Initialize()
        {
            const PropertyFlag propertyFlags = PropertyFlag.Configurable | PropertyFlag.Writable;
            var properties = new PropertyDictionary(3, checkExistingKeys: false)
            {
                ["byteLength"] = new GetSetPropertyDescriptor(get: new ClrFunctionInstance(Engine, "get byteLength", ByteLength, 0, PropertyFlag.Configurable), set: null, PropertyFlag.Configurable),
                ["constructor"] = new PropertyDescriptor(_arrayBufferConstructor, PropertyFlag.NonEnumerable),
                ["slice"] = new PropertyDescriptor(new ClrFunctionInstance(Engine, "slice", Slice, 2, PropertyFlag.Configurable), propertyFlags),
            };
            SetProperties(properties);

            var symbols = new SymbolDictionary(1)
            {
                [GlobalSymbolRegistry.ToStringTag] = new PropertyDescriptor(_toStringTag, false, false, true),
            };
            SetSymbols(symbols);
        }

        private JsValue ByteLength(JsValue thisObj, JsValue[] arguments)
        {
            var arrayBuffer = AssertArrayBufferInstance(thisObj);
            if (arrayBuffer._isShared != _arrayBufferConstructor._isShared)
            {
                ExceptionHelper.ThrowTypeError(_engine);
            }

            if (arrayBuffer._data == null)
            {
                return 0;
            }

            return arrayBuffer._byteLength;
        }

        private JsValue Slice(JsValue thisObj, JsValue[] arguments)
        {
            var arrayBuffer = AssertArrayBufferInstance(thisObj);
            if (arrayBuffer._isShared != _arrayBufferConstructor._isShared)
            {
                ExceptionHelper.ThrowTypeError(_engine);
            }

            if (arrayBuffer._data == null)
            {
                ExceptionHelper.ThrowTypeError(_engine, $"{_toStringTag} must not be detached");
            }

            var len = arrayBuffer._byteLength;
            var relativeStart = TypeConverter.ToIntegerOrInfinity(arguments.At(0));
            
            var first = 0;
            if (double.IsNegativeInfinity(relativeStart))
            {
                first = 0;
            }
            else if (relativeStart < 0)
            {
                first = (int)System.Math.Max(len + relativeStart, 0);
            }
            else
            {
                first = (int)System.Math.Min(relativeStart, len);
            }

            var end = arguments.At(1);
            var relativeEnd = end.IsUndefined() ? len : TypeConverter.ToIntegerOrInfinity(end);
            var final = 0;
            if (double.IsNegativeInfinity(relativeEnd))
            {
                final = 0;
            }
            else if (relativeEnd < 0)
            {
                final = (int)System.Math.Max(len + relativeEnd, 0);
            }
            else
            {
                final = (int)System.Math.Min(relativeEnd, len);
            }

            var newLen = System.Math.Max(final - first, 0);
            var ctor = SpeciesConstructor(arrayBuffer, _arrayBufferConstructor._isShared ? _engine.SharedArrayBuffer : _engine.ArrayBuffer);
            var newInstance = Construct(ctor, new[] { (JsValue)newLen });
            var newArrayBuffer = AssertArrayBufferInstance(newInstance);

            if (ReferenceEquals(arrayBuffer, newArrayBuffer))
                ExceptionHelper.ThrowTypeError(_engine);

            if (newArrayBuffer._byteLength < (ulong)newLen)
                ExceptionHelper.ThrowTypeError(_engine);

            Buffer.BlockCopy(arrayBuffer._data, first, newArrayBuffer._data, 0, newLen);

            return newArrayBuffer;
        }

        internal ArrayBufferInstance AssertArrayBufferInstance(JsValue thisObj)
        {
            if (!(thisObj is ArrayBufferInstance arrayBuffer))
            {
                return ExceptionHelper.ThrowTypeError<ArrayBufferInstance>(_engine, $"object must be a {_toStringTag}");
            }

            return arrayBuffer;
        }
    }
}
