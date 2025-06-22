function showConfirmDialog(message) {
    return new Promise((resolve) => {
        const dialog = document.getElementById('confirm-dialog');
        const msg = document.getElementById('confirm-message');
        const yesBtn = document.getElementById('confirm-yes');
        const noBtn = document.getElementById('confirm-no');

        msg.textContent = message;
        dialog.classList.remove('hidden');

        const cleanUp = () => {
            dialog.classList.add('hidden');
            yesBtn.removeEventListener('click', onYes);
            noBtn.removeEventListener('click', onNo);
        };

        const onYes = () => { cleanUp(); resolve(true); };
        const onNo = () => { cleanUp(); resolve(false); };

        yesBtn.addEventListener('click', onYes);
        noBtn.addEventListener('click', onNo);
    });
}

function showInfoDialog(message) {
    return new Promise((resolve) => {
        const dialog = document.getElementById('info-dialog');
        const msg = document.getElementById('info-message');
        const okBtn = document.getElementById('info-ok');

        msg.textContent = message;
        dialog.classList.remove('hidden');

        const cleanUp = () => {
            dialog.classList.add('hidden');
            okBtn.removeEventListener('click', onOk);
        };

        const onOk = () => {
            cleanUp();
            resolve();
        };

        okBtn.addEventListener('click', onOk);
    });
}