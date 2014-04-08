
; (function ($, undefined) {
    'use strict';

    $.extend({
        queryString: {
            parse: function (str) {
                /*!
                    https://github.com/sindresorhus/query-string
                    by Sindre Sorhus
                    MIT License
                */
                var set = typeof str === 'undefined';

                if (set && $.queryString.parsed)
                    return $.queryString.parsed;

                var segments = (set ? window.location.search : str).replace(/^\?/, '').split('&');

                var ret = {};
                segments.forEach(function (param) {
                    var parts = param.replace(/\+/g, ' ').split('=');
                    var key = parts[0];
                    var val = parts[1];
                    if (!(key && val))
                        return;

                    key = decodeURIComponent(key);
                    // missing `=` should be `null`:
                    // http://w3.org/TR/2012/WD-url-20120524/#collect-url-parameters
                    val = typeof val === 'undefined' ? null : decodeURIComponent(val);

                    if (!ret.hasOwnProperty(key)) {
                        ret[key] = val;
                    } else if (Array.isArray(ret[key])) {
                        ret[key].push(val);
                    } else {
                        ret[key] = [ret[key], val];
                    }
                });

                var parsed = {
                    query: ret,
                    get: function (key, ignoreCase) {
                        var ign = !!ignoreCase;
                        if (ign)
                            key = key.toUpperCase();

                        var keys = Object.keys(ret);
                        for (var i in keys) {
                            var prop = keys[i];
                            if (ign && key === prop.toUpperCase()
                                || !ign && key === prop) {
                                return ret[prop];
                            }
                        }
                    }
                };

                if (set)
                    $.queryString.parsed = parsed;

                return parsed;
            }
        },
        stringFormat: function () {
            var args = arguments;
            if (args.length == 0 || typeof args[0] !== 'string')
                return '';
            return args[0].replace(/{(\d+)}/g, function (match, number) {
                var index = parseInt(number);
                return typeof args[index + 1] !== 'undefined'
                  ? args[index + 1]
                  : match;
            });
        }
    });

    $.queryString.query = $.queryString.parse().query;
    $.queryString.get = function (key, ignoreCase) {
        return $.queryString.parse().get(key, ignoreCase);
    };

})(window.jQuery);

