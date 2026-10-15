// Autocompletado de datos vía Decolecta (DNI/RUC). Se activa desde botones
// con data-consulta="dni" o data-consulta="ruc" junto a un input del
// documento; rellena los campos indicados en data-destino (JSON) del propio
// botón. No hace nada si no encuentra los elementos en la página.
(function () {
    async function consultar(tipo, numero) {
        const resp = await fetch(`/api/consultas/${tipo}/${numero}`);
        if (!resp.ok) return { encontrado: false, error: 'No se pudo conectar con el servicio de consulta.' };
        const data = await resp.json();
        return {
            encontrado: data.encontrado,
            error: data.error,
            nombres: data.nombres,
            apellidoPaterno: data.apellidoPaterno,
            apellidoMaterno: data.apellidoMaterno,
            razonSocial: data.razonSocial,
            direccion: data.direccion,
            viaTipo: data.viaTipo,
            viaNombre: data.viaNombre,
            numero: data.numero,
            distrito: data.distrito,
            provincia: data.provincia,
            departamento: data.departamento,
            estado: data.estado,
            condicion: data.condicion,
            idDepartamentoSugerido: data.idDepartamentoSugerido,
            idProvinciaSugerido: data.idProvinciaSugerido,
            idDistritoSugerido: data.idDistritoSugerido
        };
    }

    function mostrarMensaje(el, texto, esError) {
        if (!el) return;
        el.textContent = texto;
        el.style.color = esError ? '#a4302f' : '#1f6b3a';
    }

    document.querySelectorAll('[data-consulta]').forEach(boton => {
        boton.addEventListener('click', async (e) => {
            e.preventDefault();
            const tipo = boton.getAttribute('data-consulta'); // "dni" | "ruc"
            const inputDocumentoId = boton.getAttribute('data-input');
            const mensajeId = boton.getAttribute('data-mensaje');
            const destino = JSON.parse(boton.getAttribute('data-destino') || '{}');

            const inputDocumento = document.getElementById(inputDocumentoId);
            const mensajeEl = mensajeId ? document.getElementById(mensajeId) : null;
            const numero = (inputDocumento?.value || '').trim();

            const longitudEsperada = tipo === 'dni' ? 8 : 11;
            if (numero.length !== longitudEsperada || !/^\d+$/.test(numero)) {
                mostrarMensaje(mensajeEl, `Ingresa un ${tipo.toUpperCase()} válido de ${longitudEsperada} dígitos.`, true);
                return;
            }

            const textoOriginal = boton.textContent;
            boton.disabled = true;
            boton.textContent = 'Buscando...';
            mostrarMensaje(mensajeEl, '', false);

            try {
                const resultado = await consultar(tipo, numero);

                if (!resultado.encontrado) {
                    mostrarMensaje(mensajeEl, resultado.error || 'No se encontró el documento.', true);
                    return;
                }

                Object.entries(destino).forEach(([campo, idInput]) => {
                    const input = document.getElementById(idInput);
                    if (input && resultado[campo] != null) input.value = resultado[campo];
                });

                // Si SUNAT trajo un distrito que además pudimos emparejar
                // contra nuestro catálogo de ubigeo, autoseleccionamos la
                // cascada Departamento -> Provincia -> Distrito del formulario.
                if (tipo === 'ruc' && resultado.idDistritoSugerido && window.Ubigeo) {
                    await window.Ubigeo.seleccionarCascada(
                        resultado.idDepartamentoSugerido,
                        resultado.idProvinciaSugerido,
                        resultado.idDistritoSugerido
                    );
                }

                if (tipo === 'ruc' && (resultado.direccion || resultado.distrito)) {
                    const partes = [resultado.direccion, resultado.distrito, resultado.provincia, resultado.departamento]
                        .filter(Boolean).join(', ');
                    const ubigeoOk = resultado.idDistritoSugerido
                        ? ' (ubigeo autoseleccionado)'
                        : ' (revisa el ubigeo manualmente: no se encontró una coincidencia exacta en el catálogo)';
                    mostrarMensaje(mensajeEl, `Encontrado. Dirección SUNAT: ${partes}${ubigeoOk}`, false);
                } else {
                    mostrarMensaje(mensajeEl, 'Datos encontrados y completados.', false);
                }
            } catch {
                mostrarMensaje(mensajeEl, 'Ocurrió un error al consultar el servicio.', true);
            } finally {
                boton.disabled = false;
                boton.textContent = textoOriginal;
            }
        });
    });
})();
