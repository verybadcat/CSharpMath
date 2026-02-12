var Uno;
(function (Uno) {
    var WebAssembly;
    (function (WebAssembly) {
        var Bootstrap;
        (function (Bootstrap) {
            class AotProfilerSupport {
                constructor(context, unoConfig) {
                    this._context = context;
                    this._unoConfig = unoConfig;
                    this.attachProfilerHotKey();
                }
                static initialize(context, unoConfig) {
                    if (Bootstrap.Bootstrapper.ENVIRONMENT_IS_WEB && unoConfig.generate_aot_profile) {
                        return new AotProfilerSupport(context, unoConfig);
                    }
                    return null;
                }
                attachProfilerHotKey() {
                    const altKeyName = navigator.platform.match(/^Mac/i) ? "Cmd" : "Alt";
                    console.info(`AOT Profiler stop hotkey: Shift+${altKeyName}+P (when application has focus), or Run Uno.WebAssembly.Bootstrap.AotProfilerSupport.saveAotProfile() from the browser debug console.`);
                    document.addEventListener("keydown", (evt) => {
                        if (evt.shiftKey && (evt.metaKey || evt.altKey) && evt.code === "KeyP") {
                            this.saveAotProfile();
                        }
                    });
                }
                async initializeProfile() {
                    let anyContext = this._context;
                    if (anyContext.getAssemblyExports !== undefined) {
                        this._aotProfilerExports = await anyContext.getAssemblyExports("Uno.Wasm.AotProfiler");
                    }
                    else {
                        throw `Unable to find getAssemblyExports`;
                    }
                }
                async saveAotProfile() {
                    await this.initializeProfile();
                    this._aotProfilerExports.Uno.AotProfilerSupport.StopProfile();
                    var a = window.document.createElement('a');
                    var blob = new Blob([this._context.INTERNAL.aotProfileData]);
                    a.href = window.URL.createObjectURL(blob);
                    a.download = "aot.profile";
                    document.body.appendChild(a);
                    a.click();
                    document.body.removeChild(a);
                }
            }
            Bootstrap.AotProfilerSupport = AotProfilerSupport;
        })(Bootstrap = WebAssembly.Bootstrap || (WebAssembly.Bootstrap = {}));
    })(WebAssembly = Uno.WebAssembly || (Uno.WebAssembly = {}));
})(Uno || (Uno = {}));
var Uno;
(function (Uno) {
    var WebAssembly;
    (function (WebAssembly) {
        var Bootstrap;
        (function (Bootstrap) {
            class HotReloadSupport {
                constructor(context, unoConfig) {
                    this._context = context;
                    this._unoConfig = unoConfig;
                }
                static async tryInitializeExports(getAssemblyExports) {
                    let exports = await getAssemblyExports("Uno.Wasm.MetadataUpdater");
                    this._getApplyUpdateCapabilitiesMethod = exports.Uno.Wasm.MetadataUpdate.WebAssemblyHotReload.GetApplyUpdateCapabilities;
                    this._applyHotReloadDeltaMethod = exports.Uno.Wasm.MetadataUpdate.WebAssemblyHotReload.ApplyHotReloadDelta;
                    this._initializeMethod = exports.Uno.Wasm.MetadataUpdate.WebAssemblyHotReload.Initialize;
                }
                async initializeHotReload() {
                    const webAppBasePath = this._unoConfig.environmentVariables["UNO_BOOTSTRAP_WEBAPP_BASE_PATH"];
                    let browserToolsVariable = this._context.config.environmentVariables['ASPNETCORE-BROWSER-TOOLS']
                        || this._context.config.environmentVariables['__ASPNETCORE_BROWSER_TOOLS'];
                    (function (Blazor) {
                        Blazor._internal = {
                            initialize: function () {
                                if (!HotReloadSupport._initializeMethod()) {
                                    console.warn("The application was compiled with the IL linker enabled, hot reload is disabled. See https://aka.platform.uno/wasm-il-linker for more details.");
                                }
                            },
                            applyExisting: async function () {
                                if (browserToolsVariable == "true") {
                                    try {
                                        var m = await import(`/_framework/blazor-hotreload.js`);
                                        await m.receiveHotReloadAsync();
                                    }
                                    catch (e) {
                                        console.error(`Failed to apply initial metadata delta ${e}`);
                                    }
                                }
                            },
                            getApplyUpdateCapabilities: function () {
                                Blazor._internal.initialize();
                                return HotReloadSupport._getApplyUpdateCapabilitiesMethod();
                            },
                            applyHotReload: function (moduleId, metadataDelta, ilDelta, pdbDelta, updatedTypes) {
                                Blazor._internal.initialize();
                                return HotReloadSupport._applyHotReloadDeltaMethod(moduleId, metadataDelta, ilDelta, pdbDelta || "", updatedTypes || []);
                            }
                        };
                    })(window.Blazor || (window.Blazor = {}));
                    window.Blazor._internal.initialize();
                    await window.Blazor._internal.applyExisting();
                }
            }
            Bootstrap.HotReloadSupport = HotReloadSupport;
        })(Bootstrap = WebAssembly.Bootstrap || (WebAssembly.Bootstrap = {}));
    })(WebAssembly = Uno.WebAssembly || (Uno.WebAssembly = {}));
})(Uno || (Uno = {}));
var Uno;
(function (Uno) {
    var WebAssembly;
    (function (WebAssembly) {
        var Bootstrap;
        (function (Bootstrap) {
            class Bootstrapper {
                constructor(unoConfig) {
                    this._unoConfig = unoConfig;
                    this._webAppBasePath = this._unoConfig.environmentVariables["UNO_BOOTSTRAP_WEBAPP_BASE_PATH"];
                    this._appBase = this._unoConfig.environmentVariables["UNO_BOOTSTRAP_APP_BASE"];
                    this.disableDotnet6Compatibility = false;
                    this.onConfigLoaded = config => this.configLoaded(config);
                    this.onDotnetReady = () => this.RuntimeReady();
                    globalThis.Uno = Uno;
                }
                static invokeJS(value) {
                    return eval(value);
                }
                static async bootstrap() {
                    try {
                        Bootstrapper.ENVIRONMENT_IS_WEB = typeof window === 'object';
                        Bootstrapper.ENVIRONMENT_IS_WORKER = typeof globalThis.importScripts === 'function';
                        Bootstrapper.ENVIRONMENT_IS_NODE = typeof globalThis.process === 'object' && typeof globalThis.process.versions === 'object' && typeof globalThis.process.versions.node === 'string';
                        Bootstrapper.ENVIRONMENT_IS_SHELL = !Bootstrapper.ENVIRONMENT_IS_WEB && !Bootstrapper.ENVIRONMENT_IS_NODE && !Bootstrapper.ENVIRONMENT_IS_WORKER;
                        let bootstrapper = null;
                        let DOMContentLoaded = false;
                        if (typeof window === 'object') {
                            globalThis.document.addEventListener("DOMContentLoaded", () => {
                                DOMContentLoaded = true;
                                bootstrapper?.preInit();
                            });
                        }
                        var config = await import('./uno-config.js');
                        if (document && document.uno_app_base_override) {
                            config.config.uno_app_base = document.uno_app_base_override;
                        }
                        bootstrapper = new Bootstrapper(config.config);
                        if (DOMContentLoaded) {
                            bootstrapper.preInit();
                        }
                        var m = await import(`../_framework/dotnet.js`);
                        m.dotnet
                            .withModuleConfig({
                            preRun: () => bootstrapper.wasmRuntimePreRun(),
                        })
                            .withRuntimeOptions(config.config.uno_runtime_options)
                            .withConfig({ loadAllSatelliteResources: config.config.uno_load_all_satellite_resources });
                        const dotnetRuntime = await m.default((context) => {
                            bootstrapper.configure(context);
                            return bootstrapper.asDotnetConfig();
                        });
                        bootstrapper._context.Module = dotnetRuntime.Module;
                        globalThis.Module = bootstrapper._context.Module;
                        bootstrapper._runMain = dotnetRuntime.runMain;
                        bootstrapper.setupExports(dotnetRuntime);
                    }
                    catch (e) {
                        throw `.NET runtime initialization failed (${e})`;
                    }
                }
                setupExports(dotnetRuntime) {
                    this._getAssemblyExports = dotnetRuntime.getAssemblyExports;
                    this._context.Module.getAssemblyExports = dotnetRuntime.getAssemblyExports;
                    globalThis.Module.getAssemblyExports = dotnetRuntime.getAssemblyExports;
                }
                asDotnetConfig() {
                    return {
                        disableDotnet6Compatibility: this.disableDotnet6Compatibility,
                        configSrc: this.configSrc,
                        baseUrl: this._unoConfig.uno_app_base,
                        mainScriptPath: "_framework/dotnet.js",
                        onConfigLoaded: this.onConfigLoaded,
                        onDotnetReady: this.onDotnetReady,
                        onAbort: this.onAbort,
                        exports: ["IDBFS", "FS"].concat(this._unoConfig.emcc_exported_runtime_methods),
                        onDownloadResourceProgress: (resourcesLoaded, totalResources) => this.reportDownloadResourceProgress(resourcesLoaded, totalResources),
                    };
                }
                configure(context) {
                    this._context = context;
                    globalThis.BINDING = this._context.BINDING;
                }
                async setupHotReload() {
                    if (Bootstrapper.ENVIRONMENT_IS_WEB && this.hasDebuggingEnabled()) {
                        await Bootstrap.HotReloadSupport.tryInitializeExports(this._getAssemblyExports);
                        this._hotReloadSupport = new Bootstrap.HotReloadSupport(this._context, this._unoConfig);
                    }
                }
                setupRequire() {
                    const anyModule = this._context.Module;
                    anyModule.imports = anyModule.imports || {};
                    anyModule.imports.require = globalThis.require;
                }
                wasmRuntimePreRun() {
                    if (Bootstrap.LogProfilerSupport.initializeLogProfiler(this._unoConfig)) {
                        this._logProfiler = new Bootstrap.LogProfilerSupport(this._context, this._unoConfig);
                    }
                }
                RuntimeReady() {
                    this.configureGlobal();
                    this.setupRequire();
                    this.initializeRequire();
                    this._aotProfiler = Bootstrap.AotProfilerSupport.initialize(this._context, this._unoConfig);
                }
                configureGlobal() {
                    var thatGlobal = globalThis;
                    thatGlobal.config = this._unoConfig;
                    thatGlobal.Module = this._context.Module;
                    let anyModule = this._context.Module;
                    thatGlobal.lengthBytesUTF8 = anyModule.lengthBytesUTF8;
                    thatGlobal.UTF8ToString = anyModule.UTF8ToString;
                    thatGlobal.UTF8ArrayToString = anyModule.UTF8ArrayToString;
                    thatGlobal.IDBFS = anyModule.IDBFS;
                    thatGlobal.FS = anyModule.FS;
                    if (this._unoConfig.emcc_exported_runtime_methods) {
                        this._unoConfig.emcc_exported_runtime_methods.forEach((name) => {
                            thatGlobal[name] = anyModule[name];
                        });
                    }
                }
                configLoaded(config) {
                    this._monoConfig = config;
                    if (this._unoConfig.environmentVariables) {
                        for (let key in this._unoConfig.environmentVariables) {
                            if (this._unoConfig.environmentVariables.hasOwnProperty(key)) {
                                if (this._monoConfig.debugLevel)
                                    console.log(`Setting ${key}=${this._unoConfig.environmentVariables[key]}`);
                                this._monoConfig.environmentVariables[key] = this._unoConfig.environmentVariables[key];
                            }
                        }
                    }
                    if (this._unoConfig.generate_aot_profile) {
                        this._monoConfig.aotProfilerOptions = {
                            writeAt: "Uno.AotProfilerSupport::StopProfile",
                            sendTo: "System.Runtime.InteropServices.JavaScript.JavaScriptExports::DumpAotProfileData"
                        };
                    }
                    var logProfilerConfig = this._unoConfig.environmentVariables["UNO_BOOTSTRAP_LOG_PROFILER_OPTIONS"];
                    if (logProfilerConfig) {
                        this._monoConfig.logProfilerOptions = {
                            configuration: logProfilerConfig
                        };
                    }
                }
                preInit() {
                    this.body = document.getElementById("uno-body");
                    this.initProgress();
                }
                async mainInit() {
                    try {
                        this.attachDebuggerHotkey();
                        await this.setupHotReload();
                        if (this._hotReloadSupport) {
                            await this._hotReloadSupport.initializeHotReload();
                        }
                        this._runMain(this._unoConfig.uno_main, []);
                        this.initializePWA();
                    }
                    catch (e) {
                        console.error(e);
                    }
                    this.cleanupInit();
                }
                cleanupInit() {
                    if (this.progress) {
                        this.progress.value = this.progress.max;
                    }
                    if (!this.bodyObserver && this.loader && this.loader.parentNode) {
                        this.loader.parentNode.removeChild(this.loader);
                    }
                }
                reportDownloadResourceProgress(resourcesLoaded, totalResources) {
                    this.progress.max = totalResources;
                    this.progress.value = resourcesLoaded;
                }
                initProgress() {
                    this.loader = this.body.querySelector(".uno-loader");
                    if (this.loader) {
                        this.loader.id = "loading";
                        const progress = this.loader.querySelector("progress");
                        progress.value = "";
                        this.progress = progress;
                        this.bodyObserver = new MutationObserver(() => {
                            if (!this.loader.classList.contains("uno-keep-loader")) {
                                this.loader.remove();
                            }
                            if (this.bodyObserver) {
                                this.bodyObserver.disconnect();
                                this.bodyObserver = null;
                            }
                        });
                        this.bodyObserver.observe(this.body, { childList: true });
                        this.loader.classList.add("uno-persistent-loader");
                    }
                    const configLoader = () => {
                        if (manifest && manifest.lightThemeBackgroundColor) {
                            this.loader.style.setProperty("--light-theme-bg-color", manifest.lightThemeBackgroundColor);
                        }
                        if (manifest && manifest.darkThemeBackgroundColor) {
                            this.loader.style.setProperty("--dark-theme-bg-color", manifest.darkThemeBackgroundColor);
                        }
                        if (manifest && manifest.splashScreenColor && manifest.splashScreenColor != "transparent") {
                            this.loader.style.setProperty("background-color", manifest.splashScreenColor);
                        }
                        if (manifest && manifest.accentColor) {
                            this.loader.style.setProperty("--accent-color", manifest.accentColor);
                        }
                        if (manifest && manifest.lightThemeAccentColor) {
                            this.loader.style.setProperty("--accent-color", manifest.lightThemeAccentColor);
                        }
                        if (manifest && manifest.darkThemeAccentColor) {
                            this.loader.style.setProperty("--dark-theme-accent-color", manifest.darkThemeAccentColor);
                        }
                        const img = this.loader.querySelector("img");
                        if (img) {
                            if (manifest && manifest.splashScreenImage) {
                                if (!manifest.splashScreenImage.match(/^(http(s)?:\/\/.)/g)) {
                                    manifest.splashScreenImage = `${this._unoConfig.uno_app_base}/${manifest.splashScreenImage}`;
                                }
                                img.setAttribute("src", manifest.splashScreenImage);
                            }
                            else {
                                img.setAttribute("src", "https://uno-assets.platform.uno/logos/uno-splashscreen-light.png");
                            }
                        }
                    };
                    let manifest = window["UnoAppManifest"];
                    if (manifest) {
                        configLoader();
                    }
                    else {
                        for (var i = 0; i < this._unoConfig.uno_dependencies.length; i++) {
                            if (this._unoConfig.uno_dependencies[i].endsWith('AppManifest')
                                || this._unoConfig.uno_dependencies[i].endsWith('AppManifest.js')) {
                                require([this._unoConfig.uno_dependencies[i]], function () {
                                    manifest = window["UnoAppManifest"];
                                    configLoader();
                                });
                                break;
                            }
                        }
                    }
                }
                isElectron() {
                    return navigator.userAgent.indexOf('Electron') !== -1;
                }
                initializeRequire() {
                    this._isUsingCommonJS = this._unoConfig.uno_shell_mode !== "BrowserEmbedded" && (Bootstrapper.ENVIRONMENT_IS_NODE || this.isElectron());
                    if (this._unoConfig.uno_enable_tracing)
                        console.log("Done loading the BCL");
                    if (this._unoConfig.uno_dependencies && this._unoConfig.uno_dependencies.length !== 0) {
                        let pending = this._unoConfig.uno_dependencies.length;
                        const checkDone = (dependency) => {
                            --pending;
                            if (this._unoConfig.uno_enable_tracing)
                                console.log(`Loaded dependency (${dependency}) - remains ${pending} other(s).`);
                            if (pending === 0) {
                                this.mainInit();
                            }
                        };
                        this._unoConfig.uno_dependencies.forEach((dependency) => {
                            if (this._unoConfig.uno_enable_tracing)
                                console.log(`Loading dependency (${dependency})`);
                            let processDependency = (instance) => {
                                if (instance && instance.HEAP8 !== undefined) {
                                    const existingInitializer = instance.onRuntimeInitialized;
                                    if (this._unoConfig.uno_enable_tracing)
                                        console.log(`Waiting for dependency (${dependency}) initialization`);
                                    instance.onRuntimeInitialized = () => {
                                        checkDone(dependency);
                                        if (existingInitializer)
                                            existingInitializer();
                                    };
                                }
                                else {
                                    checkDone(dependency);
                                }
                            };
                            this.require([dependency], processDependency);
                        });
                    }
                    else {
                        setTimeout(() => {
                            this.mainInit();
                        }, 0);
                    }
                }
                require(modules, callback) {
                    if (this._isUsingCommonJS) {
                        modules.forEach(id => {
                            setTimeout(() => {
                                const d = require('./' + id);
                                callback(d);
                            }, 0);
                        });
                    }
                    else {
                        if (typeof require === undefined) {
                            throw `Require.js has not been loaded yet. If you have customized your index.html file, make sure that <script src="./require.js"></script> does not contain the defer directive.`;
                        }
                        require(modules, callback);
                    }
                }
                hasDebuggingEnabled() {
                    return this._unoConfig.uno_debugging_enabled && this._currentBrowserIsChrome;
                }
                attachDebuggerHotkey() {
                    if (Bootstrapper.ENVIRONMENT_IS_WEB) {
                        let loadAssemblyUrls = this._monoConfig.assets.map(a => a.name);
                        this._currentBrowserIsChrome = window.chrome
                            && navigator.userAgent.indexOf("Edge") < 0;
                        this._hasReferencedPdbs = loadAssemblyUrls
                            .some(function (url) { return /\.pdb$/.test(url); });
                        const altKeyName = navigator.platform.match(/^Mac/i) ? "Cmd" : "Alt";
                        if (this.hasDebuggingEnabled()) {
                            console.info(`Debugging hotkey: Shift+${altKeyName}+D (when application has focus)`);
                        }
                        document.addEventListener("keydown", (evt) => {
                            if (evt.shiftKey && (evt.metaKey || evt.altKey) && evt.code === "KeyD") {
                                if (!this._hasReferencedPdbs) {
                                    console.error("Cannot start debugging, because the application was not compiled with debugging enabled.");
                                }
                                else if (!this._currentBrowserIsChrome) {
                                    console.error("Currently, only Chrome is supported for debugging.");
                                }
                                else {
                                    this.launchDebugger();
                                }
                            }
                        });
                    }
                }
                launchDebugger() {
                    const link = document.createElement("a");
                    link.href = `_framework/debug?url=${encodeURIComponent(location.href)}`;
                    link.target = "_blank";
                    link.rel = "noopener noreferrer";
                    link.click();
                }
                initializePWA() {
                    if (typeof window === 'object') {
                        if (this._unoConfig.enable_pwa && 'serviceWorker' in navigator) {
                            if (navigator.serviceWorker.controller) {
                                console.debug("Active service worker found, skipping register");
                            }
                            else {
                                const _webAppBasePath = this._unoConfig.environmentVariables["UNO_BOOTSTRAP_WEBAPP_BASE_PATH"];
                                console.debug(`Registering service worker for ${_webAppBasePath}`);
                                navigator.serviceWorker
                                    .register(`${_webAppBasePath}service-worker.js`, {
                                    scope: _webAppBasePath,
                                    type: 'module'
                                })
                                    .then(function () {
                                    console.debug('Service Worker Registered');
                                });
                            }
                        }
                    }
                }
            }
            Bootstrap.Bootstrapper = Bootstrapper;
        })(Bootstrap = WebAssembly.Bootstrap || (WebAssembly.Bootstrap = {}));
    })(WebAssembly = Uno.WebAssembly || (Uno.WebAssembly = {}));
})(Uno || (Uno = {}));
Uno.WebAssembly.Bootstrap.Bootstrapper.bootstrap();
var Uno;
(function (Uno) {
    var WebAssembly;
    (function (WebAssembly) {
        var Bootstrap;
        (function (Bootstrap) {
            class LogProfilerSupport {
                constructor(context, unoConfig) {
                    this._context = context;
                    this._unoConfig = unoConfig;
                }
                static initializeLogProfiler(unoConfig) {
                    const options = unoConfig.environmentVariables["UNO_BOOTSTRAP_LOG_PROFILER_OPTIONS"];
                    if (options) {
                        this._logProfilerEnabled = true;
                        return true;
                    }
                    return false;
                }
                postInitializeLogProfiler() {
                    if (LogProfilerSupport._logProfilerEnabled) {
                        this.attachHotKey();
                        setInterval(() => {
                            this.ensureInitializeProfilerMethods();
                            this._flushLogProfile();
                        }, 5000);
                    }
                }
                attachHotKey() {
                    if (Bootstrap.Bootstrapper.ENVIRONMENT_IS_WEB) {
                        if (LogProfilerSupport._logProfilerEnabled) {
                            const altKeyName = navigator.platform.match(/^Mac/i) ? "Cmd" : "Alt";
                            console.info(`Log Profiler save hotkey: Shift+${altKeyName}+P (when application has focus), or Run this.saveLogProfile() from the browser debug console.`);
                            document.addEventListener("keydown", (evt) => {
                                if (evt.shiftKey && (evt.metaKey || evt.altKey) && evt.code === "KeyP") {
                                    this.saveLogProfile();
                                }
                            });
                            console.info(`Log Profiler take heap shot hotkey: Shift+${altKeyName}+H (when application has focus), or Run this.takeHeapShot() from the browser debug console.`);
                            document.addEventListener("keydown", (evt) => {
                                if (evt.shiftKey && (evt.metaKey || evt.altKey) && evt.code === "KeyH") {
                                    this.takeHeapShot();
                                }
                            });
                        }
                    }
                }
                ensureInitializeProfilerMethods() {
                    if (LogProfilerSupport._logProfilerEnabled && !this._flushLogProfile) {
                        this._flushLogProfile = this._context.BINDING.bind_static_method("[Uno.Wasm.LogProfiler] Uno.LogProfilerSupport:FlushProfile");
                        this._getLogProfilerProfileOutputFile = this._context.BINDING.bind_static_method("[Uno.Wasm.LogProfiler] Uno.LogProfilerSupport:GetProfilerProfileOutputFile");
                        this.triggerHeapShotLogProfiler = this._context.BINDING.bind_static_method("[Uno.Wasm.LogProfiler] Uno.LogProfilerSupport:TriggerHeapShot");
                    }
                }
                takeHeapShot() {
                    this.ensureInitializeProfilerMethods();
                    this.triggerHeapShotLogProfiler();
                }
                readProfileFile() {
                    this.ensureInitializeProfilerMethods();
                    this._flushLogProfile();
                    var profileFilePath = this._getLogProfilerProfileOutputFile();
                    var stat = FS.stat(profileFilePath);
                    if (stat && stat.size > 0) {
                        return FS.readFile(profileFilePath);
                    }
                    else {
                        console.debug(`Unable to fetch the profile file ${profileFilePath} as it is empty`);
                        return null;
                    }
                }
                saveLogProfile() {
                    this.ensureInitializeProfilerMethods();
                    var profileArray = this.readProfileFile();
                    var a = window.document.createElement('a');
                    a.href = window.URL.createObjectURL(new Blob([profileArray]));
                    a.download = "profile.mlpd";
                    document.body.appendChild(a);
                    a.click();
                    document.body.removeChild(a);
                }
            }
            Bootstrap.LogProfilerSupport = LogProfilerSupport;
        })(Bootstrap = WebAssembly.Bootstrap || (WebAssembly.Bootstrap = {}));
    })(WebAssembly = Uno.WebAssembly || (Uno.WebAssembly = {}));
})(Uno || (Uno = {}));
