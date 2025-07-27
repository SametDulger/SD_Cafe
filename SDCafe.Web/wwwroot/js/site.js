// SDCafe Site JavaScript

// Bootstrap 5 Tooltip initialization
document.addEventListener('DOMContentLoaded', function() {
    // Initialize tooltips
    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    var tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });

    // Initialize popovers
    var popoverTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="popover"]'));
    var popoverList = popoverTriggerList.map(function (popoverTriggerEl) {
        return new bootstrap.Popover(popoverTriggerEl);
    });

    // Auto-hide alerts after 5 seconds
    var alerts = document.querySelectorAll('.alert-dismissible');
    alerts.forEach(function(alert) {
        setTimeout(function() {
            var bsAlert = new bootstrap.Alert(alert);
            bsAlert.close();
        }, 5000);
    });

    // Form validation enhancement
    var forms = document.querySelectorAll('.needs-validation');
    Array.prototype.slice.call(forms).forEach(function(form) {
        form.addEventListener('submit', function(event) {
            if (!form.checkValidity()) {
                event.preventDefault();
                event.stopPropagation();
            }
            form.classList.add('was-validated');
        }, false);
    });

    // Table row click handlers
    var tableRows = document.querySelectorAll('.table tbody tr[data-href]');
    tableRows.forEach(function(row) {
        row.addEventListener('click', function() {
            window.location.href = this.dataset.href;
        });
        row.style.cursor = 'pointer';
    });

    // Confirm delete buttons
    var deleteButtons = document.querySelectorAll('.btn-delete');
    deleteButtons.forEach(function(button) {
        button.addEventListener('click', function(e) {
            if (!confirm('Bu öğeyi silmek istediğinizden emin misiniz?')) {
                e.preventDefault();
            }
        });
    });

    // Real-time order status updates (if on kitchen page)
    if (window.location.pathname.includes('/Kitchen')) {
        updateOrderStatus();
        setInterval(updateOrderStatus, 30000); // Update every 30 seconds
    }
});

// Function to update order status
function updateOrderStatus() {
    fetch('/Kitchen/GetActiveOrders')
        .then(response => response.json())
        .then(data => {
            // Update order status display
            data.forEach(order => {
                var orderElement = document.getElementById('order-' + order.id);
                if (orderElement) {
                    var statusElement = orderElement.querySelector('.order-status');
                    if (statusElement) {
                        statusElement.textContent = order.status;
                        statusElement.className = 'order-status order-status-' + order.status.toLowerCase();
                    }
                }
            });
        })
        .catch(error => console.error('Error updating order status:', error));
}

// Function to format currency
function formatCurrency(amount) {
    return new Intl.NumberFormat('tr-TR', {
        style: 'currency',
        currency: 'TRY'
    }).format(amount);
}

// Function to format date
function formatDate(dateString) {
    return new Date(dateString).toLocaleDateString('tr-TR', {
        year: 'numeric',
        month: 'long',
        day: 'numeric',
        hour: '2-digit',
        minute: '2-digit'
    });
} 