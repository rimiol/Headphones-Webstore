document.addEventListener("DOMContentLoaded", () => {
    const carouselInner = document.querySelector(".carousel-inner");
    const carouselItems = document.querySelectorAll(".carousel-item");
    const prevButton = document.querySelector(".carousel-btn.prev");
    const nextButton = document.querySelector(".carousel-btn.next");

    let currentIndex = 0;

    const updateCarousel = () => {
        const width = carouselItems[0].offsetWidth;
        carouselInner.style.transform = `translateX(-${currentIndex * width}px)`;

        prevButton.disabled = currentIndex === 0;
        nextButton.disabled = currentIndex === carouselItems.length - 1;
    };

    const showPrevSlide = () => {
        if (currentIndex > 0) {
            currentIndex--;
            updateCarousel();
        }
    };

    const showNextSlide = () => {
        if (currentIndex < carouselItems.length - 1) {
            currentIndex++;
            updateCarousel();
        }
    };

    prevButton.addEventListener("click", showPrevSlide);
    nextButton.addEventListener("click", showNextSlide);

    // Обновление карусели при изменении размера окна
    window.addEventListener("resize", updateCarousel);

    // Инициализация
    updateCarousel();
});
