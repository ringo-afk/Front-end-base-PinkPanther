function IniciarJuegoUnity(canvasId, loadingBarId, progressBarFullId, buildFolder, gameName, idUsuarioWeb) {
    var canvas = document.querySelector(canvasId);
    var loadingBar = document.querySelector(loadingBarId);
    var progressBarFull = document.querySelector(progressBarFullId);

    var buildUrl = "/UnityBuilds/" + buildFolder + "/Build";
    
    var config = {
        dataUrl: buildUrl + "/" + buildFolder + ".data",
        frameworkUrl: buildUrl + "/" + buildFolder + ".framework.js",
        codeUrl: buildUrl + "/" + buildFolder + ".wasm",
        streamingAssetsUrl: "StreamingAssets",
        companyName: "PinkPanther",
        productName: gameName,
        productVersion: "1.0",
    };

    createUnityInstance(canvas, config, (progress) => {
        progressBarFull.style.width = (100 * progress) + "%";
    }).then((unityInstance) => {
        loadingBar.style.display = "none";

        // Inyectar el ID directamente al GameObject "ManejadorLogin" en Unity
        if (idUsuarioWeb > 0) {
            unityInstance.SendMessage('ManejadorLogin', 'AsignarUsuarioWeb', idUsuarioWeb);
        }
    }).catch((message) => {
        console.error("Error al cargar Unity WebGL: " + message);
    });
}