document.addEventListener('DOMContentLoaded', function () {
    const modeToggle = document.getElementById('mode-toggle');
    if (modeToggle) {
        modeToggle.addEventListener('change', function () {
            const isSeller = this.checked;
            const newRole = isSeller ? "SELLER" : "BUYER";

            fetch('@Url.Action("ToggleMode", "UserProfile")', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({ Role: newRole })
            })
                .then(res => res.json())
                .then(data => {
                    if (data.success) {
                        location.reload();
                    } else {
                        showInfoDialog('Failed to toggle role.');
                    }
                });
        });
    }

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

    const form = document.getElementById('uploadForm');
    const uploadSuccess = document.createElement('div');
    uploadSuccess.id = 'uploadSuccess';
    uploadSuccess.style.color = 'green';
    uploadSuccess.style.marginTop = '15px';
    uploadSuccess.style.display = 'none';
    uploadSuccess.style.fontWeight = '500';
    form.appendChild(uploadSuccess);

    form.addEventListener('submit', function (e) {
        e.preventDefault();

        if (!validateUploadForm()) return;

        const formData = new FormData(form);

        uploadSuccess.style.display = 'none';
        uploadSuccess.textContent = "";

        fetch(form.action, {
            method: 'POST',
            body: formData
        })
            .then(res => {
                if (res.ok) {
                    uploadSuccess.textContent = "Your product has been uploaded successfully and is currently pending administrative review.";
                    uploadSuccess.style.display = "block";

                    form.reset();

                    document.getElementById("Thumbnail").value = "";
                    document.getElementById("Files").value = "";

                    setTimeout(() => {
                        window.location.href = '@Url.Action("UserProfile", "UserProfile")';
                    }, 3000);
                }
                else {
                    showInfoDialog("Failed to upload the product. Please try again.");
                }
            })
            .catch(error => {
                console.error("Upload error:", error);
                showInfoDialog("An error occurred during upload.");
            });
    });
});

async function confirmDelete(button) {
    const confirmed = await showConfirmDialog("Are you sure you want to delete this product?");
    if (!confirmed) return;

    const productId = button.getAttribute('data-product-id');

    fetch('@Url.Action("DeleteProduct", "UserProfile")', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({ productId: parseInt(productId) })
    })
        .then(response => {
            if (response.ok) {
                console.log("Product deleted successfully.");
                const productCard = button.closest('.enhanced-product-card');
                productCard.style.transition = 'opacity 0.3s';
                productCard.style.opacity = 0;
                setTimeout(() => productCard.remove(), 300);
            } else {
                showInfoDialog("Failed to delete the product.");
            }
        })
        .catch(error => {
            console.error("Delete failed:", error);
            showInfoDialog("Something went wrong.");
        });
}
function validateUploadForm() {
    const name = document.getElementById("Name").value.trim();
    const description = document.getElementById("Description").value.trim();
    const price = document.getElementById("Price").value.trim();
    const category = document.getElementById("CategoryId").selectedOptions[0]?.text.trim();
    const thumbnail = document.getElementById("Thumbnail").files[0];
    const files = document.getElementById("Files").files;

    const nameError = document.getElementById("nameError");
    const descriptionError = document.getElementById("descriptionError");
    const priceError = document.getElementById("priceError");
    const categoryError = document.getElementById("categoryError");
    const thumbnailError = document.getElementById("thumbnailError");
    const filesError = document.getElementById("filesError");

    nameError.textContent = "";
    descriptionError.textContent = "";
    priceError.textContent = "";
    categoryError.textContent = "";
    thumbnailError.textContent = "";
    filesError.textContent = "";

    let isValid = true;

    if (!name) {
        nameError.textContent = "Product name is required.";
        isValid = false;
    }

    if (!description) {
        descriptionError.textContent = "Description is required.";
        isValid = false;
    }

    if (!price || parseFloat(price) <= 0) {
        priceError.textContent = "A valid price is required.";
        isValid = false;
    }

    const allowedCategories = [
        "3D Model",
        "E-Book",
        "Graphics Template",
        "Presentation Slide"
    ];

    if (!allowedCategories.includes(category)) {
        categoryError.textContent = "Please select a valid category.";
        isValid = false;
    }

    if (!thumbnail) {
        thumbnailError.textContent = "Thumbnail image is required.";
        isValid = false;
    } else {
        const thumbExt = thumbnail.name.substring(thumbnail.name.lastIndexOf(".")).toLowerCase();
        if (![".png", ".jpg", ".jpeg"].includes(thumbExt)) {
            thumbnailError.textContent = "Only .png, .jpg, or .jpeg formats are allowed for thumbnail.";
            isValid = false;
        }
    }

    if (files.length === 0) {
        filesError.textContent = "Please upload at least one product file.";
        isValid = false;
    } else {
        const allowedSingleFileTypes = {
            "3D Model": [".glb"],
            "E-Book": [".pdf"],
            "Presentation Slide": [".pptx"]
        };

        const allowedMultiFileTypes = {
            "Graphics Template": [".ai", ".jpg", ".jpeg", ".png"]
        };

        const getExtension = (filename) => filename.substring(filename.lastIndexOf(".")).toLowerCase();

        if (category in allowedSingleFileTypes) {
            if (files.length !== 1) {
                filesError.textContent = `${category} requires exactly one file.`;
                isValid = false;
            } else {
                const ext = getExtension(files[0].name);
                if (!allowedSingleFileTypes[category].includes(ext)) {
                    filesError.textContent = `Allowed file: ${allowedSingleFileTypes[category].join(", ")}`;
                    isValid = false;
                }
            }
        }

        if (category === "Graphics Template") {
            const hasAi = [...files].some(file => getExtension(file.name) === ".ai");
            if (!hasAi) {
                filesError.textContent = "Must include at least one .ai file.";
                isValid = false;
            }

            const invalidFiles = [...files].filter(file => !allowedMultiFileTypes["Graphics Template"].includes(getExtension(file.name)));
            if (invalidFiles.length > 0) {
                filesError.textContent = "Only .ai, .jpg, .jpeg, .png files are allowed.";
                isValid = false;
            }
        }
    }

    return isValid;
}