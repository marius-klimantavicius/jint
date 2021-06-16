using System;
using System.Runtime.InteropServices;
using Jint.Collections;
using Jint.Native.Object;
using Jint.Native.Symbol;
using Jint.Runtime;
using Jint.Runtime.Descriptors;
using Jint.Runtime.Interop;

namespace Jint.Native.DataView
{
    /// <summary>
    /// https://tc39.es/ecma262/multipage/structured-data.html#sec-properties-of-the-dataview-prototype-object
    /// </summary>
    public class DataViewPrototype : ObjectInstance
    {
        private DataViewConstructor _dataViewConstructor;

        public DataViewPrototype(Engine engine) : base(engine)
        {
        }

        public static DataViewPrototype CreatePrototypeObject(Engine engine, DataViewConstructor dataViewConstructor)
        {
            var obj = new DataViewPrototype(engine)
            {
                _prototype = engine.Object.PrototypeObject,
                _dataViewConstructor = dataViewConstructor,
            };

            return obj;
        }

        protected override void Initialize()
        {
            const PropertyFlag propertyFlags = PropertyFlag.Configurable | PropertyFlag.Writable;
            var properties = new PropertyDictionary(24, checkExistingKeys: false)
            {
                ["buffer"] = new GetSetPropertyDescriptor(get: new ClrFunctionInstance(Engine, "get buffer", Buffer, 0, PropertyFlag.Configurable), set: null, PropertyFlag.Configurable),
                ["byteLength"] = new GetSetPropertyDescriptor(get: new ClrFunctionInstance(Engine, "get byteLength", ByteLength, 0, PropertyFlag.Configurable), set: null, PropertyFlag.Configurable),
                ["byteOffset"] = new GetSetPropertyDescriptor(get: new ClrFunctionInstance(Engine, "get byteOffset", ByteOffset, 0, PropertyFlag.Configurable), set: null, PropertyFlag.Configurable),
                ["constructor"] = new PropertyDescriptor(_dataViewConstructor, PropertyFlag.NonEnumerable),
                ["getBigInt64"] = new PropertyDescriptor(new ClrFunctionInstance(Engine, "getBigInt64", GetBigInt64, 1, PropertyFlag.Configurable), propertyFlags),
                ["getBigUint64"] = new PropertyDescriptor(new ClrFunctionInstance(Engine, "getBigUint64", GetBigUInt64, 1, PropertyFlag.Configurable), propertyFlags),
                ["getFloat32"] = new PropertyDescriptor(new ClrFunctionInstance(Engine, "getFloat32", GetFloat32, 1, PropertyFlag.Configurable), propertyFlags),
                ["getFloat64"] = new PropertyDescriptor(new ClrFunctionInstance(Engine, "getFloat64", GetFloat64, 1, PropertyFlag.Configurable), propertyFlags),
                ["getInt8"] = new PropertyDescriptor(new ClrFunctionInstance(Engine, "getInt8", GetInt8, 1, PropertyFlag.Configurable), propertyFlags),
                ["getInt16"] = new PropertyDescriptor(new ClrFunctionInstance(Engine, "getInt16", GetInt16, 1, PropertyFlag.Configurable), propertyFlags),
                ["getInt32"] = new PropertyDescriptor(new ClrFunctionInstance(Engine, "getInt32", GetInt32, 1, PropertyFlag.Configurable), propertyFlags),
                ["getUint8"] = new PropertyDescriptor(new ClrFunctionInstance(Engine, "getUint8", GetUInt8, 1, PropertyFlag.Configurable), propertyFlags),
                ["getUint16"] = new PropertyDescriptor(new ClrFunctionInstance(Engine, "getUint16", GetUInt16, 1, PropertyFlag.Configurable), propertyFlags),
                ["getUint32"] = new PropertyDescriptor(new ClrFunctionInstance(Engine, "getUint32", GetUInt32, 1, PropertyFlag.Configurable), propertyFlags),
                ["setBigInt64"] = new PropertyDescriptor(new ClrFunctionInstance(Engine, "setBigInt64", SetBigInt64, 2, PropertyFlag.Configurable), propertyFlags),
                ["setBigUint64"] = new PropertyDescriptor(new ClrFunctionInstance(Engine, "setBigUint64", SetBigUInt64, 2, PropertyFlag.Configurable), propertyFlags),
                ["setFloat32"] = new PropertyDescriptor(new ClrFunctionInstance(Engine, "setFloat32", SetFloat32, 2, PropertyFlag.Configurable), propertyFlags),
                ["setFloat64"] = new PropertyDescriptor(new ClrFunctionInstance(Engine, "setFloat64", SetFloat64, 2, PropertyFlag.Configurable), propertyFlags),
                ["setInt8"] = new PropertyDescriptor(new ClrFunctionInstance(Engine, "setInt8", SetInt8, 2, PropertyFlag.Configurable), propertyFlags),
                ["setInt16"] = new PropertyDescriptor(new ClrFunctionInstance(Engine, "setInt16", SetInt16, 2, PropertyFlag.Configurable), propertyFlags),
                ["setInt32"] = new PropertyDescriptor(new ClrFunctionInstance(Engine, "setInt32", SetInt32, 2, PropertyFlag.Configurable), propertyFlags),
                ["setUint8"] = new PropertyDescriptor(new ClrFunctionInstance(Engine, "setUint8", SetUInt8, 2, PropertyFlag.Configurable), propertyFlags),
                ["setUint16"] = new PropertyDescriptor(new ClrFunctionInstance(Engine, "setUint16", SetUInt16, 2, PropertyFlag.Configurable), propertyFlags),
                ["setUint32"] = new PropertyDescriptor(new ClrFunctionInstance(Engine, "setUint32", SetUInt32, 2, PropertyFlag.Configurable), propertyFlags),
            };
            SetProperties(properties);

            var symbols = new SymbolDictionary(1)
            {
                [GlobalSymbolRegistry.ToStringTag] = new PropertyDescriptor("DataView", false, false, true),
            };
            SetSymbols(symbols);
        }

