import { config as unoConfig } from "./package_f6a20f3e3443d832c0be28e47eb8351854961a0b/uno-config.js";

if (unoConfig.environmentVariables["UNO_BOOTSTRAP_DEBUGGER_ENABLED"] !== "True") {
    console.debug("[ServiceWorker] Initializing");
    let uno_enable_tracing = unoConfig.uno_enable_tracing;

    // Get the number of fetch retries from environment variables or default to 1
    const fetchRetries = parseInt(unoConfig.environmentVariables["UNO_BOOTSTRAP_FETCH_RETRIES"] || "1");

    self.addEventListener('install', function (e) {
        console.debug('[ServiceWorker] Installing offline worker');
        e.waitUntil(
            caches.open('ea89e8ca-4821-4ef8-85d8-96afa9f04eba').then(async function (cache) {
                console.debug('[ServiceWorker] Caching app binaries and content');

                // Add files one by one to avoid failed downloads to prevent the
                // worker to fail installing.
                for (var i = 0; i < unoConfig.offline_files.length; i++) {
                    try {
                        const currentFile = unoConfig.offline_files[i];
                        if (uno_enable_tracing) {
                            console.debug(`[ServiceWorker] caching ${currentFile}`);
                        }

                        await cache.add(currentFile);
                    }
                    catch (e) {
                        console.debug(`[ServiceWorker] Failed to fetch ${unoConfig.offline_files[i]}: ${e.message}`);
                    }
                }

                // Add the runtime's own files to the cache. We cannot use the
                // existing cached content from the runtime as the keys contain a
                // hash we cannot reliably compute.
                try {
                    // Replace dynamic import with fetch and eval for web worker compatibility
                    // In .NET 10+, dotnet.boot.js was merged with dotnet.js for performance
                    const response = await fetch("/_framework/dotnet.js");
                    if (!response.ok) {
                        throw new Error(`Failed to fetch dotnet.js: ${response.status} ${response.statusText}`);
                    }

                    let scriptContent = await response.text();

                    // The parsing assumes that this block is present:
                    // https://github.com/dotnet/runtime/blob/41c9fa2d39a02d98cdead08e72f961e77b7888b0/src/tasks/Microsoft.NET.Sdk.WebAssembly.Pack.Tasks/BootJsonBuilderHelper.cs#L74
                    const match = scriptContent.match(/.*?\/\*json-start\*\/([\s\S]*?)\/\*json-end\*\//);

                    // If found, wrap it with parentheses so eval can treat it as an object literal
                    const c = match ? `(${match[1]})` : null;

                    if (!c) {
                        throw `Invalid config`;
                    }

                    const bootJson = eval(c);
                    const monoConfigResources = bootJson.resources || {};

                    var entries = {
                        ...(monoConfigResources.coreAssembly || {}),
                        ...(monoConfigResources.assembly || {}),
                        ...(monoConfigResources.lazyAssembly || {}),
                        ...(monoConfigResources.jsModuleWorker || {}),
                        ...(monoConfigResources.jsModuleGlobalization || {}),
                        ...(monoConfigResources.jsModuleNative || {}),
                        ...(monoConfigResources.jsModuleRuntime || {}),
                        ...(monoConfigResources.wasmNative || {}),
                        ...(monoConfigResources.icu || {})
                    };

                    for (var key in entries) {
                        var uri = `/_framework/${key}`;

                        try {
                            if (uno_enable_tracing) {
                                console.debug(`[ServiceWorker] cache ${uri}`);
                            }

                            await cache.add(uri);
                        } catch (e) {
                            console.error(`[ServiceWorker] Failed to cache ${uri}:`, e.message);
                        }
                    }
                } catch (e) {
                    // Centralized error handling for the entire boot.json processing
                    console.error('[ServiceWorker] Error processing boot configuration:', e.message);
                }
            })
        );
    });

    // Cache cleanup logic to prevent storage bloat
    // This removes any old caches that might have been created by previous
    // versions of the service worker, helping prevent storage quota issues
    self.addEventListener('activate', event => {
        event.waitUntil(
            caches.keys().then(function (cacheNames) {
                return Promise.all(
                    cacheNames.filter(function (cacheName) {
                        return cacheName !== 'ea89e8ca-4821-4ef8-85d8-96afa9f04eba';
                    }).map(function (cacheName) {
                        console.debug('[ServiceWorker] Deleting old cache:', cacheName);
                        return caches.delete(cacheName);
                    })
                );
            }).then(function () {
                return self.clients.claim();
            })
        );
    });

    self.addEventListener('fetch', event => {
        event.respondWith(
            (async function () {
                // FIXED: Critical fix for "already used" Request objects #956
                // Request objects can only be used once in a fetch operation
                // Cloning the request allows for reuse in fallback scenarios
                const requestClone = event.request.clone();

                try {
                    // Network first mode to get fresh content every time, then fallback to
                    // cache content if needed.
                    return await fetch(requestClone);
                } catch (err) {
                    // Logging to track network failures
                    console.debug(`[ServiceWorker] Network fetch failed, falling back to cache for: ${requestClone.url}`);

                    const cachedResponse = await caches.match(event.request);
                    if (cachedResponse) {
                        return cachedResponse;
                    }

                    // Add retry mechanism - attempt to fetch again if retries are configured
                    if (fetchRetries > 0) {
                        console.debug(`[ServiceWorker] Resource not in cache, attempting ${fetchRetries} network retries for: ${requestClone.url}`);

                        // Try multiple fetch attempts with exponential backoff
                        for (let retryCount = 0; retryCount < fetchRetries; retryCount++) {
                            try {
                                // Exponential backoff between retries (500ms, 1s, 2s, etc.)
                                const retryDelay = Math.pow(2, retryCount) * 500;
                                await new Promise(resolve => setTimeout(resolve, retryDelay));

                                if (uno_enable_tracing) {
                                    console.debug(`[ServiceWorker] Retry attempt ${retryCount + 1}/${fetchRetries} for: ${requestClone.url}`);
                                }

                                // Need a fresh request clone for each retry
                                return await fetch(event.request.clone());
                            } catch (retryErr) {
                                if (uno_enable_tracing) {
                                    console.debug(`[ServiceWorker] Retry ${retryCount + 1} failed: ${retryErr.message}`);
                                }
                                // Continue to next retry attempt
                            }
                        }
                    }

                    // Graceful error handling with a proper HTTP response
                    // Rather than letting the fetch fail with a generic error,
                    // we return a controlled 503 Service Unavailable response
                    console.error(`[ServiceWorker] Resource not available in cache or network after ${fetchRetries} retries: ${requestClone.url}`);
                    return new Response('Network error occurred, and resource was not found in cache.', {
                        status: 503,
                        statusText: 'Service Unavailable',
                        headers: new Headers({
                            'Content-Type': 'text/plain'
                        })
                    });
                }
            })()
        );
    });
}
else {
    // In development, always fetch from the network and do not enable offline support.
    // This is because caching would make development more difficult (changes would not
    // be reflected on the first load after each change).
    // It also breaks the hot reload feature because VS's browserlink is not always able to
    // inject its own framework in the served scripts and pages.
    self.addEventListener('fetch', () => { });
}
