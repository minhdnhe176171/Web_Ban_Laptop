// ============================================
// LAPTOPSTORE - GLOBAL JAVASCRIPT FUNCTIONS
// ============================================

// Cart functionality
function addToCart(productId, quantity = 1) {
    $.ajax({
        url: '/Cart/AddToCart',
        type: 'POST',
        data: { productId: productId, quantity: quantity },
        success: function(response) {
            if (response.success) {
                showNotification(response.message, 'success');
                updateCartBadge(response.cartCount);
            } else {
                showNotification(response.message, 'error');
            }
        },
        error: function() {
            showNotification('Có lỗi xảy ra. Vui lòng thử lại.', 'error');
        }
    });
}

function updateCartBadge(count) {
    const badge = $('.cart-badge');
    if (count > 0) {
        if (badge.length === 0) {
            $('.cart-icon-wrapper').append('<span class="cart-badge">' + count + '</span>');
        } else {
            badge.text(count).show();
        }
    } else {
        badge.hide();
    }
}

// Quick View functionality
function quickView(productId) {
    $('#quickViewModal').modal('show');
    $('#quickViewContent').html('<div style="text-align: center; padding: 40px;"><i class="fas fa-spinner fa-spin" style="font-size: 32px;"></i></div>');
    
    $.ajax({
        url: '/Shop/QuickView',
        type: 'GET',
        data: { id: productId },
        success: function(response) {
            $('#quickViewContent').html(response);
        },
        error: function() {
            $('#quickViewContent').html('<div style="text-align: center; padding: 40px; color: var(--danger);">Có lỗi xảy ra khi tải sản phẩm</div>');
        }
    });
}

// Wishlist functionality
function toggleWishlist(productId) {
    $.ajax({
        url: '/Wishlist/Toggle',
        type: 'POST',
        data: { productId: productId },
        success: function(response) {
            if (response.success) {
                // Update wishlist icon
                var btn = event.target.closest('.product-action-btn');
                if (btn) {
                    if (response.isInWishlist) {
                        btn.innerHTML = '<i class="fas fa-heart" style="color: var(--danger);"></i>';
                        btn.style.color = 'var(--danger)';
                    } else {
                        btn.innerHTML = '<i class="fas fa-heart"></i>';
                        btn.style.color = '';
                    }
                }
                // Update wishlist count in header if exists
                if (response.count !== undefined) {
                    $('.wishlist-count').text(response.count);
                }
                showNotification(response.message || (response.isInWishlist ? 'Đã thêm vào yêu thích' : 'Đã xóa khỏi yêu thích'));
            } else {
                showNotification(response.message || 'Có lỗi xảy ra', 'error');
            }
        },
        error: function() {
            showNotification('Có lỗi xảy ra. Vui lòng thử lại.', 'error');
        }
    });
}

// Compare functionality
function addToCompare(productId) {
    $.ajax({
        url: '/Compare/AddToCompare',
        type: 'POST',
        data: { productId: productId },
        success: function(response) {
            if (response.success) {
                // Update compare count
                if (response.count !== undefined) {
                    $('.compare-count').text(response.count);
                }
                showNotification(response.message, 'success');
            } else {
                showNotification(response.message || 'Có lỗi xảy ra', 'warning');
            }
        },
        error: function() {
            showNotification('Có lỗi xảy ra. Vui lòng thử lại.', 'error');
        }
    });
}

// Notification system
function showNotification(message, type = 'success') {
    // Remove existing notifications
    $('.custom-notification').remove();
    
    var bgColor = type === 'success' ? 'var(--razer-green)' : 
                  type === 'error' ? 'var(--danger)' : 
                  type === 'warning' ? 'var(--warning)' : 'var(--razer-green)';
    
    var icon = type === 'success' ? 'fa-check-circle' : 
               type === 'error' ? 'fa-exclamation-circle' : 
               type === 'warning' ? 'fa-exclamation-triangle' : 'fa-info-circle';
    
    var notification = $('<div class="custom-notification">').css({
        'position': 'fixed',
        'top': '20px',
        'right': '20px',
        'background': bgColor,
        'color': 'var(--razer-black)',
        'padding': '15px 25px',
        'border-radius': '8px',
        'z-index': '10000',
        'box-shadow': '0 0 20px rgba(0, 255, 0, 0.4)',
        'font-weight': '600',
        'display': 'flex',
        'align-items': 'center',
        'gap': '10px',
        'min-width': '300px',
        'animation': 'slideInRight 0.3s ease'
    });
    
    notification.html('<i class="fas ' + icon + '"></i><span>' + message + '</span>');
    
    $('body').append(notification);
    
    setTimeout(function() {
        notification.fadeOut(300, function() { 
            $(this).remove(); 
        });
    }, 3000);
}