        private JsValue Buffer(JsValue thisObj, JsValue[] arguments)
        {
            var dataView = AssertDataViewInstance(thisObj);
            return dataView._buffer;
        }

        private JsValue ByteLength(JsValue thisObj, JsValue[] arguments)
        {
            var dataView = AssertDataViewInstance(thisObj);
            var buffer = dataView._buffer;
            if (buffer._data == null)
            {
                ExceptionHelper.ThrowTypeError(_engine);
            }

            return dataView._byteLength;
        }

        private JsValue ByteOffset(JsValue thisObj, JsValue[] arguments)
        {
            var dataView = AssertDataViewInstance(thisObj);
            var buffer = dataView._buffer;
            if (buffer._data == null)
            {
                ExceptionHelper.ThrowTypeError(_engine);
            }

            return dataView._byteOffset;
        }

        private JsValue GetBigInt64(JsValue thisObj, JsValue[] arguments)
        {
            return GetViewValue<ElementTypeBigInt64>(thisObj, arguments);
        }

        private JsValue GetBigUInt64(JsValue thisObj, JsValue[] arguments)
        {
            return GetViewValue<ElementTypeBigUInt64>(thisObj, arguments);
        }

        private JsValue GetFloat32(JsValue thisObj, JsValue[] arguments)
        {
            return GetViewValue<ElementTypeFloat32>(thisObj, arguments);
        }

        private JsValue GetFloat64(JsValue thisObj, JsValue[] arguments)
        {
            return GetViewValue<ElementTypeFloat64>(thisObj, arguments);
        }

        private JsValue GetInt8(JsValue thisObj, JsValue[] arguments)
        {
            return GetViewValue<ElementTypeInt8>(thisObj, arguments);
        }

        private JsValue GetInt16(JsValue thisObj, JsValue[] arguments)
        {
            return GetViewValue<ElementTypeInt16>(thisObj, arguments);
        }

