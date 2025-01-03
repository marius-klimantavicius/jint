#if !NETFRAMEWORK
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using Jint.Collections;
using Jint.Native.Json;
using Jint.Native.Object;
using Jint.Native.Symbol;
using Jint.Runtime;
using Jint.Runtime.Descriptors;
using Jint.Runtime.Interop;
using Jint.Runtime.Interpreter;

namespace Jint.Native.XmlHttpRequest;

internal class XmlHttpRequestPrototype : Prototype
{
    private const int XmlHttpRequestEventAbort = 0;
    private const int XmlHttpRequestEventError = 1;
    private const int XmlHttpRequestEventLoad = 2;
    private const int XmlHttpRequestEventLoadend = 3;
    private const int XmlHttpRequestEventLoadstart = 4;
    private const int XmlHttpRequestEventProgress = 5;
    private const int XmlHttpRequestEventReadystatechange = 6;
    private const int XmlHttpRequestEventTimeout = 7;

    private readonly XmlHttpRequestConstructor _constructor;

    public XmlHttpRequestPrototype(
        Engine engine,
        Realm realm,
        XmlHttpRequestConstructor constructor,
        ObjectPrototype objectPrototype
    ) : base(engine, realm)
    {
        _prototype = objectPrototype;
        _constructor = constructor;
    }

    protected override void Initialize()
    {
        const PropertyFlag propertyFlags = PropertyFlag.Writable | PropertyFlag.Configurable;
        var properties = new PropertyDictionary(26, checkExistingKeys: false)
        {
            ["constructor"] = new PropertyDescriptor(_constructor, PropertyFlag.NonEnumerable),
            ["readyState"] = new GetSetPropertyDescriptor(new ClrFunction(_engine, "get readyState", GetReadyState), set: Undefined, propertyFlags),
            ["response"] = new GetSetPropertyDescriptor(new ClrFunction(_engine, "get response", GetResponse), set: Undefined, propertyFlags),
            ["responseText"] = new GetSetPropertyDescriptor(new ClrFunction(_engine, "get responseText", GetResponseText), set: Undefined, propertyFlags),
            ["responseType"] = new GetSetPropertyDescriptor(new ClrFunction(_engine, "get responseType", GetResponseType), set: new ClrFunction(_engine, "set responseType", SetResponseType, 1), propertyFlags),
            ["responseURL"] = new GetSetPropertyDescriptor(new ClrFunction(_engine, "get responseURL", GetResponseUrl), set: Undefined, propertyFlags),
            ["status"] = new GetSetPropertyDescriptor(new ClrFunction(_engine, "get status", GetStatus), set: Undefined, propertyFlags),
            ["statusText"] = new GetSetPropertyDescriptor(new ClrFunction(_engine, "get statusText", GetStatusText), set: Undefined, propertyFlags),
            ["timeout"] = new GetSetPropertyDescriptor(new ClrFunction(_engine, "get timeout", GetTimeout), set: new ClrFunction(_engine, "set timeout", SetTimeout, 1), propertyFlags),
            ["upload"] = new GetSetPropertyDescriptor(new ClrFunction(_engine, "get upload", GetUpload), set: Undefined, propertyFlags),
            ["withCredentials"] = new GetSetPropertyDescriptor(new ClrFunction(_engine, "get withCredentials", GetWithCredentials), set: new ClrFunction(_engine, "set withCredentials", SetWithCredentials, 1), propertyFlags),
            ["abort"] = new PropertyDescriptor(new ClrFunction(_engine, "abort", Abort), propertyFlags),
            ["getAllResponseHeaders"] = new PropertyDescriptor(new ClrFunction(_engine, "getAllResponseHeaders", GetAllResponseHeaders), propertyFlags),
            ["getResponseHeader"] = new PropertyDescriptor(new ClrFunction(_engine, "getResponseHeader", GetResponseHeader, 1), propertyFlags),
            ["open"] = new PropertyDescriptor(new ClrFunction(_engine, "open", Open, 5), propertyFlags),
            ["overrideMimeType"] = new PropertyDescriptor(new ClrFunction(_engine, "overrideMimeType", OverrideMimeType, 1), propertyFlags),
            ["send"] = new PropertyDescriptor(new ClrFunction(_engine, "send", Send, 1), propertyFlags),
            ["setRequestHeader"] = new PropertyDescriptor(new ClrFunction(_engine, "setRequestHeader", SetRequestHeader, 2), propertyFlags),
        };

        DefineEvent(properties, "onabort", XmlHttpRequestEventAbort);
        DefineEvent(properties, "onerror", XmlHttpRequestEventError);
        DefineEvent(properties, "onload", XmlHttpRequestEventLoad);
        DefineEvent(properties, "onloadend", XmlHttpRequestEventLoadend);
        DefineEvent(properties, "onloadstart", XmlHttpRequestEventLoadstart);
        DefineEvent(properties, "onprogress", XmlHttpRequestEventProgress);
        DefineEvent(properties, "onreadystatechange", XmlHttpRequestEventReadystatechange);
        DefineEvent(properties, "ontimeout", XmlHttpRequestEventTimeout);

        SetProperties(properties);

        var symbols = new SymbolDictionary(1) { [GlobalSymbolRegistry.ToStringTag] = new PropertyDescriptor("XMLHttpRequest", PropertyFlag.Configurable), };
        SetSymbols(symbols);
    }

