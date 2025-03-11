window.onscroll = function() {
    let backToTopButton = document.getElementById("backToTop");
    
    if (document.body.scrollTop > 100 || document.documentElement.scrollTop > 100) {
        backToTopButton.style.display = "block"; 
    } else {
        backToTopButton.style.display = "none";
    }
};

document.getElementById("backToTop").addEventListener("click", function() {
    window.scrollTo({ top: 0, behavior: 'smooth' });
});
