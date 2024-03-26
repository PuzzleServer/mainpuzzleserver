window.addEventListener("message", (e) => {
    if ((e.origin.includes("puzzlehunt.azurewebsites.net")) || (e.origin.includes("localhost"))) {
        e.source.postMessage({ width: document.body.scrollWidth, height: document.body.scrollHeight }, e.origin);
    }
});

document.addEventListener("DOMContentLoaded", () => {
    const params = new URLSearchParams(window.location.search);
    if (params.has("embed") && (params.get("embed").length > 0) && (params.get("embed") === "true")) {
        const contents = document.querySelector(".content-div");
        if ((contents !== null) && (contents !== undefined)) {
            contents.style.padding = "0px";
            contents.style.borderWidth = "0px";
            contents.parentElement.style.margin = "0px";
        }
        const header = document.querySelector(".header-div");
        if ((header !== null) && (header !== undefined)) {
            header.parentElement.removeChild(header);
        }
    }
});