    private void DefineEvent(PropertyDictionary properties, string name, int eventCode)
    {
        properties[name] = new GetSetPropertyDescriptor(
            new ClrFunction(_engine, $"get {name}", (thisObj, _) =>
            {
                var instance = AssertInstance(thisObj);
                return instance.Events[eventCode];
            }, 0, PropertyFlag.Configurable),
            new ClrFunction(_engine, $"set {name}", (thisObj, arguments) =>
            {
                var instance = AssertInstance(thisObj);
                var eventHandler = arguments.At(0);
                if (eventHandler == Undefined || eventHandler == Null || eventHandler is ICallable)
                    instance.Events[eventCode] = eventHandler;

                return Undefined;
            }, 1, PropertyFlag.Configurable)
        );
    }

    private JsValue GetReadyState(JsValue thisObj, JsValue[] arguments)
    {
        var instance = AssertInstance(thisObj);
        return (int) instance.ReadyState;
    }

    private JsValue GetResponse(JsValue thisObj, JsValue[] arguments)
    {
        var instance = AssertInstance(thisObj);
        return instance.Response;
    }

    private JsValue GetResponseText(JsValue thisObj, JsValue[] arguments)
    {
        var instance = AssertInstance(thisObj);
        return instance.ResponseText;
    }

    private JsValue GetResponseType(JsValue thisObj, JsValue[] arguments)
    {
        var instance = AssertInstance(thisObj);
        return instance.ResponseType switch
        {
            XmlHttpResponseType.Default => "text",
            XmlHttpResponseType.Text => "text",
            XmlHttpResponseType.ArrayBuffer => "arraybuffer",
            XmlHttpResponseType.Json => "json",
            _ => throw new JavaScriptException("Invalid response type"),
        };
    }

    private JsValue SetResponseType(JsValue thisObj, JsValue[] arguments)
    {
        var instance = AssertInstance(thisObj);
        if (instance.ReadyState >= XmlHttpRequestReadyState.Loading)
            ExceptionHelper.ThrowError(_engine, "InvalidStateError");

        var arg = arguments.At(0);
        if (arg == Null || arg == Undefined)
            return Undefined;

        var responseType = arg.AsString();
        switch (responseType)
        {
            case "text":
                instance.ResponseType = XmlHttpResponseType.Text;
                break;
            case "arraybuffer":
                instance.ResponseType = XmlHttpResponseType.ArrayBuffer;
                break;
            case "json":
                instance.ResponseType = XmlHttpResponseType.Json;
                break;
            case "":
                instance.ResponseType = XmlHttpResponseType.Default;
                break;
        }

        return Undefined;
    }

