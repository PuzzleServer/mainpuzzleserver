var parentWindow = null;
var parentOrigin = null;

async function submitAnswer(answer) {
    console.log(`Submitting ${answer}`);
    
    if (parentOrigin) {
        var searchParams = new URLSearchParams(window.location.search);
        var url = `${parentOrigin}/api/puzzleapi/submitAnswer/${searchParams.get("eventId")}/${searchParams.get("puzzleId")}/${searchParams.get("userId")}/${searchParams.get("teamPassword")}`;
        const response = await fetch(url, { method: "POST", headers: { "Content-Type": "application/json" }, body: JSON.stringify({SubmissionText: answer, AllowFreeformSharing: false}) });
        const responseData = await response.json();
    }
}

window.addEventListener("message", (e) => {
    if ((e.origin.includes("puzzlehunt.azurewebsites.net")) || (e.origin.includes("localhost"))) {
        parentWindow = e.source;
        parentOrigin = e.origin;
        e.source.postMessage({ width: document.body.scrollWidth, height: document.body.scrollHeight }, e.origin);
    }
});

document.addEventListener("DOMContentLoaded", () => {
    const params = new URLSearchParams(window.location.search);
    if (params.has("embed") && (params.get("embed").length > 0) && (params.get("embed") === "true")) {
        const contents = document.querySelector(".content-div");
        if ((contents !== null) && (contents !== undefined)) {
            contents.style.padding = "0px";
            contents.style.margin = "0px";
            contents.style.borderWidth = "0px";
            contents.parentElement.style.padding = "0px";
            contents.parentElement.style.margin = "0px";
        }
        const header = document.querySelector(".header-div");
        if ((header !== null) && (header !== undefined)) {
            header.parentElement.removeChild(header);
        }
        const dc = document.querySelector(".has-dc");
        if ((dc !== null) && (dc !== undefined)) {
            dc.parentElement.removeChild(dc);
        }
    }
});
