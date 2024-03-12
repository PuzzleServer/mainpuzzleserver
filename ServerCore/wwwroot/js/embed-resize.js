window.addEventListener("message", (e) => {
    if ((e.origin.includes("puzzlehunt.azurewebsites.net")) || (e.origin.includes("localhost"))) {
        e.source.postMessage({ width: document.body.scrollWidth, height: document.body.scrollHeight }, e.origin);
    }
});