    private JsValue GetResponseUrl(JsValue thisObj, JsValue[] arguments)
    {
        var instance = AssertInstance(thisObj);
        return instance.Url;
    }

    private JsValue GetStatus(JsValue thisObj, JsValue[] arguments)
    {
        var instance = AssertInstance(thisObj);
        return instance.Status;
    }

    private JsValue GetStatusText(JsValue thisObj, JsValue[] arguments)
    {
        var instance = AssertInstance(thisObj);
        return instance.StatusText;
    }

    private JsValue GetTimeout(JsValue thisObj, JsValue[] arguments)
    {
        var instance = AssertInstance(thisObj);
        return instance.Timeout;
    }

    private JsValue SetTimeout(JsValue thisObj, JsValue[] arguments)
    {
        var instance = AssertInstance(thisObj);
        if (instance.ReadyState >= XmlHttpRequestReadyState.Loading)
            ExceptionHelper.ThrowError(_engine, "InvalidStateError");

        // TODO: exceptions
        var arg = arguments.At(0);
        instance.Timeout = TypeConverter.ToInt32(arg);

        if (!instance.IsSent)
            instance.HttpClient.Timeout = TimeSpan.FromMilliseconds(instance.Timeout);

        return Undefined;
    }

    private JsValue GetUpload(JsValue thisObj, JsValue[] arguments)
    {
        return Undefined;
    }

    private JsValue GetWithCredentials(JsValue thisObj, JsValue[] arguments)
    {
        var instance = AssertInstance(thisObj);
        return instance.WithCredentials;
    }

    private JsValue SetWithCredentials(JsValue thisObj, JsValue[] arguments)
    {
        var instance = AssertInstance(thisObj);
        instance.WithCredentials = arguments.At(0).ToBoolean();
        return Undefined;
    }

    private JsValue Abort(JsValue thisObj, JsValue[] arguments)
    {
        var instance = AssertInstance(thisObj);

        if (instance.RequestMessage != null)
        {
            instance.HttpClient.CancelPendingRequests();

            instance.RequestMessage = null;
            instance.ReadyState = XmlHttpRequestReadyState.Unsent;
            instance.Status = 0;
            instance.StatusText = "";

            EmitEvent(instance, XmlHttpRequestEventAbort, Undefined);
        }

        return Undefined;
    }

    private JsValue GetAllResponseHeaders(JsValue thisObj, JsValue[] arguments)
    {
        var instance = AssertInstance(thisObj);
        if (instance.ResponseMessage != null)
        {
            var sb = new StringBuilder();
            foreach (var item in instance.ResponseMessage.Headers)
            {
                if ("Set-Cookie".Equals(item.Key, StringComparison.OrdinalIgnoreCase))
                    continue;

                sb.Append(item.Key.ToLowerInvariant());
                sb.Append(": ");
                sb.Append(string.Join(", ", item.Value));
                sb.Append("\r\n");
            }

            return sb.ToString();
        }

        return Null;
    }

    private JsValue GetResponseHeader(JsValue thisObj, JsValue[] arguments)
    {
        var instance = AssertInstance(thisObj);
        if (instance.ResponseMessage != null)
        {
            var name = TypeConverter.ToString(arguments.At(0));
            if (instance.ResponseMessage.Headers.TryGetValues(name, out var values))
                return string.Join(", ", values);
        }

        return Null;
    }

    private JsValue OverrideMimeType(JsValue thisObj, JsValue[] arguments)
    {
        ExceptionHelper.ThrowTypeError(_realm, "unsupported");
        return Undefined;
    }

