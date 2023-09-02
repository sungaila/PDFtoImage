// Caution! Be sure you understand the caveats before publishing an application with
// offline support. See https://aka.ms/blazor-offline-considerations
const readAsDataURLAsync = (file) => {
    const fileReader = new FileReader();

    return new Promise((resolve, reject) => {
        fileReader.onerror = error => {
            fileReader.abort();
            reject(new DOMException(error));
        };

        fileReader.onload = () => {
            resolve(fileReader.result);
        };

        fileReader.readAsDataURL(file);
    });
};

var webShareFormData = null;

self.importScripts('./service-worker-assets.js');
self.addEventListener('install', event => event.waitUntil(onInstall(event)));
self.addEventListener('activate', event => event.waitUntil(onActivate(event)));
self.addEventListener('fetch', event => event.respondWith(onFetch(event)));
self.addEventListener('message', event => onMessage(event));

const cacheNamePrefix = 'offline-cache-';
const cacheName = `${cacheNamePrefix}${self.assetsManifest.version}`;
const offlineAssetsInclude = [/\.dll$/, /\.pdb$/, /\.wasm/, /\.html/, /\.js$/, /\.json$/, /\.webmanifest/, /\.css$/, /\.woff$/, /\.png$/, /\.jpe?g$/, /\.gif$/, /\.ico$/, /\.blat$/, /\.dat$/];
const offlineAssetsExclude = [/^service-worker\.js$/];

async function onInstall(event) {
    console.info('Service worker: Install');

    // Fetch and cache all matching items from the assets manifest
    const assetsRequests = self.assetsManifest.assets
        .filter(asset => offlineAssetsInclude.some(pattern => pattern.test(asset.url)))
        .filter(asset => !offlineAssetsExclude.some(pattern => pattern.test(asset.url)))
        .map(asset => new Request(asset.url, { integrity: asset.hash, cache: 'no-cache' }));
    await caches.open(cacheName).then(cache => cache.addAll(assetsRequests));
}

async function onActivate(event) {
    console.info('Service worker: Activate');

    // Delete unused caches
    const cacheKeys = await caches.keys();
    await Promise.all(cacheKeys
        .filter(key => key.startsWith(cacheNamePrefix) && key !== cacheName)
        .map(key => caches.delete(key)));
}

async function onFetch(event) {
    let cachedResponse = null;
    if (event.request.method === 'GET') {
        // For all navigation requests, try to serve index.html from cache
        // If you need some URLs to be server-rendered, edit the following check to exclude those URLs
        const shouldServeIndexHtml = event.request.mode === 'navigate';

        const request = shouldServeIndexHtml ? 'index.html' : event.request;
        const cache = await caches.open(cacheName);
        cachedResponse = await cache.match(request);
    } else if (event.request.method === 'POST') {
        if (event.request.url.endsWith('/receive-webshare')) {
            webShareFormData = await event.request.formData();
            return Response.redirect('/PDFtoImage/', 303);
        }
    }

    return cachedResponse || fetch(event.request);
}

async function onMessage(event) {
    if (event.data !== 'receive-webshare') {
        return;
    }

    var payload = null;

    if (webShareFormData !== undefined && webShareFormData !== null) {
        var filesStringified = [];

        for (const file of webShareFormData.getAll('pdfs')) {

            var fileStringified = {
                name: file.name,
                lastModified: file.lastModified,
                size: file.size,
                type: file.type,
                data: (await readAsDataURLAsync(file)).toString()
            };

            filesStringified.push(fileStringified);
        }

        payload = {
            title: webShareFormData.get('title'),
            text: webShareFormData.get('text'),
            url: webShareFormData.get('url'),
            files: filesStringified,
        };
    }

    const response = {
        type: 'receive-webshare',
        payload: payload
    };

    event.source.postMessage(response);

    webShareFormData = null;
}