(function() {
    function initialiseAccordion(accordionSection) {
        if (!accordionSection) return;

        const accordionButton = accordionSection.querySelector('[data-accordion-button]');
        const accordionContent = accordionSection.querySelector('[data-accordion-content]');

        if (!accordionButton || !accordionContent) return;

        const chevronIcon = accordionButton.querySelector('svg');

        const initialExpanded = accordionButton.getAttribute('aria-expanded') === 'true';
        accordionContent.style.display = initialExpanded ? 'block' : 'none';
        if (chevronIcon) {
            chevronIcon.style.transform = initialExpanded ? 'rotate(180deg)' : 'rotate(0deg)';
        }

        accordionButton.addEventListener('click', function(event) {
            event.preventDefault();

            const expanded = this.getAttribute('aria-expanded') === 'true';
            const newExpandedState = !expanded;

            this.setAttribute('aria-expanded', newExpandedState);
            accordionContent.style.display = newExpandedState ? 'block' : 'none';

            if (chevronIcon) {
                chevronIcon.style.transform = newExpandedState ? 'rotate(180deg)' : 'rotate(0deg)';
            }
        });
    }

    function initialiseAllAccordions() {
        const accordionSections = document.querySelectorAll('[data-accordion-section]');
        accordionSections.forEach(initialiseAccordion);
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initialiseAllAccordions);
    } else {
        initialiseAllAccordions();
    }
})();
