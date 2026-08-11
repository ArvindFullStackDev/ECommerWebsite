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

    // Global AJAX spinner
    $(document).ajaxStart(function () {
        $('.spinner-overlay').fadeIn(150);
    }).ajaxStop(function () {
        $('.spinner-overlay').fadeOut(150);
    });

    // Back to top button
    var backToTop = $('<button class="back-to-top" title="Back to top"><i class="fas fa-chevron-up"></i></button>');
    $('body').append(backToTop);
    $(window).on('scroll', function () {
        if ($(this).scrollTop() > 300) {
            backToTop.addClass('show');
        } else {
            backToTop.removeClass('show');
        }
    });
    backToTop.on('click', function () {
        $('html, body').animate({ scrollTop: 0 }, 400);
    });

    // Scroll reveal animations
    function revealOnScroll() {
        var windowTop = $(window).scrollTop() + $(window).height() * 0.9;
        $('.reveal').each(function () {
            if ($(this).offset().top < windowTop) {
                $(this).addClass('visible');
            }
        });
    }
    $(window).on('scroll', revealOnScroll);
    revealOnScroll();

    // Add to cart
    $(document).on('click', '.add-to-cart', function (e) {
        e.preventDefault();
        var productId = $(this).data('product-id');
        var quantity = $(this).data('quantity') || 1;
        var btn = $(this);

        $.ajax({
            url: '/Cart/AddToCart',
            type: 'POST',
            data: { productId: productId, quantity: quantity },
            success: function (response) {
                if (response.success) {
                    $('.cart-count').text(response.cartCount);
                    btn.addClass('btn-added');
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
        var btn = $(this);

        $.ajax({
            url: '/Wishlist/AddToWishlist',
            type: 'POST',
            data: { productId: productId },
            success: function (response) {
                if (response.success) {
                    $('.wishlist-count').text(response.wishlistCount);
                    btn.addClass('active');
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

    // Buy now: add to cart then go to checkout
    $(document).on('click', '.buy-now', function (e) {
        e.preventDefault();
        var productId = $(this).data('product-id');
        var quantity = $(this).closest('.product-info').find('.cart-qty-input').val() || 1;
        var btn = $(this);

        $.ajax({
            url: '/Cart/AddToCart',
            type: 'POST',
            data: { productId: productId, quantity: quantity },
            success: function (response) {
                if (response.success) {
                    $('.cart-count').text(response.cartCount);
                    window.location.href = '/Checkout';
                } else {
                    showToast(response.message || 'Please login first', 'error');
                    setTimeout(function () {
                        window.location.href = '/Account/Login?returnUrl=/Checkout';
                    }, 1500);
                }
            },
            error: function () {
                showToast('Error adding to cart', 'error');
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
    $(document).off('click', '.remove-cart-item').on('click', '.remove-cart-item', function (e) {
        e.preventDefault();
        var cartItemId = $(this).data('cart-item-id');

        $.ajax({
            url: '/Cart/RemoveFromCart',
            type: 'POST',
            dataType: 'json',
            data: { cartItemId: cartItemId },
            complete: function () {
                location.reload();
            }
        });
    });

    // Move saved item back to cart
    $(document).off('click', '.move-to-cart').on('click', '.move-to-cart', function (e) {
        e.preventDefault();
        var cartItemId = $(this).data('cart-item-id');

        $.ajax({
            url: '/Cart/MoveToCart',
            type: 'POST',
            dataType: 'json',
            data: { cartItemId: cartItemId },
            complete: function () {
                location.reload();
            }
        });
    });

    // Save for later
    $(document).off('click', '.save-for-later').on('click', '.save-for-later', function (e) {
        e.preventDefault();
        var cartItemId = $(this).data('cart-item-id');

        $.ajax({
            url: '/Cart/SaveForLater',
            type: 'POST',
            dataType: 'json',
            data: { cartItemId: cartItemId },
            complete: function () {
                location.reload();
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

    // Search suggestions with debounce
    var searchTimeout;
    $('.search-input').on('input', function () {
        clearTimeout(searchTimeout);
        var $input = $(this);
        var term = $input.val().trim();
        var $suggestions = $input.closest('.input-group').find('.search-suggestions');

        if (term.length < 2) {
            $suggestions.removeClass('show').empty();
            return;
        }

        searchTimeout = setTimeout(function () {
            $.ajax({
                url: '/Catalog/Suggestions',
                type: 'GET',
                data: { term: term },
                success: function (data) {
                    if (!data || !data.length) {
                        $suggestions.removeClass('show').empty();
                        return;
                    }
                    var html = data.map(function (p) {
                        var img = p.imageUrl || '/images/products/product-small.svg';
                        var price = p.discountedPrice || p.price || 0;
                        return '<div class="suggestion-item" data-id="' + p.id + '">' +
                            '<img src="' + img + '" onerror="this.src=\'/images/products/product-small.svg\'">' +
                            '<span class="flex-grow-1">' + p.name + '</span>' +
                            '<span class="product-price" style="font-size:13px;">$' + price + '</span>' +
                            '</div>';
                    }).join('');
                    $suggestions.html(html).addClass('show');
                }
            });
        }, 300);
    });

    // Click suggestion -> go to product
    $(document).on('click', '.suggestion-item', function () {
        var id = $(this).data('id');
        window.location.href = '/Catalog/Details/' + id;
    });

    // Close suggestions on outside click
    $(document).on('click', function (e) {
        if (!$(e.target).closest('.search-form').length) {
            $('.search-suggestions').removeClass('show').empty();
        }
    });

    // Notification: mark all read
    $(document).on('click', '.mark-all-read', function (e) {
        e.preventDefault();
        $.ajax({
            url: '/Notification/MarkAllAsRead',
            type: 'POST',
            success: function (response) {
                if (response.success) {
                    $('.notification-badge').remove();
                    $('.notification-item').removeClass('unread');
                    showToast('All notifications marked as read', 'success');
                }
            }
        });
    });
});