        private JsValue GetInt32(JsValue thisObj, JsValue[] arguments)
        {
            return GetViewValue<ElementTypeInt32>(thisObj, arguments);
        }

        private JsValue GetUInt8(JsValue thisObj, JsValue[] arguments)
        {
            return GetViewValue<ElementTypeUInt8>(thisObj, arguments);
        }

        private JsValue GetUInt16(JsValue thisObj, JsValue[] arguments)
        {
            return GetViewValue<ElementTypeUInt16>(thisObj, arguments);
        }

        private JsValue GetUInt32(JsValue thisObj, JsValue[] arguments)
        {
            return GetViewValue<ElementTypeUInt32>(thisObj, arguments);
        }

        private JsValue SetBigInt64(JsValue thisObj, JsValue[] arguments)
        {
            SetViewValue<ElementTypeBigInt64>(thisObj, arguments);
            return JsValue.Undefined;
        }

        private JsValue SetBigUInt64(JsValue thisObj, JsValue[] arguments)
        {
            SetViewValue<ElementTypeBigUInt64>(thisObj, arguments);
            return JsValue.Undefined;
        }

        private JsValue SetFloat32(JsValue thisObj, JsValue[] arguments)
        {
            SetViewValue<ElementTypeFloat32>(thisObj, arguments);
            return JsValue.Undefined;
        }

        private JsValue SetFloat64(JsValue thisObj, JsValue[] arguments)
        {
            SetViewValue<ElementTypeFloat64>(thisObj, arguments);
            return JsValue.Undefined;
        }

        private JsValue SetInt8(JsValue thisObj, JsValue[] arguments)
        {
            SetViewValue<ElementTypeInt8>(thisObj, arguments);
            return JsValue.Undefined;
        }

        private JsValue SetInt16(JsValue thisObj, JsValue[] arguments)
        {
            SetViewValue<ElementTypeInt16>(thisObj, arguments);
            return JsValue.Undefined;
        }

        private JsValue SetInt32(JsValue thisObj, JsValue[] arguments)
        {
            SetViewValue<ElementTypeInt32>(thisObj, arguments);
            return JsValue.Undefined;
        }

        private JsValue SetUInt8(JsValue thisObj, JsValue[] arguments)
        {
            SetViewValue<ElementTypeUInt8>(thisObj, arguments);
            return JsValue.Undefined;
        }

        private JsValue SetUInt16(JsValue thisObj, JsValue[] arguments)
        {
            SetViewValue<ElementTypeUInt16>(thisObj, arguments);
            return JsValue.Undefined;
        }

        private JsValue SetUInt32(JsValue thisObj, JsValue[] arguments)
        {
            SetViewValue<ElementTypeUInt32>(thisObj, arguments);
            return JsValue.Undefined;
        }

        private JsValue GetViewValue<TElementType>(JsValue thisObj, JsValue[] arguments)
            where TElementType : IElementType
        {
            var elementType = default(TElementType);
            var view = AssertDataViewInstance(thisObj);
            var requestIndex = arguments.At(0);
            var littleEndian = arguments.At(1, JsBoolean.False);

            var getIndex = TypeConverter.ToIndex(_engine, requestIndex);
            var isLittleEndian = TypeConverter.ToBoolean(littleEndian);
            var buffer = view._buffer;
            if (buffer._data == null)
            {
                ExceptionHelper.ThrowTypeError(_engine);
            }

            var viewOffset = view._byteOffset;
            var viewSize = view._byteLength;
            if (getIndex + elementType.ElementSize > viewSize)
            {
                ExceptionHelper.ThrowRangeError(_engine);
            }

            var bufferIndex = getIndex + viewOffset;
            var numberValue = elementType.Read(buffer._data, bufferIndex, isLittleEndian);
            return elementType.FromValue(numberValue);
        }

