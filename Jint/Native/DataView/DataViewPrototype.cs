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
            var dataView = AssertDataViewInstance(thisObj);
            var byteOffset = arguments.At(0);
            var littleEndian = arguments.At(1, JsBoolean.False);
            var span = GetViewValue(dataView, byteOffset, littleEndian, 8);
            return span.GetBigInt64();
        }

        private JsValue GetBigUInt64(JsValue thisObj, JsValue[] arguments)
        {
            var dataView = AssertDataViewInstance(thisObj);
            var byteOffset = arguments.At(0);
            var littleEndian = arguments.At(1, JsBoolean.False);
            var span = GetViewValue(dataView, byteOffset, littleEndian, 8);
            return span.GetBigUInt64();
        }

        private JsValue GetFloat32(JsValue thisObj, JsValue[] arguments)
        {
            var dataView = AssertDataViewInstance(thisObj);
            var byteOffset = arguments.At(0);
            var littleEndian = arguments.At(1, JsBoolean.False);
            var span = GetViewValue(dataView, byteOffset, littleEndian, 4);
            return span.GetFloat32();
        }

        private JsValue GetFloat64(JsValue thisObj, JsValue[] arguments)
        {
            var dataView = AssertDataViewInstance(thisObj);
            var byteOffset = arguments.At(0);
            var littleEndian = arguments.At(1, JsBoolean.False);
            var span = GetViewValue(dataView, byteOffset, littleEndian, 8);
            return span.GetFloat64();
        }

        private JsValue GetInt8(JsValue thisObj, JsValue[] arguments)
        {
            var dataView = AssertDataViewInstance(thisObj);
            var byteOffset = arguments.At(0);
            var littleEndian = arguments.At(1, JsBoolean.False);
            var span = GetViewValue(dataView, byteOffset, littleEndian, 1);
            return span.GetInt8();
        }

        private JsValue GetInt16(JsValue thisObj, JsValue[] arguments)
        {
            var dataView = AssertDataViewInstance(thisObj);
            var byteOffset = arguments.At(0);
            var littleEndian = arguments.At(1, JsBoolean.False);
            var span = GetViewValue(dataView, byteOffset, littleEndian, 2);
            return span.GetInt16();
        }

        private JsValue GetInt32(JsValue thisObj, JsValue[] arguments)
        {
            var dataView = AssertDataViewInstance(thisObj);
            var byteOffset = arguments.At(0);
            var littleEndian = arguments.At(1, JsBoolean.False);
            var span = GetViewValue(dataView, byteOffset, littleEndian, 4);
            return span.GetInt32();
        }

        private JsValue GetUInt8(JsValue thisObj, JsValue[] arguments)
        {
            var span = GetViewValue(thisObj, arguments, 1);
            return span.GetUInt8();
        }

        private JsValue GetUInt16(JsValue thisObj, JsValue[] arguments)
        {
            var span = GetViewValue(thisObj, arguments, 2);
            return span.GetUInt16();
        }

        private JsValue GetUInt32(JsValue thisObj, JsValue[] arguments)
        {
            var span = GetViewValue(thisObj, arguments, 4);
            return span.GetUInt32();
        }

        private JsValue SetBigInt64(JsValue thisObj, JsValue[] arguments)
        {
            var span = SetViewValue(thisObj, arguments, 8, out var numberValue);
            if (double.IsNaN(numberValue) || double.IsInfinity(numberValue))
            {
                numberValue = 0;
            }
            span.SetBigInt64(numberValue);
            return JsValue.Undefined;
        }

        private JsValue SetBigUInt64(JsValue thisObj, JsValue[] arguments)
        {
            var span = SetViewValue(thisObj, arguments, 8, out var numberValue);
            if (double.IsNaN(numberValue) || double.IsInfinity(numberValue))
            {
                numberValue = 0;
            }
            span.SetBigUInt64(numberValue);
            return JsValue.Undefined;
        }

        private JsValue SetFloat32(JsValue thisObj, JsValue[] arguments)
        {
            var span = SetViewValue(thisObj, arguments, 4, out var numberValue);
            span.SetFloat32(numberValue);
            return JsValue.Undefined;
        }

        private JsValue SetFloat64(JsValue thisObj, JsValue[] arguments)
        {
            var span = SetViewValue(thisObj, arguments, 8, out var numberValue);
            span.SetFloat64(numberValue);
            return JsValue.Undefined;
        }

        private JsValue SetInt8(JsValue thisObj, JsValue[] arguments)
        {
            var span = SetViewValue(thisObj, arguments, 1, out var numberValue);
            if (double.IsNaN(numberValue) || double.IsInfinity(numberValue))
            {
                numberValue = 0;
            }
            span.SetInt8(numberValue);
            return JsValue.Undefined;
        }

        private JsValue SetInt16(JsValue thisObj, JsValue[] arguments)
        {
            var span = SetViewValue(thisObj, arguments, 2, out var numberValue);
            if (double.IsNaN(numberValue) || double.IsInfinity(numberValue))
            {
                numberValue = 0;
            }
            span.SetInt16(numberValue);
            return JsValue.Undefined;
        }

        private JsValue SetInt32(JsValue thisObj, JsValue[] arguments)
        {
            var span = SetViewValue(thisObj, arguments, 4, out var numberValue);
            if (double.IsNaN(numberValue) || double.IsInfinity(numberValue))
            {
                numberValue = 0;
            }
            span.SetInt32(numberValue);
            return JsValue.Undefined;
        }

        private JsValue SetUInt8(JsValue thisObj, JsValue[] arguments)
        {
            var span = SetViewValue(thisObj, arguments, 1, out var numberValue);
            if (double.IsNaN(numberValue) || double.IsInfinity(numberValue))
            {
                numberValue = 0;
            }
            span.SetUInt8(numberValue);
            return JsValue.Undefined;
        }

        private JsValue SetUInt16(JsValue thisObj, JsValue[] arguments)
        {
            var span = SetViewValue(thisObj, arguments, 2, out var numberValue);
            if (double.IsNaN(numberValue) || double.IsInfinity(numberValue))
            {
                numberValue = 0;
            }
            span.SetUInt16(numberValue);
            return JsValue.Undefined;
        }

        private JsValue SetUInt32(JsValue thisObj, JsValue[] arguments)
        {
            var span = SetViewValue(thisObj, arguments, 4, out var numberValue);
            if (double.IsNaN(numberValue) || double.IsInfinity(numberValue))
            {
                numberValue = 0;
            }
            span.SetUInt32(numberValue);
            return JsValue.Undefined;
        }

        private DataViewSpan SetViewValue(JsValue thisObj, JsValue[] arguments, uint elementSize, out double numberValue)
        {
            var dataView = AssertDataViewInstance(thisObj);
            var byteOffset = arguments.At(0);
            var value = arguments.At(1);
            var littleEndian = arguments.At(2, JsBoolean.False);
            return SetViewValue(dataView, byteOffset, littleEndian, elementSize, value, out numberValue);
        }

        private DataViewSpan GetViewValue(JsValue thisObj, JsValue[] arguments, uint elementSize)
        {
            var dataView = AssertDataViewInstance(thisObj);
            var byteOffset = arguments.At(0);
            var littleEndian = arguments.At(1, JsBoolean.False);
            var span = GetViewValue(dataView, byteOffset, littleEndian, elementSize);
            return span;
        }

        private DataViewSpan GetViewValue(DataViewInstance view, JsValue requestIndex, JsValue littleEndian, uint elementSize)
        {
            var getIndex = TypeConverter.ToIndex(_engine, requestIndex);
            var isLittleEndian = TypeConverter.ToBoolean(littleEndian);
            var buffer = view._buffer;
            if (buffer._data == null)
            {
                ExceptionHelper.ThrowTypeError(_engine);
            }

            var viewOffset = view._byteOffset;
            var viewSize = view._byteLength;
            if (getIndex + elementSize > viewSize)
            {
                ExceptionHelper.ThrowRangeError(_engine);
            }

            var bufferIndex = getIndex + viewOffset;
            return new DataViewSpan(buffer._data, bufferIndex, isLittleEndian);
        }

        private DataViewSpan SetViewValue(DataViewInstance view, JsValue requestIndex, JsValue littleEndian, uint elementSize, JsValue value, out double numberValue)
        {
            var getIndex = TypeConverter.ToIndex(_engine, requestIndex);
            numberValue = TypeConverter.ToNumber(value);
            var isLittleEndian = TypeConverter.ToBoolean(littleEndian);
            var buffer = view._buffer;
            if (buffer._data == null)
            {
                ExceptionHelper.ThrowTypeError(_engine);
            }

            var viewOffset = view._byteOffset;
            var viewSize = view._byteLength;
            if (getIndex + elementSize > viewSize)
            {
                ExceptionHelper.ThrowRangeError(_engine);
            }

            var bufferIndex = getIndex + viewOffset;
            return new DataViewSpan(buffer._data, bufferIndex, isLittleEndian);
        }

        private DataViewInstance AssertDataViewInstance(JsValue thisObj)
        {
            if (!(thisObj is DataViewInstance dataView))
            {
                return ExceptionHelper.ThrowTypeError<DataViewInstance>(_engine, $"object must be a DataView");
            }

            return dataView;
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct NumericConvert
        {
            [FieldOffset(0)]
            public uint UInt32Value;

            [FieldOffset(0)]
            public ulong UInt64Value;

            [FieldOffset(0)]
            public float Float32Value;

            [FieldOffset(0)]
            public double Float64Value;
        }

        private struct DataViewSpan
        {
            private byte[] _data;
            private ulong _index;
            private bool _isLittleEndian;

            public DataViewSpan(byte[] data, ulong index, bool isLittleEndian)
            {
                _data = data;
                _index = index;
                _isLittleEndian = isLittleEndian;
            }

            internal JsValue GetBigInt64()
            {
                var value = 0L;
                if (_isLittleEndian)
                {
                    value = (_data[_index + 7] << 56)
                        | (_data[_index + 6] << 48)
                        | (_data[_index + 5] << 40)
                        | (_data[_index + 4] << 32)
                        | (_data[_index + 3] << 24)
                        | (_data[_index + 2] << 16)
                        | (_data[_index + 1] << 8)
                        | (_data[_index + 0])
                        ;
                }
                else
                {
                    value = _data[_index + 7]
                        | (_data[_index + 6] << 8)
                        | (_data[_index + 5] << 16)
                        | (_data[_index + 4] << 24)
                        | (_data[_index + 3] << 32)
                        | (_data[_index + 2] << 40)
                        | (_data[_index + 1] << 48)
                        | (_data[_index + 0] << 56)
                        ;
                }

                return value;
            }

            internal JsValue GetBigUInt64()
            {
                var value = 0UL;
                if (_isLittleEndian)
                {
                    value = ((ulong)_data[_index + 7] << 56)
                        | ((ulong)_data[_index + 6] << 48)
                        | ((ulong)_data[_index + 5] << 40)
                        | ((ulong)_data[_index + 4] << 32)
                        | ((ulong)_data[_index + 3] << 24)
                        | ((ulong)_data[_index + 2] << 16)
                        | ((ulong)_data[_index + 1] << 8)
                        | _data[_index + 0]
                        ;
                }
                else
                {
                    value = _data[_index + 7]
                        | ((ulong)_data[_index + 6] << 8)
                        | ((ulong)_data[_index + 5] << 16)
                        | ((ulong)_data[_index + 4] << 24)
                        | ((ulong)_data[_index + 3] << 32)
                        | ((ulong)_data[_index + 2] << 40)
                        | ((ulong)_data[_index + 1] << 48)
                        | ((ulong)_data[_index + 0] << 56)
                        ;
                }

                return value;
            }

            internal JsValue GetFloat32()
            {
                var value = 0U;
                if (_isLittleEndian)
                {
                    value = ((uint)_data[_index + 3] << 24)
                        | ((uint)_data[_index + 2] << 16)
                        | ((uint)_data[_index + 1] << 8)
                        | _data[_index + 0]
                        ;
                }
                else
                {
                    value = _data[_index + 3]
                        | ((uint)_data[_index + 2] << 8)
                        | ((uint)_data[_index + 1] << 16)
                        | ((uint)_data[_index + 0] << 24)
                        ;
                }

                var convert = new NumericConvert
                {
                    UInt32Value = value
                };

                return convert.Float32Value;
            }

            internal JsValue GetFloat64()
            {
                var value = 0UL;
                if (_isLittleEndian)
                {
                    value = ((ulong)_data[_index + 7] << 56)
                        | ((ulong)_data[_index + 6] << 48)
                        | ((ulong)_data[_index + 5] << 40)
                        | ((ulong)_data[_index + 4] << 32)
                        | ((ulong)_data[_index + 3] << 24)
                        | ((ulong)_data[_index + 2] << 16)
                        | ((ulong)_data[_index + 1] << 8)
                        | _data[_index + 0]
                        ;
                }
                else
                {
                    value = _data[_index + 7]
                        | ((ulong)_data[_index + 6] << 8)
                        | ((ulong)_data[_index + 5] << 16)
                        | ((ulong)_data[_index + 4] << 24)
                        | ((ulong)_data[_index + 3] << 32)
                        | ((ulong)_data[_index + 2] << 40)
                        | ((ulong)_data[_index + 1] << 48)
                        | ((ulong)_data[_index + 0] << 56)
                        ;
                }

                var convert = new NumericConvert
                {
                    UInt64Value = value
                };

                return convert.Float64Value;
            }

            internal JsValue GetInt16()
            {
                var value = 0;
                if (_isLittleEndian)
                {
                    value = (_data[_index + 1] << 8)
                        | _data[_index + 0]
                        ;
                }
                else
                {
                    value = (_data[_index + 1])
                        | (_data[_index + 0] << 8)
                        ;
                }

                return (short)value;
            }

            internal JsValue GetInt32()
            {
                var value = 0;
                if (_isLittleEndian)
                {
                    value = (_data[_index + 3] << 24)
                        | (_data[_index + 2] << 16)
                        | (_data[_index + 1] << 8)
                        | _data[_index + 0]
                        ;
                }
                else
                {
                    value = _data[_index + 3]
                        | (_data[_index + 2] << 8)
                        | (_data[_index + 1] << 16)
                        | (_data[_index + 0] << 24)
                        ;
                }

                return value;
            }

            internal JsValue GetInt8()
            {
                var value = (sbyte)_data[_index];
                return value;
            }

            internal JsValue GetUInt16()
            {
                var value = 0U;
                if (_isLittleEndian)
                {
                    value = ((uint)_data[_index + 1] << 8)
                        | _data[_index + 0]
                        ;
                }
                else
                {
                    value = (_data[_index + 1])
                        | ((uint)_data[_index + 0] << 8)
                        ;
                }

                return value;
            }

            internal JsValue GetUInt32()
            {
                var value = 0U;
                if (_isLittleEndian)
                {
                    value = ((uint)_data[_index + 3] << 24)
                        | ((uint)_data[_index + 2] << 16)
                        | ((uint)_data[_index + 1] << 8)
                        | _data[_index + 0]
                        ;
                }
                else
                {
                    value = _data[_index + 3]
                        | ((uint)_data[_index + 2] << 8)
                        | ((uint)_data[_index + 1] << 16)
                        | ((uint)_data[_index + 0] << 24)
                        ;
                }

                return value;
            }

            internal JsValue GetUInt8()
            {
                return (int)_data[_index];
            }

            internal void SetBigInt64(double numberValue)
            {
                var value = (long)numberValue;
                if (_isLittleEndian)
                {
                    _data[_index + 7] = (byte)(value >> 56);
                    _data[_index + 6] = (byte)(value >> 48);
                    _data[_index + 5] = (byte)(value >> 40);
                    _data[_index + 4] = (byte)(value >> 32);
                    _data[_index + 3] = (byte)(value >> 24);
                    _data[_index + 2] = (byte)(value >> 16);
                    _data[_index + 1] = (byte)(value >> 8);
                    _data[_index + 0] = (byte)(value);
                }
                else
                {
                    _data[_index + 7] = (byte)(value);
                    _data[_index + 6] = (byte)(value >> 8);
                    _data[_index + 5] = (byte)(value >> 16);
                    _data[_index + 4] = (byte)(value >> 24);
                    _data[_index + 3] = (byte)(value >> 32);
                    _data[_index + 2] = (byte)(value >> 40);
                    _data[_index + 1] = (byte)(value >> 48);
                    _data[_index + 0] = (byte)(value >> 56);
                }
            }

            internal void SetBigUInt64(double numberValue)
            {
                var value = (ulong)numberValue;
                if (_isLittleEndian)
                {
                    _data[_index + 7] = (byte)(value >> 56);
                    _data[_index + 6] = (byte)(value >> 48);
                    _data[_index + 5] = (byte)(value >> 40);
                    _data[_index + 4] = (byte)(value >> 32);
                    _data[_index + 3] = (byte)(value >> 24);
                    _data[_index + 2] = (byte)(value >> 16);
                    _data[_index + 1] = (byte)(value >> 8);
                    _data[_index + 0] = (byte)(value);
                }
                else
                {
                    _data[_index + 7] = (byte)(value);
                    _data[_index + 6] = (byte)(value >> 8);
                    _data[_index + 5] = (byte)(value >> 16);
                    _data[_index + 4] = (byte)(value >> 24);
                    _data[_index + 3] = (byte)(value >> 32);
                    _data[_index + 2] = (byte)(value >> 40);
                    _data[_index + 1] = (byte)(value >> 48);
                    _data[_index + 0] = (byte)(value >> 56);
                }
            }

            internal void SetFloat32(double numberValue)
            {
                var convert = new NumericConvert
                {
                    Float32Value = (float)numberValue,
                };
                var value = convert.UInt64Value;
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

            internal void SetFloat64(double numberValue)
            {
                var convert = new NumericConvert
                {
                    Float64Value = numberValue,
                };
                var value = convert.UInt64Value;
                if (_isLittleEndian)
                {
                    _data[_index + 7] = (byte)(value >> 56);
                    _data[_index + 6] = (byte)(value >> 48);
                    _data[_index + 5] = (byte)(value >> 40);
                    _data[_index + 4] = (byte)(value >> 32);
                    _data[_index + 3] = (byte)(value >> 24);
                    _data[_index + 2] = (byte)(value >> 16);
                    _data[_index + 1] = (byte)(value >> 8);
                    _data[_index + 0] = (byte)(value);
                }
                else
                {
                    _data[_index + 7] = (byte)(value);
                    _data[_index + 6] = (byte)(value >> 8);
                    _data[_index + 5] = (byte)(value >> 16);
                    _data[_index + 4] = (byte)(value >> 24);
                    _data[_index + 3] = (byte)(value >> 32);
                    _data[_index + 2] = (byte)(value >> 40);
                    _data[_index + 1] = (byte)(value >> 48);
                    _data[_index + 0] = (byte)(value >> 56);
                }
            }

            internal void SetInt16(double numberValue)
            {
                var value = (short)(long)numberValue;
                if (_isLittleEndian)
                {
                    _data[_index + 1] = (byte)(value >> 8);
                    _data[_index + 0] = (byte)(value);
                }
                else
                {
                    _data[_index + 1] = (byte)(value);
                    _data[_index + 0] = (byte)(value >> 8);
                }
            }

            internal void SetInt32(double numberValue)
            {
                var value = (int)(long)numberValue;
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

            internal void SetInt8(double numberValue)
            {
                var value = (long)numberValue;
                _data[_index] = (byte)(value);
            }

            internal void SetUInt16(double numberValue)
            {
                var value = (ushort)(ulong)numberValue;
                if (_isLittleEndian)
                {
                    _data[_index + 1] = (byte)(value >> 8);
                    _data[_index + 0] = (byte)(value);
                }
                else
                {
                    _data[_index + 1] = (byte)(value);
                    _data[_index + 0] = (byte)(value >> 8);
                }
            }

            internal void SetUInt32(double numberValue)
            {
                var value = (uint)(ulong)numberValue;
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

            internal void SetUInt8(double numberValue)
            {
                var value = (long)numberValue;
                _data[_index] = (byte)(value);
            }
        }
    }
}
