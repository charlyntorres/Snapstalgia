document.addEventListener("DOMContentLoaded", () => {
    const faqItems = document.querySelectorAll(".faq-item");

    faqItems.forEach(item => {
        const question = item.querySelector(".faq-question");
        const answer = item.querySelector(".faq-answer");
        const toggle = item.querySelector(".faq-toggle");

        question.addEventListener("click", () => {
            const isOpen = item.classList.contains("open");

            // Close all first
            faqItems.forEach(i => {
                i.classList.remove("open");
                i.querySelector(".faq-toggle").textContent = "+";
                const a = i.querySelector(".faq-answer");
                a.style.maxHeight = "0";
            });

            if (!isOpen) {
                item.classList.add("open");
                toggle.textContent = "–";
                answer.style.maxHeight = answer.scrollHeight + "px";
            }
        });
    });
});