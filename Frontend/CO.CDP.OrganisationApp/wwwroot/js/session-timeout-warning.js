class SessionTimeoutWarning {
    constructor(warningTime, timeoutTime, isAuthenticated, i18n) {
        this.warningTime = warningTime;  
        this.timeoutTime = timeoutTime;
        this.i18n = i18n;
        this.startTime = Date.now();       
        this.lastTimerCheck = 0;    
        this.timeoutModal = null;
        this.fetchInProgressSince = 0;

        if (isAuthenticated) {
            this.doTick = true;
            this.createDialog();
            this.addStorageEventListener();
            requestAnimationFrame(this.tick.bind(this));
            this.resetHandler();
        } else {
            localStorage.setItem('SessionTimeoutWarning.signedOut', Date.now());
        }
    }

    addStorageEventListener() {
        window.addEventListener('storage', (event) => {
            if (event.key === 'SessionTimeoutWarning.reset') {
                this.doReset();
            }

            if (event.key === 'SessionTimeoutWarning.signedOut') {
                this.doSignOut();
            }
        });
    }

    resetHandler() {
        localStorage.setItem('SessionTimeoutWarning.reset', Date.now()); // Use localStorage to broadcast across tabs
        this.doReset();
    }

    doReset() {
        const now = Date.now();

        // Prevent multiple quick fetches from stacking up (e.g., simultaneous resets coming from other tabs)
        if (now - this.fetchInProgressSince < 5000) {
            return;
        }

        this.fetchInProgressSince = now;

        fetch('/session-timeout-keep-alive', { method: 'GET' })
            .then(response => {
                if (!response.ok) {
                    this.doSignOut();
                }
            })
            .catch(error => {
                this.doSignOut();
            })
            .finally(() => this.fetchInProgressSince = 0);

        this.startTime = now;

        if (this.timeoutModal.open) {
            this.timeoutModal.close();
        }
    }

    /**
     *
     * This method is called on each animation frame.
     * 
     * Note: using requestAnimationFrame and an explicit check of previous time means that the solution still works when things like phones go to sleep
     * Upon resuming the tab, the elapsed time relative to the timeout will be calculated correctly
     * The "obvious" solution of using setTimeout would not work in this case
     */
    tick() {
        if (!this.doTick) {
            return;
        }

        const elapsedTime = Date.now() - this.startTime;

        if (elapsedTime >= this.warningTime && !this.timeoutModal.open) {
            this.timeoutModal.showModal();
        }

        if (elapsedTime >= this.timeoutTime) {
            this.signOutHandler();
        }

        requestAnimationFrame(this.tick.bind(this));
    }

    createDialog() {
        this.timeoutModal = document.createElement('dialog');
        this.timeoutModal.classList.add('app-timeout-dialog');

        this.timeoutModal.innerHTML = `
            <h2 class="govuk-heading-m app-timeout-dialog__header">${this.i18n.timeoutDialogHeader}</h2>
            <div class="app-timeout-dialog__inner">
                <p class="govuk-body"><strong>${this.i18n.timeoutDialogLede}</strong></p>
                <p class="govuk-body">${this.i18n.timeoutDialogText}</p>
                <button id="session-timeout-warning-continue" class="govuk-button govuk-!-margin-right-2">${this.i18n.timeoutDialogContinue}</button>
                <button id="session-timeout-warning-sign-out" class="govuk-button govuk-button--secondary">${this.i18n.timeoutDialogSignOut}</button>
            </div>
        `;
        document.body.appendChild(this.timeoutModal);

        this.timeoutModal.querySelector('#session-timeout-warning-continue').addEventListener('click', this.resetHandler.bind(this));
        this.timeoutModal.querySelector('#session-timeout-warning-sign-out').addEventListener('click', this.signOutHandler.bind(this));
    }

    signOutHandler() {
        localStorage.setItem('SessionTimeoutWarning.signedOut', Date.now());
        this.doSignOut();
    }

    doSignOut() {
        this.doTick = false;
        window.location.href = '/one-login/sign-out?redirectUri=%2Flogged-out';
    }
}

export function initSessionTimeoutWarning(timeoutTime, warningTime, isAuthenticated, i18n) {
    const sessionTimeout = new SessionTimeoutWarning(
        warningTime,
        timeoutTime,
        isAuthenticated,
        i18n
    );
}