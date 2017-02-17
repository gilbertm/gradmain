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


        var url;

        // queue
        // url = "/queue/indexlist/";
        // $.post(url, { })
        //    .done(function (response) {
        //         $("#queueResult").html(response);
        //     });

        // teleprompt
        // url = "/teleprompt/indexlist/";
        // $.post(url, { })
        //     .done(function (response) {
        //         $("#telepromptResult").html(response);
        //    });

        // graduate
        url = "/graduate/indexsearch/";
        // $.post(url, { searchString: $("#searchString").val(), voicePerson: getParameterByName('voiceperson'), voiceExtra: getParameterByName('voiceextra') })
        $.post(url, { searchString: $("#searchString").val() })
            .done(function (response) {
                $("#searchResult").html(response);
            });


        //ajax call
        $("#searchString").keyup(function (e) {

            e.preventDefault();

            $.delay(function () {

                url = "/graduate/indexsearch/";
                if ($("#searchString").val().length > 0) {
                    $.post(url, { searchString: $("#searchString").val() })
                        .done(function (response) {

                         $("#searchResult").html(response);
                     });
                }

            }, 500);
        });



        $("#setShowGraduates").click(function (e) {

            var url = "/config/SetShowGraduates/";

            var setShowGrads = 0;
            if ($(this).is(':checked')) {
                setShowGrads = 1;
            }

            $.post(url, { setShowGraduates: setShowGrads })
             .done(function (response) {
                 // change look
                 window.location = "/";
             });


        });

        var interval = 1000;

        // clamping
        var $div = $('#clamp');
        var x = 0;
        var y = 0;

        function refreshStuffs() {

            /**/
            // queue
            url = "/queue/indexlist/";
            $.post(url, {})
                 .done(function (response) {
                     if (response.length > 0) {

                         var htmlText = $("#queueResult").html().replace(/(\r\n|\n|\r)/gm, "").replace(/( \s*)/g, " ").replace(/(\s*<)/g, "<");
                         var respText = response.replace(/(\r\n|\n|\r)/gm, "").replace(/( \s*)/g, " ").replace(/(\s*<)/g, "<");
                         if (htmlText !== respText) {
                             $("#queueResult").html(respText);
                         }

                     } else {
                         // console.log("queue empty");
                         $("#queueResult").html(null);
                     }
                 });

            // teleprompt
            url = "/teleprompt/indexlist/";
            $.post(url, {})
                 .done(function (response) {
                     if (response.length > 0) {

                         var htmlText = $("#telepromptResult").html().replace(/(\r\n|\n|\r)/gm, "").replace(/( \s*)/g, " ").replace(/(\s*<)/g, "<");
                         var respText = response.replace(/(\r\n|\n|\r)/gm, "").replace(/( \s*)/g, " ").replace(/(\s*<)/g, "<");
                         if (htmlText !== respText) {
                             $("#telepromptResult").html(response);
                         }

                     } else {
                         // console.log("teleprompt empty");
                         $("#telepromptResult").html(null);
                     }
                 });

            /**/

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

    /* $(window).load(function () {
        $(".showToScreen").click(function () {
            $(this).find("img").animate({"width":"100%"});
        });
    }); */
})(jQuery);