
$(document).ready(function () {
    var MAX_INPUTTEXT_LENGTH = 100000;

    var textOnChange = function () {
        var _len = $("#text").val().length; 
        var len = _len.toString().replace(/\B(?=(\d{3})+(?!\d))/g, " ");
        var $textLength = $("#textLength");
        $textLength.html("длина текста: " + len + " символов");
        if (MAX_INPUTTEXT_LENGTH < _len) $textLength.addClass("max-inputtext-length");
        else                             $textLength.removeClass("max-inputtext-length");
    };
    var getText = function ($text) {
        var text = trim_text($text.val().toString());
        if (is_text_empty(text)) {
            alert("Введите текст для обработки.");
            $text.focus();
            return (null);
        }

        if (text.length > MAX_INPUTTEXT_LENGTH) {
            if (!confirm('Превышен рекомендуемый лимит ' + MAX_INPUTTEXT_LENGTH + ' символов (на ' + (text.length - MAX_INPUTTEXT_LENGTH) + ' символов).\r\nТекст будет обрезан, продолжить?')) {
                return (null);
            }
            text = text.substr(0, MAX_INPUTTEXT_LENGTH);
            $text.val(text);
            $text.change();
        }
        return (text);
    };

    $("#text").focus(textOnChange).change(textOnChange).keydown(textOnChange).keyup(textOnChange).select(textOnChange).focus();

    $('#mainPageContent').on('click', '#processButton', function () {
        if($(this).hasClass('disabled')) return (false);

        var text = getText( $("#text") );
        if (!text) return (false);

        processing_start();

        $.ajax({
            type: "POST",
            url:  "RESTProcessHandler.ashx",
            data: {
                splitBySmiles: true,
                returnText   : true,
                text         : text
            },
            success: function (responce) {
                if (responce.err) {
                    if (responce.err == "goto-on-captcha") {
                        window.location.href = "Captcha.aspx";
                    } else {
                        processing_end();
                        $('.result-info').addClass('error').text(responce.err);
                    }
                } else {
                    if (responce.sents && responce.sents.length != 0) {
                        var result_text = '',
                            $table = $('#processResult tbody');
                            $('.result-info').removeClass('error').text(''); 
                        for (var i = 0, len = responce.sents.length; i < len; i++) {
                            var sent = responce.sents[ i ];
                            result_text += '<tr><td>' + (i + 1) + '.</td><td>' + sent.t + '</td></tr>';
                        }
                        $table.html( result_text );
                        processing_end();
                    } else {
                        processing_end();
                        $('.result-info').text('Границы предложений в тексте не определены');
                    }
                }
            },
            error: function () {
                processing_end();
                $('.result-info').text('ошибка сервера');
            }
        });
        
    });

    function processing_start(){
        $('#text').addClass('no-change').attr('readonly', 'readonly').attr('disabled', 'disabled');
        $('.result-info').removeClass('error').text('Идет обработка...');
        $('#processButton').addClass('disabled');
        $('#processResult tbody').empty();
    };
    function processing_end(){
        $('#text').removeClass('no-change').removeAttr('readonly').removeAttr('disabled');
        $('.result-info').removeClass('error').text('');
        $('#processButton').removeClass('disabled');
    };
    function trim_text(text) {
        return (text.replace(/(^\s+)|(\s+$)/g, ""));
    };
    function is_text_empty(text) {
        return (text.replace(/(^\s+)|(\s+$)/g, "") == "");
    };
});