        private void SetViewValue<TElementType>(JsValue thisObj, JsValue[] arguments)
            where TElementType : IElementType
        {
            var elementType = default(TElementType);
            var view = AssertDataViewInstance(thisObj);
            var requestIndex = arguments.At(0);
            var value = arguments.At(1);
            var littleEndian = arguments.At(2, JsBoolean.False);

            var getIndex = TypeConverter.ToIndex(_engine, requestIndex);
            var numberValue = elementType.ToValue(value); // Get number value from arg (to ensure that TypeError due to conversion is thrown before RangeError)
            var isLittleEndian = TypeConverter.ToBoolean(littleEndian);
            var buffer = view._buffer;
            if (buffer._data == null)
            {
                ExceptionHelper.ThrowTypeError(_engine);
            }

            var viewOffset = view._byteOffset;
            var viewSize = view._byteLength;
            if (getIndex + elementType.ElementSize > viewSize)
            {
                ExceptionHelper.ThrowRangeError(_engine);
            }

            var bufferIndex = getIndex + viewOffset;
            elementType.Write(buffer._data, bufferIndex, isLittleEndian, numberValue);
        }

        private DataViewInstance AssertDataViewInstance(JsValue thisObj)
        {
            if (!(thisObj is DataViewInstance dataView))
            {
                return ExceptionHelper.ThrowTypeError<DataViewInstance>(_engine, $"object must be a DataView");
            }

            return dataView;
        }
    }

    internal interface IElementType
    {
        uint ElementSize { get; }

        ElementTypeValue ToValue(JsValue value);
        JsValue FromValue(ElementTypeValue value);

        ElementTypeValue Read(byte[] data, ulong index, bool isLittleEndian);
        void Write(byte[] data, ulong index, bool isLittleEndian, ElementTypeValue numberValue);
    }

    [StructLayout(LayoutKind.Explicit)]
    internal struct ElementTypeValue
    {
        [FieldOffset(0)]
        public byte UInt8;

        [FieldOffset(0)]
        public sbyte Int8;

        [FieldOffset(0)]
        public ushort UInt16;

        [FieldOffset(0)]
        public short Int16;

        [FieldOffset(0)]
        public uint UInt32;

        [FieldOffset(0)]
        public int Int32;

        [FieldOffset(0)]
        public ulong BigUInt64;

        [FieldOffset(0)]
        public long BigInt64;

        [FieldOffset(0)]
        public float Float32;

        [FieldOffset(0)]
        public double Float64;
    }

    internal struct ElementTypeBigInt64 : IElementType
    {
        public uint ElementSize => 8;

        public ElementTypeValue ToValue(JsValue value)
        {
            var numberValue = TypeConverter.ToNumber(value);
            if (double.IsNaN(numberValue) || double.IsInfinity(numberValue))
            {
                numberValue = 0;
            }

            return new ElementTypeValue { BigInt64 = (long)numberValue };
        }

        public JsValue FromValue(ElementTypeValue value)
        {
            return value.BigInt64;
        }

        public ElementTypeValue Read(byte[] data, ulong index, bool isLittleEndian)
        {
            var value = 0L;
            if (isLittleEndian)
            {
                value = (data[index + 7] << 56)
                    | (data[index + 6] << 48)
                    | (data[index + 5] << 40)
                    | (data[index + 4] << 32)
                    | (data[index + 3] << 24)
                    | (data[index + 2] << 16)
                    | (data[index + 1] << 8)
                    | (data[index + 0])
                    ;
            }
            else
            {
                value = data[index + 7]
                    | (data[index + 6] << 8)
                    | (data[index + 5] << 16)
                    | (data[index + 4] << 24)
                    | (data[index + 3] << 32)
                    | (data[index + 2] << 40)
                    | (data[index + 1] << 48)
                    | (data[index + 0] << 56)
                    ;
            }

            return new ElementTypeValue { BigInt64 = value };
        }

