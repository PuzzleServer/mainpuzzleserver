// Write your Javascript code.
document.addEventListener("DOMContentLoaded", e => {
    document.querySelectorAll("time").forEach(t => {
        var utctimeval = t.dateTime;
        var date = new Date(utctimeval);
        var dateFormat = t.getAttribute("data-dateFormat");
        t.textContent = (dateFormat == "M/dd" ? date.toLocaleString(undefined, { month: "numeric", day: "2-digit" }) : date.toLocaleString());
    });
    document.body.offsetHeight;
});