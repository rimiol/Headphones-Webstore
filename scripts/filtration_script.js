document.addEventListener('DOMContentLoaded', () => {
    const products = [
        { name: "Audio-Technica ATH-M50x", price: 12000, brand: "Audio-Technica", image: "images/headphones/audio_technica_ath_m50x.webp" },
        { name: "Sony WH-1000XM5", price: 29000, brand: "Sony", image: "images/headphones/sony_wh1000xm5_black.webp" },
        { name: "Sony MDR-7506", price: 14000, brand: "Sony", image: "images/headphones/sony_mdr_7506_1.webp" },
        { name: "Beyerdynamic DT 770 Pro", price: 16000, brand: "Beyerdynamic", image: "images/headphones/Beyerdynamic_DT_770_PRO_000.webp" },
        { name: "Sennheiser HD 280 PRO", price: 11000, brand: "Sennheiser", image: "images/headphones/sennheiser_momentum4_white_00.webp" }
    ];

    const productContainer = document.querySelector('.catalog-products');

    const applyButton = document.getElementById('apply-filters');

    function renderProducts(filteredProducts) {
        productContainer.innerHTML = ''; 

        if (filteredProducts.length === 0) {
            productContainer.innerHTML = '<p>Нет товаров, соответствующих фильтрам.</p>';
            return;
        }

        filteredProducts.forEach(product => {
            const productCard = `
                <div class="product-item">
                    <a href="product.html" class="product-link">
                        <img src="${product.image}" alt="${product.name}" class="product-card-image"/>
                        <div class="product-card-info">
                            <div>${product.name}</div>
                            <strong>${product.price}&#8381;</strong>
                        </div>
                    </a>
                    <button class="add-to-cart">Добавить в корзину</button>
                </div>`;
            productContainer.insertAdjacentHTML('beforeend', productCard);
        });

        const productCards = document.querySelectorAll('.product-item');

        productCards.forEach(card => {
            card.addEventListener('mouseover', function() {
                this.style.transform = 'scale(1.05)';
                this.style.boxShadow = '0px 8px 15px rgba(0, 0, 0, 0.2)';
                this.style.zIndex = '10';
            });

            card.addEventListener('mouseout', function() {
                this.style.transform = 'scale(1)';
                this.style.boxShadow = 'none';
                this.style.zIndex = '1';
            });
        });
    }

    function filterProducts() {
        const priceFrom = parseInt(document.getElementById('price-from').value) || 0; 
        const priceTo = parseInt(document.getElementById('price-to').value) || Infinity; 

        const selectedBrands = Array.from(document.querySelectorAll('#brand-filters input:checked'))
                                    .map(input => input.value);

        const filtered = products.filter(product => {
            const inPriceRange = product.price >= priceFrom && product.price <= priceTo;
            const inSelectedBrands = selectedBrands.length === 0 || selectedBrands.includes(product.brand);
            return inPriceRange && inSelectedBrands;
        });

        renderProducts(filtered);
    }

    applyButton.addEventListener('click', filterProducts);

    renderProducts(products);
});