        public void Write(byte[] data, ulong index, bool isLittleEndian, ElementTypeValue numberValue)
        {
            var value = numberValue.BigInt64;
            if (isLittleEndian)
            {
                data[index + 7] = (byte)(value >> 56);
                data[index + 6] = (byte)(value >> 48);
                data[index + 5] = (byte)(value >> 40);
                data[index + 4] = (byte)(value >> 32);
                data[index + 3] = (byte)(value >> 24);
                data[index + 2] = (byte)(value >> 16);
                data[index + 1] = (byte)(value >> 8);
                data[index + 0] = (byte)(value);
            }
            else
            {
                data[index + 7] = (byte)(value);
                data[index + 6] = (byte)(value >> 8);
                data[index + 5] = (byte)(value >> 16);
                data[index + 4] = (byte)(value >> 24);
                data[index + 3] = (byte)(value >> 32);
                data[index + 2] = (byte)(value >> 40);
                data[index + 1] = (byte)(value >> 48);
                data[index + 0] = (byte)(value >> 56);
            }
        }
    }

    internal struct ElementTypeBigUInt64 : IElementType
    {
        public uint ElementSize => 8;

        public ElementTypeValue ToValue(JsValue value)
        {
            var numberValue = TypeConverter.ToNumber(value);
            if (double.IsNaN(numberValue) || double.IsInfinity(numberValue))
            {
                numberValue = 0;
            }
            return new ElementTypeValue { BigUInt64 = (ulong)numberValue };
        }

        public JsValue FromValue(ElementTypeValue value)
        {
            return value.BigUInt64;
        }

        public ElementTypeValue Read(byte[] data, ulong index, bool isLittleEndian)
        {
            var value = 0UL;
            if (isLittleEndian)
            {
                value = ((ulong)data[index + 7] << 56)
                    | ((ulong)data[index + 6] << 48)
                    | ((ulong)data[index + 5] << 40)
                    | ((ulong)data[index + 4] << 32)
                    | ((ulong)data[index + 3] << 24)
                    | ((ulong)data[index + 2] << 16)
                    | ((ulong)data[index + 1] << 8)
                    | data[index + 0]
                    ;
            }
            else
            {
                value = data[index + 7]
                    | ((ulong)data[index + 6] << 8)
                    | ((ulong)data[index + 5] << 16)
                    | ((ulong)data[index + 4] << 24)
                    | ((ulong)data[index + 3] << 32)
                    | ((ulong)data[index + 2] << 40)
                    | ((ulong)data[index + 1] << 48)
                    | ((ulong)data[index + 0] << 56)
                    ;
            }

            return new ElementTypeValue { BigUInt64 = value };
        }

        public void Write(byte[] data, ulong index, bool isLittleEndian, ElementTypeValue numberValue)
        {
            var value = numberValue.BigUInt64;
            if (isLittleEndian)
            {
                data[index + 7] = (byte)(value >> 56);
                data[index + 6] = (byte)(value >> 48);
                data[index + 5] = (byte)(value >> 40);
                data[index + 4] = (byte)(value >> 32);
                data[index + 3] = (byte)(value >> 24);
                data[index + 2] = (byte)(value >> 16);
                data[index + 1] = (byte)(value >> 8);
                data[index + 0] = (byte)(value);
            }
            else
            {
                data[index + 7] = (byte)(value);
                data[index + 6] = (byte)(value >> 8);
                data[index + 5] = (byte)(value >> 16);
                data[index + 4] = (byte)(value >> 24);
                data[index + 3] = (byte)(value >> 32);
                data[index + 2] = (byte)(value >> 40);
                data[index + 1] = (byte)(value >> 48);
                data[index + 0] = (byte)(value >> 56);
            }
        }
    }

    internal struct ElementTypeFloat32 : IElementType
    {
        public uint ElementSize => 4;

        public ElementTypeValue ToValue(JsValue value)
        {
            return new ElementTypeValue { Float32 = (float)TypeConverter.ToNumber(value) };
        }

        public JsValue FromValue(ElementTypeValue value)
        {
            return value.Float32;
        }

