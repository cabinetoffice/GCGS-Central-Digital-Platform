document.addEventListener("DOMContentLoaded", function () {
    const CpvSelector = (() => {
        const state = Object.freeze({
            selectedCodes: new Set(),
            modal: document.getElementById('cpv-selector-modal'),
            trigger: document.getElementById('browse-cpv-codes'),
            searchInput: document.querySelector('#cpv-search'),
            treeContainer: document.querySelector('#cpv-tree'),
            searchContainer: document.querySelector('#search-results-list'),
            selectionContainer: document.querySelector('#selected-codes-list')
        });
        const fetchHtml = async (url) => {
            const response = await fetch(url);
            if (!response.ok) {
                const error = new Error(`HTTP ${response.status}`);
                error.status = response.status;
                throw error;
            }
            return response.text();
        };

        const updateContainer = (container, html) => {
            if (container) container.innerHTML = html;
        };

        const getCurrentSelection = () => Array.from(state.selectedCodes);

        const loadSelectedCodes = () => {
            const hiddenInputs = document.querySelectorAll('input[name="cpv"]');
            state.selectedCodes.clear();
            hiddenInputs.forEach(input => {
                if (input.value) state.selectedCodes.add(input.value);
            });
        };

        const updateTriggerText = () => {
            const count = state.selectedCodes.size;
            const originalText = state.trigger.dataset.originalText || 'Browse CPV codes';

            if (!state.trigger.dataset.originalText) {
                state.trigger.dataset.originalText = originalText;
            }

            state.trigger.textContent = count > 0
                ? `Edit CPV code selection`
                : originalText;
        };

        const loadTree = async (expandedCode = null) => {
            try {
                const selection = getCurrentSelection();
                const params = new URLSearchParams();
                selection.forEach(code => params.append('selectedCodes', code));
                if (expandedCode) params.append('expandedCode', expandedCode);

                const html = await fetchHtml(`/cpv/tree-fragment?${params.toString()}`);
                updateContainer(state.treeContainer, html);
                setupTreeEventHandlers();
            } catch (error) {
                const errorMessage = error.status === 404
                    ? '<p class="govuk-body govuk-!-margin-top-2"><span class="govuk-!-font-weight-bold">There are no matching results.</span></p><p class="govuk-body">Double-check your spelling to improve your search results.</p>'
                    : '<p class="govuk-body govuk-!-margin-top-2">Something went wrong. Please try again later.</p>';
                updateContainer(state.treeContainer, errorMessage);
            }
        };

        const loadSearch = async (query) => {
            const searchResults = document.getElementById('cpv-search-results');

            if (!query || query.length < 2) {
                updateContainer(state.searchContainer, query && query.length < 2
                    ? '<p class="govuk-body-s govuk-!-margin-top-2">Please enter at least 2 characters to search.</p>'
                    : '');
                if (query) {
                    searchResults.classList.remove('govuk-!-display-none');
                    searchResults.classList.add('govuk-!-display-block');
                } else {
                    searchResults.classList.add('govuk-!-display-none');
                    searchResults.classList.remove('govuk-!-display-block');
                }
                return;
            }

            try {
                searchResults.classList.remove('govuk-!-display-none');
                searchResults.classList.add('govuk-!-display-block');
                updateContainer(state.searchContainer, '<p class="govuk-body-s govuk-!-margin-top-2">Searching...</p>');

                const selection = getCurrentSelection();
                const params = new URLSearchParams();
                params.append('q', query);
                selection.forEach(code => params.append('selectedCodes', code));

                const html = await fetchHtml(`/cpv/search-fragment?${params.toString()}`);
                updateContainer(state.searchContainer, html);
                setupSearchEventHandlers();
            } catch (error) {
                const errorMessage = error.status === 404
                    ? '<p class="govuk-body govuk-!-margin-top-2"><span class="govuk-!-font-weight-bold">There are no matching results.</span></p><p class="govuk-body">Double-check your spelling to improve your search results.</p>'
                    : '<p class="govuk-body govuk-!-margin-top-2">Something went wrong. Please try again later.</p>';
                updateContainer(state.searchContainer, errorMessage);
            }
        };

        const updateSelectionDisplay = async () => {
            try {
                const formData = new FormData();
                getCurrentSelection().forEach(code => formData.append('selectedCodes', code));

                const response = await fetch('/cpv/selection-fragment', {
                    method: 'POST',
                    body: formData
                });

                if (response.ok) {
                    const html = await response.text();
                    updateContainer(state.selectionContainer, html);
                    setupSelectionEventHandlers();

                    const countElement = document.getElementById('selection-count');
                    if (countElement) {
                        countElement.textContent = state.selectedCodes.size.toString();
                    }
                }
            } catch (error) {
                console.error('Failed to update selection display:', error);
            }
        };

        const toggleSelection = (code) => {
            if (state.selectedCodes.has(code)) {
                state.selectedCodes.delete(code);
            } else {
                state.selectedCodes.add(code);
            }

            updateTriggerText();
            updateSelectionDisplay();
            refreshCheckboxStates();
        };

        const refreshCheckboxStates = () => {
            document.querySelectorAll('[data-action="toggle-selection"]').forEach(element => {
                const code = element.dataset.code;
                const isSelected = state.selectedCodes.has(code);

                if (element.type === 'checkbox') {
                    element.checked = isSelected;
                }

                if (element.classList.contains('cpv-search-item')) {
                    if (isSelected) {
                        element.classList.add('cpv-search-item--selected');
                    } else {
                        element.classList.remove('cpv-search-item--selected');
                    }
                }
            });
        };

        const loadChildren = async (parentCode, container) => {
            try {
                container.innerHTML = '<div class="govuk-body-s">Loading...</div>';
                const selection = getCurrentSelection();
                const params = new URLSearchParams();
                selection.forEach(code => params.append('selectedCodes', code));

                container.innerHTML = await fetchHtml(`/cpv/children-fragment/${encodeURIComponent(parentCode)}?${params.toString()}`);
                setupTreeEventHandlers();
            } catch (error) {
                container.innerHTML = error.status === 404
                    ? '<p class="govuk-body govuk-!-margin-top-2"><span class="govuk-!-font-weight-bold">There are no matching results.</span></p><p class="govuk-body">Double-check your spelling to improve your search results.</p>'
                    : '<p class="govuk-body govuk-!-margin-top-2">Something went wrong. Please try again later.</p>';
            }
        };

        const setupTreeEventHandlers = () => {
            document.querySelectorAll('[data-action="toggle-selection"]').forEach(checkbox => {
                checkbox.addEventListener('change', (e) => {
                    e.stopPropagation();
                    toggleSelection(checkbox.dataset.code);
                });
            });

            document.querySelectorAll('details[data-cpv-code]').forEach(details => {
                if (!details.dataset.listenerAdded) {
                    details.dataset.listenerAdded = 'true';
                    details.addEventListener('toggle', async () => {
                        if (details.open) {
                            const childrenContainer = details.querySelector('[data-children-container]');
                            const parentCode = details.dataset.cpvCode;

                            if (childrenContainer && !childrenContainer.dataset.loaded && childrenContainer.children.length === 0) {
                                childrenContainer.dataset.loaded = 'true';
                                await loadChildren(parentCode, childrenContainer);
                            }
                        }
                    });
                }
            });
        };

        const setupSearchEventHandlers = () => {
            document.querySelectorAll('#search-results-list [data-action="toggle-selection"]').forEach(item => {
                const handleToggle = () => {
                    toggleSelection(item.dataset.code);
                    const searchResults = document.getElementById('cpv-search-results');
                    if (searchResults) {
                        searchResults.classList.add('govuk-!-display-none');
                        searchResults.classList.remove('govuk-!-display-block');
                    }
                };

                item.addEventListener('click', handleToggle);
                item.addEventListener('keydown', (e) => {
                    if (e.key === 'Enter' || e.key === ' ') {
                        e.preventDefault();
                        handleToggle();
                    }
                });
            });
        };

        const setupSelectionEventHandlers = () => {
            document.querySelectorAll('[data-action="remove-selection"]').forEach(button => {
                button.addEventListener('click', () => {
                    toggleSelection(button.dataset.code);
                });
            });
        };

        const setupModalHandlers = () => {
            if (!state.modal || !state.trigger) return;

            state.trigger.addEventListener('click', (e) => {
                e.preventDefault();
                openModal();
            });

            document.querySelectorAll('[data-dismiss="modal"]').forEach(button => {
                button.addEventListener('click', closeModal);
            });

            state.modal.addEventListener('click', (e) => {
                if (e.target === state.modal) closeModal();
            });

            document.addEventListener('keydown', (e) => {
                if (e.key === 'Escape' && state.modal.style.display === 'block') {
                    closeModal();
                }
            });

            if (state.searchInput) {
                state.searchInput.addEventListener('input', debounce((e) => {
                    loadSearch(e.target.value.trim());
                }, 300));

                state.searchInput.addEventListener('focus', () => {
                    if (state.searchInput.value.trim().length >= 2) {
                        const searchResults = document.getElementById('cpv-search-results');
                        searchResults.classList.remove('govuk-!-display-none');
                        searchResults.classList.add('govuk-!-display-block');
                    }
                });

                state.searchInput.addEventListener('blur', (_) => {
                    setTimeout(() => {
                        const searchResults = document.getElementById('cpv-search-results');
                        if (searchResults && !searchResults.contains(document.activeElement)) {
                            searchResults.classList.add('govuk-!-display-none');
                            searchResults.classList.remove('govuk-!-display-block');
                        }
                    }, 150);
                });
            }

            document.addEventListener('click', (e) => {
                const searchResults = document.getElementById('cpv-search-results');
                const searchInput = state.searchInput;

                if (searchResults && searchInput &&
                    !searchResults.contains(e.target) &&
                    !searchInput.contains(e.target)) {
                    searchResults.classList.add('govuk-!-display-none');
                    searchResults.classList.remove('govuk-!-display-block');
                }
            });

            document.querySelector('#apply-selection')?.addEventListener('click', applySelection);
            document.querySelector('#clear-all')?.addEventListener('click', clearAllSelections);
        };

        const openModal = async () => {
            state.modal.style.display = 'block';
            state.modal.setAttribute('aria-hidden', 'false');
            document.body.style.overflow = 'hidden';

            loadSelectedCodes();
            updateTriggerText();

            await Promise.all([
                loadTree(),
                updateSelectionDisplay()
            ]);

            setTimeout(() => state.searchInput?.focus(), 100);
        };

        const closeModal = () => {
            state.modal.style.display = 'none';
            state.modal.setAttribute('aria-hidden', 'true');
            document.body.style.overflow = '';
            state.trigger?.focus();
        };

        const clearAllSelections = () => {
            state.selectedCodes.clear();
            updateTriggerText();
            updateSelectionDisplay();
            refreshCheckboxStates();
        };

        const applySelection = () => {
            updateHiddenInputs();
            updateAccordionContent();
            closeModal();
        };

        const updateHiddenInputs = () => {
            document.querySelectorAll('input[name="cpv"]').forEach(input => input.remove());

            const container = document.querySelector('form') || document.body;
            getCurrentSelection().forEach(code => {
                const input = document.createElement('input');
                Object.assign(input, {type: 'hidden', name: 'cpv', value: code});
                container.appendChild(input);
            });
        };

        const updateAccordionContent = () => {
            const accordionContent = document.querySelector('#industry-cpv-code-content');
            if (!accordionContent || state.selectedCodes.size === 0) return;

            let displayArea = accordionContent.querySelector('.cpv-selected-display');
            if (!displayArea) {
                displayArea = document.createElement('div');
                displayArea.className = 'cpv-selected-display govuk-!-margin-bottom-3 govuk-!-padding-left-2';
                accordionContent.insertBefore(displayArea, accordionContent.lastElementChild);
            }

            displayArea.innerHTML = `
                <p class="govuk-body-s govuk-!-font-weight-bold govuk-!-margin-top-3">
                    Selected (${state.selectedCodes.size}):
                </p>
                <div class="govuk-!-margin-top-2">
                    ${Array.from(state.selectedCodes).map(code =>
                `<div class="govuk-tag govuk-tag--grey govuk-!-margin-right-1 govuk-!-margin-bottom-1">
                            ${code}
                        </div>`
            ).join('')}
                </div>
            `;
        };

        const debounce = (func, wait) => {
            let timeout;
            return (...args) => {
                clearTimeout(timeout);
                timeout = setTimeout(() => func(...args), wait);
            };
        };

        const init = () => {
            loadSelectedCodes();
            updateTriggerText();
            setupModalHandlers();
        };

        return {init};
    })();

    CpvSelector.init();
});