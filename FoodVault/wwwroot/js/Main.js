// Fridge expiry alerts and functionality
class FridgeManager {
    constructor() {
        this.initExpiryAlerts();
        this.initSearch();
    }

    initExpiryAlerts() {
        const expiringItems = document.querySelectorAll('.expiring-item');
        expiringItems.forEach(item => {
            const expiryDate = new Date(item.dataset.expiry);
            const today = new Date();
            const daysUntilExpiry = Math.ceil((expiryDate - today) / (1000 * 60 * 60 * 24));

            if (daysUntilExpiry < 0) {
                item.classList.add('expired');
                this.showExpiryAlert('Item has expired!', 'danger');
            } else if (daysUntilExpiry <= 3) {
                item.classList.add('expiring-soon');
                this.showExpiryAlert(`Item expires in ${daysUntilExpiry} days`, 'warning');
            }
        });
    }

    showExpiryAlert(message, type) {
        const alert = document.createElement('div');
        alert.className = `alert alert-${type} alert-dismissible fade show`;
        alert.innerHTML = `
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        `;
        document.querySelector('.alerts-container').appendChild(alert);
    }

    initSearch() {
        const searchForm = document.getElementById('fridge-search-form');
        if (searchForm) {
            searchForm.addEventListener('submit', (e) => {
                e.preventDefault();
                const query = document.getElementById('fridge-search-input').value;
                window.location.href = `/Fridge/SearchFridge?query=${encodeURIComponent(query)}`;
            });
        }
    }
}

// Initialize when document is ready
document.addEventListener('DOMContentLoaded', () => {
    new FridgeManager();
});