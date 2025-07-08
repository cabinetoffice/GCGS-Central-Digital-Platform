document.addEventListener('DOMContentLoaded', function () {
    const modal = document.getElementById('cpv-code-modal');
    const openButton = document.getElementById('browse-cpv-codes-link');
    const closeButtons = modal.querySelectorAll('.govuk-modal__close-button');

    function openModal() {
        modal.hidden = false;
        trapFocus(modal);
    }

    function closeModal() {
        modal.hidden = true;
    }

    openButton.addEventListener('click', function (event) {
        event.preventDefault();
        openModal();
    });

    closeButtons.forEach(function (button) {
        button.addEventListener('click', closeModal);
    });

    document.addEventListener('keydown', function (event) {
        if (event.key === 'Escape' && !modal.hidden) {
            closeModal();
        }
    });

    function trapFocus(element) {
        const focusableElements = element.querySelectorAll('button, [href], input, select, textarea, [tabindex]:not([tabindex="-1"])');
        const firstFocusableElement = focusableElements[0];
        const lastFocusableElement = focusableElements[focusableElements.length - 1];

        element.addEventListener('keydown', function (e) {
            let isTabPressed = e.key === 'Tab' || e.keyCode === 9;

            if (!isTabPressed) {
                return;
            }

            if (e.shiftKey) {
                if (document.activeElement === firstFocusableElement) {
                    lastFocusableElement.focus();
                    e.preventDefault();
                }
            } else {
                if (document.activeElement === lastFocusableElement) {
                    firstFocusableElement.focus();
                    e.preventDefault();
                }
            }
        });

        firstFocusableElement.focus();
    }
});

