document.addEventListener('DOMContentLoaded', function () {
    let currentSearchTerm = '';
    let currentMinPrice = 0;
    let currentMaxPrice = 1000;
    let currentActiveTags = [];
    let currentOtherFilter = '';

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

    function refreshProductVisibility() {
        const productCards = document.querySelectorAll('.product-card');

        productCards.forEach((card) => {
            const name = card.querySelector('.product-name')?.textContent.toLowerCase() || '';
            const priceText = card.querySelector('.product-price')?.textContent.replace(/[^\d.]/g, '') || '0';
            const price = parseFloat(priceText);
            const productId = parseInt(card.dataset.productId);
            const tags = getProductTags(productId);

            const matchesSearch = name.includes(currentSearchTerm);
            const matchesPrice = price >= currentMinPrice && price <= currentMaxPrice;
            const matchesTags = currentActiveTags.length === 0 || currentActiveTags.some(tag => tags.includes(tag));

            let matchesOther = true;
            if (currentOtherFilter === "FREE") {
                matchesOther = price === 0;
            } else if (currentOtherFilter === "PURCHASED") {
                matchesOther = purchasedIds.includes(productId);
            } else if (currentOtherFilter === "REVIEWED") {
                matchesOther = reviewedIds.includes(productId);
            }

            card.style.display = (matchesSearch && matchesPrice && matchesTags && matchesOther) ? 'flex' : 'none';
        });
    }

    function getProductTags(index) {
        return [];
    }

    const searchInput = document.getElementById('searchInput');
    if (searchInput) {
        searchInput.addEventListener('input', function (e) {
            currentSearchTerm = e.target.value.toLowerCase();
            refreshProductVisibility();
        });
    }

    function applySorting(sortType) {
        const productsGrid = document.getElementById('productsGrid');
        const productCards = Array.from(productsGrid.querySelectorAll('.product-card'));

        switch (sortType) {
            case 'POPULAR':
                productCards.sort((a, b) => {
                    const trendingA = parseFloat(a.dataset.trending || '0');
                    const trendingB = parseFloat(b.dataset.trending || '0');
                    return trendingB - trendingA;
                });
                break;

            case 'LOWEST PRICE':
                productCards.sort((a, b) => {
                    const priceA = parseFloat(a.querySelector('.product-price').textContent.replace(/[^\d.]/g, '') || '0');
                    const priceB = parseFloat(b.querySelector('.product-price').textContent.replace(/[^\d.]/g, '') || '0');
                    return priceA - priceB;
                });
                break;

            case 'HIGHEST PRICE':
                productCards.sort((a, b) => {
                    const priceA = parseFloat(a.querySelector('.product-price').textContent.replace(/[^\d.]/g, '') || '0');
                    const priceB = parseFloat(b.querySelector('.product-price').textContent.replace(/[^\d.]/g, '') || '0');
                    return priceB - priceA;
                });
                break;

            case 'PRODUCT A - Z':
                productCards.sort((a, b) => {
                    const nameA = a.querySelector('.product-name').textContent.toLowerCase();
                    const nameB = b.querySelector('.product-name').textContent.toLowerCase();
                    return nameA.localeCompare(nameB);
                });
                break;

            case 'PRODUCT Z - A':
                productCards.sort((a, b) => {
                    const nameA = a.querySelector('.product-name').textContent.toLowerCase();
                    const nameB = b.querySelector('.product-name').textContent.toLowerCase();
                    return nameB.localeCompare(nameA);
                });
                break;
        }

        productCards.forEach(card => productsGrid.appendChild(card));
    }

    window.toggleCheckbox = function (item) {
        const checkbox = item.querySelector('.checkbox');
        const isChecked = checkbox.classList.contains('checked');
        const label = item.querySelector('.checkbox-label').textContent.trim();
        const section = item.closest('section');
        const sectionTitle = section.querySelector('.section-title')?.textContent.trim().toUpperCase();

        const allCheckboxes = section.querySelectorAll('.checkbox');

        if (sectionTitle === "SORT BY") {
            allCheckboxes.forEach(cb => cb.classList.remove('checked'));
            checkbox.classList.add('checked');
            applySorting(label);
            return;
        }

        allCheckboxes.forEach(cb => cb.classList.remove('checked'));

        if (!isChecked) {
            checkbox.classList.add('checked');
            currentOtherFilter = label;
        } else {
            currentOtherFilter = "";
        }

        refreshProductVisibility();
    };


    window.toggleTag = function (button) {
        button.classList.toggle('active');
        currentActiveTags = Array.from(document.querySelectorAll('.tag-btn.active'))
            .map(btn => btn.textContent);
        refreshProductVisibility();
    };

    applySorting('POPULAR');

    document.querySelectorAll('.product-card').forEach(card => {
        card.addEventListener('click', function () {
            const productId = this.dataset.productId;
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
