const is_browser = typeof window != "undefined";
if (!is_browser) throw new Error(`Expected to be running in a browser`);

const BootConfigPath = "./_framework/blazor.boot.json";
const FrameworkHashStorageKey = "WebTranslator.FrameworkResourceHash";

const bootProgress = createBootProgress();

globalThis.webTranslatorPlatform = {
    platform() {
        return navigator.userAgentData?.platform || navigator.platform || "";
    }
};

globalThis.webTranslatorDictionary = {
    dictionaryUrl() {
        return new URL("Dict-Mini.json", document.baseURI).toString();
    },
    async root() {
        if (!navigator.storage?.getDirectory) {
            throw new Error("当前浏览器不支持持久化本地存储");
        }
        return await navigator.storage.getDirectory();
    },
    async read() {
        try {
            const root = await this.root();
            const handle = await root.getFileHandle("Dict-Mini.json");
            const file = await handle.getFile();
            return await file.text();
        } catch (error) {
            if (error?.name === "NotFoundError") return null;
            throw error;
        }
    },
    async write(text) {
        const root = await this.root();
        const handle = await root.getFileHandle("Dict-Mini.json", { create: true });
        const writable = await handle.createWritable();
        await writable.write(text);
        await writable.close();
    },
    async remove() {
        try {
            const root = await this.root();
            await root.removeEntry("Dict-Mini.json");
        } catch (error) {
            if (error?.name === "NotFoundError") return;
            throw error;
        }
    }
};

try {
    const bootConfig = await prepareFrameworkBootConfig();
    const bootHash = getBootResourceHash(bootConfig) ?? Date.now().toString();
    const { dotnet } = await import(`./_framework/dotnet.js?v=${encodeURIComponent(bootHash)}`);

    const dotnetRuntime = await dotnet
        .withDiagnosticTracing(false)
        .withApplicationArgumentsFromQuery()
        .withConfig(bootConfig)
        .withModuleConfig({
            onDownloadResourceProgress: (loaded, queued) => bootProgress.update(loaded, queued)
        })
        .withOnConfigLoaded((config) => {
            bootProgress.setTotal(countBootResources(config));
            bootProgress.setPhase("正在加载运行时资源");
        })
        .create();

    bootProgress.setPhase("正在启动应用");
    const config = dotnetRuntime.getConfig();

    await dotnetRuntime.runMain(config.mainAssemblyName, [window.location.search]);
    bootProgress.complete();
} catch (error) {
    bootProgress.fail(error);
    throw error;
}

async function prepareFrameworkBootConfig() {
    bootProgress.setPhase("正在检查资源版本");

    const config = await fetchBootConfig();
    const hash = getBootResourceHash(config);

    if (!hash) return config;

    const previousHash = readStoredFrameworkHash();
    if (previousHash !== hash) {
        bootProgress.setPhase(previousHash ? "检测到新版本，正在清理资源缓存" : "正在初始化资源缓存");
        await clearFrameworkCaches();
        writeStoredFrameworkHash(hash);
    }

    return config;
}

async function fetchBootConfig() {
    const response = await fetch(BootConfigPath, {
        cache: "no-store",
        credentials: "same-origin",
        headers: {
            "Cache-Control": "no-cache"
        }
    });

    if (!response.ok) throw new Error(`启动清单加载失败: ${response.status} ${response.statusText}`);

    return await response.json();
}

async function clearFrameworkCaches() {
    if (!globalThis.caches) return;

    const keys = await caches.keys();
    const dotnetResourceKeys = keys.filter((key) => key.startsWith("dotnet-resources-"));

    await Promise.all(dotnetResourceKeys.map((key) => caches.delete(key)));
}

function getBootResourceHash(config) {
    return typeof config?.resources?.hash === "string" ? config.resources.hash : null;
}

function readStoredFrameworkHash() {
    try {
        return localStorage.getItem(FrameworkHashStorageKey);
    } catch {
        return null;
    }
}

function writeStoredFrameworkHash(hash) {
    try {
        localStorage.setItem(FrameworkHashStorageKey, hash);
    } catch {
        // Some private browsing modes block localStorage. The app can still run; it just revalidates next load.
    }
}

