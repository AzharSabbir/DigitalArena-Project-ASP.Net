document.addEventListener('DOMContentLoaded', function () {
    // ✅ Define modal functions first
    window.openModal = function () {
        document.getElementById("uploadProductModal").style.display = "flex";
    };

    window.closeModal = function () {
        document.getElementById("uploadProductModal").style.display = "none";
    };

    window.onclick = function (event) {
        const modal = document.getElementById("uploadProductModal");
        if (event.target === modal) {
            modal.style.display = "none";
        }
    };

    const urlParams = new URLSearchParams(window.location.search);
    if (urlParams.get("fromLanding") === "true") {
        openModal();

        window.history.replaceState({}, document.title, window.location.pathname);
    }

    // ✅ Seller mode toggle logic
    const modeToggle = document.getElementById('mode-toggle');
    if (modeToggle) {
        modeToggle.addEventListener('change', function () {
            const isSeller = this.checked;
            const newRole = isSeller ? "SELLER" : "BUYER";

            fetch('@Url.Action("ToggleMode", "UserProfile")', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
                },
                body: JSON.stringify({ Role: newRole })
            })
                .then(res => res.json())
                .then(data => {
                    if (data.success) {
                        location.reload();
                    } else {
                        alert('Failed to toggle role.');
                    }
                });
        });
    }
});

async function confirmDelete(button) {
    const confirmed = await showConfirmDialog("Are you sure you want to delete this product?");
    if (!confirmed) return;

    const productId = button.getAttribute('data-product-id');

    fetch('@Url.Action("DeleteProduct", "UserProfile")', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
        },
        body: JSON.stringify({ productId: parseInt(productId) })
    })
        .then(response => {
            if (response.ok) {
                const productCard = button.closest('.enhanced-product-card');
                productCard.style.transition = 'opacity 0.3s';
                productCard.style.opacity = 0;
                setTimeout(() => productCard.remove(), 300);
            } else {
                alert("Failed to delete the product.");
            }
        })
        .catch(error => {
            console.error("Delete failed:", error);
            alert("Something went wrong.");
        });
}