// Chat Support
function openChat() {
    showNotification('Tính năng chat hỗ trợ đang được phát triển. Vui lòng liên hệ qua hotline: 1900.5301', 'info');
}

// Cart page functions
function updateQuantity(productId, quantity) {
    if (quantity < 1) quantity = 1;
    
    $.ajax({
        url: '/Cart/UpdateQuantity',
        type: 'POST',
        data: { productId: productId, quantity: quantity },
        success: function(response) {
            if (response.success) {
                $('#qty-' + productId).val(response.quantity || quantity);
                $('#subtotal-' + productId).text(response.subTotal.toLocaleString('vi-VN') + '₫');
                $('#cart-total').text(response.cartTotal.toLocaleString('vi-VN') + '₫');
                $('#cart-total-final').text(response.cartTotal.toLocaleString('vi-VN') + '₫');
                
                updateCartBadge(response.cartCount);
            }
        },
        error: function() {
            showNotification('Có lỗi xảy ra. Vui lòng thử lại.', 'error');
        }
    });
}

function removeItem(productId) {
    if (!confirm('Bạn có chắc chắn muốn xóa sản phẩm này khỏi giỏ hàng?')) {
        return;
    }

    $.ajax({
        url: '/Cart/RemoveItem',
        type: 'POST',
        data: { productId: productId },
        success: function(response) {
            if (response.success) {
                $('#cart-item-' + productId).fadeOut(function() {
                    $(this).remove();
                    if ($('.cart-item').length === 0) {
                        location.reload();
                    }
                });
                $('#cart-total').text(response.cartTotal.toLocaleString('vi-VN') + '₫');
                $('#cart-total-final').text(response.cartTotal.toLocaleString('vi-VN') + '₫');
                
                updateCartBadge(response.cartCount);
                showNotification('Đã xóa sản phẩm khỏi giỏ hàng', 'success');
            }
        },
        error: function() {
            showNotification('Có lỗi xảy ra. Vui lòng thử lại.', 'error');
        }
    });
}

// Compare page functions
function removeFromCompare(productId) {
    $.ajax({
        url: '/Compare/RemoveFromCompare',
        type: 'POST',
        data: { productId: productId },
        success: function(response) {
            if (response.success) {
                showNotification('Đã xóa sản phẩm khỏi danh sách so sánh', 'success');
                setTimeout(function() {
                    location.reload();
                }, 500);
            }
        },
        error: function() {
            showNotification('Có lỗi xảy ra. Vui lòng thử lại.', 'error');
        }
    });
}

// Product Details functions
function increaseQuantity() {
    const input = document.getElementById('quantity');
    input.value = parseInt(input.value) + 1;
}

function decreaseQuantity() {
    const input = document.getElementById('quantity');
    if (parseInt(input.value) > 1) {
        input.value = parseInt(input.value) - 1;
    }
}

// Quick View Add to Cart
function addToCartQuick(productId) {
    $.ajax({
        url: '/Cart/AddToCart',
        type: 'POST',
        data: { productId: productId, quantity: 1 },
        success: function(response) {
            if (response.success) {
                $('#quickViewModal').modal('hide');
                showNotification('Đã thêm vào giỏ hàng!', 'success');
                updateCartBadge(response.cartCount);
            } else {
                showNotification(response.message || 'Có lỗi xảy ra', 'error');
            }
        },
        error: function() {
            showNotification('Có lỗi xảy ra. Vui lòng thử lại.', 'error');
        }
    });
}

// Initialize on page load
$(document).ready(function() {
    // Update cart badge on page load
    // This will be handled by server-side rendering
    
    // Add CSS animation for notifications
    if (!$('#notification-styles').length) {
        $('head').append(`
            <style id="notification-styles">
                @keyframes slideInRight {
                    from {
                        transform: translateX(100%);
                        opacity: 0;
                    }
                    to {
                        transform: translateX(0);
                        opacity: 1;
                    }
                }
            </style>
        `);
    }
    
    // Handle form submissions with loading states
    $('form').on('submit', function() {
        var submitBtn = $(this).find('button[type="submit"]');
        if (submitBtn.length) {
            submitBtn.prop('disabled', true).html('<i class="fas fa-spinner fa-spin"></i> Đang xử lý...');
        }
    });
});
