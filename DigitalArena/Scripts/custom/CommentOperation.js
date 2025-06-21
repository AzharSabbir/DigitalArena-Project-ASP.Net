let selectedRating = 0;

const stars = document.querySelectorAll('#star-rating .star');
const emptyStar = '/Assets/images/star_empty.png';
const filledStar = '/Assets/images/img_star.png';

function highlightStars(rating, targetStars = stars) {
    targetStars.forEach(star => {
        const val = parseInt(star.dataset.value);
        star.src = val <= rating ? filledStar : emptyStar;
    });
}


// Top rating bar (for new comment)
stars.forEach(star => {
    star.addEventListener('mouseover', () => {
        highlightStars(parseInt(star.dataset.value));
    });

    star.addEventListener('mouseout', () => {
        highlightStars(selectedRating);
    });

    star.addEventListener('click', () => {
        const clickedValue = parseInt(star.dataset.value);
        selectedRating = selectedRating === clickedValue ? 0 : clickedValue;
        highlightStars(selectedRating);
    });
});


// ✅ Submit comment
document.getElementById('submit-comment').addEventListener('click', () => {
    const commentTextEl = document.getElementById('comment-text');
    const commentText = commentTextEl.value.trim();
    const productId = parseInt(document.getElementById('submit-comment').getAttribute('data-value'));

    if (selectedRating === 0 || commentText === '') {
        showInfoDialog('Please select a rating and enter your comment.');
        return;
    }

    fetch('/Review/AddReview', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({
            productId,
            rating: selectedRating,
            comment: commentText
        })
    })
        .then(async res => {
            const text = await res.text();
            if (!res.ok) throw new Error("Server returned error: " + text);
            return JSON.parse(text);
        })
        .then(async data => {
            if (data.success) {
                await showInfoDialog("Your comment is added and is pending approval.");
                location.reload();
            } else {
                showInfoDialog("Error: " + data.error);
            }
        })
        .catch(err => console.error(err));
});


