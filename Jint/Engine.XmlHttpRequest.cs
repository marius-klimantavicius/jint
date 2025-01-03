#if !NETFRAMEWORK
using System.Reflection;
using Jint.Collections;
using Jint.Native;
using Jint.Native.Object;
using Jint.Runtime;
using Jint.Runtime.Descriptors;
using Jint.Runtime.Descriptors.Specialized;
using Jint.Runtime.Modules;

namespace Jint;

public partial class Engine
{
    public void EnableHttp()
    {
        var core = (JsSymbol)Evaluate("Symbol.for('tjs.internal.core')");
        Global.Set(core, new CoreObjectInstance(this));

        var loader = new polyfillModuleLoader();
        var modules = new ModuleOperations(this, loader);
        loader.Modules = modules;

        modules.Import("./dom-exception.js");
        modules.Import("./event-target-polyfill.js");
        modules.Import("./text-encoding.js");
        modules.Import("./text-encode-transform.js");
        modules.Import("./xhr.js");
        modules.Import("./fetch/polyfill.js");
    }

    private class polyfillModuleLoader : IModuleLoader
    {
        private readonly Uri _basePath = new Uri("assembly:///Jint/Native/Polyfill/", UriKind.Absolute);
        private readonly Assembly _assembly = typeof(Engine).Assembly;

        public ModuleOperations? Modules { get; set; }

        public ResolvedSpecifier Resolve(string? referencingModuleLocation, ModuleRequest moduleRequest)
        {
            var specifier = moduleRequest.Specifier;
            if (string.IsNullOrEmpty(specifier))
            {
                ExceptionHelper.ThrowModuleResolutionException("Invalid Module Specifier", specifier, referencingModuleLocation);
                return default;
            }

            // Specifications from ESM_RESOLVE Algorithm: https://nodejs.org/api/esm.html#resolution-algorithm

            Uri resolved;
            if (Uri.TryCreate(specifier, UriKind.Absolute, out var uri))
            {
                resolved = uri;
            }
            else if (IsRelative(specifier))
            {
                var baseUri = BuildBaseUri(referencingModuleLocation);
                resolved = new Uri(baseUri, specifier);
            }
            else if (specifier[0] == '#')
            {
                ExceptionHelper.ThrowNotSupportedException($"PACKAGE_IMPORTS_RESOLVE is not supported: '{specifier}'");
                return default;
            }
            else
            {
                return new ResolvedSpecifier(
                    moduleRequest,
                    specifier,
                    Uri: null,
                    SpecifierType.Bare
                );
            }

            if (resolved.IsFile)
            {
                if (resolved.UserEscaped)
                {
                    ExceptionHelper.ThrowModuleResolutionException("Invalid Module Specifier", specifier, referencingModuleLocation);
                    return default;
                }

                if (!Path.HasExtension(resolved.LocalPath))
                {
                    ExceptionHelper.ThrowModuleResolutionException("Unsupported Directory Import", specifier, referencingModuleLocation);
                    return default;
                }
            }

            return new ResolvedSpecifier(
                moduleRequest,
                resolved.AbsoluteUri,
                resolved,
                SpecifierType.RelativeOrAbsolute
            );
        }

        public Runtime.Modules.Module LoadModule(Engine engine, ResolvedSpecifier resolved)
        {
            string code;
            try
            {
                code = LoadModuleContents(resolved);
            }
            catch (Exception)
            {
                ExceptionHelper.ThrowJavaScriptException(engine, $"Could not load module {resolved.ModuleRequest.Specifier}", AstExtensions.DefaultLocation);
                return default!;
            }

            var isJson = resolved.ModuleRequest.IsJsonModule();
            var moduleRecord = isJson
                ? ModuleFactory.BuildJsonModule(engine, resolved, code)
                : ModuleFactory.BuildSourceTextModule(engine, Modules, resolved, code);

            return moduleRecord;
        }
        
        private Uri BuildBaseUri(string? referencingModuleLocation)
        {
            if (referencingModuleLocation is not null)
            {
                /*
                  "referencingModuleLocation" might be relative or an invalid URI when a module imports other
                   modules and the importing module is called directly from .NET code.
                   e.g. "engine.Modules.Import("my-module")" and "my-module" imports other modules.

                   Path traversal prevention is not a concern here because it is checked later
                   (if _restrictToBasePath is set to true).
                */
                if (Uri.TryCreate(referencingModuleLocation, UriKind.Absolute, out var referencingLocation) ||
                    Uri.TryCreate(_basePath, referencingModuleLocation, out referencingLocation))
                    return referencingLocation;
            }

            return _basePath;
        }

        private string LoadModuleContents(ResolvedSpecifier resolved)
        {
            var specifier = resolved.ModuleRequest.Specifier;
            if (resolved.Type != SpecifierType.RelativeOrAbsolute)
            {
                ExceptionHelper.ThrowNotSupportedException($"The default module loader can only resolve files. You can define modules directly to allow imports using {nameof(Engine)}.{nameof(Engine.Modules.Add)}(). Attempted to resolve: '{specifier}'.");
            }

            if (resolved.Uri == null)
            {
                ExceptionHelper.ThrowInvalidOperationException($"Module '{specifier}' of type '{resolved.Type}' has no resolved URI.");
            }

            if (!string.IsNullOrEmpty(resolved.Uri.Host))
            {
                ExceptionHelper.ThrowInvalidOperationException($"Module '{specifier}' of type '{resolved.Type}' has invalid host '{resolved.Uri.Host}'.");
            }

            var fileName = Uri.UnescapeDataString(resolved.Uri.AbsolutePath).Replace('/', '.').TrimStart('.');

            using var stream = _assembly.GetManifestResourceStream(fileName);
            if (stream is null)
                ExceptionHelper.ThrowModuleResolutionException("Module Not Found", specifier, parent: null, fileName);

            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }

        private static bool IsRelative(string specifier)
        {
            return specifier.StartsWith('.') || specifier.StartsWith('/');
        }
    }

    private class CoreObjectInstance : ObjectInstance
    {
        public CoreObjectInstance(Engine engine) : base(engine)
        {
        }

        protected override void Initialize()
        {
            const PropertyFlag PropertyFlags = PropertyFlag.Configurable | PropertyFlag.Writable;

            var properties = new StringDictionarySlim<PropertyDescriptor>(1);
            properties.AddDangerous("XMLHttpRequest", new LazyPropertyDescriptor<CoreObjectInstance>(this, static global => global._engine.Intrinsics.XmlHttpRequest, PropertyFlags));

            SetProperties(properties);
        }
    }
}
#endif
