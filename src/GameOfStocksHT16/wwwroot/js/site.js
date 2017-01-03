// Write your Javascript code.
$(function () {
    if ($('#user-money').length) {
        $.get("http://gameofstocksht16.azurewebsites.net/api/users/getmoney", function (data) {
            $('#user-money').html(data);
        });
    }
});