// Edit/Delete Handler
document.getElementById('user-comment-list').addEventListener('click', (e) => {
    const commentItem = e.target.closest('.comment-item');
    if (!commentItem) return;

    const reviewId = commentItem.getAttribute('data-reviewid');
    const commentTextEl = commentItem.querySelector('.comment-text');
    const originalText = commentTextEl?.textContent.trim() ?? '';

    if (e.target.classList.contains('btn-delete')) {
        showConfirmDialog("Are you sure you want to delete this comment?")
            .then((confirmed) => {
                if (!confirmed) return;

                fetch('/Review/DeleteReview', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({ reviewId: parseInt(reviewId) })
                })
                    .then(res => res.json())
                    .then(data => {
                        if (data.success) {
                            commentItem.remove();
                            updateCommentCount();
                        } else {
                            showInfoDialog("Failed to delete: " + data.message);
                        }
                    });
            });
    }

    if (e.target.classList.contains('btn-edit')) {
        if (commentItem.querySelector('textarea')) return;

        const textarea = document.createElement('textarea');
        textarea.className = 'comment-edit-textarea';
        textarea.value = originalText;
        textarea.style.cssText = 'width: 100%; border-radius: 8px; padding: 8px; margin-top: 6px; font-size: 16px;';
        commentTextEl.replaceWith(textarea);
        textarea.focus();

        const starContainer = commentItem.querySelector('.rating-stars');
        const initialRating = parseFloat(starContainer.dataset.rating) || 0;
        selectedRating = initialRating;

        starContainer.innerHTML = '';
        for (let i = 1; i <= 5; i++) {
            const star = document.createElement('img');
            star.src = i <= selectedRating ? filledStar : emptyStar;
            star.alt = 'Star';
            star.dataset.value = i;
            star.className = 'star editable-star';
            star.style.cssText = 'width: 22px; height: 22px; cursor: pointer; margin-right: 4px;';
            starContainer.appendChild(star);
        }

        const editStars = starContainer.querySelectorAll('.editable-star');
        highlightStars(selectedRating, editStars);
        editStars.forEach(star => {
            star.addEventListener('mouseover', () => {
                highlightStars(parseInt(star.dataset.value), editStars);
            });
            star.addEventListener('mouseout', () => {
                highlightStars(selectedRating, editStars);
            });
            star.addEventListener('click', () => {
                const val = parseInt(star.dataset.value);
                selectedRating = selectedRating === val ? 0 : val;
                highlightStars(selectedRating, editStars);
            });
        });

        const actionsDiv = commentItem.querySelector('.comment-actions');
        actionsDiv.innerHTML = `
            <span class="text-action btn-save" data-reviewid="${reviewId}">💾 Save</span> |
            <span class="text-action btn-cancel" data-reviewid="${reviewId}">❌ Cancel</span>
        `;

        actionsDiv.querySelector('.btn-save').addEventListener('click', () => {
            const newText = textarea.value.trim();
            if (!newText) return showInfoDialog("Comment cannot be empty.");
            if (!selectedRating) return showInfoDialog("Please select a rating.");

            fetch('/Review/UpdateReview', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    reviewId: parseInt(reviewId),
                    comment: newText,
                    rating: selectedRating
                })
            })
                .then(res => res.json())
                .then(data => {
                    if (data.success) {
                        const updatedP = document.createElement('p');
                        updatedP.className = 'comment-text font-inria-sans-400-20 color-text-primary';
                        updatedP.textContent = newText;
                        textarea.replaceWith(updatedP);

                        starContainer.innerHTML = '';
                        for (let i = 1; i <= 5; i++) {
                            const starImg = document.createElement('img');
                            starImg.src = i <= selectedRating ? filledStar : emptyStar;
                            starImg.alt = 'Star';
                            starImg.className = 'star';
                            starContainer.appendChild(starImg);
                        }

                        actionsDiv.innerHTML = `
                            <span class="text-action btn-edit" data-reviewid="${reviewId}">🖋️ Edit</span> |
                            <span class="text-action btn-delete" data-reviewid="${reviewId}">🚫 Delete</span>
                        `;

                        showInfoDialog("Your comment was updated and is pending approval.");
                    } else {
                        showInfoDialog("Failed to update: " + data.message);
                    }
                });
        });

        actionsDiv.querySelector('.btn-cancel').addEventListener('click', () => {
            const originalP = document.createElement('p');
            originalP.className = 'comment-text font-inria-sans-400-20 color-text-primary';
            originalP.textContent = originalText;
            textarea.replaceWith(originalP);

            starContainer.innerHTML = '';
            for (let i = 1; i <= 5; i++) {
                const starImg = document.createElement('img');
                starImg.src = i <= selectedRating ? filledStar : emptyStar;
                starImg.alt = 'Star';
                starImg.className = 'star';
                starContainer.appendChild(starImg);
            }

            actionsDiv.innerHTML = `
                <span class="text-action btn-edit" data-reviewid="${reviewId}">🖋️ Edit</span> |
                <span class="text-action btn-delete" data-reviewid="${reviewId}">🚫 Delete</span>
            `;
        });
    }
});

// Suggested Products
document.addEventListener("DOMContentLoaded", () => {
    const slides = document.querySelectorAll(".suggested-product-slide");
    let currentIndex = 0;
    const visibleCount = 3; // show 3 at once

    function showSlides() {
        slides.forEach((slide, i) => {
            slide.style.display = (i >= currentIndex && i < currentIndex + visibleCount) ? "block" : "none";
        });
    }

    function rotateSlides() {
        currentIndex += visibleCount;
        if (currentIndex >= slides.length) currentIndex = 0;
        showSlides();
    }

    if (slides.length > visibleCount) {
        showSlides();
        setInterval(rotateSlides, 5000); // every 5 seconds
    } else {
        slides.forEach(slide => slide.style.display = "block");
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
function updateCommentCount() {
    const comments = document.querySelectorAll('.comment-item').length;
    const countElem = document.getElementById('comment-count');
    const labelElem = countElem.nextSibling;

    if (countElem) countElem.textContent = comments;
    if (labelElem?.nodeType === 3) {
        labelElem.textContent = comments === 1 ? " Comment" : " Comments";
    }
}
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


