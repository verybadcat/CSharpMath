var Microsoft;
(function (Microsoft) {
    var UI;
    (function (UI) {
        var Xaml;
        (function (Xaml) {
            var Controls;
            (function (Controls) {
                class WebView {
                    static buildImports(assembly) {
                        if (!WebView.unoExports) {
                            window.Module.getAssemblyExports(assembly)
                                .then((e) => {
                                WebView.unoExports = e.Microsoft.UI.Xaml.Controls.NativeWebView;
                            });
                        }
                    }
                    static reload(htmlId) {
                        document.getElementById(htmlId).contentWindow.location.reload();
                    }
                    static stop(htmlId) {
                        document.getElementById(htmlId).contentWindow.stop();
                    }
                    static goBack(htmlId) {
                        document.getElementById(htmlId).contentWindow.history.back();
                    }
                    static goForward(htmlId) {
                        document.getElementById(htmlId).contentWindow.history.forward();
                    }
                    static executeScript(htmlId, script) {
                        return document.getElementById(htmlId).contentWindow.eval(script);
                    }
                    static getDocumentTitle(htmlId) {
                        return document.getElementById(htmlId).contentDocument.title;
                    }
                    static setAttribute(htmlId, name, value) {
                        document.getElementById(htmlId).setAttribute(name, value);
                    }
                    static getAttribute(htmlId, name) {
                        return document.getElementById(htmlId).getAttribute(name);
                    }
                    static initializeStyling(htmlId) {
                        const iframe = document.getElementById(htmlId);
                        iframe.style.backgroundColor = "transparent";
                        iframe.style.border = "0";
                    }
                    static getPackageBase() {
                        if (WebView.cachedPackageBase !== null) {
                            return WebView.cachedPackageBase;
                        }
                        const pathsToCheck = [
                            ...Array.from(document.getElementsByTagName('script')).map(s => s.src),
                        ];
                        for (const path of pathsToCheck) {
                            const m = path === null || path === void 0 ? void 0 : path.match(/\/package_[^\/]+/);
                            if (m) {
                                const packageBase = "./" + m[0].substring(1);
                                WebView.cachedPackageBase = packageBase;
                                return packageBase;
                            }
                        }
                        WebView.cachedPackageBase = ".";
                        return ".";
                    }
                    static setupEvents(htmlId) {
                        const iframe = document.getElementById(htmlId);
                        iframe.addEventListener('load', WebView.onLoad);
                    }
                    static cleanupEvents(htmlId) {
                        const iframe = document.getElementById(htmlId);
                        iframe.removeEventListener('load', WebView.onLoad);
                    }
                    static onLoad(event) {
                        const iframe = event.currentTarget;
                        const absoluteUrl = iframe.contentWindow.location.href;
                        WebView.unoExports.DispatchLoadEvent(iframe.id, absoluteUrl);
                    }
                }
                WebView.cachedPackageBase = null;
                Controls.WebView = WebView;
            })(Controls = Xaml.Controls || (Xaml.Controls = {}));
        })(Xaml = UI.Xaml || (UI.Xaml = {}));
    })(UI = Microsoft.UI || (Microsoft.UI = {}));
})(Microsoft || (Microsoft = {}));
var Uno;
(function (Uno) {
    var UI;
    (function (UI) {
        var Runtime;
        (function (Runtime) {
            var Skia;
            (function (Skia) {
                class Accessibility {
                    constructor() {
                    }
                    static async buildImports() {
                        let anyModule = window.Module;
                        if (anyModule.getAssemblyExports !== undefined) {
                            const browserExports = await anyModule.getAssemblyExports("Uno.UI.Runtime.Skia.WebAssembly.Browser");
                            this.managedEnableAccessibility = browserExports.Uno.UI.Runtime.Skia.WebAssemblyAccessibility.EnableAccessibility;
                            this.managedOnScroll = browserExports.Uno.UI.Runtime.Skia.WebAssemblyAccessibility.OnScroll;
                        }
                    }
                    static createLiveElement(kind) {
                        const element = document.createElement("div");
                        element.classList.add("uno-aria-live");
                        element.setAttribute("aria-live", kind);
                        return element;
                    }
                    static async setup() {
                        await this.buildImports();
                        this.containerElement = document.getElementById("uno-body");
                        this.politeElement = Accessibility.createLiveElement("polite");
                        this.assertiveElement = Accessibility.createLiveElement("assertive");
                        this.containerElement.appendChild(this.politeElement);
                        this.containerElement.appendChild(this.assertiveElement);
                        this.enableAccessibilityButton = document.createElement("div");
                        this.enableAccessibilityButton.id = "uno-enable-accessibility";
                        this.enableAccessibilityButton.setAttribute("aria-live", "polite");
                        this.enableAccessibilityButton.setAttribute("role", "button");
                        this.enableAccessibilityButton.setAttribute("tabindex", "0");
                        this.enableAccessibilityButton.setAttribute("aria-label", "Enable accessibility");
                        this.enableAccessibilityButton.addEventListener("click", this.onEnableAccessibilityButtonClicked.bind(this));
                        this.containerElement.appendChild(this.enableAccessibilityButton);
                        this.semanticsRoot = document.createElement("div");
                        this.semanticsRoot.id = "uno-semantics-root";
                        this.containerElement.appendChild(this.semanticsRoot);
                    }
                    static createSemanticElement(x, y, width, height, handle, isFocusable) {
                        let element = document.createElement("div");
                        element.style.position = "absolute";
                        element.addEventListener('wheel', (e) => {
                            // When scrolling with wheel, we want to prevent scroll events.
                            e.preventDefault();
                        }, { passive: false });
                        element.addEventListener('scroll', (e) => {
                            let element = e.target;
                            this.managedOnScroll(handle, element.scrollLeft, element.scrollTop);
                        });
                        Accessibility.updateElementFocusability(element, isFocusable);
                        element.style.left = `${x}px`;
                        element.style.top = `${y}px`;
                        element.style.width = `${width}px`;
                        element.style.height = `${height}px`;
                        //element.style.boxShadow = "inset 0px 0px 5px 0px red"; // FOR DEBUGGING ONLY.
                        element.id = `uno-semantics-${handle}`;
                        return element;
                    }
                    static updateElementFocusability(element, isFocusable) {
                        if (isFocusable) {
                            element.tabIndex = 0;
                            element.style.pointerEvents = "all";
                            element.style.touchAction = "";
                        }
                        else {
                            element.removeAttribute("tabIndex");
                            element.style.pointerEvents = "none";
                            element.style.touchAction = "none";
                        }
                    }
                    static getSemanticElementByHandle(handle) {
                        return document.getElementById(`uno-semantics-${handle}`);
                    }
                    static announcePolite(text) {
                        Accessibility.announce(Accessibility.politeElement, text);
                    }
                    static announceAssertive(text) {
                        Accessibility.announce(Accessibility.assertiveElement, text);
                    }
                    static announce(ariaLiveElement, text) {
                        let child = document.createElement("div");
                        child.innerText = text;
                        ariaLiveElement.appendChild(child);
                        setTimeout(() => ariaLiveElement.removeChild(child), 300);
                    }
                    static onEnableAccessibilityButtonClicked(evt) {
                        this.containerElement.removeChild(this.enableAccessibilityButton);
                        this.managedEnableAccessibility();
                        this.announceAssertive("Accessibility enabled successfully.");
                    }
                    static focusSemanticElement(handle) {
                        const element = Accessibility.getSemanticElementByHandle(handle);
                        if (element) {
                            element.focus();
                        }
                    }
                    static addRootElementToSemanticsRoot(rootHandle, width, height, x, y, isFocusable) {
                        let element = Accessibility.createSemanticElement(x, y, width, height, rootHandle, isFocusable);
                        this.semanticsRoot.appendChild(element);
                    }
                    static addSemanticElement(parentHandle, handle, index, width, height, x, y, role, automationId, isFocusable, ariaChecked, isVisible, horizontallyScrollable, verticallyScrollable, temporary) {
                        const parent = Accessibility.getSemanticElementByHandle(parentHandle);
                        if (!parent) {
                            return false;
                        }
                        let element = Accessibility.createSemanticElement(x, y, width, height, handle, isFocusable);
                        element.setAttribute('ElementType', temporary);
                        if (!isVisible) {
                            element.hidden = true;
                        }
                        if (role) {
                            element.setAttribute("role", role);
                        }
                        if (ariaChecked) {
                            element.setAttribute("aria-checked", ariaChecked);
                        }
                        if (automationId) {
                            element.setAttribute("aria-label", automationId);
                        }
                        if (horizontallyScrollable) {
                            element.style.overflowX = "scroll";
                        }
                        if (verticallyScrollable) {
                            element.style.overflowY = "scroll";
                        }
                        if (index != null && index < parent.childElementCount) {
                            parent.insertBefore(element, parent.children[index]);
                        }
                        else {
                            parent.appendChild(element);
                        }
                        return true;
                    }
                    static removeSemanticElement(parentHandle, childHandle) {
                        const parent = Accessibility.getSemanticElementByHandle(parentHandle);
                        if (parent) {
                            const child = Accessibility.getSemanticElementByHandle(childHandle);
                            parent.removeChild(child);
                        }
                    }
                    static updateIsFocusable(handle, isFocusable) {
                        const element = Accessibility.getSemanticElementByHandle(handle);
                        if (element) {
                            Accessibility.updateElementFocusability(element, isFocusable);
                        }
                    }
                    static updateAriaLabel(handle, automationId) {
                        const element = Accessibility.getSemanticElementByHandle(handle);
                        if (element) {
                            element.setAttribute("aria-label", automationId);
                        }
                    }
                    static updateAriaChecked(handle, ariaChecked) {
                        const element = Accessibility.getSemanticElementByHandle(handle);
                        if (element) {
                            element.setAttribute("aria-checked", ariaChecked);
                        }
                    }
                    static updateNativeScrollOffsets(handle, horizontalOffset, verticalOffset) {
                        const element = Accessibility.getSemanticElementByHandle(handle);
                        if (element) {
                            element.scrollLeft = horizontalOffset;
                            element.scrollTop = verticalOffset;
                        }
                    }
                    static hideSemanticElement(handle) {
                        const element = Accessibility.getSemanticElementByHandle(handle);
                        if (element) {
                            element.hidden = true;
                        }
                    }
                    static updateSemanticElementPositioning(handle, width, height, x, y) {
                        const element = Accessibility.getSemanticElementByHandle(handle);
                        if (element) {
                            element.hidden = false;
                            element.style.left = `${x}px`;
                            element.style.top = `${y}px`;
                            element.style.width = `${width}px`;
                            element.style.height = `${height}px`;
                        }
                    }
                }
                Skia.Accessibility = Accessibility;
            })(Skia = Runtime.Skia || (Runtime.Skia = {}));
        })(Runtime = UI.Runtime || (UI.Runtime = {}));
    })(UI = Uno.UI || (Uno.UI = {}));
})(Uno || (Uno = {}));
var Uno;
(function (Uno) {
    var UI;
    (function (UI) {
        var Runtime;
        (function (Runtime) {
            var Skia;
            (function (Skia) {
                var _a;
                class BrowserInvisibleTextBoxViewExtension {
                    static async initialize() {
                        const module = window.Module;
                        if (BrowserInvisibleTextBoxViewExtension._exports == undefined
                            && module.getAssemblyExports !== undefined) {
                            const browserExports = (await module.getAssemblyExports("Uno.UI.Runtime.Skia.WebAssembly.Browser"));
                            BrowserInvisibleTextBoxViewExtension._exports = browserExports.Uno.UI.Runtime.Skia.BrowserInvisibleTextBoxViewExtension;
                            document.onselectionchange = () => {
                                let input = document.activeElement;
                                if (input instanceof HTMLInputElement) {
                                    BrowserInvisibleTextBoxViewExtension.isInSelectionChange = true;
                                    if (BrowserInvisibleTextBoxViewExtension.waitingAsyncOnSelectionChange) {
                                        BrowserInvisibleTextBoxViewExtension.waitingAsyncOnSelectionChange = false;
                                        input.setSelectionRange(BrowserInvisibleTextBoxViewExtension.nextSelectionStart, BrowserInvisibleTextBoxViewExtension.nextSelectionEnd, BrowserInvisibleTextBoxViewExtension.nextSelectionDirection);
                                    }
                                    else {
                                        if (input.selectionDirection == "backward") {
                                            BrowserInvisibleTextBoxViewExtension._exports.OnSelectionChanged(input.selectionEnd, input.selectionStart - input.selectionEnd);
                                        }
                                        else {
                                            BrowserInvisibleTextBoxViewExtension._exports.OnSelectionChanged(input.selectionStart, input.selectionEnd - input.selectionStart);
                                        }
                                    }
                                    BrowserInvisibleTextBoxViewExtension.isInSelectionChange = false;
                                }
                            };
                        }
                    }
                    static createInput(isPasswordBox, text, acceptsReturn, inputMode, enterKeyHint) {
                        const input = document.createElement(acceptsReturn && !isPasswordBox ? "textarea" : "input");
                        if (isPasswordBox) {
                            input.type = "password";
                            input.autocomplete = "password";
                        }
                        input.id = BrowserInvisibleTextBoxViewExtension.inputElementId;
                        input.spellcheck = false;
                        input.style.whiteSpace = "pre-wrap";
                        input.style.position = "absolute";
                        input.style.padding = "0px";
                        input.style.opacity = "0";
                        input.style.color = "transparent";
                        input.style.background = "transparent";
                        input.style.caretColor = "transparent";
                        input.style.outline = "none";
                        input.style.border = "none";
                        input.style.resize = "none";
                        input.style.textShadow = "none";
                        input.style.overflow = "hidden";
                        input.style.pointerEvents = "none";
                        input.style.zIndex = "99";
                        input.style.top = "0px";
                        input.style.left = "0px";
                        input.value = text;
                        input.setAttribute("inputmode", inputMode);
                        input.setAttribute("enterkeyhint", enterKeyHint);
                        input.oninput = ev => {
                            let input = ev.target;
                            if (input.selectionDirection == "backward") {
                                BrowserInvisibleTextBoxViewExtension._exports.OnInputTextChanged(input.value, input.selectionEnd, input.selectionStart - input.selectionEnd);
                            }
                            else {
                                BrowserInvisibleTextBoxViewExtension._exports.OnInputTextChanged(input.value, input.selectionStart, input.selectionEnd - input.selectionStart);
                            }
                        };
                        input.onpaste = ev => {
                            BrowserInvisibleTextBoxViewExtension._exports.OnNativePaste(ev.clipboardData.getData("text"));
                            ev.preventDefault();
                        };
                        input.onkeydown = ev => {
                            if (ev.ctrlKey || (ev.metaKey && BrowserInvisibleTextBoxViewExtension.isMacOS)) {
                                // Due to browser security considerations, we need to let the clipboard operations be handled natively.
                                // So, we do stopPropagation instead of preventDefault
                                if (ev.key == "c" || ev.key == "C" || ev.key == "v" || ev.key == "V" || ev.key == "x" || ev.key == "X") {
                                    ev.stopPropagation();
                                    return;
                                }
                            }
                            ev.preventDefault();
                        };
                        document.body.appendChild(input);
                        BrowserInvisibleTextBoxViewExtension.inputElement = input;
                    }
                    static setEnterKeyHint(enterKeyHint) {
                        const input = BrowserInvisibleTextBoxViewExtension.inputElement;
                        if (input) {
                            input.setAttribute("enterkeyhint", enterKeyHint);
                        }
                    }
                    static setInputMode(inputMode) {
                        const input = BrowserInvisibleTextBoxViewExtension.inputElement;
                        if (input) {
                            input.setAttribute("inputmode", inputMode);
                        }
                    }
                    static focus(isPassword, text, acceptsReturn, inputMode, enterKeyHint) {
                        var _a;
                        // NOTE: We can get focused as true while we have inputElement.
                        // This happens when TextBox is focused twice with different FocusStates (e.g, Pointer, Programmatic, Keyboard)
                        // For such case, we do call StartEntry twice without any EndEntry in between.
                        // So, cleanup the existing inputElement and create a new one.
                        (_a = BrowserInvisibleTextBoxViewExtension.inputElement) === null || _a === void 0 ? void 0 : _a.remove();
                        this.createInput(isPassword, text, acceptsReturn, inputMode, enterKeyHint);
                        // It's necessary to actually focus the native input, not just make it visible. This is particularly
                        // important to mobile browsers (to open the software keyboard) and for assistive technology to not steal
                        // events and properly recognize password inputs to not read it.
                        BrowserInvisibleTextBoxViewExtension.inputElement.focus();
                    }
                    static blur() {
                        var _a, _b;
                        // reset focus
                        (_a = document.activeElement) === null || _a === void 0 ? void 0 : _a.blur();
                        (_b = BrowserInvisibleTextBoxViewExtension.inputElement) === null || _b === void 0 ? void 0 : _b.remove();
                    }
                    static setText(text) {
                        const input = BrowserInvisibleTextBoxViewExtension.inputElement;
                        if (input != null) {
                            // input could be null beccause we could call setText without focusing first
                            if (input.value != text) {
                                // When setting input.value, the browser will try to set the selection to the end, which isn't what we want.
                                // The browser doesn't raise onselectionchange synchronously though, so we set a flag that we're waiting
                                // for a future selection change that is the result of setting value.
                                // And we set the existing values of selection start and selection end.
                                // On the next onselectionchange event, we will ignore the browser provided selection and use these values.
                                // Also, in case we got a managed selection in between here and the next onselectionchange, we will
                                // use that instead (see updateSelection below).
                                BrowserInvisibleTextBoxViewExtension.waitingAsyncOnSelectionChange = true;
                                BrowserInvisibleTextBoxViewExtension.nextSelectionStart = input.selectionStart;
                                BrowserInvisibleTextBoxViewExtension.nextSelectionEnd = input.selectionEnd;
                                BrowserInvisibleTextBoxViewExtension.nextSelectionDirection = input.selectionDirection;
                                input.value = text;
                            }
                        }
                    }
                    static updateSize(width, height) {
                        const input = BrowserInvisibleTextBoxViewExtension.inputElement;
                        if (input != null) {
                            input.style.width = `${width}`;
                            input.style.height = `${height}`;
                        }
                    }
                    static updatePosition(x, y) {
                        const input = BrowserInvisibleTextBoxViewExtension.inputElement;
                        if (input != null) {
                            input.style.top = `${Math.round(y)}px`;
                            input.style.left = `${Math.round(x)}px`;
                        }
                    }
                    static updateSelection(start, length, direction) {
                        if (!BrowserInvisibleTextBoxViewExtension.isInSelectionChange) {
                            const input = BrowserInvisibleTextBoxViewExtension.inputElement;
                            // See comment in setText.
                            if (BrowserInvisibleTextBoxViewExtension.waitingAsyncOnSelectionChange) {
                                BrowserInvisibleTextBoxViewExtension.nextSelectionStart = start;
                                BrowserInvisibleTextBoxViewExtension.nextSelectionEnd = start + length;
                                BrowserInvisibleTextBoxViewExtension.nextSelectionDirection = direction;
                            }
                            input === null || input === void 0 ? void 0 : input.setSelectionRange(start, start + length, direction);
                        }
                    }
                }
                BrowserInvisibleTextBoxViewExtension.inputElementId = "uno-input";
                BrowserInvisibleTextBoxViewExtension.isMacOS = (_a = navigator === null || navigator === void 0 ? void 0 : navigator.platform.toUpperCase().includes('MAC')) !== null && _a !== void 0 ? _a : false;
                Skia.BrowserInvisibleTextBoxViewExtension = BrowserInvisibleTextBoxViewExtension;
            })(Skia = Runtime.Skia || (Runtime.Skia = {}));
        })(Runtime = UI.Runtime || (UI.Runtime = {}));
    })(UI = Uno.UI || (Uno.UI = {}));
})(Uno || (Uno = {}));
var Uno;
(function (Uno) {
    var UI;
    (function (UI) {
        var Runtime;
        (function (Runtime) {
            var Skia;
            (function (Skia) {
                class BrowserKeyboardInputSource {
                    constructor(managedSource) {
                        this._source = managedSource;
                        this.subscribeKeyboardEvents();
                    }
                    static async initialize(inputSource) {
                        const module = window.Module;
                        if (BrowserKeyboardInputSource._exports == undefined
                            && module.getAssemblyExports !== undefined) {
                            const browserExports = (await module.getAssemblyExports("Uno.UI.Runtime.Skia.WebAssembly.Browser"));
                            BrowserKeyboardInputSource._exports = browserExports.Uno.UI.Runtime.Skia.BrowserKeyboardInputSource;
                        }
                        return new BrowserKeyboardInputSource(inputSource);
                    }
                    subscribeKeyboardEvents() {
                        document.addEventListener("keydown", this.onKeyboardEvent.bind(this));
                        document.addEventListener("keyup", this.onKeyboardEvent.bind(this));
                    }
                    onKeyboardEvent(evt) {
                        let result = BrowserKeyboardInputSource._exports.OnNativeKeyboardEvent(this._source, evt.type == "keydown", evt.ctrlKey, evt.shiftKey, evt.altKey, evt.metaKey, evt.code, evt.key);
                        if (result == Skia.HtmlEventDispatchResult.PreventDefault) {
                            evt.preventDefault();
                        }
                    }
                }
                Skia.BrowserKeyboardInputSource = BrowserKeyboardInputSource;
            })(Skia = Runtime.Skia || (Runtime.Skia = {}));
        })(Runtime = UI.Runtime || (UI.Runtime = {}));
    })(UI = Uno.UI || (Uno.UI = {}));
})(Uno || (Uno = {}));
var Uno;
(function (Uno) {
    var UI;
    (function (UI) {
        var Runtime;
        (function (Runtime) {
            var Skia;
            (function (Skia) {
                class BrowserMediaPlayerExtension {
                    static buildImports() {
                        window.Module.getAssemblyExports("Uno.UI.Runtime.Skia.WebAssembly.Browser")
                            .then((e) => {
                            BrowserMediaPlayerExtension.unoExports = e.Uno.UI.Runtime.Skia.BrowserMediaPlayerExtension;
                        });
                    }
                    static getVideoPlaybackRate(elementId) {
                        const videoElement = document.getElementById(elementId);
                        if (videoElement) {
                            return videoElement.playbackRate;
                        }
                        return 1;
                    }
                    static setVideoPlaybackRate(elementId, playbackRate) {
                        const videoElement = document.getElementById(elementId);
                        if (videoElement) {
                            videoElement.playbackRate = playbackRate;
                            videoElement.buffered;
                        }
                    }
                    static getIsVideoLooped(elementId) {
                        const videoElement = document.getElementById(elementId);
                        if (videoElement) {
                            return videoElement.loop;
                        }
                        return false;
                    }
                    static setIsVideoLooped(elementId, isLooped) {
                        const videoElement = document.getElementById(elementId);
                        if (videoElement) {
                            videoElement.loop = isLooped;
                        }
                    }
                    static getDuration(elementId) {
                        const videoElement = document.getElementById(elementId);
                        if (videoElement) {
                            return videoElement.duration;
                        }
                        return 0;
                    }
                    static setSource(elementId, uri) {
                        const videoElement = document.getElementById(elementId);
                        if (videoElement) {
                            videoElement.src = uri;
                            videoElement.load();
                        }
                    }
                    static play(elementId) {
                        const videoElement = document.getElementById(elementId);
                        if (videoElement) {
                            videoElement.play();
                        }
                    }
                    static pause(elementId) {
                        const videoElement = document.getElementById(elementId);
                        if (videoElement) {
                            videoElement.pause();
                        }
                    }
                    static stop(elementId) {
                        const videoElement = document.getElementById(elementId);
                        if (videoElement) {
                            videoElement.load(); // this is not a typo, loading an already-loaded video resets (i.e. stops) it.
                        }
                    }
                    static getPosition(elementId) {
                        const videoElement = document.getElementById(elementId);
                        if (videoElement) {
                            return videoElement.currentTime;
                        }
                        return 0;
                    }
                    static setPosition(elementId, position) {
                        const videoElement = document.getElementById(elementId);
                        if (videoElement) {
                            videoElement.currentTime = position;
                        }
                    }
                    static setMuted(elementId, muted) {
                        const videoElement = document.getElementById(elementId);
                        if (videoElement) {
                            videoElement.muted = muted;
                        }
                    }
                    static setVolume(elementId, volume) {
                        const videoElement = document.getElementById(elementId);
                        if (videoElement) {
                            videoElement.volume = volume;
                        }
                    }
                    static setupEvents(elementId) {
                        const videoElement = document.getElementById(elementId);
                        if (videoElement) {
                            videoElement.onloadedmetadata = e => BrowserMediaPlayerExtension.unoExports.OnLoadedMetadata(elementId, videoElement.videoWidth !== 0);
                            videoElement.onstalled = e => BrowserMediaPlayerExtension.unoExports.OnStalled(elementId);
                            videoElement.onratechange = e => BrowserMediaPlayerExtension.unoExports.OnRateChange(elementId);
                            videoElement.ondurationchange = e => BrowserMediaPlayerExtension.unoExports.OnDurationChange(elementId);
                            videoElement.onended = e => BrowserMediaPlayerExtension.unoExports.OnEnded(elementId);
                            videoElement.onerror = e => BrowserMediaPlayerExtension.unoExports.OnError(elementId);
                            videoElement.onpause = e => BrowserMediaPlayerExtension.unoExports.OnPause(elementId);
                            videoElement.onplaying = e => BrowserMediaPlayerExtension.unoExports.OnPlaying(elementId);
                            videoElement.onseeked = e => BrowserMediaPlayerExtension.unoExports.OnSeeked(elementId);
                            videoElement.onvolumechange = e => BrowserMediaPlayerExtension.unoExports.OnVolumeChange(elementId);
                            // videoElement.ontimeupdate = e => BrowserMediaPlayerExtension.unoExports.OnTimeUpdate(elementId);
                        }
                    }
                }
                Skia.BrowserMediaPlayerExtension = BrowserMediaPlayerExtension;
            })(Skia = Runtime.Skia || (Runtime.Skia = {}));
        })(Runtime = UI.Runtime || (UI.Runtime = {}));
    })(UI = Uno.UI || (Uno.UI = {}));
})(Uno || (Uno = {}));
var Uno;
(function (Uno) {
    var UI;
    (function (UI) {
        var Runtime;
        (function (Runtime) {
            var Skia;
            (function (Skia) {
                class BrowserMediaPlayerPresenterExtension {
                    static buildImports() {
                        window.Module.getAssemblyExports("Uno.UI.Runtime.Skia.WebAssembly.Browser")
                            .then((e) => {
                            BrowserMediaPlayerPresenterExtension.unoExports = e.Uno.UI.Runtime.Skia.BrowserMediaPlayerPresenterExtension;
                        });
                    }
                    static getVideoNaturalHeight(elementId) {
                        const videoElement = document.getElementById(elementId);
                        if (videoElement) {
                            return videoElement.videoHeight;
                        }
                        return 0;
                    }
                    static getVideoNaturalWidth(elementId) {
                        const videoElement = document.getElementById(elementId);
                        if (videoElement) {
                            return videoElement.videoWidth;
                        }
                        return 0;
                    }
                    static requestFullscreen(elementId) {
                        const videoElement = document.getElementById(elementId);
                        if (videoElement) {
                            videoElement.requestFullscreen();
                        }
                    }
                    static exitFullscreen(elementId) {
                        const videoElement = document.getElementById(elementId);
                        if (videoElement && document.fullscreenElement === videoElement) {
                            document.exitFullscreen();
                        }
                    }
                    static requestPictureInPicture(elementId) {
                        const videoElement = document.getElementById(elementId);
                        if (videoElement) {
                            // The cast is here because tsc complains about requestPictureInPicture not being present in HTMLVideoElement
                            videoElement.requestPictureInPicture();
                        }
                    }
                    static exitPictureInPicture(elementId) {
                        const videoElement = document.getElementById(elementId);
                        if (videoElement && document.fullscreenElement === videoElement) {
                            // The cast is here because tsc complains about exitPictureInPicture not being present in Document
                            document.exitPictureInPicture();
                        }
                    }
                    static updateStretch(elementId, stretch) {
                        const videoElement = document.getElementById(elementId);
                        if (videoElement) {
                            switch (stretch) {
                                case "None":
                                    videoElement.style.objectFit = "none";
                                    break;
                                case "Fill":
                                    videoElement.style.objectFit = "fill";
                                    break;
                                case "Uniform":
                                    videoElement.style.objectFit = "contain";
                                    break;
                                case "UniformToFill":
                                    videoElement.style.objectFit = "cover";
                                    break;
                            }
                        }
                    }
                    static setupEvents(elementId) {
                        const videoElement = document.getElementById(elementId);
                        if (videoElement) {
                            videoElement.onfullscreenchange = e => {
                                if (!document.fullscreenElement) {
                                    BrowserMediaPlayerPresenterExtension.unoExports.OnExitFullscreen(elementId);
                                }
                            };
                        }
                    }
                }
                Skia.BrowserMediaPlayerPresenterExtension = BrowserMediaPlayerPresenterExtension;
            })(Skia = Runtime.Skia || (Runtime.Skia = {}));
        })(Runtime = UI.Runtime || (UI.Runtime = {}));
    })(UI = Uno.UI || (Uno.UI = {}));
})(Uno || (Uno = {}));
var Uno;
(function (Uno) {
    var UI;
    (function (UI) {
        var NativeElementHosting;
        (function (NativeElementHosting) {
            class BrowserHtmlElement {
                static async initialize() {
                    let anyModule = window.Module;
                    if (anyModule.getAssemblyExports !== undefined) {
                        const browserExports = await anyModule.getAssemblyExports("Uno.UI");
                        BrowserHtmlElement.dispatchEventNativeElementMethod = browserExports.Uno.UI.NativeElementHosting.BrowserHtmlElement.DispatchEventNativeElementMethod;
                    }
                    else {
                        throw `BrowserHtmlElement: Unable to find dotnet exports`;
                    }
                }
                static setSvgClipPathForNativeElementHost(path) {
                    if (!document.getElementById("unoNativeElementHostClipPath")) {
                        const svgContainer = document.createElementNS("http://www.w3.org/2000/svg", "svg");
                        svgContainer.setAttribute("width", "0");
                        svgContainer.setAttribute("height", "0");
                        const defs = document.createElementNS("http://www.w3.org/2000/svg", "defs");
                        const clipPath = document.createElementNS("http://www.w3.org/2000/svg", "clipPath");
                        clipPath.setAttribute("id", "unoNativeElementHostClipPath");
                        this.clipPath = document.createElementNS("http://www.w3.org/2000/svg", "path");
                        clipPath.appendChild(this.clipPath);
                        defs.appendChild(clipPath);
                        svgContainer.appendChild(defs);
                        document.body.appendChild(svgContainer);
                        clipPath.appendChild(this.clipPath);
                    }
                    this.clipPath.setAttributeNS(null, "d", path);
                }
                static getNativeElementHost() {
                    let nativeElementHost = document.getElementById("uno-native-element-host");
                    if (!nativeElementHost) {
                        nativeElementHost = document.createElement("div");
                        nativeElementHost.id = "uno-native-element-host";
                        nativeElementHost.style.position = "absolute";
                        nativeElementHost.style.height = "100%";
                        nativeElementHost.style.width = "100%";
                        nativeElementHost.style.overflow = "hidden";
                        nativeElementHost.style.clipPath = "url(#unoNativeElementHostClipPath)";
                        let unoBody = document.getElementById("uno-body");
                        unoBody.appendChild(nativeElementHost);
                    }
                    return nativeElementHost;
                }
                // We have a store to put elements in before they are loaded/unloaded as native elements.
                // We have to put these elements somewhere we can access, so we do so in a hidden div.
                // Only elements in the store are considered part of the "uno world" and can be
                // embedded inside the uno canvas. Clients need to request html elements to be added
                // to the store before any native hosting takes place.
                static getNativeElementStore() {
                    let nativeElementStore = document.getElementById("uno-native-element-store");
                    if (!nativeElementStore) {
                        nativeElementStore = document.createElement("div");
                        nativeElementStore.id = "uno-native-element-store";
                        nativeElementStore.style.display = "none";
                        let unoBody = document.getElementById("uno-body");
                        unoBody.insertBefore(nativeElementStore, unoBody.firstChild);
                    }
                    return nativeElementStore;
                }
                static isNativeElement(content) {
                    for (let child of this.getNativeElementStore().children) {
                        if (child.id === content) {
                            return true;
                        }
                    }
                    return false;
                }
                static attachNativeElement(content) {
                    let element = this.getElementOrThrow(content);
                    element.remove(); // remove from the store
                    this.getNativeElementHost().appendChild(element); // add to the native host
                }
                static detachNativeElement(content) {
                    let element = this.getElementOrThrow(content);
                    element.remove(); // remove from the native host
                    this.getNativeElementStore().appendChild(element); // add to the native host
                }
                static arrangeNativeElement(content, x, y, width, height) {
                    let element = this.getElementOrThrow(content);
                    element.style.position = "absolute";
                    element.style.left = `${x}px`;
                    element.style.top = `${y}px`;
                    element.style.width = `${width}px`;
                    element.style.height = `${height}px`;
                }
                static changeNativeElementOpacity(content, opacity) {
                    let element = this.getElementOrThrow(content);
                    element.style.opacity = opacity.toString();
                }
                static createHtmlElementAndAddToStore(id, tagName) {
                    let element = document.createElement(tagName);
                    element.id = id;
                    this.getNativeElementStore().appendChild(element);
                }
                static addToStore(id) {
                    this.getNativeElementStore().appendChild(this.getElementOrThrow(id));
                }
                static disposeHtmlElement(id) {
                    this.getElementOrThrow(id).remove();
                }
                static createSampleComponent(parentId, text) {
                    let element = this.getElementOrThrow(parentId);
                    let btn = document.createElement("button");
                    btn.textContent = text;
                    btn.style.display = "inline-block";
                    btn.style.width = "100%";
                    btn.style.height = "100%";
                    btn.style.backgroundColor = "#ff69b4"; /* Hot pink */
                    btn.style.color = "white";
                    element.appendChild(btn);
                    element.addEventListener("pointerdown", _ => alert(`button ${text} clicked`));
                }
                static setStyleString(elementId, name, value) {
                    const element = this.getElementOrThrow(elementId);
                    element.style.setProperty(name, value);
                }
                static resetStyle(elementId, names) {
                    const element = this.getElementOrThrow(elementId);
                    for (const name of names) {
                        element.style.setProperty(name, "");
                    }
                }
                static setClasses(elementId, cssClassesList, classIndex) {
                    const element = this.getElementOrThrow(elementId);
                    for (let i = 0; i < cssClassesList.length; i++) {
                        if (i === classIndex) {
                            element.classList.add(cssClassesList[i]);
                        }
                        else {
                            element.classList.remove(cssClassesList[i]);
                        }
                    }
                }
                static setUnsetCssClasses(elementId, classesToUnset) {
                    const element = this.getElementOrThrow(elementId);
                    classesToUnset.forEach(c => {
                        element.classList.remove(c);
                    });
                }
                static setAttribute(elementId, name, value) {
                    const element = this.getElementOrThrow(elementId);
                    element.setAttribute(name, value);
                }
                static getAttribute(elementId, name) {
                    const element = this.getElementOrThrow(elementId);
                    return element.getAttribute(name);
                }
                static removeAttribute(elementId, name) {
                    const element = this.getElementOrThrow(elementId);
                    element.removeAttribute(name);
                }
                static setContentHtml(elementId, html) {
                    const element = this.getElementOrThrow(elementId);
                    element.innerHTML = html;
                }
                static registerNativeHtmlEvent(owner, elementId, eventName, managedHandler) {
                    const element = this.getElementOrThrow(elementId);
                    if (!BrowserHtmlElement.dispatchEventNativeElementMethod) {
                        throw `BrowserHtmlElement: The initialize method has not been called`;
                    }
                    const eventHandler = (event) => {
                        BrowserHtmlElement.dispatchEventNativeElementMethod(owner, eventName, managedHandler, event);
                    };
                    // Register the handler using a string representation of the managed handler
                    // the managed representation assumes that the string contains a unique id.
                    BrowserHtmlElement.nativeHandlersMap["" + managedHandler] = eventHandler;
                    element.addEventListener(eventName, eventHandler);
                }
                static unregisterNativeHtmlEvent(elementId, eventName, managedHandler) {
                    const element = this.getElementOrThrow(elementId);
                    if (!BrowserHtmlElement.dispatchEventNativeElementMethod) {
                        throw `BrowserHtmlElement: The initialize method has not been called`;
                    }
                    const key = "" + managedHandler;
                    const eventHandler = BrowserHtmlElement.nativeHandlersMap[key];
                    if (eventHandler) {
                        element.removeEventListener(eventName, eventHandler);
                        delete BrowserHtmlElement.nativeHandlersMap[key];
                    }
                }
                static invokeJS(command) {
                    return String(eval(command) || "");
                }
                static async invokeAsync(command) {
                    // Preseve the original emscripten marshalling semantics
                    // to always return a valid string.
                    var result = await eval(command);
                    return String(result || "");
                }
                /**
                 * Returns the element with the given id, or throws an error if not found.
                 */
                static getElementOrThrow(id) {
                    const element = document.getElementById(id);
                    if (!element) {
                        throw new Error(`BrowserHtmlElement: Element with id '${id}' not found.`);
                    }
                    return element;
                }
            }
            /** Native elements created with the BrowserHtmlElement class */
            BrowserHtmlElement.nativeHandlersMap = {};
            NativeElementHosting.BrowserHtmlElement = BrowserHtmlElement;
        })(NativeElementHosting = UI.NativeElementHosting || (UI.NativeElementHosting = {}));
    })(UI = Uno.UI || (Uno.UI = {}));
})(Uno || (Uno = {}));
var Uno;
(function (Uno) {
    var UI;
    (function (UI) {
        var Runtime;
        (function (Runtime) {
            var Skia;
            (function (Skia) {
                //import PointerDeviceType = Windows.Devices.Input.PointerDeviceType;
                let HtmlPointerEvent;
                (function (HtmlPointerEvent) {
                    HtmlPointerEvent[HtmlPointerEvent["pointerover"] = 1] = "pointerover";
                    HtmlPointerEvent[HtmlPointerEvent["pointerout"] = 2] = "pointerout";
                    HtmlPointerEvent[HtmlPointerEvent["pointerdown"] = 4] = "pointerdown";
                    HtmlPointerEvent[HtmlPointerEvent["pointerup"] = 8] = "pointerup";
                    HtmlPointerEvent[HtmlPointerEvent["pointercancel"] = 16] = "pointercancel";
                    // Optional pointer events
                    HtmlPointerEvent[HtmlPointerEvent["pointermove"] = 32] = "pointermove";
                    HtmlPointerEvent[HtmlPointerEvent["lostpointercapture"] = 64] = "lostpointercapture";
                    HtmlPointerEvent[HtmlPointerEvent["wheel"] = 128] = "wheel";
                })(HtmlPointerEvent = Skia.HtmlPointerEvent || (Skia.HtmlPointerEvent = {}));
                // TODO: Duplicate of Uno.UI.HtmlEventDispatchResult to merge!
                let HtmlEventDispatchResult;
                (function (HtmlEventDispatchResult) {
                    HtmlEventDispatchResult[HtmlEventDispatchResult["Ok"] = 0] = "Ok";
                    HtmlEventDispatchResult[HtmlEventDispatchResult["StopPropagation"] = 1] = "StopPropagation";
                    HtmlEventDispatchResult[HtmlEventDispatchResult["PreventDefault"] = 2] = "PreventDefault";
                    HtmlEventDispatchResult[HtmlEventDispatchResult["NotDispatched"] = 128] = "NotDispatched";
                })(HtmlEventDispatchResult = Skia.HtmlEventDispatchResult || (Skia.HtmlEventDispatchResult = {}));
                // TODO: Duplicate of Windows.Devices.Input.PointerDeviceType to import instead of duplicate!
                let PointerDeviceType;
                (function (PointerDeviceType) {
                    PointerDeviceType[PointerDeviceType["Touch"] = 0] = "Touch";
                    PointerDeviceType[PointerDeviceType["Pen"] = 1] = "Pen";
                    PointerDeviceType[PointerDeviceType["Mouse"] = 2] = "Mouse";
                })(PointerDeviceType = Skia.PointerDeviceType || (Skia.PointerDeviceType = {}));
                class BrowserPointerInputSource {
                    constructor(manageSource) {
                        // Cached reference to #uno-native-element-host. Refreshed if detached/replaced.
                        this._nativeElementHost = null;
                        this._bootTime = Date.now() - performance.now();
                        this._source = manageSource;
                        BrowserPointerInputSource._exports.OnInitialized(manageSource, this._bootTime);
                        this.subscribePointerEvents(); // Subscribe only after the managed initialization is done
                    }
                    static async initialize(inputSource) {
                        const module = window.Module;
                        if (BrowserPointerInputSource._exports == undefined
                            && module.getAssemblyExports !== undefined) {
                            const browserExports = (await module.getAssemblyExports("Uno.UI.Runtime.Skia.WebAssembly.Browser"));
                            BrowserPointerInputSource._exports = browserExports.Uno.UI.Runtime.Skia.BrowserPointerInputSource;
                        }
                        return new BrowserPointerInputSource(inputSource);
                    }
                    static setPointerCapture(pointerId) {
                        // Capture disabled for now on skia for wasm
                        //document.body.setPointerCapture(pointerId);
                    }
                    static releasePointerCapture(pointerId) {
                        // Capture disabled for now on skia for wasm
                        //document.body.releasePointerCapture(pointerId);
                    }
                    subscribePointerEvents() {
                        const element = document.body;
                        element.addEventListener("pointerover", this.onPointerEventReceived.bind(this), { capture: true });
                        element.addEventListener("pointerout", this.onPointerEventReceived.bind(this), { capture: true });
                        element.addEventListener("pointerdown", this.onPointerEventReceived.bind(this), { capture: true });
                        element.addEventListener("pointerup", this.onPointerEventReceived.bind(this), { capture: true });
                        //element.addEventListener("lostpointercapture", this.onPointerEventReceived.bind(this), { capture: true });
                        element.addEventListener("pointercancel", this.onPointerEventReceived.bind(this), { capture: true });
                        element.addEventListener("pointermove", this.onPointerEventReceived.bind(this), { capture: true });
                        element.addEventListener("wheel", this.onPointerEventReceived.bind(this), { capture: true, passive: false });
                    }
                    // Retrieve and cache the native element host reference.
                    // Refreshes if the node was detached or replaced (e.g., during hot reload).
                    getNativeElementHostCached() {
                        if (this._nativeElementHost === null || this._nativeElementHost.isConnected === false) {
                            this._nativeElementHost = document.getElementById("uno-native-element-host");
                        }
                        return this._nativeElementHost;
                    }
                    // Returns true if the event originated from within the native host subtree.
                    // Traverses regular DOM first, then crosses Shadow DOM boundaries when required.
                    // Uses identity comparisons only; avoids selector matching, allocations, and redundant lookups.
                    isEventFromNativeElementHost(eventTarget) {
                        const hostElement = this.getNativeElementHostCached();
                        if (hostElement === null) {
                            return false; // No host exists; nothing to filter.
                        }
                        let currentNode = eventTarget;
                        while (currentNode !== null) {
                            // Fast identity comparison.
                            if (currentNode === hostElement) {
                                return true;
                            }
                            // Normal DOM climb first (fastest path)
                            const parent = currentNode.parentNode;
                            if (parent) {
                                currentNode = parent;
                                continue;
                            }
                            // Only if parentNode is null, check for a shadow boundary.
                            const rootNode = currentNode.getRootNode();
                            // If we're inside a shadow root, jump to its host to continue traversal
                            if (rootNode instanceof ShadowRoot && rootNode.host) {
                                currentNode = rootNode.host; // cross shadow boundary
                                continue;
                            }
                            // Reached the top (Document or no further nodes)
                            break;
                        }
                        return false;
                    }
                    onPointerEventReceived(evt) {
                        var _a;
                        let id = (_a = evt.target) === null || _a === void 0 ? void 0 : _a.id;
                        if (id === "uno-enable-accessibility") {
                            // We have a div to enable accessibility (see enableA11y in WebAssemblyWindowWrapper).
                            // Pressing space on keyboard to click it will trigger pointer event which we want to ignore.
                            return;
                        }
                        if (this.isEventFromNativeElementHost(evt.target)) {
                            // Events from the native host are handled by the native control directly.
                            // We don't want to interfere with them.
                            return;
                        }
                        const event = BrowserPointerInputSource.toHtmlPointerEvent(evt.type);
                        let pointerId, pointerType, pressure;
                        let wheelDeltaX, wheelDeltaY;
                        if (evt instanceof WheelEvent) {
                            pointerId = evt.mozInputSource ? 0 : 1; // Try to match the mouse pointer ID 0 for FF, 1 for others
                            pointerType = PointerDeviceType.Mouse;
                            pressure = 0.5; // like WinUI
                            wheelDeltaX = evt.deltaX;
                            wheelDeltaY = evt.deltaY;
                            switch (evt.deltaMode) {
                                case WheelEvent.DOM_DELTA_LINE: // Actually this is supported only by FF
                                    const lineSize = BrowserPointerInputSource.wheelLineSize;
                                    wheelDeltaX *= lineSize;
                                    wheelDeltaY *= lineSize;
                                    break;
                                case WheelEvent.DOM_DELTA_PAGE:
                                    wheelDeltaX *= document.documentElement.clientWidth;
                                    wheelDeltaY *= document.documentElement.clientHeight;
                                    break;
                            }
                        }
                        else {
                            pointerId = evt.pointerId;
                            pointerType = BrowserPointerInputSource.toPointerDeviceType(evt.pointerType);
                            pressure = evt.pressure;
                            wheelDeltaX = 0;
                            wheelDeltaY = 0;
                        }
                        const result = BrowserPointerInputSource._exports.OnNativeEvent(this._source, event, //byte @event, // ONE of NativePointerEvent
                        evt.timeStamp, //double timestamp,
                        pointerType, //int deviceType, // ONE of _PointerDeviceType
                        pointerId, //double pointerId, // Warning: This is a Number in JS, and it might be negative on safari for iOS
                        evt.clientX, //double x,
                        evt.clientY, //double y,
                        evt.ctrlKey, //bool ctrl,
                        evt.shiftKey, //bool shift,
                        evt.buttons, //int buttons,
                        evt.button, //int buttonUpdate,
                        pressure, //double pressure,
                        wheelDeltaX, //double wheelDeltaX,
                        wheelDeltaY, //double wheelDeltaY,
                        evt.relatedTarget !== null //bool hasRelatedTarget)
                        );
                        // pointer events may have some side effects (like changing focus or opening a context menu on right clicking)
                        // We blanket-disable all the native behaviour so we don't have to whack-a-mole all the edge cases.
                        // We only allow wheel events with ctrl key pressed to allow zooming in/out.
                        const isZooming = evt instanceof WheelEvent && evt.ctrlKey;
                        if (result == HtmlEventDispatchResult.PreventDefault ||
                            !isZooming) {
                            evt.preventDefault();
                        }
                    }
                    static get wheelLineSize() {
                        // In web browsers, scroll might happen by pixels, line or page.
                        // But WinUI works only with pixels, so we have to convert it before send the value to the managed code.
                        // The issue is that there is no easy way get the "size of a line", instead we have to determine the CSS "line-height"
                        // defined in the browser settings. 
                        // https://stackoverflow.com/questions/20110224/what-is-the-height-of-a-line-in-a-wheel-event-deltamode-dom-delta-line
                        if (this._wheelLineSize == undefined) {
                            const el = document.createElement("div");
                            el.style.fontSize = "initial";
                            el.style.display = "none";
                            document.body.appendChild(el);
                            const fontSize = window.getComputedStyle(el).fontSize;
                            document.body.removeChild(el);
                            this._wheelLineSize = fontSize ? parseInt(fontSize) : 16; /* 16 = The current common default font size */
                            // Based on observations, even if the event reports 3 lines (the settings of windows),
                            // the browser will actually scroll of about 6 lines of text.
                            this._wheelLineSize *= 2.0;
                        }
                        return this._wheelLineSize;
                    }
                    //#endregion
                    //#region Helpers
                    static toHtmlPointerEvent(eventName) {
                        switch (eventName) {
                            case "pointerover":
                                return HtmlPointerEvent.pointerover;
                            case "pointerout":
                                return HtmlPointerEvent.pointerout;
                            case "pointerdown":
                                return HtmlPointerEvent.pointerdown;
                            case "pointerup":
                                return HtmlPointerEvent.pointerup;
                            case "pointercancel":
                                return HtmlPointerEvent.pointercancel;
                            case "pointermove":
                                return HtmlPointerEvent.pointermove;
                            case "wheel":
                                return HtmlPointerEvent.wheel;
                            default:
                                return undefined;
                        }
                    }
                    static toPointerDeviceType(type) {
                        switch (type) {
                            case "touch":
                                return PointerDeviceType.Touch;
                            case "pen":
                                // Note: As of 2019-11-28, once pen pressed events pressed/move/released are reported as TOUCH on Firefox
                                //		 https://bugzilla.mozilla.org/show_bug.cgi?id=1449660
                                return PointerDeviceType.Pen;
                            case "mouse":
                            default:
                                return PointerDeviceType.Mouse;
                        }
                    }
                }
                //#region WheelLineSize
                BrowserPointerInputSource._wheelLineSize = undefined;
                Skia.BrowserPointerInputSource = BrowserPointerInputSource;
            })(Skia = Runtime.Skia || (Runtime.Skia = {}));
        })(Runtime = UI.Runtime || (UI.Runtime = {}));
    })(UI = Uno.UI || (Uno.UI = {}));
})(Uno || (Uno = {}));
var Uno;
(function (Uno) {
    var UI;
    (function (UI) {
        var Runtime;
        (function (Runtime) {
            var Skia;
            (function (Skia) {
                class BrowserRenderer {
                    constructor(managedHandle) {
                        this.managedHandle = managedHandle;
                        this.canvas = undefined;
                        this.requestRender = undefined;
                        BrowserRenderer.anyGL = window.GL;
                        this.buildImports();
                    }
                    async buildImports() {
                        let anyModule = window.Module;
                        if (anyModule.getAssemblyExports !== undefined) {
                            const skiaSharpExports = await anyModule.getAssemblyExports("Uno.UI.Runtime.Skia.WebAssembly.Browser");
                            this.requestRender = () => skiaSharpExports.Uno.UI.Runtime.Skia.BrowserRenderer.RenderFrame(this.managedHandle);
                        }
                    }
                    static createInstance(managedHandle) {
                        return new BrowserRenderer(managedHandle);
                    }
                    setCanvasSize() {
                        var scale = window.devicePixelRatio || 1;
                        var rect = document.documentElement.getBoundingClientRect();
                        var width = rect.width;
                        var height = rect.height;
                        var w = width * scale;
                        var h = height * scale;
                        if (this.canvas.width !== w)
                            this.canvas.width = w;
                        if (this.canvas.height !== h)
                            this.canvas.height = h;
                        // We request to repaint on the next frame. Without this, the first frame after resizing the window will be
                        // blank and will cause a flickering effect when you drag the window's border to resize.
                        // See also https://github.com/unoplatform/uno-private/issues/902.
                        BrowserRenderer.invalidate(this);
                    }
                    static invalidate(instance) {
                        window.requestAnimationFrame(() => {
                            window.GL.makeContextCurrent(instance.glCtx);
                            instance.requestRender();
                        });
                    }
                    static createContextStatic(instance, canvasOrCanvasId) {
                        return instance.createContext(canvasOrCanvasId);
                    }
                    createContext(canvasOrCanvasId) {
                        if (!canvasOrCanvasId)
                            throw 'No <canvas> element or ID was provided';
                        var canvas = document.getElementById(canvasOrCanvasId);
                        if (!canvas)
                            throw `No <canvas> with id ${canvasOrCanvasId} was found`;
                        this.glCtx = BrowserRenderer.createWebGLContext(canvas);
                        if (!this.glCtx || this.glCtx < 0)
                            throw `Failed to create WebGL context: err ${this.glCtx}`;
                        // make current
                        BrowserRenderer.anyGL.makeContextCurrent(this.glCtx);
                        // Starting from .NET 7 the GLctx is defined in an inaccessible scope
                        // when the current GL context changes. We need to pick it up from the
                        // GL.currentContext instead.
                        let currentGLctx = BrowserRenderer.anyGL.currentContext && BrowserRenderer.anyGL.currentContext.GLctx;
                        if (!currentGLctx)
                            throw `Failed to get current WebGL context`;
                        // read values
                        this.canvas = canvas;
                        this.setCanvasSize();
                        window.addEventListener("resize", x => this.setCanvasSize());
                        return {
                            ctx: this.glCtx,
                            fbo: currentGLctx.getParameter(currentGLctx.FRAMEBUFFER_BINDING),
                            stencil: currentGLctx.getParameter(currentGLctx.STENCIL_BITS),
                            sample: 0,
                            depth: currentGLctx.getParameter(currentGLctx.DEPTH_BITS),
                        };
                    }
                    static createWebGLContext(canvas) {
                        var contextAttributes = {
                            alpha: 1,
                            depth: 1,
                            stencil: 8,
                            antialias: 1,
                            premultipliedAlpha: 1,
                            preserveDrawingBuffer: 0,
                            preferLowPowerToHighPerformance: 0,
                            failIfMajorPerformanceCaveat: 0,
                            majorVersion: 2,
                            minorVersion: 0,
                            enableExtensionsByDefault: 1,
                            explicitSwapControl: 0,
                            renderViaOffscreenBackBuffer: 0,
                        };
                        var ctx = BrowserRenderer.anyGL.createContext(canvas, contextAttributes);
                        if (!ctx && contextAttributes.majorVersion > 1) {
                            console.warn('Falling back to WebGL 1.0');
                            contextAttributes.majorVersion = 1;
                            contextAttributes.minorVersion = 0;
                            ctx = BrowserRenderer.anyGL.createContext(canvas, contextAttributes);
                        }
                        return ctx;
                    }
                }
                Skia.BrowserRenderer = BrowserRenderer;
            })(Skia = Runtime.Skia || (Runtime.Skia = {}));
        })(Runtime = UI.Runtime || (UI.Runtime = {}));
    })(UI = Uno.UI || (Uno.UI = {}));
})(Uno || (Uno = {}));
var Uno;
(function (Uno) {
    var UI;
    (function (UI) {
        var Runtime;
        (function (Runtime) {
            var Skia;
            (function (Skia) {
                class ImageLoader {
                    static loadFromArray(array) {
                        const seqNo = ++ImageLoader.sequenceNumber;
                        return new Promise((resolve) => {
                            ImageLoader.pendingPromiseResolvers.set(seqNo, resolve);
                            if (ImageLoader.availableWorkers.length == 0) {
                                ImageLoader.pendingJobs.push(worker => worker.postMessage([array, seqNo], [array.buffer]));
                            }
                            else {
                                const worker = ImageLoader.availableWorkers.pop();
                                worker.postMessage([array, seqNo], [array.buffer]);
                            }
                        });
                    }
                }
                ImageLoader.workerScriptUrl = URL.createObjectURL(new Blob([`
const offscreenCanvas = new OffscreenCanvas(0, 0);
const context = offscreenCanvas.getContext("2d", { willReadFrequently: true });
onmessage = async (e) => {
	const [array, seqNo] = e.data;
	const image = new Blob([array]);
	try {
		const imageBitmap = await createImageBitmap(image, { premultiplyAlpha: "premultiply" });
		offscreenCanvas.width = imageBitmap.width;
		offscreenCanvas.height = imageBitmap.height;
		context.drawImage(imageBitmap, 0, 0);
		const imageData = context.getImageData(
			0, 0,
			imageBitmap.width,
			imageBitmap.height
		);
		
		// Due to a bug in Skia on WASM, we need the pixels to be
		// alpha-premultiplied because using SKAlphaType.Unpremul is not working
		// correctly (see also https://github.com/unoplatform/uno/issues/20727),
		// so we multiply the RGB values by the alpha by hand instead.
		// This is somehow a LOT faster than doing it with webgl in a fragment shader
		const buffer = imageData.data;
		for (let i = 0; i < buffer.byteLength; i += 4) {
			const a = buffer[i + 3];
			buffer[i] = (buffer[i] * a) / 255;
			buffer[i + 1] = (buffer[i + 1] * a) / 255;
			buffer[i + 2] = (buffer[i + 2] * a) / 255;
		}
		postMessage({
			seqNo: seqNo,
			response: {
				error: null,
				bytes: new Uint8Array(imageData.data.buffer), // does not copy
				width: imageBitmap.width,
				height: imageBitmap.height
			}
		},
		[imageData.data.buffer]);
	} catch (e) {
		postMessage({seqNo: seqNo, response: { error: e.toString() }});
	}
};`], { type: 'application/javascript' }));
                ImageLoader.availableWorkers = Array.from({ length: navigator.hardwareConcurrency }).map((_, i) => {
                    const worker = new Worker(ImageLoader.workerScriptUrl);
                    const listener = (ev) => {
                        const promiseResolver = ImageLoader.pendingPromiseResolvers.get(ev.data.seqNo);
                        ImageLoader.pendingPromiseResolvers.delete(ev.data.seqNo);
                        promiseResolver(ev.data.response);
                        if (ImageLoader.pendingJobs.length == 0) {
                            ImageLoader.availableWorkers.push(worker);
                        }
                        else {
                            const job = ImageLoader.pendingJobs.pop();
                            window.setImmediate(() => job(worker));
                        }
                    };
                    worker.addEventListener("message", listener);
                    return worker;
                });
                ImageLoader.pendingJobs = new Array();
                ImageLoader.sequenceNumber = 0;
                ImageLoader.pendingPromiseResolvers = new Map();
                Skia.ImageLoader = ImageLoader;
            })(Skia = Runtime.Skia || (Runtime.Skia = {}));
        })(Runtime = UI.Runtime || (UI.Runtime = {}));
    })(UI = Uno.UI || (Uno.UI = {}));
})(Uno || (Uno = {}));
var Uno;
(function (Uno) {
    var UI;
    (function (UI) {
        var Interop;
        (function (Interop) {
            class Runtime {
                static init() {
                    return "";
                }
                static InvokeJS(command) {
                    // Preseve the original emscripten marshalling semantics
                    // to always return a valid string.
                    return String(eval(command) || "");
                }
            }
            Runtime.engine = Runtime.init();
            Interop.Runtime = Runtime;
        })(Interop = UI.Interop || (UI.Interop = {}));
    })(UI = Uno.UI || (Uno.UI = {}));
})(Uno || (Uno = {}));
var Uno;
(function (Uno) {
    var UI;
    (function (UI) {
        var Runtime;
        (function (Runtime) {
            var Skia;
            (function (Skia) {
                class WebAssemblyWindowWrapper {
                    constructor(owner) {
                        this.owner = owner;
                        this.build();
                    }
                    static initialize(owner) {
                        WebAssemblyWindowWrapper.activeInstances[owner] = new WebAssemblyWindowWrapper(owner);
                    }
                    static persistBootstrapperLoader() {
                        let bootstrapperLoaders = document.getElementsByClassName(WebAssemblyWindowWrapper.unoPersistentLoaderClassName);
                        if (bootstrapperLoaders.length > 0) {
                            let bootstrapperLoader = bootstrapperLoaders[0];
                            bootstrapperLoader.classList.add(WebAssemblyWindowWrapper.unoKeepLoaderClassName);
                        }
                    }
                    async build() {
                        await this.buildImports();
                        this.containerElement = document.getElementById("uno-body");
                        if (!this.containerElement) {
                            // If not found, we simply create a new one.
                            this.containerElement = document.createElement("div");
                            this.containerElement.id = "uno-root";
                            document.body.appendChild(this.containerElement);
                        }
                        this.canvasElement = document.createElement("canvas");
                        this.canvasElement.id = "uno-canvas";
                        this.canvasElement.setAttribute("aria-hidden", "true");
                        this.containerElement.appendChild(this.canvasElement);
                        await Skia.Accessibility.setup();
                        window.addEventListener("resize", x => this.resize());
                        window.addEventListener("contextmenu", x => {
                            x.preventDefault();
                        });
                        this.resize();
                        await this.prefetchFonts();
                        this.removeLoading();
                    }
                    removeLoading() {
                        const element = document.getElementById(WebAssemblyWindowWrapper.loadingElementId);
                        if (element) {
                            element.parentElement.removeChild(element);
                        }
                        let bootstrapperLoaders = document.getElementsByClassName(WebAssemblyWindowWrapper.unoPersistentLoaderClassName);
                        if (bootstrapperLoaders.length > 0) {
                            let bootstrapperLoader = bootstrapperLoaders[0];
                            bootstrapperLoader.parentElement.removeChild(bootstrapperLoader);
                        }
                    }
                    async buildImports() {
                        let anyModule = window.Module;
                        if (anyModule.getAssemblyExports !== undefined) {
                            const browserExports = await anyModule.getAssemblyExports("Uno.UI.Runtime.Skia.WebAssembly.Browser");
                            this.onResize = browserExports.Uno.UI.Runtime.Skia.WebAssemblyWindowWrapper.OnResize;
                            this.prefetchFonts = browserExports.Uno.UI.Runtime.Skia.WebAssemblyWindowWrapper.PrefetchFonts;
                        }
                    }
                    static getInstance(owner) {
                        const instance = this.activeInstances[owner];
                        if (!instance) {
                            throw `WebAssemblyWindowWrapper for instance ${owner} not found.`;
                        }
                        return instance;
                    }
                    static getContainerId(owner) {
                        return WebAssemblyWindowWrapper.getInstance(owner).containerElement.id;
                    }
                    static getCanvasId(owner) {
                        return WebAssemblyWindowWrapper.getInstance(owner).canvasElement.id;
                    }
                    resize() {
                        var rect = document.documentElement.getBoundingClientRect();
                        this.onResize(this.owner, rect.width, rect.height, globalThis.devicePixelRatio);
                    }
                    static setCursor(cssCursor) {
                        document.body.style.cursor = cssCursor;
                    }
                    resizeWindow(width, height) {
                        window.resizeTo(width, height);
                    }
                    moveWindow(x, y) {
                        window.moveTo(x, y);
                    }
                }
                WebAssemblyWindowWrapper.unoPersistentLoaderClassName = "uno-persistent-loader";
                WebAssemblyWindowWrapper.loadingElementId = "uno-loading";
                WebAssemblyWindowWrapper.unoKeepLoaderClassName = "uno-keep-loader";
                WebAssemblyWindowWrapper.activeInstances = {};
                Skia.WebAssemblyWindowWrapper = WebAssemblyWindowWrapper;
            })(Skia = Runtime.Skia || (Runtime.Skia = {}));
        })(Runtime = UI.Runtime || (UI.Runtime = {}));
    })(UI = Uno.UI || (Uno.UI = {}));
})(Uno || (Uno = {}));
