// Combos en cascada Departamento -> Provincia -> Distrito.
// Busca los <select> por convención de IDs (los que genera el TagHelper
// asp-for de ASP.NET Core para una propiedad anidada "Direccion.IdX"):
//   Direccion_IdDepartamento, Direccion_IdProvincia, Direccion_IdDistrito
window.Ubigeo = (function () {
    const idDep = 'Direccion_IdDepartamento';
    const idProv = 'Direccion_IdProvincia';
    const idDist = 'Direccion_IdDistrito';

    function llenarSelect(select, items, valueField, textField, placeholder) {
        select.innerHTML = '';
        const opcionVacia = document.createElement('option');
        opcionVacia.value = '';
        opcionVacia.textContent = placeholder;
        select.appendChild(opcionVacia);
        items.forEach(item => {
            const opt = document.createElement('option');
            opt.value = item[valueField];
            opt.textContent = item[textField];
            select.appendChild(opt);
        });
    }

    async function cargarDepartamentos() {
        const select = document.getElementById(idDep);
        if (!select) return;
        const resp = await fetch('/api/ubigeo/departamentos');
        const data = await resp.json();
        llenarSelect(select, data, 'idDepartamento', 'nombre', '-- Departamento --');
    }

    async function cargarProvincias(idDepartamento, seleccionar) {
        const select = document.getElementById(idProv);
        if (!select) return;
        if (!idDepartamento) { llenarSelect(select, [], null, null, '-- Provincia --'); return; }
        const resp = await fetch(`/api/ubigeo/provincias/${idDepartamento}`);
        const data = await resp.json();
        llenarSelect(select, data, 'idProvincia', 'nombre', '-- Provincia --');
        if (seleccionar) select.value = String(seleccionar);
    }

    async function cargarDistritos(idProvincia, seleccionar) {
        const select = document.getElementById(idDist);
        if (!select) return;
        if (!idProvincia) { llenarSelect(select, [], null, null, '-- Distrito --'); return; }
        const resp = await fetch(`/api/ubigeo/distritos/${idProvincia}`);
        const data = await resp.json();
        llenarSelect(select, data, 'idDistrito', 'nombre', '-- Distrito --');
        if (seleccionar) select.value = String(seleccionar);
    }

    function inicializar() {
        const selDep = document.getElementById(idDep);
        const selProv = document.getElementById(idProv);
        if (!selDep) return; // esta vista no tiene bloque de dirección

        cargarDepartamentos();

        selDep.addEventListener('change', () => {
            cargarProvincias(selDep.value || null);
            cargarDistritos(null);
        });
        selProv.addEventListener('change', () => {
            cargarDistritos(selProv.value || null);
        });
    }

    // Llamado desde consultas.js cuando SUNAT sugiere un distrito ya
    // emparejado contra nuestro catálogo (ver ConsultasController.Ruc).
    async function seleccionarCascada(idDepartamento, idProvincia, idDistrito) {
        const selDep = document.getElementById(idDep);
        if (!selDep) return;

        await cargarDepartamentos();
        selDep.value = String(idDepartamento);
        await cargarProvincias(idDepartamento, idProvincia);
        await cargarDistritos(idProvincia, idDistrito);
    }

    // Se llama directamente (no se espera a DOMContentLoaded): este script
    // se carga al final del <body>, así que cuando esta línea se ejecuta el
    // DOM ya está completo — esperar ese evento aquí significaba que nunca
    // se disparaba (ya había pasado), y por lo tanto los combos de
    // Departamento/Provincia/Distrito nunca se llenaban al cargar la página.
    inicializar();

    return { seleccionarCascada };
})();
