document.addEventListener('DOMContentLoaded', function () {
    // Dark mode toggle functionality
    const toggleBtn = document.getElementById('moon-btn');
    const darkModeEnabled = localStorage.getItem('darkMode') === 'on';

    // Apply dark mode if stored in local storage
    if (darkModeEnabled) {
        document.documentElement.classList.add('darkmode');
        document.getElementById('moon-img').src = '/images/sun.png';
    }

    // Toggle dark mode on button click
    toggleBtn.addEventListener('click', function () {
        const isDarkMode = document.documentElement.classList.toggle('darkmode');
        document.getElementById('moon-img').src = isDarkMode ? '/images/sun.png' : '/images/moon.png';
        localStorage.setItem('darkMode', isDarkMode ? 'on' : 'off');
    });
});