        public ElementTypeValue Read(byte[] data, ulong index, bool isLittleEndian)
        {
            var value = 0U;
            if (isLittleEndian)
            {
                value = ((uint)data[index + 3] << 24)
                    | ((uint)data[index + 2] << 16)
                    | ((uint)data[index + 1] << 8)
                    | data[index + 0]
                    ;
            }
            else
            {
                value = data[index + 3]
                    | ((uint)data[index + 2] << 8)
                    | ((uint)data[index + 1] << 16)
                    | ((uint)data[index + 0] << 24)
                    ;
            }

            return new ElementTypeValue
            {
                UInt32 = value
            };
        }

        public void Write(byte[] _data, ulong _index, bool _isLittleEndian, ElementTypeValue numberValue)
        {
            var value = numberValue.UInt32;
            if (_isLittleEndian)
            {
                _data[_index + 3] = (byte)(value >> 24);
                _data[_index + 2] = (byte)(value >> 16);
                _data[_index + 1] = (byte)(value >> 8);
                _data[_index + 0] = (byte)(value);
            }
            else
            {
                _data[_index + 3] = (byte)(value);
                _data[_index + 2] = (byte)(value >> 8);
                _data[_index + 1] = (byte)(value >> 16);
                _data[_index + 0] = (byte)(value >> 24);
            }
        }
    }

    internal struct ElementTypeFloat64 : IElementType
    {
        public uint ElementSize => 8;

        public ElementTypeValue ToValue(JsValue value)
        {
            return new ElementTypeValue { Float64 = TypeConverter.ToNumber(value) };
        }

        public JsValue FromValue(ElementTypeValue value)
        {
            return value.Float64;
        }

        public ElementTypeValue Read(byte[] data, ulong index, bool isLittleEndian)
        {
            var value = 0UL;
            if (isLittleEndian)
            {
                value = ((ulong)data[index + 7] << 56)
                    | ((ulong)data[index + 6] << 48)
                    | ((ulong)data[index + 5] << 40)
                    | ((ulong)data[index + 4] << 32)
                    | ((ulong)data[index + 3] << 24)
                    | ((ulong)data[index + 2] << 16)
                    | ((ulong)data[index + 1] << 8)
                    | data[index + 0]
                    ;
            }
            else
            {
                value = data[index + 7]
                    | ((ulong)data[index + 6] << 8)
                    | ((ulong)data[index + 5] << 16)
                    | ((ulong)data[index + 4] << 24)
                    | ((ulong)data[index + 3] << 32)
                    | ((ulong)data[index + 2] << 40)
                    | ((ulong)data[index + 1] << 48)
                    | ((ulong)data[index + 0] << 56)
                    ;
            }

            return new ElementTypeValue
            {
                BigUInt64 = value
            };
        }

        public void Write(byte[] data, ulong index, bool isLittleEndian, ElementTypeValue numberValue)
        {
            var value = numberValue.BigUInt64;
            if (isLittleEndian)
            {
                data[index + 7] = (byte)(value >> 56);
                data[index + 6] = (byte)(value >> 48);
                data[index + 5] = (byte)(value >> 40);
                data[index + 4] = (byte)(value >> 32);
                data[index + 3] = (byte)(value >> 24);
                data[index + 2] = (byte)(value >> 16);
                data[index + 1] = (byte)(value >> 8);
                data[index + 0] = (byte)(value);
            }
            else
            {
                data[index + 7] = (byte)(value);
                data[index + 6] = (byte)(value >> 8);
                data[index + 5] = (byte)(value >> 16);
                data[index + 4] = (byte)(value >> 24);
                data[index + 3] = (byte)(value >> 32);
                data[index + 2] = (byte)(value >> 40);
                data[index + 1] = (byte)(value >> 48);
                data[index + 0] = (byte)(value >> 56);
            }
        }
    }

    internal struct ElementTypeInt16 : IElementType
    {
        public uint ElementSize => 2;

