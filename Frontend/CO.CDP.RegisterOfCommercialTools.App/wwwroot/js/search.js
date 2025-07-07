(function() {
    function initialiseAccordion() {
        const accordionSection = document.getElementById('accordion-commercial-tool');

        if (!accordionSection) return;

        const accordionButton = accordionSection.getElementsByTagName('button')[0];
        const accordionContent = document.getElementById('accordion-commercial-content');

        if (!accordionButton || !accordionContent) return;

        const chevronIcon = accordionButton.getElementsByTagName('svg')[0];

        const initialExpanded = accordionButton.getAttribute('aria-expanded') === 'true';
        accordionContent.style.display = initialExpanded ? 'block' : 'none';

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

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initialiseAccordion);
    } else {
        initialiseAccordion();
    }
})();

