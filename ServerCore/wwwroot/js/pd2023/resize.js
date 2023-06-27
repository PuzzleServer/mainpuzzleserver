window.addEventListener("message", (e) => {
    if ((e.origin.includes("puzzlehunt.azurewebsites.net")) || (e.origin.includes("localhost"))) {
        const contents = document.querySelector(".content-div");
        contents.style.padding = "0px";
        contents.style.margin = "0px";
        contents.style.borderWidth = "0px";
        const header = document.querySelector(".header-div");
        if ((header !== null) && (header !== undefined)) {
            header.parentElement.removeChild(header);
        }
        contents.parentElement.style.padding = "0px";
        contents.parentElement.style.margin = "0px";
        const windowSize = { width: document.body.scrollWidth, height: document.body.scrollHeight };
        e.source.postMessage(windowSize, e.origin);
    }
});
