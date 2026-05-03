(function () {
    if (!window.signalR) {
        return;
    }

    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/hubs/messages")
        .withAutomaticReconnect()
        .build();

    connection.on("ReceiveMessage", function (message) {
        updateMessageBadge(message.unreadCount);
        prependInboxRow(message);
        showMessageToast(message);
    });

    connection.start().catch(function (error) {
        console.warn("Не вдалося підключитися до live-повідомлень.", error);
    });

    function updateMessageBadge(count) {
        const button = document.querySelector("[data-live-message-link]");
        if (!button) {
            return;
        }

        let badge = button.querySelector("[data-live-message-count]");
        if (count <= 0) {
            badge?.remove();
            return;
        }

        if (!badge) {
            badge = document.createElement("span");
            badge.className = "badge bg-red text-white badge-notification";
            badge.setAttribute("data-live-message-count", "true");
            button.appendChild(badge);
        }

        badge.textContent = count;
    }

    function prependInboxRow(message) {
        const tbody = document.querySelector("[data-live-inbox-body]");
        if (!tbody) {
            return;
        }

        document.querySelector("[data-live-inbox-empty]")?.remove();
        document.querySelector("[data-live-inbox-card]")?.classList.remove("d-none");

        const row = document.createElement("tr");
        row.className = "table-warning";
        row.innerHTML = `
            <td>${escapeHtml(message.subject)}</td>
            <td>${escapeHtml(message.senderName)}</td>
            <td>Нове</td>
            <td>${escapeHtml(message.sentAt)}</td>
            <td class="text-end">
                <a href="${message.detailsUrl}" class="btn btn-sm btn-outline-primary">Відкрити</a>
            </td>`;

        tbody.prepend(row);
    }

    function showMessageToast(message) {
        const container = getToastContainer();
        const toast = document.createElement("div");
        toast.className = "alert alert-info shadow-sm live-message-toast";
        toast.setAttribute("role", "alert");
        toast.innerHTML = `
            <div class="fw-semibold">Нове повідомлення від ${escapeHtml(message.senderName)}</div>
            <div>${escapeHtml(message.subject)}</div>
            <a href="${message.detailsUrl}" class="alert-link">Відкрити</a>`;

        container.appendChild(toast);
        window.setTimeout(function () {
            toast.remove();
        }, 8000);
    }

    function getToastContainer() {
        let container = document.querySelector("[data-live-message-toasts]");
        if (!container) {
            container = document.createElement("div");
            container.setAttribute("data-live-message-toasts", "true");
            container.className = "live-message-toast-container";
            document.body.appendChild(container);
        }

        return container;
    }

    function escapeHtml(value) {
        const div = document.createElement("div");
        div.textContent = value || "";
        return div.innerHTML;
    }
})();
