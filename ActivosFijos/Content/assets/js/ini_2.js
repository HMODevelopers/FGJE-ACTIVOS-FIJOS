$(document).ready(function () {
    // Evento para deshabilitar el botón si no se selecciona ningún archivo
    $("#archivo").change(function () {
        $("#enviar-registro").prop("disabled", this.files.length == 0);
    });

    // Cargar el archivo mdb-lightbox-ui.html al elemento #mdb-lightbox-ui
    $("#mdb-lightbox-ui").load("mdb-addons/mdb-lightbox-ui.html");

    // Evento click para botones con atributo data-ajax
    $("body").on('click', 'button[data-ajax="true"]', function () {
        var button = $(this);
        var form = button.closest("form");
        var url = button.data('url') || form.attr('action');

        if (button.data('confirm') && !confirm(button.data('confirm'))) return false;

        if (button.data('delete')) {
            url = button.data('url');
        } else {
            if (!form.valid()) {
                return false;
            }
        }

        var block = $('<div class="block-loading" />').prependTo(form);
        $(".alert", form).remove();

        if (form.hasClass('CKupdate')) CKupdate();

        form.ajaxSubmit({
            dataType: 'JSON',
            type: 'POST',
            url: url,
            success: function (r) {
                block.remove();
                handleResponse(r, form, button);
            },
            error: function (jqXHR, textStatus, errorThrown) {
                block.remove();
                form.prepend('<div class="alert alert-warning alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>' + errorThrown + ' | <b>' + textStatus + '</b></div>');
            }
        });

        return false;
    });

    // Función para manejar la respuesta del servidor
    function handleResponse(response, form, button) {
        console.log(response.response);
        if (response.response) {
            if (!button.data('reset')) {
                form[0].reset();
            } else {
                form.find('input:file').val('');
            }
        }

        if (response.message) {
            var css = response.response ? "alert-success" : "alert-danger";
            var titulo = response.response ? "¡Buen Trabajo!" : "Error";
            var message = '<div class="alert ' + css + ' alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>' + response.message + '</div>';
            form.prepend(message);
        }

        if (response.function) {
            setTimeout(response.function, 0);
        }

        if (response.href) {
            if (response.href == 'self') {
                window.location.reload(true);
            } else {
                window.location.href = response.href;
            }
        }
    }
});

// Método para restablecer el formulario
jQuery.fn.reset = function () {
    $("input:password, input:file, input:text, textarea", $(this)).val('');
    $("input:checkbox:checked", $(this)).click();
    $("select", $(this)).each(function () {
        $(this).val($(this).find("option:first").val());
    });
};
