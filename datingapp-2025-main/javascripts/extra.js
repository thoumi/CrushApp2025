// Custom JavaScript for CrushApp Documentation

// Add copy to clipboard enhancement
document.addEventListener('DOMContentLoaded', function() {
    // Analytics (optionnel)
    console.log('CrushApp Documentation loaded');
    
    // Smooth scroll for anchor links
    document.querySelectorAll('a[href^="#"]').forEach(anchor => {
        anchor.addEventListener('click', function (e) {
            e.preventDefault();
            const target = document.querySelector(this.getAttribute('href'));
            if (target) {
                target.scrollIntoView({
                    behavior: 'smooth',
                    block: 'start'
                });
            }
        });
    });

    // Add external link icons
    document.querySelectorAll('a[href^="http"]').forEach(link => {
        if (!link.hostname.includes(window.location.hostname)) {
            link.setAttribute('target', '_blank');
            link.setAttribute('rel', 'noopener noreferrer');
        }
    });

    // Reading time estimation
    const content = document.querySelector('.md-content__inner');
    if (content) {
        const text = content.textContent;
        const wordCount = text.trim().split(/\s+/).length;
        const readingTime = Math.ceil(wordCount / 200); // 200 mots/min
        
        const readingTimeElement = document.createElement('div');
        readingTimeElement.className = 'reading-time';
        readingTimeElement.innerHTML = `📖 Temps de lecture estimé : ${readingTime} min`;
        readingTimeElement.style.cssText = 'padding: 0.5rem 1rem; background: var(--md-code-bg-color); border-radius: 0.5rem; margin-bottom: 1rem; font-size: 0.9rem;';
        
        const firstH1 = content.querySelector('h1');
        if (firstH1 && firstH1.nextSibling) {
            firstH1.parentNode.insertBefore(readingTimeElement, firstH1.nextSibling);
        }
    }

    // Copy code block with notification
    document.querySelectorAll('.md-clipboard').forEach(button => {
        button.addEventListener('click', function() {
            const notification = document.createElement('div');
            notification.textContent = '✓ Copié !';
            notification.style.cssText = 'position: fixed; top: 20px; right: 20px; background: #28a745; color: white; padding: 0.5rem 1rem; border-radius: 0.5rem; z-index: 9999; animation: fadeOut 2s forwards;';
            document.body.appendChild(notification);
            
            setTimeout(() => notification.remove(), 2000);
        });
    });
});

// Add fadeOut animation
const style = document.createElement('style');
style.textContent = `
    @keyframes fadeOut {
        0% { opacity: 1; }
        70% { opacity: 1; }
        100% { opacity: 0; }
    }
`;
document.head.appendChild(style);

