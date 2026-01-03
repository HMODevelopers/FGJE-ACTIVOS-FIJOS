
$(".clean-filter").each(function (index, item) {
    if ($(item).prev().val().length > 0) {
        $(this).css({ display: "flex" });
    }
})

$(".filtrar")
    .change(function () {
        var vElement = $(this);
        if ($(vElement).val().length > 0) {
            $(vElement).siblings(".clean-filter").css({ display: "flex" });
        }
        else {
            $(vElement).siblings(".clean-filter").css({ display: "none" });
        }
        $("#ibox-index").children('.ibox-content').addClass('sk-loading');

        $(vElement).blur();

        setTimeout(function () {
            //$("#form0").submit();
            $(vElement).parents("form").submit();
        }, 200);
    })
    .keypress(function () {
        if (event.charCode == 13) {
            $("#ibox-index").children('.ibox-content').addClass('sk-loading');
            //$("#form0").submit();
            $(this).parents("form").submit();
        }
    })
    .keyup(function () {
        if ($(this).val().length > 0) {
            $(this).siblings(".clean-filter").css({ display: "flex" });
        }
        else {
            $(this).siblings(".clean-filter").css({ display: "none" });
        }
    })
    .click(function () {
        if ($(this).is(":button")) {
            $("#ibox-index").children('.ibox-content').addClass('sk-loading');
            //$("#form0").submit();
            $(this).parents("form").submit();
        }
    })

$(".clean-filter").click(function () {
    var $vElement = $(this).parent().find(".filtrar");
    $vElement.val("").trigger("change");
    if ($vElement.hasClass("select2")) {
        $vElement.select2("val", 0);
    }
})