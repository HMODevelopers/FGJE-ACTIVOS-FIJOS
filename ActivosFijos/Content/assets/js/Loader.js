

$.ajaxSetup({
    beforeSend: function (xhr) {
        ShowLoading();
    },
    complete: function () {
        HideLoading();
    }
});




function MyToast(vTitulo, vMensaje, vTipo, vTiempo) {
    vTiempo = vTiempo == undefined ? "5000" : vTiempo;
    toastr.options = {
        "closeButton": true,
        "debug": false,
        "progressBar": true,
        "preventDuplicates": true,
        "positionClass": "toast-top-right",//toast-top-right
        "onclick": null,
        "showDuration": "400",
        "hideDuration": "1000",
        "timeOut": vTiempo,
        "extendedTimeOut": "1000",
        "showEasing": "swing",
        "hideEasing": "linear",
        "showMethod": "slideDown",
        "hideMethod": "slideUp"
    }

    toastr[vTipo](vMensaje, vTitulo);
}

var ShowLoading = function () {
    $(".wrapper-loader").show();
    $(".wrapper-loader").addClass('show');
}

var HideLoading = function () {
    setTimeout(function () {
        $(".wrapper-loader").removeClass('show')
        setTimeout(function () {
            $(".wrapper-loader").hide();
        }, 200);
    }, 400)
}