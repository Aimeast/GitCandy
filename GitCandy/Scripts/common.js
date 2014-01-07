
$(function () {
    $('#md').html(marked($('#md').text()));
    $('.focus :text').focus();
    $('.copy-clip').each(function () {
        var $clip = $(this);
        $clip.parent().children(':text').click(function () { $(this).select(); });
        var zero = new ZeroClipboard($clip, {
            moviePath: '/Content/ZeroClipboard.swf',
            //useNoCache: false, // Be careful!! Not supported by IE?
        });
        var $tip = $(zero.htmlBridge);
        zero.on('complete', function () {
            $tip.tooltip('destroy').tooltip({ title: $clip.data('copied'), placement: 'right' }).tooltip('show');
        })
        .on('mouseover', function () {
            $tip.tooltip('destroy').tooltip({ title: $clip.data('tips'), placement: 'right' }).tooltip('show');
        })
        .on('noflash wrongflash', function () {
            console.error('No flash or wrong flash version');
        });
    });
})
