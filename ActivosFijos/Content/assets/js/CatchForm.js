$(document).ready(function () {
    loadLightboxUI();

    $("body").on('click', 'button[data-ajax="true"]', function () {
        var button = $(this);
        var form = button.closest("form");
        var url = button.data('delete') ? button.data('url') : form.attr('action');

        if (!confirmAction(button)) return;

        if (!form.valid()) return;

        var block = $('<div class="block-loading" />');
        form.prepend(block);

        $(".alert", form).remove();

        form.ajaxSubmit({
            dataType: 'JSON',
            type: 'POST',
            url: url,
            success: function (response) {
                handleSuccess(response, form, button);
            },
            error: function (jqXHR, textStatus, errorThrown) {
                handleAjaxError(jqXHR, textStatus, errorThrown, form);
            },
            complete: function () {
                block.remove();
            }
        });

        return false;
    });
});

function loadLightboxUI() {
    $("#mdb-lightbox-ui").load("mdb-addons/mdb-lightbox-ui.html");
}

function confirmAction(button) {
    var confirmMessage = button.data('confirm');
    if (confirmMessage === undefined) return true;
    return confirm(confirmMessage);
}

function handleSuccess(response, form, button) {

    if (response.message) {
        showAlert(response);
    }

    if (response.response && button.data('reset')) {
        form[0].reset();
    } else {
        form.find('input:file').val('');
    }

    if (response.function) {
        setTimeout(response.function, 0);
    }

}

function showAlert(response) {
    var cssClass = response.response ? 'success' : 'error';
    var title = response.response ? '¡Buen Trabajo!' : 'Error';
    var icon = response.response ? 'success' : 'error';

    Swal.fire({
        title: title,
        text: response.message,
        type: icon
    }).then((result) => {
        redirect(response.href)
    });
}

function handleAjaxError(jqXHR, textStatus, errorThrown, form) {
    form.prepend('<div class="alert alert-warning alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>' + errorThrown + ' | <b>' + textStatus + '</b></div>');
}

function redirect(href) {
    if (href === 'self') {
        window.location.reload(true);
    } else {
        window.location.href = href;
    }
}
