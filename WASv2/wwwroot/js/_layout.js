// wwwroot/js/site.js

// Highlight active menu item based on current URL
document.addEventListener('DOMContentLoaded', function () {
    highlightActiveMenuItem();

    // Add any other initialization code here
    initializeTooltips();
});

// Function to highlight active menu item
function highlightActiveMenuItem() {
    const currentUrl = window.location.pathname.toLowerCase();
    const navLinks = document.querySelectorAll('#sidebar .nav-link');

    // Remove active class from all links first
    navLinks.forEach(link => {
        link.classList.remove('active');
    });

    // Add active class to current page link
    navLinks.forEach(link => {
        const href = link.getAttribute('href').toLowerCase();

        // Check if current URL matches the link
        if (currentUrl === href.toLowerCase() ||
            (currentUrl.includes(href.toLowerCase()) && href !== '/')) {
            link.classList.add('active');
        }

        // Special case for root/home
        if (currentUrl === '/' && href === '/') {
            link.classList.add('active');
        }
    });
}

// Initialize Bootstrap tooltips if needed
function initializeTooltips() {
    // Check if Bootstrap is loaded and tooltips are available
    if (typeof bootstrap !== 'undefined' && bootstrap.Tooltip) {
        var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
        tooltipTriggerList.map(function (tooltipTriggerEl) {
            return new bootstrap.Tooltip(tooltipTriggerEl);
        });
    }
}

// Confirm logout action (optional)
function confirmLogout() {
    return confirm('Are you sure you want to logout?');
}

// Handle responsive sidebar toggle (if needed)
function toggleSidebar() {
    const sidebar = document.getElementById('sidebar');
    const content = document.getElementById('content');

    if (window.innerWidth <= 768) {
        sidebar.classList.toggle('show');
    }
}

// Export functions if using modules (optional)
// export { highlightActiveMenuItem, confirmLogout, toggleSidebar };