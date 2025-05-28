// Write your Javascript code.
document.addEventListener("DOMContentLoaded", e => {
    document.querySelectorAll("time").forEach(t => {
        var utctimeval = t.dateTime;
        var date = new Date(utctimeval);
        t.textContent = date.toLocaleString();
    });
    document.body.offsetHeight;
});