// List your certificates and achievements here
const certificates = [
    {
        title: "NSEC Certificate",
        desc: "NSEC merit by ASSOCIATION OF CHEMISTRY TEACHERS India",
        type: "pdf",
        path: "/certs/nseccert.pdf",
        thumb: "/images/act-logo.jpg" // Use a small image as a thumbnail, or fallback to a PDF icon
    },
    {
        title: "KVPY Certificate",
        desc: "Kishore Vaigyanik Protsahan Yojana (KVPY) Fellowship by IISC Bangalore",
        type: "pdf",
        path: "/certs/kvpycert.pdf",
        thumb: "/images/kvpycert-thumb.webp" // Use a small image as a thumbnail, or fallback to a PDF icon
    }
    
];

const achievements = [
    {
        title: "Samudbhav Community Platform",
        desc: "Built and Deployed Samudbhav Community Platform",
        type: "image",
        path: "https://samudbhav.tech",
        thumb: "/images/externallink-icon.webp"
    },
    {
        title: "Open Source Contribution",
        desc: "Contributed to supatham project on GitHub",
        type: "image",
        path: "https://github.com/ADOGC-Org/supatham",
        thumb: "/images/externallink-icon.webp"
    },

    
];

const resumes = [
    {
        title: "Resume",
        desc: "Download my latest Resume (PDF)",
        type: "pdf",
        path: "/certs/Resume_SaiGopalChalla.pdf",
        thumb: "/images/pdf-icon.webp" // Optional: add a thumbnail or use a PDF icon
    },
    {
        title: "CV",
        desc: "Download my detailed CV (PDF)",
        type: "pdf",
        path: "/certs/CV_SaiGopalChalla.pdf",
        thumb: "/images/pdf-icon.webp" // Optional: add a thumbnail or use a PDF icon
    }
];

function createBox(item) {
    const box = document.createElement('div');
    box.className = 'works-certs-box';
    box.onclick = () => window.open(item.path, '_blank');

    const thumb = document.createElement('div');
    thumb.className = 'works-certs-thumb';

    if (item.type === 'pdf') {
        // Show thumbnail if available, else show a PDF icon
        if (item.thumb) {
            const img = document.createElement('img');
            img.src = item.thumb;
            img.alt = item.title;
            img.style.width = "100%";
            img.style.height = "100%";
            img.style.objectFit = "cover";
            thumb.appendChild(img);
        } else {
            thumb.innerHTML = '<span style="font-size:2.2rem;color:#ffe066;">📄</span>';
        }
    } else {
        const img = document.createElement('img');
        img.src = item.thumb || item.path;
        img.alt = item.title;
        img.style.width = "100%";
        img.style.height = "100%";
        img.style.objectFit = "cover";
        thumb.appendChild(img);
    }

    const info = document.createElement('div');
    info.className = 'works-certs-info';
    info.innerHTML = `<div class="works-certs-title-sm">${item.title}</div>
                      <div class="works-certs-desc">${item.desc || ""}</div>`;

    box.appendChild(thumb);
    box.appendChild(info);

    return box;
}

document.addEventListener('DOMContentLoaded', function () {
    const certsList = document.getElementById('certificates-list');
    const achList = document.getElementById('achievements-list');
    const resumeList = document.getElementById('resume-list');

    certificates.forEach(cert => certsList.appendChild(createBox(cert)));
    achievements.forEach(ach => achList.appendChild(createBox(ach)));
    resumes.forEach(resume => resumeList.appendChild(createBox(resume)));
});