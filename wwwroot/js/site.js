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
