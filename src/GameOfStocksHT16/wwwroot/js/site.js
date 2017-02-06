// Write your Javascript code.
$(function () {
    if ($('#user-money').length) {
        console.log(window.location.protocol);
        $.get("http://localhost:49275/api/users/getmoney", function (data) {
            $('#user-money').html("Balance: "+data + " kr");
        });
    }
});