        public ElementTypeValue ToValue(JsValue value)
        {
            var numberValue = TypeConverter.ToNumber(value);
            if (double.IsNaN(numberValue) || double.IsInfinity(numberValue))
            {
                numberValue = 0;
            }
            return new ElementTypeValue { Int16 = (short)(long)numberValue };
        }

        public JsValue FromValue(ElementTypeValue value)
        {
            return value.Int16;
        }

        public ElementTypeValue Read(byte[] data, ulong index, bool isLittleEndian)
        {
            var value = 0;
            if (isLittleEndian)
            {
                value = (data[index + 1] << 8)
                    | data[index + 0]
                    ;
            }
            else
            {
                value = (data[index + 1])
                    | (data[index + 0] << 8)
                    ;
            }

            return new ElementTypeValue { Int16 = (short)value };
        }

        public void Write(byte[] data, ulong index, bool isLittleEndian, ElementTypeValue numberValue)
        {
            var value = numberValue.Int16;
            if (isLittleEndian)
            {
                data[index + 1] = (byte)(value >> 8);
                data[index + 0] = (byte)(value);
            }
            else
            {
                data[index + 1] = (byte)(value);
                data[index + 0] = (byte)(value >> 8);
            }
        }
    }

    internal struct ElementTypeInt32 : IElementType
    {
        public uint ElementSize => 4;

        public ElementTypeValue ToValue(JsValue value)
        {
            var numberValue = TypeConverter.ToNumber(value);
            if (double.IsNaN(numberValue) || double.IsInfinity(numberValue))
            {
                numberValue = 0;
            }
            return new ElementTypeValue { Int32 = (int)(long)numberValue };
        }

        public JsValue FromValue(ElementTypeValue value)
        {
            return value.Int32;
        }

        public ElementTypeValue Read(byte[] data, ulong index, bool isLittleEndian)
        {
            var value = 0;
            if (isLittleEndian)
            {
                value = (data[index + 3] << 24)
                    | (data[index + 2] << 16)
                    | (data[index + 1] << 8)
                    | data[index + 0]
                    ;
            }
            else
            {
                value = data[index + 3]
                    | (data[index + 2] << 8)
                    | (data[index + 1] << 16)
                    | (data[index + 0] << 24)
                    ;
            }

            return new ElementTypeValue { Int32 = value };
        }

        public void Write(byte[] data, ulong index, bool isLittleEndian, ElementTypeValue numberValue)
        {
            var value = numberValue.Int32;
            if (isLittleEndian)
            {
                data[index + 3] = (byte)(value >> 24);
                data[index + 2] = (byte)(value >> 16);
                data[index + 1] = (byte)(value >> 8);
                data[index + 0] = (byte)(value);
            }
            else
            {
                data[index + 3] = (byte)(value);
                data[index + 2] = (byte)(value >> 8);
                data[index + 1] = (byte)(value >> 16);
                data[index + 0] = (byte)(value >> 24);
            }
        }
    }

    internal struct ElementTypeUInt8 : IElementType
    {
        public uint ElementSize => 1;

        public ElementTypeValue ToValue(JsValue value)
        {
            var numberValue = TypeConverter.ToNumber(value);
            if (double.IsNaN(numberValue) || double.IsInfinity(numberValue))
            {
                numberValue = 0;
            }
            return new ElementTypeValue { UInt8 = (byte)(ulong)numberValue };
        }

        public JsValue FromValue(ElementTypeValue value)
        {
            return (uint)value.UInt8;
        }

        public ElementTypeValue Read(byte[] data, ulong index, bool isLittleEndian)
        {
            return new ElementTypeValue { UInt8 = data[index] };
        }

        public void Write(byte[] data, ulong index, bool isLittleEndian, ElementTypeValue value)
        {
            data[index] = value.UInt8;
        }
    }

    internal struct ElementTypeInt8 : IElementType
    {
        public uint ElementSize => 1;

