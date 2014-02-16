
; window.jQuery(function ($) {
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
    // blame page
    $('[data-brush]').each(function () {
        var $section = $(this),
            $blocks = $section.find('.no-highlight'),
            brush = $section.data('brush'),
            language = hljs.getLanguage(brush),
            code = '',
            queue = [];
        if (!language)
            return;
        $blocks.each(function () {
            var text = $(this).text();
            var numOfLines = text.split(/\r\n|\r|\n/).length;
            queue.push(numOfLines);
            code += text + '\n';
        });
        var lines = hljs.highlight(brush, code).value.split(/\r\n|\r|\n/);
        $blocks.each(function () {
            var $cell = $(this),
                num = queue.shift(),
                html = '';
            for (var i = 0; i < num; i++)
                html += (i ? '\n' : '') + lines.shift();
            $cell.html(html);
        });
    });
});
