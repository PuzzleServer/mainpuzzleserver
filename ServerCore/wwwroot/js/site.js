// Write your Javascript code.
$("time").each(function (elem) {
    var utctimeval = $(this).html();
    var date = new Date(utctimeval);
    $(this).html(date.toLocaleString());
});