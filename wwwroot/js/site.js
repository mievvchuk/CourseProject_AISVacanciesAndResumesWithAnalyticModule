document.addEventListener('click', (event) => {
    const toggle = event.target.closest('[data-password-toggle]');
    if (!toggle) {
        return;
    }

    const group = toggle.closest('.input-group');
    const input = group?.querySelector('[data-password-toggle-input]');
    const icon = toggle.querySelector('i');

    if (!input) {
        return;
    }

    const isHidden = input.type === 'password';
    input.type = isHidden ? 'text' : 'password';
    toggle.setAttribute('aria-label', isHidden ? 'Сховати пароль' : 'Показати пароль');
    toggle.setAttribute('title', isHidden ? 'Сховати пароль' : 'Показати пароль');

    if (icon) {
        icon.classList.toggle('ti-eye', !isHidden);
        icon.classList.toggle('ti-eye-off', isHidden);
    }
});

document.addEventListener('DOMContentLoaded', () => {
    const modalElement = document.getElementById('app-confirm-modal');
    const titleElement = document.getElementById('app-confirm-title');
    const messageElement = document.getElementById('app-confirm-message');
    const submitButton = document.getElementById('app-confirm-submit');

    if (!modalElement || !titleElement || !messageElement || !submitButton || !window.bootstrap) {
        return;
    }

    const modal = new bootstrap.Modal(modalElement);
    let pendingForm = null;

    document.addEventListener('submit', (event) => {
        const form = event.target.closest('form[data-confirm]');
        if (!form || form.dataset.confirmed === 'true') {
            return;
        }

        event.preventDefault();
        pendingForm = form;
        titleElement.textContent = form.dataset.confirmTitle || 'Підтвердити дію?';
        messageElement.textContent = form.dataset.confirmMessage || 'Цю дію потрібно підтвердити.';
        submitButton.textContent = form.dataset.confirmSubmit || 'Підтвердити';
        submitButton.classList.remove('btn-danger', 'btn-success', 'btn-primary', 'btn-warning');
        submitButton.classList.add(`btn-${form.dataset.confirmVariant || 'danger'}`);
        modal.show();
    });

    submitButton.addEventListener('click', () => {
        if (!pendingForm) {
            return;
        }

        pendingForm.dataset.confirmed = 'true';
        modal.hide();
        pendingForm.requestSubmit();
        pendingForm = null;
    });

    modalElement.addEventListener('hidden.bs.modal', () => {
        pendingForm = null;
        submitButton.textContent = 'Підтвердити';
        submitButton.classList.remove('btn-success', 'btn-primary', 'btn-warning');
        submitButton.classList.add('btn-danger');
    });
});
