/*
    This script provides functionality to automatically close the navigation
    toggler, change the expand button text to "Collapse" on click, to open
    collapsed sections, to change skill icon color on project hover, to toggle
    dark mode, to change the background based on mouse position, and to track
    users' eye' location when it is toggled.

*/

/**
    Automatically closes the navigation list upon clicking one of its anchors.
*/
function closeNavList() {
    if (window.innerWidth < 710) {
        const navButton = document.getElementById("toggler");
        navButton.click();
    }
}

/**
    Expands the education and experiences collapsible on clicking the
    "My Credentials" button.
*/
function openEduExp() {
    const expandButton = document.getElementById("edu-exp-expand");
    if (expandButton.innerHTML === 'Expand<br>∨') {
        expandButton.click();
    }
}

/**
    Expands the skills collapsible on clicking the
    "more" button.
*/
function openSkills() {
    const skillExpandButton = document.getElementById("skill-expand-button")
    if (skillExpandButton.innerHTML === 'Expand<br>∨') {
        skillExpandButton.click();
    }
}

/**
    Changes button text of the .expand class when they are clicked to
    display "Collapse" instead of "Expand" when the associated dropdown
    is open.
*/
function toggleExpandText(event) {
    const expandButton = event.target;
    const targetId = expandButton.getAttribute('data-bs-target');
    const targetElement = document.querySelector(targetId);

    setTimeout(() => {
        if (targetElement.classList.contains('show')) {
            expandButton.innerHTML = 'Collapse<br>∨';
        } else {
            expandButton.innerHTML = 'Expand<br>∨';
        }
    }, 360);
}

/**
    Changes the color of the skill icons depending on which project is being
    hovered over.
*/
//add components while adding projects

function addSkillColors(event) {
    const project = event.currentTarget;
    if (project.id === 'sgcportfolio') {
        for (skill of ['html5', 'css3', 'javascript', 'csharp', 'dotnet', 'bootstrap', 'photoshop']) {
            skillElement = document.getElementById(skill);
            skillElement.classList.add("selected");
        }
    }
    //DotNet | JSON | Oracle SQL | C# | Redis | Bootstrap | JavaScript
    if (project.id === 'signin&upMS') {
        for (skill of ['dotnet', 'bootstrap', 'html5', 'css3', 'javascript', 'json', 'redis', 'oraclesql']) {
            skillElement = document.getElementById(skill);
            skillElement.classList.add("selected");
        }
    }
    if (project.id === 'BlogEngineSGC') {
        for (skill of ['dotnet', 'bootstrap', 'html5', 'css3', 'javascript', 'json', 'redis', 'oraclesql']) {
            skillElement = document.getElementById(skill);
            skillElement.classList.add("selected");
        }
    }
    if (project.id === 'AutomatedAlgoTradingApp') {
        for (skill of ['python', 'pandas', 'html5', 'css3', 'javascript', 'json', 'redis', 'oraclesql']) {
            skillElement = document.getElementById(skill);
            skillElement.classList.add("selected");
        }
    }
}

/**
    Reverts colors of skill icons after hover ends.
*/
function removeSkillColors(event) {
    for (skill of document.getElementsByClassName('skill-icon')) {
        skill.classList.remove("selected");
    }
}
/**
    Animates items as they scroll into frame.
*/
function animateScroll() {
    const elements = document.querySelectorAll(".animate-scroll");
    for (const element of elements) {
        const rect = element.getBoundingClientRect();
        const display = window.getComputedStyle(element).display;
        if (display !== 'none' &&
            rect.bottom >= 75 &&
            rect.top <= (window.innerHeight || document.documentElement.clientHeight) - 75) {
            element.classList.add('animate');
        } else {
            element.classList.remove('animate');
        }
    }
}

/**
    Scales the background div in the introduction section when the mouse moves
    across the screen.
*/
function animateBackgroundAndHeadshot(event) {
    try {
        const width = window.innerWidth;
        const div = document.getElementById('bg-div')
        const mousePosX = Math.min(.7, event.clientX / width);
        if (width >= 710) {
            div.style.width = '38.5vw';
            const mousePosX = Math.min(.7, event.clientX / width);
            div.style.width = (0.37 + (0.03 * mousePosX)) * width + 'px';
        } else {
            div.style.width = '100vw';
        }

        const headshot = document.getElementById('headshot-color');
        headshot.style.maxWidth = mousePosX / 0.7 * 100 + '%';
    } catch { }
}

