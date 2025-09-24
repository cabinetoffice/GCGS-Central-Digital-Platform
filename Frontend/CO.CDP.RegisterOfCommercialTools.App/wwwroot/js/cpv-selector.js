const CpvSelector = (() => {
    const instances = new Map();

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

    const debounce = (func, wait) => {
        let timeout;
        return (...args) => {
            clearTimeout(timeout);
            timeout = setTimeout(() => func(...args), wait);
        };
    };

    const init = (options) => {
        const modalId = options.modalId || 'default';

        if (instances.has(modalId)) {
            return instances.get(modalId);
        }

        const instance = createInstance(options);
        instances.set(modalId, instance);
        return instance;
    };

    const createInstance = (options) => {
        const config = {
            codeType: 'CPV',
            routePrefix: 'cpv',
            fieldName: 'cpv',
            modalId: 'cpv-selector-modal',
            triggerId: 'browse-cpv-codes',
            searchInputId: 'cpv-search',
            treeContainerId: 'cpv-tree',
            searchContainerId: 'search-results-list',
            selectionContainerId: 'selected-codes-list',
            accordionContentId: 'industry-cpv-code-content',
            ...options
        };

        const state = {
            selectedCodes: new Set(),
            modal: document.getElementById(config.modalId),
            trigger: document.getElementById(config.triggerId),
            searchInput: document.querySelector(`#${config.searchInputId}`),
            treeContainer: document.querySelector(`#${config.treeContainerId}`),
            searchContainer: document.querySelector(`#${config.searchContainerId}`),
            selectionContainer: document.querySelector(`#${config.selectionContainerId}`)
        };

        const getStorageKey = (suffix) => `${config.fieldName}-${suffix}`;

        const saveToSessionStorage = () => {
            const codes = Array.from(state.selectedCodes);
            sessionStorage.setItem(getStorageKey('selected-codes'), JSON.stringify(codes));
        };

        const loadFromSessionStorage = () => {
            try {
                const stored = sessionStorage.getItem(getStorageKey('selected-codes'));
                if (stored) {
                    const codes = JSON.parse(stored);
                    state.selectedCodes.clear();
                    codes.forEach(code => state.selectedCodes.add(code));
                    updateHiddenInputs();
                }
            } catch (e) {
                state.selectedCodes.clear();
            }
        };

        const getCurrentSelection = () => Array.from(state.selectedCodes);

        const loadSelectedCodes = () => {
            const hiddenInputs = document.querySelectorAll(`input[name="${config.fieldName}"]`);
            state.selectedCodes.clear();
            hiddenInputs.forEach(input => {
                if (input.value) state.selectedCodes.add(input.value);
            });

            if (state.selectedCodes.size === 0) {
                loadFromSessionStorage();
            }
        };

        const updateTriggerText = () => {
            if (!state.trigger) return;

            const count = state.selectedCodes.size;
            const originalText = state.trigger.dataset.originalText || `Browse ${config.codeType} codes`;

            if (!state.trigger.dataset.originalText) {
                state.trigger.dataset.originalText = originalText;
            }

            state.trigger.textContent = count > 0
                ? `Edit selected ${config.codeType} codes`
                : originalText;
        };

        const updateHiddenInputs = () => {
            document.querySelectorAll(`input[name="${config.fieldName}"]`).forEach(input => input.remove());

            const container = document.querySelector('form') || document.body;
            getCurrentSelection().forEach(code => {
                const input = document.createElement('input');
                Object.assign(input, {type: 'hidden', name: config.fieldName, value: code});
                container.appendChild(input);
            });
        };

        const updateAccordionContent = () => {
            const accordionContent = document.querySelector(`#${config.accordionContentId}`);
            if (!accordionContent) return;

            const createDisplayArea = () => {
                const selectedCodesDisplay = document.createElement('div');
                selectedCodesDisplay.className = `${config.codeType.toLowerCase()}-selected-display govuk-!-margin-bottom-3 govuk-!-padding-left-2`;
                accordionContent.insertBefore(selectedCodesDisplay, accordionContent.lastElementChild);
                return selectedCodesDisplay;
            };

            const existingSelectedDisplay = accordionContent.querySelector(`.${config.codeType.toLowerCase()}-selected-display`);

            if (state.selectedCodes.size === 0) {
                if (existingSelectedDisplay) {
                    existingSelectedDisplay.remove();
                }
                return;
            }

            const selectedCodesDisplay = existingSelectedDisplay || createDisplayArea();

            selectedCodesDisplay.innerHTML = `
                <p class="govuk-body-s govuk-!-font-weight-bold govuk-!-margin-top-3">
                    Selected (${state.selectedCodes.size}):
                </p>
                <div class="govuk-!-margin-top-2">
                    ${getCurrentSelection().sort().map(code =>
                `<div class="govuk-tag govuk-tag--grey govuk-!-margin-right-1 govuk-!-margin-bottom-1">${escapeHtml(code)}</div>`
            ).join('')}
                </div>
            `;
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

                    const countElement = document.getElementById(`${config.modalId}-selection-count`);
                    if (countElement) {
                        countElement.textContent = state.selectedCodes.size.toString();
                    }

                    refreshCheckboxStates();
                }
            } catch (error) {
            }
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

        const toggleSelection = (code) => {
            if (state.selectedCodes.has(code)) {
                state.selectedCodes.delete(code);
            } else {
                if (!hasAncestorSelected(code)) {
                    removeDescendants(code);
                    state.selectedCodes.add(code);
                }
            }

            saveToSessionStorage();
            updateTriggerText();
            updateSelectionDisplay();
            refreshCheckboxStates();
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

                if (element.classList.contains('code-search-item')) {
                    if (isSelected) {
                        element.classList.add('code-search-item--selected');
                    } else {
                        element.classList.remove('code-search-item--selected');
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

        const clearAllSelections = () => {
            state.selectedCodes.clear();
            saveToSessionStorage();
            updateTriggerText();
            updateSelectionDisplay();
            refreshCheckboxStates();
            updateHiddenInputs();
            updateAccordionContent();
        };

        const applySelection = () => {
            updateHiddenInputs();
            updateAccordionContent();
            closeModal();
        };

        const openModal = async () => {
            if (!state.modal) return;

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

            const promises = [loadTree()];
            if (state.selectedCodes.size > 0) {
                promises.push(updateSelectionDisplay());
            }
            await Promise.all(promises);

            setTimeout(() => state.searchInput?.focus(), 100);
        };

        const closeModal = () => {
            if (!state.modal) return;

            state.modal.style.display = 'none';
            state.modal.setAttribute('aria-hidden', 'true');
            document.body.style.overflow = '';
            state.trigger?.focus();
        };

        const setupModalHandlers = () => {
            if (!state.modal || !state.trigger) return;

            state.trigger.addEventListener('click', (e) => {
                e.preventDefault();
                openModal();
            });

            state.modal.querySelectorAll('[data-dismiss="modal"]').forEach(button => {
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
                        if (searchResults) {
                            searchResults.classList.remove('govuk-!-display-none');
                            searchResults.classList.add('govuk-!-display-block');
                        }
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

            state.modal.querySelector('#apply-selection')?.addEventListener('click', applySelection);
            state.modal.querySelector('#clear-all')?.addEventListener('click', clearAllSelections);
        };

        loadFromSessionStorage();
        loadSelectedCodes();
        updateTriggerText();
        updateAccordionContent();
        setupModalHandlers();

        return {
            config,
            state,
            openModal,
            closeModal,
            updateAccordionContent
        };
    };

    return {init};
})();