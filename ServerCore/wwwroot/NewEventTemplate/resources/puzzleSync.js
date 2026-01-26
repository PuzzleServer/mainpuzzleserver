let syncing = false;
let changeQueue = [];
let markoverChangeQueue = [];
let pageId = null;
let savedMode = null;

document.addEventListener('DOMContentLoaded', function () {
    pageId = document.body.getAttribute("data-puzzle-page-id");

    // no co-op on standalone please
    if (window.self === window.top) return;

    let resetStyles = new CSSStyleSheet();
    resetStyles.replaceSync(".puzzle-entry.hide-reset .puzzle-reset-button { display: none; }");
    document.adoptedStyleSheets.push(resetStyles);

    setCoopMode(localStorage.getItem(`${pageId}-coopState`) ?? "coop");
});

window.addEventListener("message", function (ev) {
    if (!syncing && ev.data.type === "syncComponentLoad") {
        let mode = localStorage.getItem(`${pageId}-coopState`) ?? "coop";
        syncing = true;
        window.parent.postMessage({ type: "puzzleLoad", mode: mode }, "*");
        window.parent.postMessage({ type: "puzzlechanged", changes: changeQueue }, "*");
        changeQueue = [];
    }
    else if (ev.data.type === "puzzlesynced") {
        const grids = this.document.querySelectorAll(".puzzle-grid, .puzzle-entry-content");
        // TODO: consider batching by puzzleId
        for (change of ev.data.changes) {
            // If the change is a markover change, we need to handle it differently
            if (change.propertyKey == "MK" && (typeof Mk !== "undefined")) {
                let changes = [];
                changes.push(toMarkoverFormat(change));
                Mk.bulkApplyChanges(changes);
                continue;
            }

            for (let grid of grids) {
                if (grid.puzzleGrid.puzzleId == change.puzzleId) {
                    grid.puzzleGrid.changeWithoutUndo([change]);
                }
            }
        }
    }
    else if (ev.data.type === "setCoopMode") {
        setCoopMode(ev.data.mode);
    }
});
window.parent.postMessage({ type: "puzzleLoad", mode: localStorage.getItem(`${pageId}-coopState`) ?? "coop" }, "*");

function sendToServer(changes) {
    
    // Don't sync if the current mode is solo
    if (savedMode == "solo") return;

    if (syncing) {
        window.parent.postMessage({ type: "puzzlechanged", changes: changes }, "*");
    }
    // If the sync component is still loading, queue the changes to send them later
    else {
        changeQueue.push(changes);
    }
}

document.addEventListener("puzzlechanged", e => {
    let changes = e.detail;

    sendToServer(changes);
});

window.addEventListener("markoverchanged", (event) => {
    sendToServer(new Array(toServerFormat(...event.detail)));
});

function toServerFormat(contentId, locationKey, value) {
    return {
        puzzleId: contentId,
        propertyKey: 'MK',
        locationKey: '' + locationKey,
        value: JSON.stringify(value)
    }
}

function toMarkoverFormat(change) {
    return [change.puzzleId, parseInt(change.locationKey), JSON.parse(change.value)];
}

// Set a co-op mode, either on first load or by user change
function setCoopMode(mode) {
    localStorage.setItem(`${pageId}-coopState`, mode);
    savedMode = mode;

    if (typeof Mk !== "undefined") {
        if (mode == "coop") {
            Mk.setSyncMode('server');
        }
        else if (mode == "solo") {
            Mk.setSyncMode('local');
        }
    }

    document.querySelectorAll(".puzzle-entry").forEach((puzzle) => {
        puzzle.setAttribute("data-coop-mode", mode);

        if (mode == "coop") {
            // clear puzzle entirely, stop saving state as well
            puzzle.puzzleEntry.prepareToReset();
            puzzle.puzzleEntry.rebuildContents();
            puzzle.classList.add("hide-reset");
        }

        if (mode == "solo") {
            // continue saving state
            puzzle.puzzleEntry.puzzleGrids.forEach(grid => { grid.inhibitSave = false; });
            changeQueue = [];
            puzzle.classList.remove("hide-reset");
        }
    });
}