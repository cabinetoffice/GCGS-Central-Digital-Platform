document.addEventListener("DOMContentLoaded", function () {
    const cpvModalTrigger = document.getElementById('browse-cpv-codes');
    const cpvModal = document.getElementById('cpv-selector-modal');

    if (!cpvModalTrigger || !cpvModal) {
        return;
    }

    let selectedCodes = new Set();
    let cpvData = [];
    const searchInput = cpvModal.querySelector('#cpv-search');
    const selectionCount = cpvModal.querySelector('#selection-count');
    const selectedCodesList = cpvModal.querySelector('#selected-codes-list');
    const searchResults = cpvModal.querySelector('#cpv-search-results');
    const searchResultsList = cpvModal.querySelector('#search-results-list');
    const cpvTree = cpvModal.querySelector('#cpv-tree');
    const applyButton = cpvModal.querySelector('#apply-selection');
    const clearAllButton = cpvModal.querySelector('#clear-all');

    init();

    async function init() {
        loadSelectedCodes();
        updateSelectionDisplay();
        setupEventListeners();
        await loadCpvData();
        renderCpvTree();
    }

    function setupEventListeners() {
        cpvModalTrigger.addEventListener('click', function (e) {
            e.preventDefault();
            openModal();
        });

        const closeButtons = cpvModal.querySelectorAll('[data-dismiss="modal"]');
        closeButtons.forEach(button => {
            button.addEventListener('click', closeModal);
        });

        cpvModal.addEventListener('click', function (e) {
            if (e.target === cpvModal) {
                closeModal();
            }
        });

        document.addEventListener('keydown', function (e) {
            if (e.key === 'Escape' && cpvModal.style.display === 'block') {
                closeModal();
            }
        });

        if (searchInput) {
            searchInput.addEventListener('input', debounce(handleSearch, 300));
        }

        if (applyButton) {
            applyButton.addEventListener('click', applySelection);
        }

        if (clearAllButton) {
            clearAllButton.addEventListener('click', clearAllSelections);
        }
    }

    function openModal() {
        cpvModal.style.display = 'block';
        cpvModal.setAttribute('aria-hidden', 'false');
        document.body.style.overflow = 'hidden';

        if (searchInput) {
            setTimeout(() => searchInput.focus(), 100);
        }
    }

    function closeModal() {
        cpvModal.style.display = 'none';
        cpvModal.setAttribute('aria-hidden', 'true');
        document.body.style.overflow = '';

        if (cpvModalTrigger) {
            cpvModalTrigger.focus();
        }
    }

    function loadSelectedCodes() {
        const hiddenInputs = document.querySelectorAll('input[name="cpv"]');
        hiddenInputs.forEach(input => {
            if (input.value) {
                selectedCodes.add(input.value);
            }
        });
    }

    function updateSelectionDisplay() {
        if (selectionCount) {
            selectionCount.textContent = selectedCodes.size.toString();
        }

        if (selectedCodesList) {
            updateSelectedCodesList();
        }

        updateTriggerText();
    }

    function updateSelectedCodesList() {
        if (!selectedCodesList) {
            return;
        }

        if (selectedCodes.size === 0) {
            selectedCodesList.innerHTML = '<p class="govuk-body-s govuk-!-margin-bottom-0">No CPV codes selected</p>';
            return;
        }

        const selectedItems = Array.from(selectedCodes).map(code => {
            const item = findCpvItem(cpvData, code);
            return item || {Code: code, DescriptionEn: 'Unknown code'};
        });

        selectedCodesList.innerHTML = selectedItems.map(item => `
            <div class="govuk-tag govuk-tag--grey govuk-!-margin-right-1 govuk-!-margin-bottom-1" data-code="${item.Code}">
                ${item.DescriptionEn}
                <button type="button" class="govuk-tag__remove" aria-label="Remove ${item.DescriptionEn}" data-code="${item.Code}">
                    &times;
                </button>
            </div>
        `).join('');

        selectedCodesList.querySelectorAll('.govuk-tag__remove').forEach(button => {
            button.addEventListener('click', function () {
                const code = this.getAttribute('data-code');
                selectedCodes.delete(code);
                updateSelectionDisplay();
                renderCpvTree();
            });
        });
    }

    function updateTriggerText() {
        const count = selectedCodes.size;
        const originalText = cpvModalTrigger.dataset.originalText || 'Browse CPV codes';

        if (!cpvModalTrigger.dataset.originalText) {
            cpvModalTrigger.dataset.originalText = originalText;
        }

        if (count > 0) {
            cpvModalTrigger.textContent = `Edit CPV code selection (${count} selected)`;
        } else {
            cpvModalTrigger.textContent = originalText;
        }
    }

    async function loadCpvData() {
        try {
            cpvData = [{
                Code: "03000000",
                DescriptionEn: "Agricultural, farming, fishing, forestry and related products",
                Children: [{
                    Code: "03100000",
                    DescriptionEn: "Agricultural and horticultural products",
                    Children: [{Code: "03110000", DescriptionEn: "Seeds", Children: []}, {
                        Code: "03120000",
                        DescriptionEn: "Plants and flowers",
                        Children: []
                    }]
                }, {
                    Code: "03200000",
                    DescriptionEn: "Cereals, potatoes, vegetables, fruits and nuts",
                    Children: [{Code: "03210000", DescriptionEn: "Cereals and leguminous vegetables", Children: []}]
                }]
            }, {
                Code: "09000000",
                DescriptionEn: "Petroleum products, fuel, electricity and other sources of energy",
                Children: [{Code: "09100000", DescriptionEn: "Fuels", Children: []}, {
                    Code: "09300000", DescriptionEn: "Electricity, heating, solar and nuclear energy", Children: []
                }]
            }];
        } catch (error) {
            cpvData = [];
        }
    }

    function renderCpvTree() {
        if (!cpvTree) return;

        cpvTree.innerHTML = '';
        cpvData.forEach(item => {
            const node = createTreeNode(item, 0);
            if (node) {
                cpvTree.appendChild(node);
            }
        });
    }

    function createTreeNode(item, level) {
        if (!item || !item.Code) {
            return null;
        }

        const hasChildren = item.Children && item.Children.length > 0;
        const isSelected = selectedCodes.has(item.Code);
        const checkboxId = `cpv-checkbox-${item.Code.replace(/\D/g, '')}`;

        if (hasChildren) {
            const details = document.createElement('details');
            details.className = `cpv-details cpv-details--level-${level}`;

            const summary = document.createElement('summary');
            summary.className = 'cpv-details__summary';
            summary.innerHTML = `
                <input class="govuk-checkboxes__input govuk-checkboxes__input--small cpv-details__checkbox"
                       type="checkbox"
                       id="${checkboxId}"
                       data-code="${item.Code}"
                       ${isSelected ? 'checked' : ''}>
                <span class="cpv-details__content">
                    <span class="govuk-body-s"><strong>${item.Code}</strong> ${item.DescriptionEn}</span>
                </span>
            `;

            const childrenContainer = document.createElement('div');
            childrenContainer.className = 'cpv-details__children';

            item.Children.forEach(child => {
                const childNode = createTreeNode(child, level + 1);
                childrenContainer.appendChild(childNode);
            });

            details.appendChild(summary);
            details.appendChild(childrenContainer);

            const checkbox = summary.querySelector('.cpv-details__checkbox');

            if (checkbox) {
                checkbox.addEventListener('click', function (e) {
                    e.stopPropagation();
                });

                checkbox.addEventListener('change', function (e) {
                    e.stopPropagation();
                    handleCheckboxChange(item.Code, this.checked);
                });
            }

            return details;
        } else {
            const div = document.createElement('div');
            div.className = `cpv-item cpv-item--level-${level}`;

            div.innerHTML = `
                <div class="cpv-item__content">
                    <input class="govuk-checkboxes__input govuk-checkboxes__input--small cpv-item__checkbox"
                           type="checkbox"
                           id="${checkboxId}"
                           data-code="${item.Code}"
                           ${isSelected ? 'checked' : ''}>
                    <span class="cpv-item__text">
                        <span class="govuk-body-s"><strong>${item.Code}</strong> ${item.DescriptionEn}</span>
                    </span>
                </div>
            `;

            const checkbox = div.querySelector('.cpv-item__checkbox');
            if (checkbox) {
                checkbox.addEventListener('change', function () {
                    handleCheckboxChange(item.Code, this.checked);
                });
            }

            return div;
        }
    }

    function handleCheckboxChange(code, checked) {
        if (checked) {
            if (selectedCodes.size < 20) {
                selectedCodes.add(code);
                console.log('Added code:', code, 'Total selected:', selectedCodes.size);
            } else {
                const checkbox = document.querySelector(`input[data-code="${code}"]`);
                if (checkbox) checkbox.checked = false;
                alert('Maximum 20 selections allowed. Please remove a selection before adding more.');
                return;
            }
        } else {
            selectedCodes.delete(code);
        }

        updateSelectionDisplay();
    }

    function handleSearch() {
        const query = searchInput.value.toLowerCase().trim();

        if (!query) {
            searchResults.style.display = 'none';
            return;
        }

        const results = searchCpvData(cpvData, query);

        if (results.length === 0) {
            searchResultsList.innerHTML = '<p class="govuk-body-s">No matching codes found.</p>';
        } else {
            searchResultsList.innerHTML = results.slice(0, 10).map((item, index) => {
                const searchCheckboxId = `cpv-search-${item.Code.replace(/\D/g, '')}-${index}`;
                return `
                    <div class="govuk-checkboxes__item" style="margin-bottom: 10px; display: flex; align-items: center;">
                        <input class="govuk-checkboxes__input govuk-checkboxes__input--small"
                               type="checkbox"
                               id="${searchCheckboxId}"
                               data-code="${item.Code}"
                               ${selectedCodes.has(item.Code) ? 'checked' : ''}>
                        <label class="govuk-label govuk-checkboxes__label"
                               for="${searchCheckboxId}"
                               style="margin: 0; cursor: pointer;">
                            <span class="govuk-body-s"><strong>${item.Code}</strong> ${item.DescriptionEn}</span>
                        </label>
                    </div>
                `;
            }).join('');

            searchResultsList.querySelectorAll('input[type="checkbox"]').forEach(checkbox => {
                checkbox.addEventListener('change', function () {
                    handleCheckboxChange(this.dataset.code, this.checked);
                    renderCpvTree();
                });
            });
        }

        searchResults.style.display = 'block';
    }

    function searchCpvData(data, query) {
        const results = [];

        function searchNode(node) {
            if (node.Code.toLowerCase().includes(query) || node.DescriptionEn.toLowerCase().includes(query)) {
                results.push(node);
            }

            if (node.Children) {
                node.Children.forEach(searchNode);
            }
        }

        data.forEach(searchNode);
        return results;
    }

    function findCpvItem(data, code) {
        for (const item of data) {
            if (item.Code === code) {
                return item;
            }
            if (item.Children) {
                const found = findCpvItem(item.Children, code);
                if (found) return found;
            }
        }
        return null;
    }

    function applySelection() {
        updateHiddenInputs();

        updateAccordionContent();

        closeModal();
    }

    function updateHiddenInputs() {
        const existingInputs = document.querySelectorAll('input[name="cpv"]');
        existingInputs.forEach(input => input.remove());

        const container = document.querySelector('[name="cpv"]')?.parentNode || document.body;
        selectedCodes.forEach(code => {
            const input = document.createElement('input');
            input.type = 'hidden';
            input.name = 'cpv';
            input.value = code;
            container.appendChild(input);
        });
    }

    function updateAccordionContent() {
        const accordionContent = document.querySelector('#industry-cpv-code-content');
        if (accordionContent && selectedCodes.size > 0) {
            const selectedItems = Array.from(selectedCodes).map(code => {
                const item = findCpvItem(cpvData, code);
                return item || {Code: code, DescriptionEn: 'Unknown code'};
            });

            const tagsHtml = selectedItems.map(item => `<div class="govuk-tag govuk-tag--grey govuk-!-margin-right-1 govuk-!-margin-bottom-1">
                    ${item.DescriptionEn}
                </div>`).join('');

            let displayArea = accordionContent.querySelector('.cpv-selected-display');
            if (!displayArea) {
                displayArea = document.createElement('div');
                displayArea.className = 'cpv-selected-display govuk-!-margin-bottom-3 govuk-!-padding-left-2';
                accordionContent.insertBefore(displayArea, accordionContent.lastElementChild);
            }

            displayArea.innerHTML = `
                <p class="govuk-body-s govuk-!-font-weight-bold govuk-!-margin-top-2">Selected (${selectedCodes.size} of 20):</p>
                <div class="govuk-!-margin-top-2">${tagsHtml}</div>
            `;
        }
    }

    function clearAllSelections() {
        selectedCodes.clear();
        updateSelectionDisplay();
        renderCpvTree();
    }

    function debounce(func, wait) {
        let timeout;
        return function executedFunction(...args) {
            const later = () => {
                clearTimeout(timeout);
                func(...args);
            };
            clearTimeout(timeout);
            timeout = setTimeout(later, wait);
        };
    }
});