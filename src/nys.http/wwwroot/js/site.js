// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

$(document).ajaxStop(function () { $("#loadMore").text("Get more").prop("disabled", false); });
$(document).ajaxStart(function () { $("#loadMore").text("We are thinking...").prop("disabled", true); });

$(document).ready(function () {
    load();
    $("#loadMore").click(function() {
        load();
    });
});

function load() {
    $(".items-container").empty();


    $.ajax({
        url: "/api/v1/names?count=5",
        success: function(data){
            $.each(data, function( index, value ) {
                $(".items-container").append("<p class='lead'>www.<b class='suggestion'>" + value + "</b></p>");
            });
            
        },
        cache: false});
    
}