using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jint.Collections;
using Jint.Native.DataView;
using Jint.Native.Function;
using Jint.Native.Object;
using Jint.Native.Symbol;
using Jint.Runtime;
using Jint.Runtime.Descriptors;
using Jint.Runtime.Interop;

namespace Jint.Native.ArrayBuffer
{
    /// <summary>
    /// https://tc39.es/ecma262/multipage/structured-data.html#sec-arraybuffer-constructor
    /// </summary>
    public class ArrayBufferConstructor : FunctionInstance, IConstructor
    {
        private static readonly JsString _arrayBufferName = new JsString("ArrayBuffer");
        private static readonly JsString _sharedArrayBufferName = new JsString("SharedArrayBuffer");

        internal readonly bool _isShared;

        private ArrayBufferConstructor(Engine engine, bool isShared) 
            : base(engine, isShared ? _sharedArrayBufferName : _arrayBufferName)
        {
            _isShared = isShared;
        }

        public static ArrayBufferConstructor CreateArrayBufferConstructor(Engine engine)
        {
            var obj = new ArrayBufferConstructor(engine, false)
            {
                _prototype = engine.Function.PrototypeObject
            };

            obj.PrototypeObject = ArrayBufferPrototype.CreatePrototypeObject(engine, obj);

            obj._length = new PropertyDescriptor(1, PropertyFlag.Configurable);
            obj._prototypeDescriptor = new PropertyDescriptor(obj.PrototypeObject, PropertyFlag.AllForbidden);

            return obj;
        }

        public static ArrayBufferConstructor CreateSharedArrayBufferConstructor(Engine engine)
        {
            var obj = new ArrayBufferConstructor(engine, true)
            {
                _prototype = engine.Function.PrototypeObject
            };

            obj.PrototypeObject = ArrayBufferPrototype.CreatePrototypeObject(engine, obj);

            obj._length = new PropertyDescriptor(1, PropertyFlag.Configurable);
            obj._prototypeDescriptor = new PropertyDescriptor(obj.PrototypeObject, PropertyFlag.AllForbidden);

            return obj;
        }

        protected override void Initialize()
        {
            var properties = new PropertyDictionary(1, checkExistingKeys: false)
            {
                ["isView"] = new PropertyDescriptor(new PropertyDescriptor(new ClrFunctionInstance(Engine, "isView", IsView, 1, PropertyFlag.Configurable), PropertyFlag.NonEnumerable))
            };
            SetProperties(properties);

            var symbols = new SymbolDictionary(1)
            {
                [GlobalSymbolRegistry.Species] = new GetSetPropertyDescriptor(get: new ClrFunctionInstance(_engine, "get [Symbol.species]", Species, 0, PropertyFlag.Configurable), set: Undefined, PropertyFlag.Configurable)
            };
            SetSymbols(symbols);
        }

        private static JsValue IsView(JsValue thisObj, JsValue[] arguments)
        {
            var o = arguments.At(0);

            return IsView(o);
        }

        private static JsValue IsView(JsValue o)
        {
            if (!(o is ObjectInstance oi))
            {
                return JsBoolean.False;
            }

            return oi is DataViewInstance;
        }

        private static JsValue Species(JsValue thisObject, JsValue[] arguments)
        {
            return thisObject;
        }

        public override JsValue Call(JsValue thisObject, JsValue[] arguments)
        {
            return ExceptionHelper.ThrowTypeError<JsValue>(_engine, $"Constructor {(_isShared ? "SharedArrayBuffer" : "ArrayBuffer")} requires 'new'");
        }

        public ObjectInstance Construct(JsValue[] arguments, JsValue newTarget)
        {
            if (newTarget.IsUndefined())
            {
                ExceptionHelper.ThrowTypeError(_engine);
            }

            var byteLength = 0UL;
            if (arguments.Length > 0)
            {
                byteLength = TypeConverter.ToIndex(Engine, arguments[0]);
            }

            try
            {
                var arrayBuffer = OrdinaryCreateFromConstructor(newTarget, PrototypeObject, static (engine, _) => new ArrayBufferInstance(engine));
                arrayBuffer._isShared = _isShared;
                arrayBuffer._data = new byte[byteLength];
                arrayBuffer._byteLength = byteLength;
                return arrayBuffer;
            }
            catch (OverflowException)
            {
                return ExceptionHelper.ThrowRangeError<ObjectInstance>(_engine);
            }
        }

        public ArrayBufferPrototype PrototypeObject { get; private set; }
    }
}
