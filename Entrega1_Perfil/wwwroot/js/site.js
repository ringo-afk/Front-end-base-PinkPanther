// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.


// cambiar foto de perfil
document.addEventListener("DOMContentLoaded", function () {

    const input = document.getElementById("inputFoto");
    const fotoPerfil = document.getElementById("fotoPerfil");
    const fotoNavbar = document.getElementById("fotoNavbar");

    if (input) {

        input.addEventListener("change", function (event) {

            const file = event.target.files[0];

            if (file) {

                const reader = new FileReader();

                reader.onload = function (e) {

                    // cambia la foto principal
                    fotoPerfil.src = e.target.result;

                    // cambia la foto del navbar
                    if (fotoNavbar) {
                        fotoNavbar.src = e.target.result;
                    }

                };

                reader.readAsDataURL(file);

            }

        });

    }

});

// contador de caracteres resumen

document.addEventListener("DOMContentLoaded", function () {

    const textarea = document.querySelector(".textarea-resumen");
    const contador = document.getElementById("contadorResumen");

    if (textarea && contador) {

        function actualizarContador() {

            const longitud = textarea.value.length;

            contador.textContent =
                longitud + " / 400 caracteres";

        }

        textarea.addEventListener("input", actualizarContador);

        actualizarContador();

    }

});