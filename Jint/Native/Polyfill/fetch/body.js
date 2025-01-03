function isDataView(obj) {
    return obj && isPrototypeOf(DataView.prototype, obj);
}

function consumed(body) {
    if (body._noBody) {
        return;
    }

    if (body.bodyUsed) {
        return Promise.reject(new TypeError('Already read'));
    }

    body.bodyUsed = true;
}

function readArrayBufferAsText(buf) {
    var view = new Uint8Array(buf);
    var chars = new Array(view.length);

    for (var i = 0; i < view.length; i++) {
        chars[i] = String.fromCharCode(view[i]);
    }

    return chars.join('');
}

function bufferClone(buf) {
    if (buf.slice) {
        return buf.slice(0);
    } else {
        const view = new Uint8Array(buf.byteLength);

        view.set(new Uint8Array(buf));

        return view.buffer;
    }
}

function isPrototypeOf(a, b) {
    return Object.prototype.isPrototypeOf.call(a, b);
}

export const BodyMixin = {
    bodyUsed: false,

    _initBody(body) {
        this._bodyInit = body;

        if (!body) {
            this._noBody = true;
            this._bodyText = '';
        } else if (typeof body === 'string') {
            this._bodyText = body;
        } else if (isDataView(body)) {
            this._bodyArrayBuffer = bufferClone(body.buffer);
        } else if (isPrototypeOf(ArrayBuffer.prototype, body) || ArrayBuffer.isView(body)) {
            this._bodyArrayBuffer = bufferClone(body);
        } else {
            this._bodyText = body = Object.prototype.toString.call(body);
        }

        if (!this.headers.get('content-type')) {
            if (typeof body === 'string') {
                this.headers.set('content-type', 'text/plain;charset=UTF-8');
            }
        }
    },

    arrayBuffer() {
        if (this._bodyArrayBuffer) {
            var isConsumed = consumed(this);

            if (isConsumed) {
                return isConsumed;
            } else if (ArrayBuffer.isView(this._bodyArrayBuffer)) {
                return Promise.resolve(
                    this._bodyArrayBuffer.buffer.slice(
                        this._bodyArrayBuffer.byteOffset,
                        this._bodyArrayBuffer.byteOffset + this._bodyArrayBuffer.byteLength
                    )
                );
            } else {
                return Promise.resolve(this._bodyArrayBuffer);
            }
        } else {
            var encoder = new TextEncoder();
            return Promise.resolve(encoder.encode(this._bodyText));
        }
    },

    text() {
        const rejected = consumed(this);

        if (rejected) {
            return rejected;
        }

        if (this._bodyArrayBuffer) {
            return Promise.resolve(readArrayBufferAsText(this._bodyArrayBuffer));
        } else {
            return Promise.resolve(this._bodyText);
        }
    },

    json() {
        return this.text().then(JSON.parse);
    },
};
