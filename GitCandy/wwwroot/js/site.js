;
hljs.initHighlightingOnLoad();

window.jQuery(function ($) {
    'use strict';

    $('#md').html(marked($('#md').text()));
});
