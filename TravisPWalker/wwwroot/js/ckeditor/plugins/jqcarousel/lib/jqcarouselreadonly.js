var deleteEditControls = function () {
    //debugger
    var carousels = $('.carousel.slide');
    var carouselCount = carousels.length;
    for (let i = 0; i < carouselCount; i++) {
        var carouselID = carousels[i].id;
        var carousel = $('#' + carouselID); 

        var carousel_slides = carousel.find('.carousel-inner .carousel-item.' + carouselID);
        var slideCount = carousel_slides.length;

        for (let i = 0; i < slideCount; i++) {
            if (carousel_slides[i].children[0].id.toLowerCase().localeCompare('slide-controls') == 0) {
                carousel_slides[i].children[0].remove();
            }
        }

        var leftControl = carousel.find('.carousel-control-prev');
        leftControl = leftControl[0];
        if (leftControl.children[0].getAttribute('class') != 'carousel-control-prev-icon') {
            leftControl.insertAdjacentHTML('afterbegin', '<span class="carousel-control-prev-icon" aria-hidden="true">');
        }

        var rightControl = carousel.find('.carousel-control-next');
        rightControl = rightControl[0];
        if (rightControl.children[0].getAttribute('class') != 'carousel-control-next-icon') {
            rightControl.insertAdjacentHTML('afterbegin', '<span class="carousel-control-next-icon" aria-hidden="true">');
        }
    }
};


document.addEventListener('DOMContentLoaded', function () {
    deleteEditControls();
});