; window.jQuery(function ($) {
    'use strict';

    $('#md').html(marked($('#md').text()));
    $('.focus :text').focus();
    $('.focus :text[name=query]').val($.queryString.get('query', true));
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
    // prevent all empty link
    $('a[href=#]').click(function () {
        event.preventDefault();
    });
    $('.branch-from, .branch-to').each(function () {
        var $con = $(this);
        $con.find('.tab-content li').click(function () {
            var $bn = $con.find('.branch-name'),
                $li = $(this);
            $bn.text($li.text());
            $bn.prev('i').attr('class',
                $li.closest('div').attr('id').indexOf('branch') == 0 ? 'icon-random' : 'icon-tag');
            $con.find('.dropdown').removeClass('open');
        });
    });
    $('.branch-compare').click(function () {
        var from = $('.branch-from .branch-name').text().replace(/\//g, ';'),
            to = $('.branch-to .branch-name').text().replace(/\//g, ';'),
            relative = from + '...' + to,
            base = window.location.href,
            seg = base.split('/'),
            last = seg[seg.length - 1],
            append = last != '' && last.indexOf('...') == -1;

        append
        ? window.location.href += '/' + relative
        : window.location.href = relative;
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
    // delete a branch
    $('[data-branch]').click(function () {
        var $this = $(this),
            name = $this.data('branch');
        if (confirm($.stringFormat(deleteBranch_params.words, name)))
        {
            $this.disabled = true;
            $.post(deleteBranch_params.url, { path: name }, function () {
                $this.closest('tr').remove();
            });
        }
    });
    // chooser
    typeof chooser_params !== 'undefined'
        && chooser_params instanceof Array
        && chooser_params.forEach(function (params) {
            /*
                params ->
                    data        -> data
                    controller  -> controller for anchor
                    container   -> container selector
                    add_label   -> add
                    del_label-> remove
                    add_action
                        -> [action object]
                    del_action
                        -> [action object]
                    action_array-> [] several objects
                        -> four pair and four objects with three name (checked, unchecked)
                        -> key
                        -> on_label
                        -> off_label
                        -> other each object
                            -> [action object]

                [action object]
                    -> url
                    -> query : function (item){} // return a json object such as {team: team, user: item, act: "del"}
            */
            var $container = $(params.container),
                $searcher = $('<input type="text" autocomplete="off">'),
                $add_btn = $('<button type="button" class="btn btn-primary">' + params.add_label + '</button>'),
                $alert_holder = $('<div class="span6 alert_placeholder">'),
                $grid = $('<div class="span10 offset1">');

            var warning = function (message) {
                $alert_holder.html('<div class="alert alert-error"><a class="close" data-dismiss="alert">&times;</a><span>' + message + '</span></div>')
            };
            var clearWarning = function () {
                $alert_holder.html('');
            };
            var parseResponseJson = function (text) {
                try {
                    return JSON.parse(text);
                } catch (e) {
                    return 'Connection error';
                }
            };
            var add_row = function (item) {
                var row_html =
                    '<div class="row border-area">'
                        + '<div class="span2"><a href="/' + params.controller + '/Detail/' + item.Name + '">' + item.Name + '</a></div>'
                    + '</div>',
                    $row = $(row_html);

                params.action_array.forEach(function (action) {
                    var cell_html =
                        '<div class="span2">'
                           + '<div class="switch switch-small" tabindex="0" data-on-label="' + action.on_label + '" data-off-label="' + action.off_label + '">'
                               + '<input type="checkbox"/>'
                           + '</div>'
                       + '</div>';

                    var $cell = $(cell_html),
                        $checkbox = $cell.find('input'),
                        $switcher = $cell.find('.switch');

                    $row.append($cell);

                    $checkbox.prop('checked', item[action.key]);
                    $checkbox.parent().bootstrapSwitch();

                    var tobe = null;
                    $switcher.on('switch-change', function (e, data) {
                        var value = data.value;
                        if (tobe == value) {
                            tobe = null;
                            return;
                        }
                        $switcher.bootstrapSwitch('setActive', false);
                        clearWarning();
                        var xhr = $.post(value ? action.checked.url : action.unchecked.url,
                            value ? action.checked.query(item.Name) : action.unchecked.query(item.Name),
                            function (data) { })
                        .fail(function () {
                            tobe = !value;
                            $switcher.bootstrapSwitch('setState', tobe);
                            warning(parseResponseJson(xhr.responseText));
                        })
                        .always(function () { $switcher.bootstrapSwitch('setActive', true); });
                    });
                });

                var remover_html =
                    '<div class="span1">'
                        + '<a href="#" class="remover">(' + params.del_label + ')</a>'
                    + '</div>',
                    $remover = $(remover_html);

                $row.append($remover);

                $remover.click(function () {
                    clearWarning();
                    var $mask = $('<div class="disable-mask"></div>');
                    $row.append($mask);
                    event.preventDefault();
                    var xhr = $.post(params.del_action.url, params.del_action.query(item.Name), function (data) {
                        $row.remove();
                    })
                    .fail(function () {
                        $mask.remove();
                        warning(parseResponseJson(xhr.responseText));
                    });
                });

                $grid.append($row);
            };
            $searcher.typeahead({
                source: function (query, process) {
                    return $.post('/' + params.controller + '/Search', { query: query }, function (data) {
                        return process(data);
                    });
                },
                items: 10
            });
            $add_btn.click(function () {
                clearWarning();
                var name = $searcher.val();
                var xhr = $.post(params.add_action.url, params.add_action.query(name), function (data) {
                    add_row($.extend({}, { Name: name }, data));
                    $searcher.val('');
                })
                .fail(function () {
                    warning(parseResponseJson(xhr.responseText));
                });
            });

            var $row = $('<div class="row">');

            $row.append($searcher.wrap('<div class="span3">').parent());
            $row.append($add_btn.wrap('<div class="span2">').parent());

            $container.append($row);
            $container.append($alert_holder.wrap('<div class="row">').parent());
            $container.append($grid.wrap('<div class="row">').parent());

            params.data.forEach(function (item) { add_row(item) });
        });
});