/**
    Toggles dark mode by changing the color of certain elements and changing
    certain images
*/
/*
function toggleDarkMode() {
    const toggleBtn = document.getElementById('moon-btn');
    elements = document.querySelectorAll('*');
    const darkModeImage = document.getElementById('moon-img');
    const topRightLine = document.getElementById('top-right-line');
    const bottomLeftLine = document.getElementById('bottom-left-line');
    if (toggleBtn.checked) {
        localStorage.setItem('darkMode', 'on');
        for (const element of elements) {
            element.classList.add('darkmode');
        }
        darkModeImage.src = '/images/sun.png';
        try {
            topRightLine.src = '/images/dark-top-right-line.png';
            bottomLeftLine.src = '/images/dark-bottom-left-line.png';
        } catch { }

    } else {
        localStorage.setItem('darkMode', 'off');
        for (const element of elements) {
            element.classList.remove('darkmode')
        }
        darkModeImage.src = '/images/moon.png'
        try {
            topRightLine.src = '/images/top-right-line.png';
            bottomLeftLine.src = '/images/bottom-left-line.png';
        } catch { }
    }
}
*/

function toggleDarkMode() {
    const toggleBtn = document.getElementById('moon-btn');
    const elements = document.querySelectorAll('*');
    const darkModeImage = document.getElementById('moon-img');
    const topRightLine = document.getElementById('top-right-line');
    const bottomLeftLine = document.getElementById('bottom-left-line');

    // Check for the custom property to track state
    const isDarkMode = toggleBtn.dataset.darkMode === "true";

    if (!isDarkMode) {
        // Enable dark mode
        localStorage.setItem('darkMode', 'on');
        for (const element of elements) {
            element.classList.add('darkmode');
        }
        darkModeImage.src = '/images/sun.png'; // Change image to sun
        toggleBtn.dataset.darkMode = "true"; // Set custom attribute to track state

        try {
            topRightLine.src = '/images/dark-top-right-line.png';
            bottomLeftLine.src = '/images/dark-bottom-left-line.png';
        } catch { }

    } else {
        // Disable dark mode
        localStorage.setItem('darkMode', 'off');
        for (const element of elements) {
            element.classList.remove('darkmode');
        }
        darkModeImage.src = '/images/moon.png'; // Change image to moon
        toggleBtn.dataset.darkMode = "false"; // Update state

        try {
            topRightLine.src = '/images/top-right-line.png';
            bottomLeftLine.src = '/images/bottom-left-line.png';
        } catch { }
    }
}

/**
    Syncs dark mode settings between the privacy and home pages.
*/
function syncDarkMode() {
    try {
        const status = localStorage.getItem('darkMode');
        if (status === 'on') {
            if (!document.getElementById('moon-btn').checked) {
                document.getElementById('moon-btn').click();
            }
        } else {
            if (document.getElementById('moon-btn').checked) {
                document.getElementById('moon-btn').click();
            }
        }
    } catch { }
}

document.addEventListener('DOMContentLoaded', function () {
    // closeNavList()
    const links = document.getElementsByClassName("nav-link");
    for (const link of links) {
        link.addEventListener('click', closeNavList);
    }

    // openEduExp()
    const credButton = document.getElementById('credentials-button');
    try {
        credButton.addEventListener('click', openEduExp);
    } catch { }

    // openSkills()
    const moreButton = document.getElementById('more-skills');
    try {
        moreButton.addEventListener('click', openSkills);
    } catch { }

    // toggleExpandText()
    const expandButtons = document.getElementsByClassName('expand');
    for (const btn of expandButtons) {
        btn.addEventListener('click', toggleExpandText);
    }

    // addSkillColors() + removeSkillColors()
    const projects = document.getElementsByClassName('project');
    for (const project of projects) {
        project.addEventListener('mouseover', addSkillColors);
        project.addEventListener('mouseout', removeSkillColors);
    }

    // animateScroll()
    window.addEventListener('scroll', animateScroll);
    window.addEventListener('resize', animateScroll);
    animateScroll();
    const expandbtns = document.getElementsByClassName('expand');
    for (const btn of expandbtns) {
        btn.addEventListener('click', animateScroll);
    }

    // animateBackgroundAndHeadshot()
    window.addEventListener('resize', animateBackgroundAndHeadshot);
    window.addEventListener('mousemove', animateBackgroundAndHeadshot);

    // toggleDarkMode()
    toggleBtn = document.getElementById('moon-btn');
    toggleBtn.addEventListener('click', toggleDarkMode);
    moonBtn = document.getElementById('moon-btn');

    if (window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches) {
        if (!moonBtn.checked) {
            moonBtn.click();
        }
    }
    window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', event => {
        if (event.matches) {
            if (!moonBtn.checked) {
                moonBtn.click();
            }
        } else {
            if (moonBtn.checked) {
                moonBtn.click();
            }
        }
    });

    // syncDarkMode()
    window.addEventListener('DOMContentLoaded', syncDarkMode);

    // Skill hover functionality for proficiency bars
    document.querySelectorAll('.skill-container').forEach(container => {
        const proficiencyBar = container.querySelector('.proficiency-bar');

        // Mouse enter event
        container.addEventListener('mouseenter', () => {
            const percentage = proficiencyBar.getAttribute('data-percentage');
            proficiencyBar.style.width = `${percentage}%`; // Set the width dynamically
        });

        // Mouse leave event
        container.addEventListener('mouseleave', () => {
            proficiencyBar.style.width = '0%'; // Reset width when not hovering
        });
    });

    /*// Add showPopup functionality for project containers
    const moreButtons = document.querySelectorAll('.more-btn');
    moreButtons.forEach(button => {
        button.addEventListener('click', function () {
            showPopup(button);
        });
    });*/
});
    // Function to show the project details in a pop-up
