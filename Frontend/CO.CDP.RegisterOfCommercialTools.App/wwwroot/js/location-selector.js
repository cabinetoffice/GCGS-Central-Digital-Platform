const LocationSelector = (() => {
    let config, state;

    const fetchHtml = async (url) => {
        try {
            const response = await fetch(url);

            if (response.status >= 500) {
                window.location.href = '/error';
                return null;
            }

            if (!response.ok) {
                return {
                    success: false,
                    status: response.status,
                    message: getErrorMessage(response.status)
                };
            }

            const html = await response.text();
            return {success: true, html};

        } catch (networkError) {
            return {
                success: false,
                status: 0,
                message: 'Connection problem. Check your internet connection and try again.'
            };
        }
    };

    const getErrorMessage = (status) => {
        switch (status) {
            case 404:
                return 'No results found. Try different search terms or browse the categories below.';
            case 408:
                return 'Request timed out. Please try again.';
            default:
                return 'Something went wrong. Please try again.';
        }
    };

    const escapeHtml = (unsafeString) => {
        return String(unsafeString)
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;')
            .replace(/'/g, '&#39;');
    };

    const createErrorMessage = (message) => {
        return `<p class="govuk-body govuk-!-margin-top-2">${escapeHtml(message)}</p>`;
    };

    const updateContainer = (container, html) => {
        if (container) container.innerHTML = html;
    };

    const getCurrentSelection = () => Array.from(state.selectedCodes);
    const loadSelectedCodes = () => {
        state.selectedCodes.clear();

        const urlParams = new URLSearchParams(window.location.search);
        const cpvParams = urlParams.getAll(config.fieldName);
        cpvParams.forEach(code => {
            if (code) state.selectedCodes.add(code);
        });
    };

    const updateTriggerText = () => {
        const count = state.selectedCodes.size;
        const originalText = state.trigger.dataset.originalText || `Browse ${config.codeType.toLowerCase()}s`;

        if (!state.trigger.dataset.originalText) {
            state.trigger.dataset.originalText = originalText;
        }

        state.trigger.textContent = count > 0
            ? `Edit ${config.codeType.toLowerCase()}s`
            : originalText;
    };

    const loadTree = async (expandedCode = null) => {
        const selection = getCurrentSelection();
        const params = new URLSearchParams();
        selection.forEach(code => params.append('selectedCodes', code));
        if (expandedCode) params.append('expandedCode', expandedCode);

        const response = await fetchHtml(`/${config.routePrefix}/tree-fragment?${params.toString()}`);

        if (response && response.success) {
            updateContainer(state.treeContainer, response.html);
            setupTreeEventHandlers();
            refreshCheckboxStates();
        } else if (response && !response.success) {
            updateContainer(state.treeContainer, createErrorMessage(response.message));
        }
    };

    const loadSearch = async (query) => {
        const searchResults = document.getElementById(`${config.searchInputId}-results`);

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

        searchResults.classList.remove('govuk-!-display-none');
        searchResults.classList.add('govuk-!-display-block');
        updateContainer(state.searchContainer, '<p class="govuk-body-s govuk-!-margin-top-2">Searching...</p>');

        const selection = getCurrentSelection();
        const params = new URLSearchParams();
        params.append('q', query);
        selection.forEach(code => params.append('selectedCodes', code));

        const response = await fetchHtml(`/${config.routePrefix}/search-fragment?${params.toString()}`);

        if (response && response.success) {
            updateContainer(state.searchContainer, response.html);
            setupSearchEventHandlers();
            refreshCheckboxStates();
        } else if (response && !response.success) {
            updateContainer(state.searchContainer, createErrorMessage(response.message));
        }
    };

    const updateSelectionDisplay = async () => {
        const countElement = document.getElementById(`${config.modalId}-selection-count`);
        if (countElement) {
            countElement.textContent = state.selectedCodes.size.toString();
        }

        if (state.selectedCodes.size === 0) {
            updateContainer(state.selectionContainer, '<p class="govuk-body-s govuk-!-margin-bottom-0">No locations selected</p>');
            return;
        }

        try {
            const formData = new FormData();
            getCurrentSelection().forEach(code => formData.append('selectedCodes', code));

            const response = await fetch(`/${config.routePrefix}/selection-fragment`, {
                method: 'POST',
                body: formData
            });

            if (response.ok) {
                const html = await response.text();
                updateContainer(state.selectionContainer, html);
                setupSelectionEventHandlers();
                refreshCheckboxStates();
            }
        } catch (error) {
        }
    };

    const toggleSelection = (code) => {
        if (state.selectedCodes.has(code)) {
            state.selectedCodes.delete(code);
        } else {
            if (!hasAncestorSelected(code)) {
                removeDescendants(code);
                state.selectedCodes.add(code);
            }
        }

        updateTriggerText();
        updateSelectionDisplay();
        refreshCheckboxStates();
        updateUrlParams();
        updateHiddenInputs();
        updateAccordionContent();
    };

    const refreshCheckboxStates = (container = document) => {
        container.querySelectorAll('[data-action="toggle-selection"]').forEach(element => {
            const code = element.dataset.code;
            const isSelected = state.selectedCodes.has(code);
            const isDisabled = hasAncestorSelected(code);

            if (element.type === 'checkbox') {
                element.checked = isSelected || isDisabled;
                element.disabled = isDisabled;
            }

            if (element.classList.contains(`${config.codeType.toLowerCase()}-search-item`)) {
                if (isSelected) {
                    element.classList.add(`${config.codeType.toLowerCase()}-search-item--selected`);
                } else {
                    element.classList.remove(`${config.codeType.toLowerCase()}-search-item--selected`);
                }

                if (isDisabled) {
                    element.setAttribute('aria-disabled', 'true');
                } else {
                    element.removeAttribute('aria-disabled');
                }
            }
        });
    };

    const loadChildren = async (parentCode, container) => {
        container.innerHTML = '<div class="govuk-body-s">Loading...</div>';
        const selection = getCurrentSelection();
        const params = new URLSearchParams();
        selection.forEach(code => params.append('selectedCodes', code));

        const response = await fetchHtml(`/${config.routePrefix}/children-fragment/${encodeURIComponent(parentCode)}?${params.toString()}`);

        if (response && response.success) {
            container.innerHTML = response.html;
            setupTreeEventHandlers();
            refreshCheckboxStates(container);
        } else if (response && !response.success) {
            container.innerHTML = createErrorMessage(response.message);
        } else {
            container.innerHTML = createErrorMessage('Something went wrong. Please try again later.');
        }
    };

    const setupTreeEventHandlers = () => {
        if (state.treeContainer && !state.treeContainer.dataset.delegatedListeners) {
            state.treeContainer.dataset.delegatedListeners = 'true';

            state.treeContainer.addEventListener('change', (e) => {
                if (e.target.matches('[data-action="toggle-selection"]') && !e.target.disabled) {
                    e.stopPropagation();
                    toggleSelection(e.target.dataset.code);
                }
            });

            state.treeContainer.addEventListener('toggle', async (e) => {
                if (e.target.matches(`details[data-${config.codeType.toLowerCase()}-code]`)) {
                    const details = e.target;
                    if (details.open) {
                        const childrenContainer = details.querySelector('[data-children-container]');
                        const parentCode = details.dataset[`${config.codeType.toLowerCase()}Code`];

                        if (childrenContainer && !childrenContainer.dataset.loaded && childrenContainer.children.length === 0) {
                            childrenContainer.dataset.loaded = 'true';
                            await loadChildren(parentCode, childrenContainer);
                        }
                    }
                }
            }, true);
        }
    };

    const setupSearchEventHandlers = () => {
        document.querySelectorAll(`#${config.searchContainerId} [data-action="toggle-selection"]`).forEach(item => {
            const handleToggle = () => {
                if (!item.hasAttribute('aria-disabled')) {
                    toggleSelection(item.dataset.code);
                    const searchResults = document.getElementById(`${config.searchInputId}-results`);
                    if (searchResults) {
                        searchResults.classList.add('govuk-!-display-none');
                        searchResults.classList.remove('govuk-!-display-block');
                    }
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
                    const searchResults = document.getElementById(`${config.searchInputId}-results`);
                    searchResults.classList.remove('govuk-!-display-none');
                    searchResults.classList.add('govuk-!-display-block');
                }
            });

            state.searchInput.addEventListener('blur', (_) => {
                setTimeout(() => {
                    const searchResults = document.getElementById(`${config.searchInputId}-results`);
                    if (searchResults && !searchResults.contains(document.activeElement)) {
                        searchResults.classList.add('govuk-!-display-none');
                        searchResults.classList.remove('govuk-!-display-block');
                    }
                }, 150);
            });
        }

        document.addEventListener('click', (e) => {
            const searchResults = document.getElementById(`${config.searchInputId}-results`);
            const searchInput = state.searchInput;

            if (searchResults && searchInput &&
                !searchResults.contains(e.target) &&
                !searchInput.contains(e.target)) {
                searchResults.classList.add('govuk-!-display-none');
                searchResults.classList.remove('govuk-!-display-block');
            }
        });

        document.querySelector(`#${config.modalId}-apply-selection`)?.addEventListener('click', applySelection);
        document.querySelector(`#${config.modalId}-clear-all`)?.addEventListener('click', clearAllSelections);
    };

    const openModal = async () => {
        state.modal.style.display = 'block';
        state.modal.setAttribute('aria-hidden', 'false');
        document.body.style.overflow = 'hidden';

        const searchResults = document.getElementById(`${config.searchInputId}-results`);
        if (searchResults) {
            searchResults.classList.add('govuk-!-display-none');
            searchResults.classList.remove('govuk-!-display-block');
        }

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

    const updateUrlParams = () => {
        const url = new URL(window.location);

        url.searchParams.delete(config.fieldName);

        getCurrentSelection().forEach(code => {
            url.searchParams.append(config.fieldName, code);
        });

        window.history.replaceState({}, '', url);
    };

    const updateHiddenInputs = () => {
        const searchForm = document.querySelector('#search-form');
        if (!searchForm) return;

        searchForm.querySelectorAll(`input[name="${config.fieldName}"]`).forEach(input => {
            input.remove();
        });

        getCurrentSelection().forEach(code => {
            const input = document.createElement('input');
            input.type = 'hidden';
            input.name = config.fieldName;
            input.value = code;
            searchForm.appendChild(input);
        });
    };

    const applySelection = () => {
        updateUrlParams();
        updateHiddenInputs();
        updateAccordionContent();
        closeModal();
    };

    const clearAllSelections = () => {
        state.selectedCodes.clear();
        updateTriggerText();
        updateSelectionDisplay();
        refreshCheckboxStates();
        updateUrlParams();
        updateHiddenInputs();
        updateAccordionContent();
    };

    const updateAccordionContent = async () => {
        const accordionContent = document.querySelector(`#${config.accordionContentId}`);
        if (!accordionContent) return;

        const browseLink = accordionContent.querySelector('.accordion-highlight');
        if (!browseLink) return;

        const existingDisplay = document.getElementById(`${config.fieldName}-selected-codes-display`);
        if (existingDisplay) {
            existingDisplay.remove();
        }

        if (state.selectedCodes.size === 0) {
            return;
        }

        const formData = new FormData();
        Array.from(state.selectedCodes).forEach(code => formData.append('selectedCodes', code));

        try {
            const response = await fetch(`/${config.routePrefix}/accordion-selection-fragment`, {
                method: 'POST',
                body: formData
            });

            if (response.ok) {
                const html = await response.text();
                const selectedCodesDisplay = document.createElement('div');
                selectedCodesDisplay.className = `${config.fieldName}-selected-display govuk-!-margin-bottom-0 govuk-!-margin-top-3`;
                selectedCodesDisplay.id = `${config.fieldName}-selected-codes-display`;
                selectedCodesDisplay.innerHTML = html;
                browseLink.appendChild(selectedCodesDisplay);
            }
        } catch (error) {
        }
    };

    const debounce = (func, wait) => {
        let timeout;
        return (...args) => {
            clearTimeout(timeout);
            timeout = setTimeout(() => func(...args), wait);
        };
    };

    const buildHierarchyCache = () => {
        const cache = {};
        document.querySelectorAll('[data-code][data-parent-code]').forEach(element => {
            const code = element.dataset.code;
            const parentCode = element.dataset.parentCode;
            const level = parseInt(element.dataset.level) || 0;

            if (code) {
                cache[code] = {
                    code: code,
                    parentCode: parentCode || null,
                    level: level
                };
            }
        });
        return cache;
    };

    const isAncestorOf = (potentialAncestor, code) => {
        if (potentialAncestor === code) return false;

        const cache = buildHierarchyCache();
        const findAncestor = (childCode) => {
            const childData = cache[childCode];
            if (!childData?.parentCode) return false;
            if (childData.parentCode === potentialAncestor) return true;
            return findAncestor(childData.parentCode);
        };

        return cache[code] ? findAncestor(code) : false;
    };

    const hasAncestorSelected = (code) => {
        for (const selectedCode of state.selectedCodes) {
            if (isAncestorOf(selectedCode, code)) {
                return true;
            }
        }
        return false;
    };

    const removeDescendants = (parentCode) => {
        const toRemove = [];
        for (const code of state.selectedCodes) {
            if (isAncestorOf(parentCode, code)) {
                toRemove.push(code);
            }
        }
        toRemove.forEach(code => state.selectedCodes.delete(code));
    };

    const init = (options) => {
        config = {
            codeType: 'Code',
            routePrefix: 'codes',
            fieldName: 'codes',
            modalId: 'code-selector-modal',
            triggerId: 'browse-codes',
            searchInputId: 'code-search',
            treeContainerId: 'code-tree',
            searchContainerId: 'search-results-list',
            selectionContainerId: 'selected-codes-list',
            ...options
        };

        state = Object.freeze({
            selectedCodes: new Set(),
            modal: document.getElementById(config.modalId),
            trigger: document.getElementById(config.triggerId),
            searchInput: document.querySelector(`#${config.searchInputId}`),
            treeContainer: document.querySelector(`#${config.treeContainerId}`),
            searchContainer: document.querySelector(`#${config.searchContainerId}`),
            selectionContainer: document.querySelector(`#${config.selectionContainerId}`)
        });

        loadSelectedCodes();
        updateTriggerText();
        setupModalHandlers();
    };

    return {init};
})();