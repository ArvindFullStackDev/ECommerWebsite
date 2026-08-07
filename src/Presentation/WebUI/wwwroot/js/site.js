// ECommerce - Site Scripts

$(function () {
    // Toast notification
    window.showToast = function (message, type) {
        type = type || 'success';
        var toast = $('#liveToast');
        var icon = type === 'success' ? 'fa-check-circle text-success' : 'fa-exclamation-circle text-danger';
        toast.find('.toast-header i').attr('class', 'fas ' + icon + ' me-2');
        toast.find('.toast-body').text(message);
        var bsToast = new bootstrap.Toast(toast);
        bsToast.show();
    };

    // Add to cart
    $(document).on('click', '.add-to-cart', function (e) {
        e.preventDefault();
        var productId = $(this).data('product-id');
        var quantity = $(this).data('quantity') || 1;

        $.ajax({
            url: '/Cart/AddToCart',
            type: 'POST',
            data: { productId: productId, quantity: quantity },
            success: function (response) {
                if (response.success) {
                    $('.cart-count').text(response.cartCount);
                    showToast('Added to cart!', 'success');
                } else {
                    showToast(response.message || 'Error adding to cart', 'error');
                }
            },
            error: function () {
                showToast('Error adding to cart', 'error');
            }
        });
    });

    // Add to wishlist
    $(document).on('click', '.add-to-wishlist', function (e) {
        e.preventDefault();
        var productId = $(this).data('product-id');

        $.ajax({
            url: '/Wishlist/AddToWishlist',
            type: 'POST',
            data: { productId: productId },
            success: function (response) {
                if (response.success) {
                    $('.wishlist-count').text(response.wishlistCount);
                    showToast('Added to wishlist!', 'success');
                } else {
                    showToast(response.message || 'Error adding to wishlist', 'error');
                }
            },
            error: function () {
                showToast('Please login to add to wishlist', 'error');
            }
        });
    });

    // Update cart quantity
    $(document).on('click', '.qty-minus, .qty-plus', function () {
        var input = $(this).closest('.qty-selector').find('input');
        var currentVal = parseInt(input.val());
        if ($(this).hasClass('qty-minus') && currentVal > 1) {
            input.val(currentVal - 1);
        } else if ($(this).hasClass('qty-plus')) {
            input.val(currentVal + 1);
        }
        input.trigger('change');
    });

    // Update cart on quantity change
    $(document).on('change', '.cart-qty-input', function () {
        var cartItemId = $(this).data('cart-item-id');
        var quantity = $(this).val();

        $.ajax({
            url: '/Cart/UpdateQuantity',
            type: 'POST',
            data: { cartItemId: cartItemId, quantity: quantity },
            success: function (response) {
                if (response.success) {
                    location.reload();
                }
            }
        });
    });

    // Remove cart item
    $(document).on('click', '.remove-cart-item', function (e) {
        e.preventDefault();
        if (!confirm('Remove this item from cart?')) return;
        var cartItemId = $(this).data('cart-item-id');

        $.ajax({
            url: '/Cart/RemoveFromCart',
            type: 'POST',
            data: { cartItemId: cartItemId },
            success: function (response) {
                if (response.success) {
                    $('.cart-count').text(response.cartCount);
                    location.reload();
                }
            }
        });
    });

    // Product image gallery
    $(document).on('click', '.thumb-img', function () {
        var src = $(this).data('image');
        $('.gallery-img').attr('src', src);
        $('.thumb-img').removeClass('active');
        $(this).addClass('active');
    });

    // Flash sale countdown
    function updateCountdown() {
        $('.flash-countdown').each(function () {
            var endDate = new Date($(this).data('end')).getTime();
            var now = new Date().getTime();
            var diff = endDate - now;

            if (diff > 0) {
                var hours = Math.floor((diff % (1000 * 60 * 60 * 24)) / (1000 * 60 * 60));
                var minutes = Math.floor((diff % (1000 * 60 * 60)) / (1000 * 60));
                var seconds = Math.floor((diff % (1000 * 60)) / 1000);
                $(this).text(hours + 'h ' + minutes + 'm ' + seconds + 's');
            } else {
                $(this).text('Sale ended');
            }
        });
    }

    setInterval(updateCountdown, 1000);
    updateCountdown();

    // Auto search with debounce
    var searchTimeout;
    $('.search-input').on('input', function () {
        clearTimeout(searchTimeout);
        var term = $(this).val();
        if (term.length < 3) return;

        searchTimeout = setTimeout(function () {
            $.ajax({
                url: '/Catalog/Suggestions',
                type: 'GET',
                data: { term: term },
                success: function (data) {
                    // Handle suggestions dropdown
                }
            });
        }, 500);
    });
});