    private JsValue SetRequestHeader(JsValue thisObj, JsValue[] arguments)
    {
        var instance = AssertInstance(thisObj);
        var name = arguments.At(0);
        if (!name.IsString())
            return Undefined;

        if (instance.ReadyState < XmlHttpRequestReadyState.Opened || instance.IsSent)
            ExceptionHelper.ThrowError(_engine, "InvalidStateError");

        instance.RequestMessage ??= new HttpRequestMessage();

        var value = arguments.At(1);
        if (value != Null && value != Undefined)
        {
            var stringValue = TypeConverter.ToString(value);
            instance.RequestMessage.Headers.Add(name.AsString(), stringValue);
        }

        return Undefined;
    }

    private JsValue Open(JsValue thisObj, JsValue[] arguments)
    {
        var instance = AssertInstance(thisObj);
        if (instance.ReadyState == XmlHttpRequestReadyState.Done)
        {
            instance.RequestMessage = null;
            instance.ResponseMessage = null;
            instance.ReadyState = XmlHttpRequestReadyState.Unsent;

            for (var i = 0; i < instance.Events.Length; i++)
                instance.Events[i] = Undefined;
        }

        if (instance.ReadyState < XmlHttpRequestReadyState.Opened)
        {
            var method = TypeConverter.ToString(arguments.At(0));
            var url = TypeConverter.ToString(arguments.At(1));
            var async = ToBoolean(arguments.At(2), true);
            var user = ToStringOrNull(arguments.At(3));
            var password = ToStringOrNull(arguments.At(4));

            instance.RequestMessage ??= new HttpRequestMessage();

            instance.RequestMessage.Method = new HttpMethod(method);
            instance.RequestMessage.RequestUri = new Uri(url);

            if (!string.IsNullOrEmpty(user) || !string.IsNullOrEmpty(password))
                instance.RequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($"{user}:{password}")));

            instance.IsAsync = async;
            instance.ReadyState = XmlHttpRequestReadyState.Opened;
            instance.Url = url;

