// Write your Javascript code.



(function ($) {

    $.extend({

        delay: function (callback, ms) {

            // now call a callback function
            if (typeof callback === 'function') {

                var clock = 0;
                clearTimeout(clock);
                clock = setTimeout(callback, ms);

            }

        }
    });

    $(document).ready(function () {

        //function getparameterbyname(name, url) {
        //    if (!url) url = window.location.href;
        //    name = name.replace(/[\[\]]/g, "\\$&");
        //    var regex = new regexp("[?&]" + name + "(=([^&#]*)|&|#|$)"),
        //        results = regex.exec(url);
        //    if (!results) return null;
        //    if (!results[2]) return '';
        //    return decodeuricomponent(results[2].replace(/\+/g, " "));
        //}

        $.ajaxSetup({
            // Disable caching of AJAX responses
            cache: false
        });

        // queue
        $.post("/queue/indexlist/", { check : false })
            .done(function (response) {
                $("#queueResult").html(response);
            });

        // teleprompt
        $.post("/teleprompt/indexlist/", { check: false })
            .done(function (response) {
                $("#telepromptResult").html(response);
         });

        // graduate
        $.post("/graduate/indexsearch/", { searchString: $("#searchString").val() })
            .done(function (response) {
                $("#searchResult").html(response);
            });

        //ajax call
        $("#searchString").keyup(function (e) {

            e.preventDefault();

            $.delay(function () {
                if ($("#searchString").val().length > 0) {
                    $.post("/graduate/indexsearch/", { searchString: $("#searchString").val() })
                        .done(function (response) {

                         $("#searchResult").html(response);
                     });
                }

            }, 500);
        });

        $(".searchText").autocomplete({
            source: function (request, response) {
                
                $.ajax({
                    url: "/scan/graduates",
                    dataType: "json",
                    data: {
                        term: request.term
                    },
                    success: function (data) {

                        if (data.length == 0) {
                            response(null);
                        }
                        else {
                            response($.map(data, function (item) {

                                return {
                                    label: item.text,
                                    value: item.value,
                                    id: item.value,
                                }
                            }));
                        }
                    },
                    error: function (x, y, z) {
                        alert('Error');
                    }

                });
            },
            select: function (event, ui) {
                console.log("Selected: " + ui.item.value + " aka " + ui.item.id);
            }
        });



        $("#setShowGraduates").on('click', function (e) {

            var setShowGrads = 0;
            if ($(this).is(':checked')) {
                setShowGrads = 1;
            }

            $.post("/config/SetShowGraduates/", { setShowGraduates: setShowGrads })
             .done(function (response) {
                 // change look
                 window.location = "/";
             });


        });

        var interval = 5000;

        // clamping
        var $div = $('#clamp');
        var x = 0;
        var y = 0;

        function refreshStuffs() {

            /**/
            // queue
            $.post("/queue/indexlist/", { check : true })
                 .done(function (response) {
                     if (response.length > 0) {

                         if (response !== $("#uniqueString").html()) {

                             // changed
                             // repopulate query data
                             $.post("/queue/indexlist/", { check: false })
                                 .done(function (resp) {
                                     if ($("#queueResult").length) {
                                         if (resp.length > 0) {
                                             $("#queueResult").html(resp.replace(/(\r\n|\n|\r)/gm, "").replace(/( \s*)/g, " ").replace(/(\s*<)/g, "<"));
                                         } else {
                                             $("#queueResult").html(null);
                                         }
                                     }
                                 });
                         }

                     } else {
                         $("#queueResult").html(null);
                     }
                 });

            // teleprompt
            $.post("/teleprompt/indexlist/", { check: true })
                .done(function (response) {
                    if (response.length > 0) {

                        if (response !== $("#uniqueString").html()) {

                            // changed
                            // repopulate query data
                            $.post("/teleprompt/indexlist/", { check: false })
                                .done(function (resp) {
                                    if ($("#telepromptResult").length) {
                                        if (resp.length > 0) {
                                            $("#telepromptResult").html(resp.replace(/(\r\n|\n|\r)/gm, "").replace(/( \s*)/g, " ").replace(/(\s*<)/g, "<"));
                                        } else {
                                            $("#telepromptResult").html(null);
                                        }
                                    }
                                    
                                });
                        }

                    } else {
                        $("#telepromptResult").html(null);
                    }
                });


            x++;
            if (x > $(window).width()) {
                x = 0;
                y++;
            }

            $div.stop(true, true).css({ "left": x, "top": y });

            setTimeout(refreshStuffs, interval);
        }

        refreshStuffs();

    });

})(jQuery);