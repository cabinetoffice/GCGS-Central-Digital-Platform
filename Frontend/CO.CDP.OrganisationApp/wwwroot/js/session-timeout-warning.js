class SessionTimeoutWarning {
    constructor(warningTime, timeoutTime) {
        this.warningTime = warningTime;  
        this.timeoutTime = timeoutTime;  
        this.startTime = Date.now();       
        this.lastTimerCheck = 0;    
        this.timeoutModal = null;
        this.fetchInProgressSince = 0;
        this.createDialog();
        this.addStorageEventListener();
        requestAnimationFrame(this.tick.bind(this)); 
    }

    addStorageEventListener() {
        window.addEventListener('storage', (event) => {
            if (event.key === 'SessionTimeoutWarning.reset') {
                this.doReset();
            }
        });
    }

    resetButtonHandler() {
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

        // TODO: Call real endpoint
        fetch('/refresh-token', { method: 'POST' })
            .then(response => {
                if (!response.ok) {
                    console.error('Failed to refresh token');
                }
            })
            .catch(error => {
                this.signOut();
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
        const elapsedTime = Date.now() - this.startTime;
        console.log("tick", elapsedTime, this.warningTime, this.timeoutTime)

        if (!this.timeoutModal.open && elapsedTime >= this.warningTime) {
            this.timeoutModal.showModal();
        }

        if (elapsedTime >= this.timeoutTime) {
            // TODO: Stop ticking once this has been called - otherwise if it gets called in a backgrounded tab, you end up with a ridiculous loop
            this.signOut();
        }

        requestAnimationFrame(this.tick.bind(this));
    }

    createDialog() {
        this.timeoutModal = document.createElement('dialog');
        const minutesLeft = Math.round((this.timeoutTime - this.warningTime) / 60000);

        // TODO: Translations
        // TODO: Gov styling to match FTS
        // TODO: Close button
        this.timeoutModal.innerHTML = `
            <p>Your session will time out soon. You will be signed out of your account if you do not respond in ${minutesLeft} minutes. We do this to keep your information secure.</p>
            <button id="session-timeout-warning-continue">Continue session</button>
            <button id="session-timeout-warning-sign-out">Sign out</button>
        `;
        document.body.appendChild(this.timeoutModal);

        this.timeoutModal.querySelector('#session-timeout-warning-continue').addEventListener('click', this.resetButtonHandler.bind(this));
        this.timeoutModal.querySelector('#session-timeout-warning-sign-out').addEventListener('click', this.signOut.bind(this));
    }

    signOut() {
        window.location.href = '/logged-out';
    }
}

export function initSessionTimeoutWarning(timeoutTime, warningTime) {
    const sessionTimeout = new SessionTimeoutWarning(
        warningTime,
        timeoutTime
    );
}