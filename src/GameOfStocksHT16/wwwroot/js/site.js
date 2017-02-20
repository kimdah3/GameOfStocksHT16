// Write your Javascript code.
$(function () {
    if ($('#user-money').length) {
        console.log(window.location.protocol);
        $.get("../api/users/getmoney", function (data) {
            $('#user-money').html("Balance: "+data + " kr");
        });
    }
});