(function (global, $) {
    'use strict';

    function openPdfBlobFromBase64(base64) {
        if (!base64 || base64 === 'null') {
            return null;
        }

        var byteCharacters = atob(base64);
        var byteNumbers = new Array(byteCharacters.length);
        for (var i = 0; i < byteCharacters.length; i++) {
            byteNumbers[i] = byteCharacters.charCodeAt(i);
        }
        var byteArray = new Uint8Array(byteNumbers);
        var blob = new Blob([byteArray], { type: 'application/pdf' });
        var url = URL.createObjectURL(blob);
        window.open(url, '_blank');
        return url;
    }

    function openPdfFromUrl(url, data, options) {
        var settings = $.extend({
            method: 'POST',
            onError: null,
            onEmpty: null,
            onSuccess: null,
            base64Selector: null
        }, options || {});

        return $.ajax({
            url: url,
            type: settings.method,
            data: data,
            success: function (response) {
                var base64 = settings.base64Selector ? settings.base64Selector(response) : (response && response.PdfBase64);
                if (!base64 || base64 === 'null') {
                    if (settings.onEmpty) {
                        settings.onEmpty(response);
                    }
                    return;
                }

                openPdfBlobFromBase64(base64);

                if (settings.onSuccess) {
                    settings.onSuccess(response);
                }
            },
            error: function (error) {
                if (settings.onError) {
                    settings.onError(error);
                    return;
                }
                console.error('Error al generar o abrir el PDF.', error);
            }
        });
    }

    global.openPdfBlobFromBase64 = openPdfBlobFromBase64;
    global.openPdfFromUrl = openPdfFromUrl;
})(window, window.jQuery);