            EmitEvent(instance, XmlHttpRequestEventReadystatechange, Undefined);
        }

        return Undefined;
    }

    private JsValue Send(JsValue thisObj, JsValue[] arguments)
    {
        var instance = AssertInstance(thisObj);

        if (!instance.IsSent)
        {
            var requestMessage = instance.RequestMessage;
            if (requestMessage == null)
                return Undefined;

            instance.IsSent = true;

            var body = arguments.At(0);
            if (body.IsString())
                requestMessage.Content = new StringContent(body.AsString());
            else if (body.IsArrayBuffer())
                requestMessage.Content = new ByteArrayContent(body.AsArrayBuffer() ?? System.Array.Empty<byte>());
            else if (body.IsDataView())
                requestMessage.Content = new ByteArrayContent(body.AsDataView() ?? System.Array.Empty<byte>());
            else if (body.IsUint8Array())
                requestMessage.Content = new ByteArrayContent(body.AsUint8Array());

            if (instance.ResponseType == XmlHttpResponseType.Json)
                requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            else if (instance.ResponseType == XmlHttpResponseType.Text)
                requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/plain"));
            else if (instance.ResponseType == XmlHttpResponseType.ArrayBuffer)
                requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/octet-stream"));

            if (instance.IsAsync)
            {
                instance.AsyncTask = SendAsync(instance, requestMessage, CancellationToken.None);
            }
            else
            {
                try
                {
#if NETCOREAPP
                    var responseMessage = instance.ResponseMessage = instance.HttpClient.Send(requestMessage, HttpCompletionOption.ResponseHeadersRead);
#else
                    var responseMessage = instance.ResponseMessage = instance.HttpClient.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead).GetAwaiter().GetResult();
#endif

                    EmitEvent(instance, XmlHttpRequestEventLoadstart, Undefined);

                    instance.Status = (int) responseMessage.StatusCode;
                    instance.StatusText = responseMessage.ReasonPhrase;

                    instance.ReadyState = XmlHttpRequestReadyState.HeadersReceived;
                    EmitEvent(instance, XmlHttpRequestEventReadystatechange, Undefined);

                    instance.ReadyState = XmlHttpRequestReadyState.Loading;
                    EmitEvent(instance, XmlHttpRequestEventReadystatechange, Undefined);

                    var ev = _engine.Intrinsics.Object.Construct(3);
                    ev.FastSetProperty("lengthComputable", new PropertyDescriptor(true, PropertyFlag.Enumerable | PropertyFlag.Configurable));

                    if (instance.ResponseType == XmlHttpResponseType.ArrayBuffer)
                    {
                        var responsebBody = responseMessage.Content.ReadAsByteArrayAsync().GetAwaiter().GetResult();
                        ev.FastSetProperty("loaded", new PropertyDescriptor(responsebBody.Length, PropertyFlag.Enumerable | PropertyFlag.Configurable));
                        ev.FastSetProperty("total", new PropertyDescriptor(responsebBody.Length, PropertyFlag.Enumerable | PropertyFlag.Configurable));

                        instance.Response = _engine.Intrinsics.ArrayBuffer.Construct(responsebBody);
                    }
                    else
                    {
                        var responseText = responseMessage.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                        ev.FastSetProperty("loaded", new PropertyDescriptor(responseText.Length, PropertyFlag.Enumerable | PropertyFlag.Configurable));
                        ev.FastSetProperty("total", new PropertyDescriptor(responseText.Length, PropertyFlag.Enumerable | PropertyFlag.Configurable));

                        instance.ResponseText = responseText;
                        instance.Response = responseText;

                        if (instance.ResponseType == XmlHttpResponseType.Json)
                        {
                            var parser = new JsonParser(_engine);
                            instance.Response = parser.Parse(responseText);
                        }
                    }

                    instance.ReadyState = XmlHttpRequestReadyState.Done;
                    EmitEvent(instance, XmlHttpRequestEventReadystatechange, Undefined);

                    EmitEvent(instance, XmlHttpRequestEventLoadend, ev);
                    EmitEvent(instance, XmlHttpRequestEventLoad, ev);
                }
                catch (TaskCanceledException)
                {
                    instance.ReadyState = XmlHttpRequestReadyState.Done;
                    EmitEvent(instance, XmlHttpRequestEventReadystatechange, Undefined);

                    EmitEvent(instance, XmlHttpRequestEventLoadend, Undefined);
                    EmitEvent(instance, XmlHttpRequestEventTimeout, Undefined);
                }
                catch (Exception)
                {
                    instance.ReadyState = XmlHttpRequestReadyState.Done;
                    EmitEvent(instance, XmlHttpRequestEventReadystatechange, Undefined);

                    EmitEvent(instance, XmlHttpRequestEventLoadend, Undefined);
                    EmitEvent(instance, XmlHttpRequestEventError, Undefined);
                }
            }
        }

        return Undefined;
    }

    private async Task SendAsync(XmlHttpRequestInstance instance, HttpRequestMessage requestMessage, CancellationToken cancellationToken)
    {
        try
        {
            var responseMessage = instance.ResponseMessage = await instance.HttpClient.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);

            EmitEvent(instance, XmlHttpRequestEventLoadstart, Undefined);

            instance.Status = (int) responseMessage.StatusCode;
            instance.StatusText = responseMessage.ReasonPhrase;

            instance.ReadyState = XmlHttpRequestReadyState.HeadersReceived;
            EmitEvent(instance, XmlHttpRequestEventReadystatechange, Undefined);

            instance.ReadyState = XmlHttpRequestReadyState.Loading;
            EmitEvent(instance, XmlHttpRequestEventReadystatechange, Undefined);

            var ev = _engine.Intrinsics.Object.Construct(3);
            ev.FastSetProperty("lengthComputable", new PropertyDescriptor(true, PropertyFlag.Enumerable | PropertyFlag.Configurable));

            // TODO: implement progress events
            if (instance.ResponseType == XmlHttpResponseType.ArrayBuffer)
            {
#if NETCOREAPP
                var responsebBody = await responseMessage.Content.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false);
#else
                var responsebBody = await responseMessage.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
#endif
                ev.FastSetProperty("loaded", new PropertyDescriptor(responsebBody.Length, PropertyFlag.Enumerable | PropertyFlag.Configurable));
                ev.FastSetProperty("total", new PropertyDescriptor(responsebBody.Length, PropertyFlag.Enumerable | PropertyFlag.Configurable));
                instance.Response = _engine.Intrinsics.ArrayBuffer.Construct(responsebBody);
            }
            else
            {
#if NETCOREAPP
                var responseText = await responseMessage.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
#else
                var responseText = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
#endif
                ev.FastSetProperty("loaded", new PropertyDescriptor(responseText.Length, PropertyFlag.Enumerable | PropertyFlag.Configurable));
                ev.FastSetProperty("total", new PropertyDescriptor(responseText.Length, PropertyFlag.Enumerable | PropertyFlag.Configurable));

                instance.ResponseText = responseText;
                instance.Response = responseText;

                if (instance.ResponseType == XmlHttpResponseType.Json)
                {
                    var parser = new JsonParser(_engine);
                    instance.Response = parser.Parse(responseText);
                }
            }

            instance.ReadyState = XmlHttpRequestReadyState.Done;
            EmitEvent(instance, XmlHttpRequestEventReadystatechange, Undefined);

            EmitEvent(instance, XmlHttpRequestEventLoadend, ev);
            EmitEvent(instance, XmlHttpRequestEventLoad, ev);
        }
        catch (TimeoutException)
        {
            instance.ReadyState = XmlHttpRequestReadyState.Done;
            EmitEvent(instance, XmlHttpRequestEventReadystatechange, Undefined);

            EmitEvent(instance, XmlHttpRequestEventLoadend, Undefined);
            EmitEvent(instance, XmlHttpRequestEventTimeout, Undefined);
        }
        catch (Exception)
        {
            instance.ReadyState = XmlHttpRequestReadyState.Done;
            EmitEvent(instance, XmlHttpRequestEventReadystatechange, Undefined);

            EmitEvent(instance, XmlHttpRequestEventLoadend, Undefined);
            EmitEvent(instance, XmlHttpRequestEventError, Undefined);
        }
    }

    private void EmitEvent(XmlHttpRequestInstance instance, int eventCode, JsValue arg)
    {
        var eventHandler = instance.Events[eventCode];
        if (eventHandler is ICallable callable)
        {
            var engine = _engine;
            var callingContext = _engine._activeEvaluationContext;

            engine.AddToEventLoop(() =>
            {
                var ownsContext = engine._activeEvaluationContext is null;
                engine._activeEvaluationContext ??= callingContext;
                engine._activeEvaluationContext ??= new EvaluationContext(engine);
                try
                {
                    var arguments = new JsValue[] { arg };
                    callable.Call(Undefined, arguments);
                }
                finally
                {
                    if (ownsContext)
                        engine._activeEvaluationContext = null;
                }
            });

        }
    }

    private XmlHttpRequestInstance AssertInstance(JsValue thisObj)
    {
        if (thisObj is XmlHttpRequestInstance instance)
            return instance;

        ExceptionHelper.ThrowTypeError(_realm, "object must be an XMLHttpRequest");
        return null;
    }

    private static string? ToStringOrNull(JsValue value)
    {
        if (value == Undefined || value == Null)
            return null;

        return TypeConverter.ToString(value);
    }

    private static bool ToBoolean(JsValue value, bool defaultValue)
    {
        if (value == Undefined || value == Null)
            return defaultValue;

        return TypeConverter.ToBoolean(value);
    }
}
#endif
