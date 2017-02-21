// Write your Javascript code.
$(function () {
    if ($('#user-money').length) {
        $.get("../api/users/getmoney", function (data) {
            $('#user-money').append(data);
        });
    }
});