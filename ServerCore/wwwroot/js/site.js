// Write your Javascript code.
window.renderLocalTimes = function(root) {
    (root || document).querySelectorAll("time").forEach(t => {
        var utctimeval = t.dateTime;
        var date = new Date(utctimeval);
        var dateFormat = t.getAttribute("data-dateFormat");
        t.textContent = (dateFormat == "M/dd" ? date.toLocaleString(undefined, { month: "numeric", day: "2-digit" }) : date.toLocaleString());
    });
}

document.addEventListener("DOMContentLoaded", e => {
    window.renderLocalTimes();
    document.body.offsetHeight;
});