        public ElementTypeValue ToValue(JsValue value)
        {
            var numberValue = TypeConverter.ToNumber(value);
            if (double.IsNaN(numberValue) || double.IsInfinity(numberValue))
            {
                numberValue = 0;
            }
            return new ElementTypeValue { Int8 = (sbyte)(long)numberValue };
        }

        public JsValue FromValue(ElementTypeValue value)
        {
            return value.Int8;
        }

        public ElementTypeValue Read(byte[] data, ulong index, bool isLittleEndian)
        {
            return new ElementTypeValue { Int8 = (sbyte)data[index] };
        }

        public void Write(byte[] data, ulong index, bool isLittleEndian, ElementTypeValue value)
        {
            data[index] = (byte)value.Int8;
        }
    }

    internal struct ElementTypeUInt16 : IElementType
    {
        public uint ElementSize => 2;

        public ElementTypeValue ToValue(JsValue value)
        {
            var numberValue = TypeConverter.ToNumber(value);
            if (double.IsNaN(numberValue) || double.IsInfinity(numberValue))
            {
                numberValue = 0;
            }
            return new ElementTypeValue { UInt16 = (ushort)(ulong)numberValue };
        }

        public JsValue FromValue(ElementTypeValue value)
        {
            return (uint)value.UInt16;
        }

        public ElementTypeValue Read(byte[] data, ulong index, bool isLittleEndian)
        {
            var value = 0U;
            if (isLittleEndian)
            {
                value = ((uint)data[index + 1] << 8)
                    | data[index + 0]
                    ;
            }
            else
            {
                value = (data[index + 1])
                    | ((uint)data[index + 0] << 8)
                    ;
            }

            return new ElementTypeValue { UInt16 = (ushort)value };
        }

        public void Write(byte[] data, ulong index, bool isLittleEndian, ElementTypeValue numberValue)
        {
            var value = numberValue.UInt16;
            if (isLittleEndian)
            {
                data[index + 1] = (byte)(value >> 8);
                data[index + 0] = (byte)(value);
            }
            else
            {
                data[index + 1] = (byte)(value);
                data[index + 0] = (byte)(value >> 8);
            }
        }
    }

    internal struct ElementTypeUInt32 : IElementType
    {
        public uint ElementSize => 4;

        public ElementTypeValue ToValue(JsValue value)
        {
            var numberValue = TypeConverter.ToNumber(value);
            if (double.IsNaN(numberValue) || double.IsInfinity(numberValue))
            {
                numberValue = 0;
            }
            return new ElementTypeValue { UInt32 = (uint)(ulong)numberValue };
        }

        public JsValue FromValue(ElementTypeValue value)
        {
            return value.UInt32;
        }

        public ElementTypeValue Read(byte[] data, ulong index, bool isLittleEndian)
        {
            var value = 0U;
            if (isLittleEndian)
            {
                value = ((uint)data[index + 3] << 24)
                    | ((uint)data[index + 2] << 16)
                    | ((uint)data[index + 1] << 8)
                    | data[index + 0]
                    ;
            }
            else
            {
                value = data[index + 3]
                    | ((uint)data[index + 2] << 8)
                    | ((uint)data[index + 1] << 16)
                    | ((uint)data[index + 0] << 24)
                    ;
            }

            return new ElementTypeValue { UInt32 = value };
        }

        public void Write(byte[] data, ulong index, bool isLittleEndian, ElementTypeValue numberValue)
        {
            var value = numberValue.UInt32;
            if (isLittleEndian)
            {
                data[index + 3] = (byte)(value >> 24);
                data[index + 2] = (byte)(value >> 16);
                data[index + 1] = (byte)(value >> 8);
                data[index + 0] = (byte)(value);
            }
            else
            {
                data[index + 3] = (byte)(value);
                data[index + 2] = (byte)(value >> 8);
                data[index + 1] = (byte)(value >> 16);
                data[index + 0] = (byte)(value >> 24);
            }
        }
    }
}
