const skillsData = {
    languages: {
        title: "Languages",
        color: "var(--q1-color)",
        skills: [
            {
                name: "Java",
                percentage: 35,
                projects: [
                    { name: "E-commerce Platform", url: "#" },
                    { name: "Banking Application", url: "#" }
                ]
            },
            {
                name: "C#",
                percentage: 35,
                projects: [
                    { name: "CRM System", url: "#" },
                    { name: "Inventory Management", url: "#" }
                ]
            },
            {
                name: "Python",
                percentage: 15,
                projects: [
                    { name: "CRM System", url: "#" },
                    { name: "Inventory Management", url: "#" }
                ]
            },
            {
                name: "C",
                percentage: 15,
                projects: [
                    { name: "CRM System", url: "#" },
                    { name: "Inventory Management", url: "#" }
                ]
            }
        ]
    },
    frameworks: {
        title: "Frameworks",
        color: "var(--q2-color)",
        skills: [
            {
                name: ".NET",
                percentage: 30,
                projects: [
                    { name: "Enterprise Portal", url: "#" },
                    { name: "Healthcare System", url: "#" }
                ]
            },
            {
                name: "Angular",
                percentage: 20,
                projects: [
                    { name: "Dashboard Application", url: "#" },
                    { name: "Analytics Platform", url: "#" }
                ]
            },
            {
                name: "SpringBoot",
                percentage: 20,
                projects: [
                    { name: "Dashboard Application", url: "#" },
                    { name: "Analytics Platform", url: "#" }
                ]
            },
            {
                name: "SQL",
                percentage: 30,
                projects: [
                    { name: "Dashboard Application", url: "#" },
                    { name: "Analytics Platform", url: "#" }
                ]
            }            
        ]
    },
    tools: {
        title: "Tools & Libs",
        color: "var(--q3-color)",
        skills: [
            {
                name: "APIs",
                percentage: 20,
                projects: [
                    { name: "Message Queue System", url: "#" },
                    { name: "Real-time Processing", url: "#" }
                ]
            },
            {
                name: "Caching",
                percentage: 25,
                projects: [
                    { name: "Caching Service", url: "#" },
                    { name: "Real-time Analytics", url: "#" }
                ]
            },
            {
                name: "Proxies",
                percentage: 20,
                projects: [
                    { name: "Service Architecture", url: "#" },
                    { name: "Distributed Systems", url: "#" }
                ]
            },
            {
                name: "Microservices",
                percentage: 20,
                projects: [
                    { name: "API Security", url: "#" },
                    { name: "Service Protection", url: "#" }
                ]
            }
        ]
    },
    techTopics: {
        title: "Tech Topics",
        color: "var(--q4-color)",
        skills: [
            {
                name: "PostgreSQL",
                percentage: 30,
                projects: [
                    { name: "Data Warehouse", url: "#" },
                    { name: "User Management System", url: "#" }
                ]
            },
            {
                name: "React",
                percentage: 25,
                projects: [
                    { name: "Admin Dashboard", url: "#" },
                    { name: "Client Portal", url: "#" }
                ]
            },
            {
                name: "JWT",
                percentage: 20,
                projects: [
                    { name: "Authentication Service", url: "#" },
                    { name: "API Gateway", url: "#" }
                ]
            },
            {
                name: "Rate Limiter",
                percentage: 20,
                projects: [
                    { name: "RESTful Services", url: "#" },
                    { name: "GraphQL Endpoints", url: "#" }
                ]
            },
            {
                name: "Event Streaming",
                percentage: 20,
                projects: [
                    { name: "Reverse Proxy", url: "#" },
                    { name: "API Gateway", url: "#" }
                ]
            }
        ]
    }
};

