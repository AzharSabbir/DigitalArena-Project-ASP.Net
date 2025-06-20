let selectedRating = 0;
const starContainer = document.getElementById('star-rating');
const stars = starContainer.querySelectorAll('.star');

// ⭐ Hover effect and selection
stars.forEach(star => {
    star.addEventListener('mouseover', () => {
        const hoverVal = parseInt(star.dataset.value);
        highlightStars(hoverVal);
    });

    star.addEventListener('mouseout', () => {
        highlightStars(selectedRating);
    });

    star.addEventListener('click', () => {
        selectedRating = parseInt(star.dataset.value);
        highlightStars(selectedRating);
    });
});

function highlightStars(rating) {
    stars.forEach(star => {
        const val = parseInt(star.dataset.value);
        star.src = val <= rating
            ? '/Assets/images/img_star.png'
            : '/Assets/images/star_empty.png';
    });
}

// ✅ Submit comment with AJAX to controller
document.getElementById('submit-comment').addEventListener('click', () => {
    const commentText = document.getElementById('comment-text').value.trim();
    const productId = parseInt(document.getElementById('submit-comment').getAttribute('data-value'));

    if (selectedRating === 0 || commentText === '') {
        alert('Please select a rating and enter your comment.');
        return;
    }
    console.log("Submitting comment...");

    // Send POST to controller
    fetch('/Review/AddReview', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'RequestVerificationToken': getAntiForgeryToken()
        },
        body: JSON.stringify({
            productId: productId,
            rating: selectedRating,
            comment: commentText
        })
    })
        .then(res => res.json())
        .then(data => {
            if (data.success) {
                const list = document.getElementById('user-comment-list');
                const commentHtml = `
    <div class="comment-item" data-reviewid="${data.reviewId}">
        <img src="/Assets/images/placeholder_avatar.png" alt="User avatar" class="user-avatar-placeholder">
        <div class="comment-content">
            <div class="comment-header">
                <span class="comment-author">${data.username}</span>
                <span class="comment-date">${data.createdAt}</span>
                <span class="comment-status">(Pending)</span>
            </div>
            <div class="rating-stars">
                ${renderStars(selectedRating)}
            </div>
            <p class="comment-text">${escapeHtml(commentText)}</p>
            <div class="comment-actions">
                <button class="btn-edit btn btn-sm btn-secondary" data-reviewid="${data.reviewId}">Edit</button>
                <button class="btn-delete btn btn-sm btn-danger" data-reviewid="${data.reviewId}">Delete</button>
            </div>
        </div>
    </div>
`;
                list.insertAdjacentHTML('afterbegin', commentHtml);
                updateCommentCount();

                // Reset form
                selectedRating = 0;
                document.getElementById('comment-text').value = '';
                highlightStars(0);
            } else {
                alert("Error: " + data.error);
            }
        });
});

// ✏️ Edit / ❌ Delete comment
document.getElementById('user-comment-list').addEventListener('click', (e) => {
    const commentItem = e.target.closest('.comment-item');
    if (!commentItem) return;

    const commentText = commentItem.querySelector('.comment-text');
    const reviewId = commentItem.getAttribute('data-reviewid');

    if (e.target.classList.contains('btn-delete')) {
        if (!confirm("Are you sure you want to delete this comment?")) return;

        fetch(`/Review/DeleteReview`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': getAntiForgeryToken()
            },
            body: JSON.stringify({ reviewId: parseInt(reviewId) })
        })
            .then(res => res.json())
            .then(data => {
                if (data.success) {
                    commentItem.remove();
                    updateCommentCount();
                } else {
                    alert("Failed to delete: " + data.message);
                }
            });
    }

    if (e.target.classList.contains('btn-edit')) {
        const newText = prompt('Edit your comment:', commentText.textContent);
        if (newText !== null && newText.trim() !== '') {
            fetch(`/Review/UpdateReview`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': getAntiForgeryToken()
                },
                body: JSON.stringify({
                    reviewId: parseInt(reviewId),
                    comment: newText.trim(),
                    rating: 5 // Optional: use previous or allow editing stars too
                })
            })
                .then(res => res.json())
                .then(data => {
                    if (data.success) {
                        commentText.textContent = newText.trim();
                    } else {
                        alert("Failed to update: " + data.message);
                    }
                });
        }
    }
});


// Helpers
function renderStars(count) {
    let stars = '';
    for (let i = 1; i <= 5; i++) {
        stars += `<img src="/Assets/images/${i <= count ? 'img_star.png' : 'star_empty.png'}" alt="Star" class="star" />`;
    }
    return stars;
}

function escapeHtml(text) {
    return text.replace(/[&<>"']/g, function (char) {
        return ({
            '&': '&amp;',
            '<': '&lt;',
            '>': '&gt;',
            '"': '&quot;',
            "'": '&#039;'
        })[char];
    });
}

function getAntiForgeryToken() {
    const token = document.querySelector('input[name="__RequestVerificationToken"]');
    return token ? token.value : '';
}

function updateCommentCount() {
    const comments = document.querySelectorAll('.comment-item').length;
    const countElem = document.getElementById('comment-count');
    const labelElem = countElem.nextSibling;

    if (countElem) countElem.textContent = comments;
    if (labelElem?.nodeType === 3) {
        labelElem.textContent = comments === 1 ? " Comment" : " Comments";
    }
}
