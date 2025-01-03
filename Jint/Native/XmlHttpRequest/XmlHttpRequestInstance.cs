#if !NETFRAMEWORK
using System.Net.Http;
using Jint.Native.Object;

namespace Jint.Native.XmlHttpRequest;

internal class XmlHttpRequestInstance : ObjectInstance
{
    public HttpClient HttpClient { get; set; } = new HttpClient();

    public HttpRequestMessage? RequestMessage { get; set; }
    public HttpResponseMessage? ResponseMessage { get; set; }

    public bool IsSent { get; set; }
    public bool IsAsync { get; set; }
    public bool WithCredentials { get; set; }
    public int Timeout { get; set; }
    public XmlHttpRequestReadyState ReadyState { get; set; }
    public XmlHttpResponseType ResponseType { get; set; }
    public JsValue Status { get; set; } = 0;
    public JsValue StatusText { get; set; } = "";

    public JsValue Url { get; set; } = Undefined;
    public JsValue Headers { get; set; } = Undefined;
    public JsValue Response { get; set; } = Undefined;
    public JsValue ResponseText { get; set; } = Undefined;

    public JsValue[] Events { get; } = new JsValue[8];
    
    public Task? AsyncTask { get; set; }

    public XmlHttpRequestInstance(Engine engine) : base(engine)
    {
    }
}
#endif
