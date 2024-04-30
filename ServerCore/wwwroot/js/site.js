// Write your Javascript code.
document.addEventListener("DOMContentLoaded", e => {
    document.querySelectorAll("time").forEach(t => {
        var utctimeval = t.innerText;
        var date = new Date(utctimeval);
        t.innerText = date.toLocaleString();
    });
});