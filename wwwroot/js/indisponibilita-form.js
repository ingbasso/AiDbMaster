(function () {
    "use strict";

    function aggiornaTipo() {
        var isMezzo = document.getElementById("tipoMezzo").checked;

        var bloccoMezzo = document.getElementById("bloccoMezzo");
        var bloccoAutista = document.getElementById("bloccoAutista");
        if (bloccoMezzo) bloccoMezzo.style.display = isMezzo ? "" : "none";
        if (bloccoAutista) bloccoAutista.style.display = isMezzo ? "none" : "";

        // Mostra solo le causali pertinenti al tipo selezionato
        var optMezzo = document.getElementById("optgroupMezzo");
        var optAutista = document.getElementById("optgroupAutista");
        if (optMezzo) optMezzo.style.display = isMezzo ? "" : "none";
        if (optAutista) optAutista.style.display = isMezzo ? "none" : "";

        // Se la causale selezionata appartiene all'altro gruppo, azzerala
        var sel = document.getElementById("selCausale");
        if (sel && sel.selectedOptions.length) {
            var parent = sel.selectedOptions[0].parentElement;
            if (parent && parent.id) {
                var causaleErrata = (isMezzo && parent.id === "optgroupAutista") ||
                                    (!isMezzo && parent.id === "optgroupMezzo");
                if (causaleErrata) sel.value = "";
            }
        }
    }

    function aggiornaFascia() {
        var chk = document.getElementById("chkGiornoIntero");
        var giornoIntero = chk ? chk.checked : true;
        document.querySelectorAll(".fascia-oraria").forEach(function (el) {
            el.style.display = giornoIntero ? "none" : "";
        });
    }

    document.addEventListener("DOMContentLoaded", function () {
        var radios = document.querySelectorAll('input[name="Tipo"]');
        radios.forEach(function (r) { r.addEventListener("change", aggiornaTipo); });

        var chk = document.getElementById("chkGiornoIntero");
        if (chk) chk.addEventListener("change", aggiornaFascia);

        aggiornaTipo();
        aggiornaFascia();
    });
})();
