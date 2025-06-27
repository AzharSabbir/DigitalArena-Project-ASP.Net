function toggleNotificationDropdown() {
    const dropdown = document.getElementById('notificationDropdown');
    dropdown.style.display = dropdown.style.display === 'none' ? 'block' : 'none';

    if (dropdown.style.display === 'block') {
        fetchNotifications();
    }
}

function fetchNotifications() {
    fetch('/UserProfile/GetNotifications')
        .then(res => res.json())
        .then(data => {
            const list = document.getElementById('notificationList');
            list.innerHTML = '';

            if (!data.success || !data.notifications || data.notifications.length === 0) {
                list.innerHTML = '<div class="notification-item">No notifications found.</div>';
                return;
            }

            data.notifications.forEach(n => {
                let badgeHtml = '';
                if (n.Status !== 'Sent') {
                    const badgeClass =
                        n.Status === 'Approved' ? 'badge-approved' :
                            n.Status === 'Rejected' ? 'badge-rejected' :
                                'badge-pending';

                    badgeHtml = `<span class="badge ${badgeClass}">${n.Status}</span>`;
                }

                const item = document.createElement('div');
                item.className = 'notification-item';
                item.innerHTML = `
                            <div class="info">
                                <div class="subject">${n.Subject}</div>
                                <div class="message">${n.Message}</div>
                                ${badgeHtml}
                            </div>
                            <div class="delete-btn" onclick="deleteNotification(${n.NotificationId})">&times;</div>
                        `;
                list.appendChild(item);
            });
        })
        .catch(err => {
            console.error("Failed to fetch notifications", err);
        });
}

function deleteNotification(id) {
    const tokenEl = document.querySelector('input[name="__RequestVerificationToken"]');
    const token = tokenEl ? tokenEl.value : '';

    fetch('/UserProfile/DeleteNotification', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'RequestVerificationToken': token
        },
        body: JSON.stringify({ id })
    })
        .then(res => res.json())
        .then(data => {
            if (data.success) {
                fetchNotifications();
            } else {
                alert('Failed to delete notification.');
            }
        })
        .catch(err => {
            console.error("Failed to delete notification", err);
        });
}