function createBootProgress() {
    const phaseElement = document.getElementById("boot-phase");
    const countElement = document.getElementById("boot-resource-count");
    const percentElement = document.getElementById("boot-progress-percent");
    const barElement = document.getElementById("boot-progress-value");

    let total = 0;
    let loaded = 0;

    render();

    return {
        setPhase(phase) {
            if (phaseElement) phaseElement.textContent = phase;
        },
        setTotal(value) {
            total = Math.max(total, value);
            render();
        },
        update(loadedValue, queuedValue) {
            loaded = Math.max(loaded, loadedValue);
            if (total === 0 && queuedValue > 0) total = queuedValue;
            if (loaded > total) total = loaded;
            render();
        },
        complete() {
            if (total === 0) total = loaded;
            loaded = total;
            render();
        },
        fail(error) {
            if (phaseElement) phaseElement.textContent = "加载失败，请刷新后重试";
            if (countElement) countElement.textContent = error?.message ?? "未知错误";
        }
    };

    function render() {
        const visibleTotal = total > 0 ? total.toString() : "?";
        const percent = total > 0 ? Math.min(100, Math.round((loaded / total) * 100)) : 0;

        if (countElement) countElement.textContent = `已加载 ${loaded} / ${visibleTotal} 个资源`;
        if (percentElement) percentElement.textContent = `${percent}%`;
        if (barElement) barElement.style.width = `${percent}%`;
    }
}

function countBootResources(config) {
    const resources = config?.resources ?? {};
    let total = 0;

    total += countResourceGroup(resources.wasmNative);
    total += countResourceGroup(resources.coreAssembly);
    total += countResourceGroup(resources.assembly);
    total += countNestedResourceGroups(resources.coreVfs);
    total += countNestedResourceGroups(resources.vfs);

    if (config?.debugLevel !== 0) {
        total += countResourceGroup(resources.corePdb);
        total += countResourceGroup(resources.pdb);
        total += countResourceGroup(resources.wasmSymbols);
    }

    if (config?.loadAllSatelliteResources) {
        total += countNestedResourceGroups(resources.satelliteResources);
    }

    total += countIcuResources(config, resources.icu);
    total += countAppSettings(config);

    return total;
}

function countResourceGroup(group) {
    return group ? Object.keys(group).length : 0;
}

function countNestedResourceGroups(groups) {
    if (!groups) return 0;
    return Object.values(groups).reduce((total, group) => total + countResourceGroup(group), 0);
}

function countAppSettings(config) {
    const appSettings = config?.appsettings;
    if (!Array.isArray(appSettings)) return 0;

    const environment = config?.applicationEnvironment ?? "Production";
    return appSettings.filter((path) => {
        const name = path.substring(path.lastIndexOf("/") + 1);
        return name === "appsettings.json" || name === `appsettings.${environment}.json`;
    }).length;
}

function countIcuResources(config, icuResources) {
    if (!icuResources || config?.globalizationMode === "invariant") return 0;

    const names = Object.keys(icuResources);
    const segmentationRuleCount = names.filter((name) =>
        name.startsWith("segmentation-rules") && name.endsWith(".json")).length;
    const preferred = getPreferredIcuAsset(config, names);

    return segmentationRuleCount + (preferred ? 1 : 0);
}

function getPreferredIcuAsset(config, names) {
    if (names.length === 0) return null;
    if (config?.globalizationMode === "custom") return names[0];

    let assetName = "icudt.dat";
    const culture = config?.applicationCulture ?? navigator.languages?.[0] ?? navigator.language;

    if (config?.globalizationMode === "hybrid") {
        assetName = "icudt_hybrid.dat";
    } else if (culture && config?.globalizationMode !== "all") {
        const language = culture.split("-")[0];
        if (language === "en" || ["fr", "fr-FR", "it", "it-IT", "de", "de-DE", "es", "es-ES"].includes(culture)) {
            assetName = "icudt_EFIGS.dat";
        } else if (["zh", "ko", "ja"].includes(language)) {
            assetName = "icudt_CJK.dat";
        } else {
            assetName = "icudt_no_CJK.dat";
        }
    }

    return names.includes(assetName) ? assetName : null;
}
