// Main JavaScript for FoodVault MVC App
document.addEventListener('DOMContentLoaded', function () {
    initializeApp();
});

function initializeApp() {
    initMobileMenu();
    initRecipeModal();
    initPagination();
    initSetBg();
}

// Mobile Menu Functionality
function initMobileMenu() {
    const mobileMenuToggle = document.getElementById('mobile-menu-toggle');
    const mobileMenuClose = document.getElementById('mobileMenuClose');
    const mobileMenuOverlay = document.getElementById('mobileMenuOverlay');
    const mobileMenuSidebar = document.getElementById('mobileMenuSidebar');

    if (mobileMenuToggle) {
        mobileMenuToggle.addEventListener('click', function (e) {
            e.preventDefault();
            openMobileMenu();
        });
    }

    if (mobileMenuClose) {
        mobileMenuClose.addEventListener('click', closeMobileMenu);
    }

    if (mobileMenuOverlay) {
        mobileMenuOverlay.addEventListener('click', closeMobileMenu);
    }

    function openMobileMenu() {
        mobileMenuOverlay.classList.add('active');
        mobileMenuSidebar.classList.add('active');
        document.body.style.overflow = 'hidden';
    }

    function closeMobileMenu() {
        mobileMenuOverlay.classList.remove('active');
        mobileMenuSidebar.classList.remove('active');
        document.body.style.overflow = '';
    }

    // Close menu on window resize
    window.addEventListener('resize', function () {
        if (window.innerWidth > 767) {
            closeMobileMenu();
        }
    });
}

// Recipe Modal Functionality
function initRecipeModal() {
    const recipeModal = document.getElementById('recipeModal');
    const modalOverlay = document.getElementById('modalOverlay');
    const modalClose = document.getElementById('modalClose');
    const modalBody = document.getElementById('modalBody');

    if (!recipeModal) return;

    // Click recipe items to open modal
    document.querySelectorAll('.recipe__item').forEach(item => {
        item.addEventListener('click', function () {
            const recipeId = this.getAttribute('data-recipe') || '1';
            showRecipeModal(recipeId);
        });
    });

    // Save recipe buttons
    document.querySelectorAll('.save-recipe-btn').forEach(btn => {
        btn.addEventListener('click', function (e) {
            e.stopPropagation();
            const recipeId = this.getAttribute('data-recipe') || '1';
            saveRecipe(recipeId);
        });
    });

    // Close modal events
    if (modalOverlay) modalOverlay.addEventListener('click', closeModal);
    if (modalClose) modalClose.addEventListener('click', closeModal);

    function showRecipeModal(recipeId) {
        modalBody.innerHTML = `
            <div class="modal-recipe-content">
                <h3>Recipe Details</h3>
                <p>Showing details for recipe ID: ${recipeId}</p>
                <div class="recipe-info">
                    <p>This is where the full recipe details would appear.</p>
                    <p>Ingredients, instructions, cooking time, etc.</p>
                </div>
            </div>
        `;
        recipeModal.classList.add('active');
    }

    function closeModal() {
        recipeModal.classList.remove('active');
    }

    function saveRecipe(recipeId) {
        alert(`Recipe ${recipeId} saved to favorites!`);
        // Add actual save logic here
    }
}

// Pagination Functionality
function initPagination() {
    const paginations = document.querySelectorAll('.product-pagination');

    paginations.forEach(pagination => {
        const prevBtn = pagination.querySelector('.prev-btn');
        const nextBtn = pagination.querySelector('.next-btn');
        const pageBtns = pagination.querySelectorAll('.page-numbers .page-btn');
        const paginationInfo = pagination.querySelector('.pagination-info');

        let currentPage = 1;

        if (nextBtn) {
            nextBtn.addEventListener('click', function () {
                const totalPages = pageBtns.length;
                if (currentPage < totalPages) {
                    currentPage++;
                    updatePagination();
                }
            });
        }

        if (prevBtn) {
            prevBtn.addEventListener('click', function () {
                if (currentPage > 1) {
                    currentPage--;
                    updatePagination();
                }
            });
        }

        pageBtns.forEach(btn => {
            btn.addEventListener('click', function () {
                currentPage = parseInt(this.textContent);
                updatePagination();
            });
        });

        function updatePagination() {
            // Update active state
            pageBtns.forEach(btn => {
                btn.classList.remove('active');
                if (parseInt(btn.textContent) === currentPage) {
                    btn.classList.add('active');
                }
            });

            // Update button states
            if (prevBtn) prevBtn.disabled = currentPage === 1;
            if (nextBtn) nextBtn.disabled = currentPage === pageBtns.length;

            // Update info text
            if (paginationInfo) {
                const productsPerPage = 4;
                const startProduct = (currentPage - 1) * productsPerPage + 1;
                const endProduct = currentPage * productsPerPage;
                const totalProducts = pageBtns.length * productsPerPage;
                paginationInfo.textContent = `Showing ${startProduct}-${endProduct} of ${totalProducts} products`;
            }
        }

        updatePagination();
    });
}

// Background Image Helper
function initSetBg() {
    document.querySelectorAll('.set-bg').forEach(el => {
        const bg = el.getAttribute('data-setbg');
        if (bg) {
            el.style.backgroundImage = `url(${bg})`;
        }
    });
}

// Utility Functions
function showNotification(message, type = 'success') {
    // Simple notification implementation
    console.log(`${type.toUpperCase()}: ${message}`);
    // You can add toast notification here
}
// Search bar functionality
document.addEventListener('DOMContentLoaded', function () {
    // Tag click event
    document.querySelectorAll('.tag-badge').forEach(tag => {
        tag.addEventListener('click', function () {
            const tagName = this.getAttribute('data-tag');
            const searchInput = document.querySelector('.search-input');
            searchInput.value = tagName;
            searchInput.focus();

            // Trigger search (you can implement your search logic here)
            performSearch(tagName);
        });
    });

    // Search button click
    document.querySelector('.search-btn').addEventListener('click', function () {
        const searchTerm = document.querySelector('.search-input').value;
        performSearch(searchTerm);
    });

    // Enter key in search input
    document.querySelector('.search-input').addEventListener('keypress', function (e) {
        if (e.key === 'Enter') {
            performSearch(this.value);
        }
    });

    function performSearch(searchTerm) {
        if (searchTerm.trim()) {
            // Implement your search logic here
            console.log('Searching for:', searchTerm);
            // Example: window.location.href = `/Recipes/Search?q=${encodeURIComponent(searchTerm)}`;

            // Show loading animation
            const searchBtn = document.querySelector('.search-btn');
            searchBtn.innerHTML = '<i class="fas fa-spinner fa-spin"></i>';

            setTimeout(() => {
                searchBtn.innerHTML = '<i class="fas fa-search"></i>';
            }, 1000);
        }
    }
});