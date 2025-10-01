document.addEventListener("DOMContentLoaded", function () {
    const accordionControls = document.querySelectorAll("[data-accordion-section]");
    const storageKey = "accordionStates";

    const states = JSON.parse(sessionStorage.getItem(storageKey)) || {};

    accordionControls.forEach(control => {
        const button = control.querySelector("[data-accordion-button]");
        const content = control.querySelector("[data-accordion-content]");
        const accordionSection = control.querySelector(".govuk-accordion__section");
        const input = control.querySelector('[data-accordion-input]');

        if (!button || !content || !input || !accordionSection) {
            return;
        }

        const id = content.id.replace("-content", "");

        if (states[id] === undefined) {
            states[id] = accordionSection.classList.contains("govuk-accordion__section--expanded");
        }

        const isExpanded = states[id];
        button.setAttribute("aria-expanded", isExpanded);
        if (isExpanded) {
            accordionSection.classList.add("govuk-accordion__section--expanded");
        } else {
            accordionSection.classList.remove("govuk-accordion__section--expanded");
        }
        input.value = isExpanded ? id : "";

        button.addEventListener("click", function () {
            const wasExpanded = this.getAttribute("aria-expanded") === "true";
            const isNowExpanded = !wasExpanded;

            this.setAttribute("aria-expanded", isNowExpanded);

            if (isNowExpanded) {
                accordionSection.classList.add("govuk-accordion__section--expanded");
                input.value = id;
            } else {
                accordionSection.classList.remove("govuk-accordion__section--expanded");
                input.value = "";
            }

            states[id] = isNowExpanded;
            sessionStorage.setItem(storageKey, JSON.stringify(states));
        });
    });

    sessionStorage.setItem(storageKey, JSON.stringify(states));
});