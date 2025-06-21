document.addEventListener('DOMContentLoaded', function () {
    // Globals
    let currentSearchTerm = '';
    let currentMinPrice = 0;
    let currentMaxPrice = 1000;
    let currentActiveTags = [];

    // Price Range slider init
    const priceSlider = document.getElementById('price-slider');
    if (priceSlider) {
        noUiSlider.create(priceSlider, {
            start: [0, 1000],
            connect: true,
            range: { min: 0, max: 1000 },
            step: 10,
            tooltips: true,
            format: {
                to: value => Math.round(value),
                from: value => Number(value)
            }
        });

        priceSlider.noUiSlider.on('update', (values) => {
            currentMinPrice = parseFloat(values[0]);
            currentMaxPrice = parseFloat(values[1]);
            refreshProductVisibility();
        });
    }

    // Filtering function
    function refreshProductVisibility() {
        const productCards = document.querySelectorAll('.product-card');

        productCards.forEach((card, index) => {
            const name = card.querySelector('.product-name')?.textContent.toLowerCase() || '';
            const priceText = card.querySelector('.product-price')?.textContent.replace(/[^\d.]/g, '') || '0';
            const price = parseFloat(priceText);
            const tags = getProductTags(index);

            const matchesSearch = name.includes(currentSearchTerm);
            const matchesPrice = price >= currentMinPrice && price <= currentMaxPrice;
            const matchesTags = currentActiveTags.length === 0 || currentActiveTags.some(tag => tags.includes(tag));

            card.style.display = (matchesSearch && matchesPrice && matchesTags) ? 'flex' : 'none';
        });
    }

    // Optional: Stub for tag logic (modify as needed)
    function getProductTags(index) {
        return []; // Implement if you store tags per product
    }

    // Search Input
    const searchInput = document.getElementById('searchInput');
    if (searchInput) {
        searchInput.addEventListener('input', function (e) {
            currentSearchTerm = e.target.value.toLowerCase();
            refreshProductVisibility();
        });
    }

    // Sorting logic
    function applySorting(sortType) {
        const productsGrid = document.getElementById('productsGrid');
        const productCards = Array.from(productsGrid.querySelectorAll('.product-card'));

        switch (sortType) {
            //case 'POPULAR':
            //    productCards.sort((a, b) => {
            //        const likesA = parseInt(a.querySelector('.likes .stat-text').textContent);
            //        const likesB = parseInt(b.querySelector('.likes .stat-text').textContent);
            //        return likesB - likesA;
            //    });
            //    break;
            case 'LOWEST PRICE':
                productCards.sort((a, b) => {
                    const priceA = parseInt(a.querySelector('.product-price').textContent.replace(/[^\d]/g, ''));
                    const priceB = parseInt(b.querySelector('.product-price').textContent.replace(/[^\d]/g, ''));
                    return priceA - priceB;
                });
                break;
            case 'HIGHEST PRICE':
                productCards.sort((a, b) => {
                    const priceA = parseInt(a.querySelector('.product-price').textContent.replace(/[^\d]/g, ''));
                    const priceB = parseInt(b.querySelector('.product-price').textContent.replace(/[^\d]/g, ''));
                    return priceB - priceA;
                });
                break;
            case 'PRODUCT A - Z':
                productCards.sort((a, b) => {
                    const nameA = a.querySelector('.product-name').textContent;
                    const nameB = b.querySelector('.product-name').textContent;
                    return nameA.localeCompare(nameB);
                });
                break;
            case 'PRODUCT Z - A':
                productCards.sort((a, b) => {
                    const nameA = a.querySelector('.product-name').textContent;
                    const nameB = b.querySelector('.product-name').textContent;
                    return nameB.localeCompare(nameA);
                });
                break;
        }

        productCards.forEach(card => productsGrid.appendChild(card));
    }

    // Toggle sorting checkbox
    window.toggleCheckbox = function (item) {
        const checkbox = item.querySelector('.checkbox');
        const isChecked = checkbox.classList.contains('checked');
        const section = item.closest('section');
        const allCheckboxes = section.querySelectorAll('.checkbox');
        allCheckboxes.forEach(cb => cb.classList.remove('checked'));

        if (!isChecked) {
            checkbox.classList.add('checked');
        }

        applySorting(item.querySelector('.checkbox-label').textContent);
    };

    // Toggle tag buttons
    window.toggleTag = function (button) {
        button.classList.toggle('active');
        currentActiveTags = Array.from(document.querySelectorAll('.tag-btn.active'))
            .map(btn => btn.textContent);
        refreshProductVisibility();
    };

    // Initial sorting
    applySorting('POPULAR');

    // Hover + click card behavior
    document.querySelectorAll('.product-card').forEach(card => {
        card.addEventListener('click', function () {
            const productId = this.dataset.productId;
            const productName = this.querySelector('.product-name').textContent;
            if (!productId) {
                alert("No product ID found!");
                return;
            }
            window.location.href = `/ProductDetails/ProductDetails/${productId}`;
        });

        card.addEventListener('mouseenter', function () {
            this.style.transform = 'translateY(-5px)';
            this.style.transition = 'transform 0.3s ease';
        });

        card.addEventListener('mouseleave', function () {
            this.style.transform = 'translateY(0)';
        });
    });
});
