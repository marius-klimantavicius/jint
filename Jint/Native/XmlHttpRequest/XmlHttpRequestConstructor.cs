#if !NETFRAMEWORK
using Jint.Collections;
using Jint.Native.Function;
using Jint.Native.Object;
using Jint.Native.Symbol;
using Jint.Runtime;
using Jint.Runtime.Descriptors;
using Jint.Runtime.Interop;

namespace Jint.Native.XmlHttpRequest;

internal class XmlHttpRequestConstructor : Constructor
{
    private static readonly JsString _functionName = new JsString("XMLHttpRequest");

    public XmlHttpRequestPrototype PrototypeObject { get; set; }

    public XmlHttpRequestConstructor(
        Engine engine,
        Realm realm,
        FunctionPrototype functionPrototype,
        ObjectPrototype objectPrototype) : base(engine, realm, _functionName)
    {
        _prototype = functionPrototype;
        PrototypeObject = new XmlHttpRequestPrototype(engine, realm, this, objectPrototype);
        _length = new PropertyDescriptor(0, PropertyFlag.Configurable);
        _prototypeDescriptor = new PropertyDescriptor(PrototypeObject, PropertyFlag.AllForbidden);
    }

    public override ObjectInstance Construct(JsValue[] arguments, JsValue newTarget)
    {
        var result = OrdinaryCreateFromConstructor(
            newTarget,
            static intrinsics => intrinsics.XmlHttpRequest.PrototypeObject,
            static (Engine engine, Realm _, object? _) => new XmlHttpRequestInstance(engine));

        return result;
    }

    protected override void Initialize()
    {
        const PropertyFlag PropertyFlags = PropertyFlag.Writable | PropertyFlag.Configurable;
        var properties = new PropertyDictionary(6, checkExistingKeys: false)
        {
            ["prototype"] = new PropertyDescriptor(PrototypeObject, PropertyFlag.AllForbidden),
            ["UNSENT"] = new((int)XmlHttpRequestReadyState.Unsent, PropertyFlags),
            ["OPENED"] = new((int)XmlHttpRequestReadyState.Opened, PropertyFlags),
            ["HEADERS_RECEIVED"] = new((int)XmlHttpRequestReadyState.HeadersReceived, PropertyFlags),
            ["LOADING"] = new((int)XmlHttpRequestReadyState.Loading, PropertyFlags),
            ["DONE"] = new((int)XmlHttpRequestReadyState.Done, PropertyFlags),
        };
        SetProperties(properties);

        var symbols = new SymbolDictionary(1)
        {
            [GlobalSymbolRegistry.Species] = new GetSetPropertyDescriptor(get: new ClrFunction(_engine, "get [Symbol.species]", (thisObj, _) => thisObj, 0, PropertyFlag.Configurable), set: Undefined, PropertyFlag.Configurable)
        };
        SetSymbols(symbols);
    }
}
#endif
