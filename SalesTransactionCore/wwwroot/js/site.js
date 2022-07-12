// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

$(function () {
    $("#loaderbody").addClass('hide');

    $(document).bind('ajaxStart', function () {
        $("#loaderbody").removeClass('hide');
    }).bind('ajaxStop', function () {
        $("#loaderbody").addClass('hide');
    });
});

showInPopup = (url, title) => {
    $.ajax({
        type: 'GET',
        url: url,
        success: function (res) {
            $('#form-modal .modal-body').html(res);
            $('#form-modal .modal-title').html(title);
            $('#form-modal').modal('show');
        }
    })
}
//Invoice
showInPopupInvoice = (url, title) => {
    $.ajax({
        type: 'GET',
        url: url,
        success: function (res) {
            $('#form-modal1 .modal-body').html(res);
            $('#form-modal1 .modal-title').html(title);
            $('#form-modal1').modal('show');
        }
    })
}


//$(document).ready(function () {

//    $('.modal').on('hidden.bs.modal', function (event) {
//        $(this).removeClass('fv-modal-stack');
//        $('body').data('fv_open_modals', $('body').data('fv_open_modals') - 1);
//    });

//    $('.modal').on('shown.bs.modal', function (event) {
//        // keep track of the number of open modals
//        if (typeof ($('body').data('fv_open_modals')) == 'undefined') {
//            $('body').data('fv_open_modals', 0);
//        }

//        // if the z-index of this modal has been set, ignore.
//        if ($(this).hasClass('fv-modal-stack')) {
//            return;
//        }

//        $(this).addClass('fv-modal-stack');
//        $('body').data('fv_open_modals', $('body').data('fv_open_modals') + 1);
//        $(this).css('z-index', 1040 + (10 * $('body').data('fv_open_modals')));
//        $('.modal-backdrop').not('.fv-modal-stack').css('z-index', 1039 + (10 * $('body').data('fv_open_modals')));
//        $('.modal-backdrop').not('fv-modal-stack').addClass('fv-modal-stack');

//    });
//});

//
jQueryAjaxPost = form => {
    debugger
    try {
        $.ajax({
            type: 'POST',
            url: form.action,
            data: new FormData(form),
            contentType: false,
            processData: false,
            success: function (res) {
                if (res.isValid) {
                    $('#view-all').html(res.html)
                    $('#form-modal .modal-body').html('');
                    $('#form-modal .modal-title').html('');
                    $('#form-modal').modal('hide');
                    debugger
                    $.notify('Created Successfully', { globalPosition: 'top center', className: 'success' });
                }
                else
                    $('#form-modal .modal-body').html(res.html);
            },
            error: function (err) {
                console.log(err)
            }
        })
        //to prevent default form submit event
        return false;
    } catch (ex) {
        console.log(ex)
    }
}

jQueryAjaxDelete = form => {
    debugger;
    if (confirm("Are you sure want to delete this record")) {
        try {
            debugger
            $.ajax({             
                type: 'POST',
                url: form.action,
                data: new FormData(form),
                contentType: false,
                processData: false,
                success: function (res) {
                    debugger;
                    $('#view-all').html(res.html)                   
                    $.notify('Deleted Successfully', { globalPosition: 'top center', className: 'success' });
                    location.reload();
                    debugger;
                },
                error: function (err) {
                    console.log(err)
                }
            })
        }
        catch (e) {
            console.log(e)
        }
    }
    //to prevent default form submit event
    return false;
}