// Function to show the project details in a pop-up
// Function to show the project details in a pop-up
function showPopup(button) {
    // Check if a popup is already open
    let existingPopup = document.querySelector('.popup-container');
    if (existingPopup) {
        // If a popup is already open, remove it first
        document.body.removeChild(existingPopup);
    }

    // Create a popup container
    const popupContainer = document.createElement('div');
    const popupContent = document.createElement('div');
    const closeButton = document.createElement('button');

    // Set classes for styling
    popupContainer.className = 'popup-container';
    popupContent.className = 'popup-content';
    closeButton.className = 'popup-close';
    closeButton.innerHTML = '&times;'; // Close button

    // Append close button
    popupContent.appendChild(closeButton);

    // Clone the project details for display in the popup
    const projectClone = button.closest('.project').cloneNode(true);
    projectClone.querySelector('.more-text').style.display = 'block'; // Show full text
    projectClone.querySelector('.limited-text').style.display = 'none'; // Hide limited text

    // Remove "Show More" button from cloned project
    const showMoreBtn = projectClone.querySelector('.more-btn');
    if (showMoreBtn) {
        showMoreBtn.remove(); // Check if the button exists before trying to remove it
    }

    popupContent.appendChild(projectClone);
    popupContainer.appendChild(popupContent);
    document.body.appendChild(popupContainer);

    // Display popup
    popupContainer.classList.add('active');

    // Close the popup when close button is clicked
    closeButton.addEventListener('click', function () {
        document.body.removeChild(popupContainer);
    });

    // Close popup when clicking outside of content
    popupContainer.addEventListener('click', function (event) {
        if (event.target === popupContainer) {
            document.body.removeChild(popupContainer);
        }
    });
}

// Add click event to project containers
const projects = document.querySelectorAll('.project');
projects.forEach(project => {
    project.addEventListener('click', function (event) {
        // Prevent clicks on the close button from firing the project click event
        showPopup(this); // Call showPopup on any click inside the project container
    });
});





/*document.addEventListener('DOMContentLoaded', function () {
    // closeNavList()
    const links = document.getElementsByClassName("nav-link");
    for (const link of links) {
        link.addEventListener('click', closeNavList)
    }

    // openEduExp()
    const credButton = document.getElementById('credentials-button');
    try {
        credButton.addEventListener('click', openEduExp);
    } catch {

    }

    // openSkills()
    const moreButton = document.getElementById('more-skills');
    try {
        moreButton.addEventListener('click', openSkills);
    } catch {

    }

    // toggleExpandText()
    const expandButtons = document.getElementsByClassName('expand');
    for (const btn of expandButtons) {
        btn.addEventListener('click', toggleExpandText);
    }

    // addSkillColors() + removeSkillColors()
    const projects = document.getElementsByClassName('project');
    for (const project of projects) {
        project.addEventListener('mouseover', addSkillColors)
        project.addEventListener('mouseout', removeSkillColors)
    }

    // animateScroll()
    window.addEventListener('scroll', animateScroll);
    window.addEventListener('resize', animateScroll);
    animateScroll();
    const expandbtns = document.getElementsByClassName('expand');
    for (const btn of expandbtns) {
        btn.addEventListener('click', animateScroll);
    }

    // animateBackgroundAndHeadshot()
    window.addEventListener('resize', animateBackgroundAndHeadshot);
    window.addEventListener('mousemove', animateBackgroundAndHeadshot);

    // toggleDarkMode()
    toggleBtn = document.getElementById('moon-btn');
    toggleBtn.addEventListener('click', toggleDarkMode);
    moonBtn = document.getElementById('moon-btn');

    if (window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches) {
        if (!moonBtn.checked) {
            moonBtn.click();
        }
    }
    window.matchMedia('(prefers-color-scheme: dark)')
        .addEventListener('change', event => {
            if (event.matches) {
                if (!moonBtn.checked) {
                    moonBtn.click();
                }
            } else {
                if (moonBtn.checked) {
                    moonBtn.click();
                }
            }
        })

    // syncDarkMode()
    window.addEventListener('DOMContentLoaded', syncDarkMode);

    // Skill hover functionality for proficiency bars
    document.querySelectorAll('.skill-container').forEach(container => {
        const proficiencyBar = container.querySelector('.proficiency-bar');

        // Mouse enter event
        container.addEventListener('mouseenter', () => {
            const percentage = proficiencyBar.getAttribute('data-percentage');
            proficiencyBar.style.width = `${percentage}%`; // Set the width dynamically
        });

        // Mouse leave event
        container.addEventListener('mouseleave', () => {
            proficiencyBar.style.width = '0%'; // Reset width when not hovering
        });
    });
});*/

