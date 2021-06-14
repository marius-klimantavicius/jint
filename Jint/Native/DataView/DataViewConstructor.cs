using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Jint.Collections;
using Jint.Native.Function;
using Jint.Native.Object;
using Jint.Native.Symbol;
using Jint.Runtime;
using Jint.Runtime.Descriptors;
using Jint.Runtime.Interop;

namespace Jint.Native.DataView
{
    /// <summary>
    /// https://tc39.es/ecma262/multipage/structured-data.html#sec-dataview-constructor
    /// </summary>
    public class DataViewConstructor : FunctionInstance, IConstructor
    {
        private static readonly JsString _functionName = new JsString("DataView");

        public DataViewConstructor(Engine engine) : base(engine, _functionName)
        {
        }

        public static DataViewConstructor CreateDataViewConstructor(Engine engine)
        {
            var obj = new DataViewConstructor(engine)
            {
                _prototype = engine.Function.PrototypeObject
            };

            obj.PrototypeObject = DataViewPrototype.CreatePrototypeObject(engine, obj);

            obj._length = new PropertyDescriptor(1, PropertyFlag.Configurable);
            obj._prototypeDescriptor = new PropertyDescriptor(obj.PrototypeObject, PropertyFlag.AllForbidden);

            return obj;
        }

        public override JsValue Call(JsValue thisObject, JsValue[] arguments)
        {
            return ExceptionHelper.ThrowTypeError<JsValue>(_engine, $"Constructor DataView requires 'new'");
        }

        public ObjectInstance Construct(JsValue[] arguments, JsValue newTarget)
        {
            if (newTarget.IsUndefined())
            {
                ExceptionHelper.ThrowTypeError(_engine);
            }

            var buffer = arguments.At(0) as ArrayBuffer.ArrayBufferInstance;
            if (buffer == null)
            {
                ExceptionHelper.ThrowTypeError(_engine);
            }

            var offset = TypeConverter.ToIndex(_engine, arguments.At(1));
            if (buffer._data == null)
            {
                ExceptionHelper.ThrowTypeError(_engine);
            }

            var bufferByteLength = buffer._byteLength;
            if (offset > bufferByteLength)
            {
                ExceptionHelper.ThrowRangeError(_engine);
            }

            var byteLength = arguments.At(2);
            var viewByteLength = 0UL;
            if (byteLength.IsUndefined())
            {
                viewByteLength = bufferByteLength - offset;
            }
            else
            {
                viewByteLength = TypeConverter.ToIndex(_engine, byteLength);
                if (offset + viewByteLength > bufferByteLength)
                {
                    ExceptionHelper.ThrowRangeError(_engine);
                }
            }

            var dataView = OrdinaryCreateFromConstructor(newTarget, PrototypeObject, static (engine, _) => new DataViewInstance(engine));
            if (buffer._data == null)
            {
                ExceptionHelper.ThrowTypeError(_engine);
            }

            dataView._buffer = buffer;
            dataView._byteLength = viewByteLength;
            dataView._byteOffset = offset;
            return dataView;
        }

        public DataViewPrototype PrototypeObject { get; private set; }
    }
}
