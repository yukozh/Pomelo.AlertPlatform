$(document).ready(function () {
    $('.nested').hide();
    $(window).click(function (e) {
        if ($(e.target).hasClass('active-uplevel') || $(e.target).parents('.active-uplevel').length) {
            var dom = $(e.target).hasClass('active-uplevel') ? $(e.target) : $(e.target).parents('.active-uplevel');
            console.log(dom);
            var group = dom.attr('data-group');
            var s = dom.attr('data-switch');
            if (s === 'on') {
                dom.attr('data-switch', 'off');
                $('.nested[data-group="' + group + '"]').hide();
            } else {
                dom.attr('data-switch', 'on');
                $('.nested[data-group="' + group + '"]').show();
            }
        }
    });

    var activeGroup = $('.nested.active').attr('data-group');
    $('.active-uplevel[data-group="' + activeGroup + '"]').click();
});