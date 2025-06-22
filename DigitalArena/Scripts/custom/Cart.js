document.addEventListener('DOMContentLoaded', function () {
    const selectAllCheckbox = document.getElementById('select-all');
    const itemCheckboxes = document.querySelectorAll('.select-item');
    const cartItems = document.querySelectorAll('.cart-item');
    const subtotalDisplay = document.querySelector('.subtotal');
    const checkoutForm = document.getElementById('checkout-form');

    // Handle select all
    selectAllCheckbox.addEventListener('change', function () {
        itemCheckboxes.forEach((checkbox, index) => {
            checkbox.checked = this.checked;
            cartItems[index].classList.toggle('selected', this.checked);
        });
        updateSubtotal();
    });

    // Toggle item selection on card click
    cartItems.forEach((item, index) => {
        item.addEventListener('click', function (e) {
            if (e.target.tagName.toLowerCase() !== 'button' && e.target.type !== 'checkbox' && e.target.tagName.toLowerCase() !== 'a') {
                itemCheckboxes[index].checked = !itemCheckboxes[index].checked;
                item.classList.toggle('selected');
                updateSubtotal();
            }
        });

        // Reflect checkbox changes on class
        itemCheckboxes[index].addEventListener('change', function () {
            item.classList.toggle('selected', this.checked);
            updateSubtotal();
        });
    });

    // Update subtotal based on selected items
    function updateSubtotal() {
        let total = 0;
        itemCheckboxes.forEach((cb, idx) => {
            if (cb.checked) {
                const priceText = cartItems[idx].querySelector('.product-price').textContent.replace(/[^\d.]/g, '');
                total += parseFloat(priceText);
            }
        });
        subtotalDisplay.textContent = `Subtotal: ৳${total.toFixed(2)}`;
    }

    // Handle checkout submission
    checkoutForm.addEventListener('submit', function (e) {
        const selectedIds = Array.from(itemCheckboxes)
            .filter(cb => cb.checked)
            .map(cb => cb.value);

        if (selectedIds.length === 0) {
            e.preventDefault();
            alert('Please select at least one product to checkout.');
            return;
        }

        const input = document.createElement('input');
        input.type = 'hidden';
        input.name = 'selectedItemIds';
        input.value = selectedIds.join(',');
        checkoutForm.appendChild(input);
    });
});