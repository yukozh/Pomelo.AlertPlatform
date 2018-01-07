$(document).ready(function () {
    $('.nested').hide();
    $('.active-uplevel').click(function (e) {
        var group = $(this).attr('data-group');
        var s = $(this).attr('data-switch');
        if (s === 'on') {
            $(e.target).attr('data-switch', 'off');
            $('.nested[data-group="' + group + '"]').hide();
        } else {
            $(e.target).attr('data-switch', 'on');
            $('.nested[data-group="' + group + '"]').show();
        }
    });

    var activeGroup = $('.nested.active').attr('data-group');
    $('.active-uplevel[data-group="' + activeGroup + '"]').click();
});