// Campana de notificaciones:
// obtiene el número de notificaciones pendientes
// y recibe notificaciones nuevas mediante SignalR.
(function () {
    const btn = document.getElementById('btnNotificaciones');

    if (!btn) {
        return;
    }

    const badge = document.getElementById('badgeNotificaciones');

    function actualizarBadge(cantidad) {
        if (!badge) {
            return;
        }

        if (cantidad > 0) {
            badge.hidden = false;
            badge.textContent = cantidad > 9
                ? '9+'
                : cantidad;
        } else {
            badge.hidden = true;
            badge.textContent = '0';
        }
    }

    async function cargarNoLeidas() {
        try {
            const resp = await fetch(
                '/api/notificaciones?soloNoLeidas=true'
            );

            if (!resp.ok) {
                return;
            }

            const data = await resp.json();

            actualizarBadge(data.length);

            return data.length;
        } catch (error) {
            console.error(
                'No se pudieron cargar las notificaciones:',
                error
            );
        }
    }

    function mostrarToast(mensaje) {
        const toast = document.createElement('div');

        toast.textContent = mensaje;

        toast.style.cssText = [
            'position:fixed',
            'right:24px',
            'bottom:24px',
            'max-width:320px',
            'background:#0c3855',
            'color:#fff',
            'padding:14px 18px',
            'border-radius:14px',
            'font-family:Poppins,sans-serif',
            'font-size:0.85rem',
            'box-shadow:0 20px 40px -20px rgba(0,0,0,0.5)',
            'z-index:9999',
            'opacity:0',
            'transition:opacity .3s ease'
        ].join(';');

        document.body.appendChild(toast);

        requestAnimationFrame(() => {
            toast.style.opacity = '1';
        });

        setTimeout(() => {
            toast.style.opacity = '0';

            setTimeout(() => {
                toast.remove();
            }, 300);
        }, 6000);
    }

    cargarNoLeidas();

    if (window.signalR) {
        const connection = new signalR.HubConnectionBuilder()
            .withUrl('/hubs/notificaciones')
            .withAutomaticReconnect()
            .build();

        connection.on(
            'NuevaNotificacion',
            function (notificacion) {
                cargarNoLeidas();

                if (
                    notificacion &&
                    notificacion.mensaje
                ) {
                    mostrarToast(
                        notificacion.mensaje
                    );
                }
            }
        );

        connection.start()
            .catch(error => {
                console.error(
                    'Error de conexión con SignalR:',
                    error
                );
            });
    }
})();