document.addEventListener('DOMContentLoaded', function () {
    // Elements
    const popupOverlay = document.getElementById('popup-overlay');
    const popupCircle = document.getElementById('popup-circle');
    const closePopupBtn = document.getElementById('close-popup');
    const popupTitle = document.getElementById('popup-title');
    const popupSkills = document.getElementById('popup-skills');
    const projectBox = document.getElementById('project-box');
    const skillNameEl = document.getElementById('skill-name');
    const projectList = document.getElementById('project-list');
    const factTexts = document.querySelectorAll('.fact-text');

    // Variables
    let currentFactIndex = 0;
    let selectedSkill = null;
    let currentQuadrant = null;

    // Initialize facts rotation
    startFactsRotation();

    // Render quadrants with skill pie charts
    renderQuadrants();

    // Initialize quadrant click handlers
    setupQuadrantClickHandlers();

    // Initialize popup close handlers
    setupPopupCloseHandlers();

    // Setup Functions
    function renderQuadrants() {
        renderQuadrantSkills('q1', skillsData.languages.skills);
        renderQuadrantSkills('q2', skillsData.frameworks.skills);
        renderQuadrantSkills('q3', skillsData.tools.skills);
        renderQuadrantSkills('q4', skillsData.techTopics.skills);
    }

    function renderQuadrantSkills(quadrantId, skills) {
        const quadrant = document.getElementById(quadrantId);
        if (!quadrant) return;

        const visibleSkillsContainer = quadrant.querySelector('.visible-skills');
        if (!visibleSkillsContainer) return;

        visibleSkillsContainer.innerHTML = '';

        const ul = document.createElement('ul');
        ul.className = 'skills-list';

        // Show only first 3
        skills.slice(0, 3).forEach(skill => {
            const li = document.createElement('li');
            li.textContent = skill.name;
            ul.appendChild(li);
        });

        // If more, add "+n more"
        if (skills.length > 3) {
            const li = document.createElement('li');
            li.textContent = `+${skills.length - 3} more`;
            li.style.opacity = "0.7";
            ul.appendChild(li);
        }

        visibleSkillsContainer.appendChild(ul);
    }

    function setupQuadrantClickHandlers() {
        const quadrants = document.querySelectorAll('.quadrant');

        quadrants.forEach(quadrant => {
            quadrant.addEventListener('click', (event) => {
                const quadrantId = quadrant.getAttribute('data-quadrant');
                const quadrantTitle = quadrant.getAttribute('data-title');

                showQuadrantPopup(quadrantId, quadrantTitle);
                event.stopPropagation();
            });
        });
    }

    function setupPopupCloseHandlers() {
        // Close on X button click
        closePopupBtn.addEventListener('click', closePopup);

        // Close on clicking outside popup
        popupOverlay.addEventListener('click', (event) => {
            if (event.target === popupOverlay) {
                closePopup();
            }
        });

        // Prevent closing when clicking on popup-circle
        popupCircle.addEventListener('click', (event) => {
            event.stopPropagation();
        });

        // Prevent closing when clicking on project-box
        projectBox.addEventListener('click', (event) => {
            event.stopPropagation();
        });
    }

    // Fact Rotation
    function startFactsRotation() {
        setInterval(() => {
            rotateFacts();
        }, 5000);
    }

    function rotateFacts() {
        factTexts.forEach((fact, index) => {
            if (index === currentFactIndex) {
                fact.style.opacity = '0';
            }
        });

        currentFactIndex = (currentFactIndex + 1) % factTexts.length;

        setTimeout(() => {
            factTexts.forEach((fact, index) => {
                if (index === currentFactIndex) {
                    fact.style.opacity = '1';
                }
            });
        }, 500);
    }

    // Popup Functions
    function showQuadrantPopup(quadrantId, quadrantTitle) {
        currentQuadrant = quadrantId;

        let skillsObj;
        switch (quadrantId) {
            case '1': skillsObj = skillsData.languages; break;
            case '2': skillsObj = skillsData.frameworks; break;
            case '3': skillsObj = skillsData.tools; break;
            case '4': skillsObj = skillsData.techTopics; break;
            default: return;
        }

        // Animate quadrant
        const quadrant = document.querySelector(`.quadrant[data-quadrant="${quadrantId}"]`);
        quadrant.classList.add('quadrant-animate');

        document.querySelector('.skills-circle').classList.add('blurred');

        setTimeout(() => {
            // Set popup background color
            popupCircle.style.backgroundColor = skillsObj.color;
            // Set popup title
            popupTitle.textContent = skillsObj.title;
            // Generate skill items
            popupSkills.innerHTML = '';
            skillsObj.skills.forEach(skill => {
                const skillElement = document.createElement('div');
                skillElement.className = 'popup-skill';
                const pieContainer = document.createElement('div');
                pieContainer.className = 'popup-skill-pie';
                const pieFill = document.createElement('div');
                pieFill.className = 'popup-skill-pie-fill';
                const percentage = skill.percentage;
                const degrees = (percentage / 100) * 360;
                if (degrees <= 180) {
                    const x = 50 + 50 * Math.sin(degrees * Math.PI / 180);
                    const y = 50 - 50 * Math.cos(degrees * Math.PI / 180);
                    pieFill.style.clipPath = `polygon(50% 50%, 50% 0%, ${x}% ${y}%)`;
                } else {
                    pieFill.style.clipPath = `polygon(50% 50%, 50% 0%, 100% 0%, 100% 100%, 0% 100%, 0% 0%, 50% 0%)`;
                }
                pieContainer.appendChild(pieFill);
                const nameSpan = document.createElement('span');
                nameSpan.textContent = skill.name;
                skillElement.appendChild(pieContainer);
                skillElement.appendChild(nameSpan);
                skillElement.addEventListener('mouseenter', () => showProjectBox(skill));
                skillElement.addEventListener('mouseleave', hideProjectBox);
                popupSkills.appendChild(skillElement);
            });
            popupOverlay.classList.add('active');
            positionPopup();
            quadrant.classList.remove('quadrant-animate');
        }, 400);
    }

    function closePopup() {
        popupOverlay.classList.remove('active');
        hideProjectBox();
        document.querySelector('.skills-circle').classList.remove('blurred');
    }

    // Project Box Functions
    function showProjectBox(skill) {
        selectedSkill = skill;

        // Update skill name
        skillNameEl.textContent = skill.name;

        // Clear and update project list
        projectList.innerHTML = '';

        if (skill.projects && skill.projects.length > 0) {
            skill.projects.forEach(project => {
                const projectItem = document.createElement('li');
                const projectLink = document.createElement('a');
                projectLink.href = project.url;
                projectLink.className = 'project-item';
                projectLink.textContent = project.name;
                projectItem.appendChild(projectLink);
                projectList.appendChild(projectItem);
            });
        } else {
            const noProjects = document.createElement('li');
            noProjects.className = 'project-item';
            noProjects.textContent = 'No projects yet';
            projectList.appendChild(noProjects);
        }

        // Position and show project box
        positionProjectBox();
        projectBox.style.opacity = '1';
    }

    function hideProjectBox() {
        projectBox.style.opacity = '0';
        selectedSkill = null;
    }

    // Position Calculations
    function positionPopup() {
        // No-op: CSS handles centering
    }

    function positionProjectBox() {
        const isMobile = window.innerWidth <= 768;

        if (isMobile) {
            // Mobile positioning
            projectBox.style.top = 'auto';
            projectBox.style.right = 'auto';
            projectBox.style.bottom = '-8rem';
            projectBox.style.left = '50%';
            projectBox.style.transform = 'translateX(-50%)';
        } else {
            // Desktop positioning
            const rect = popupCircle.getBoundingClientRect();
            const boxRect = projectBox.getBoundingClientRect();

            // Base position near the circle
            let top = rect.top + rect.height / 2 - boxRect.height / 2;
            let left = 0;

            // Adjust based on quadrant
            switch (currentQuadrant) {
                case '1': // Top-left quadrant
                case '3': // Bottom-left quadrant
                    left = rect.right + 16; // Position to the right
                    break;
                case '2': // Top-right quadrant
                case '4': // Bottom-right quadrant
                    left = rect.left - boxRect.width - 16; // Position to the left
                    break;
            }

            // Ensure box stays within viewport
            if (top < 16) top = 16;
            if (top + boxRect.height > window.innerHeight - 16) {
                top = window.innerHeight - boxRect.height - 16;
            }

            projectBox.style.top = `${top}px`;
            projectBox.style.left = `${left}px`;
            projectBox.style.transform = 'none';
        }
    }

    // Handle window resize events
    window.addEventListener('resize', () => {
        if (popupOverlay.classList.contains('active') && selectedSkill) {
            positionProjectBox();
        }
    });

    // Initialize on load
    renderQuadrants();
});
