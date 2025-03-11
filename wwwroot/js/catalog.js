document.addEventListener('DOMContentLoaded', function () {
    loadProducts(1); // Загружаем первую страницу при загрузке документа
});

function loadProducts(page) {
    fetch(`/api/products?page=${page}`)
        .then(response => response.json())
        .then(data => {
            // Обновляем контейнер с товарами
            const container = document.getElementById('productContainer');
            container.innerHTML = ''; // Очистка контейнера
            data.products.forEach(product => {
                const productDiv = document.createElement('div');
                productDiv.classList.add('product-item');
                productDiv.innerHTML = `
                    <a href="product.html" class="product-link">
                    <img src="${product.imageURL}" alt="${product.name}">
                    <p>${product.name}</p>
                    <p>Цена: ${product.price} ₽</p>
                    <button class="add-to-cart">Добавить в корзину</button>
                `;
                container.appendChild(productDiv);
            });
            // Обновляем пагинацию
            generatePagination(data.totalPages, page);
        })
        .catch(error => console.error('Ошибка при загрузке товаров:', error));
}

function generatePagination(totalPages, currentPage) {
    const pagination = document.getElementById('pagination');
    pagination.innerHTML = ''; // Очистка блока пагинации
    for (let i = 1; i <= totalPages; i++) {
        const btn = document.createElement('button');
        btn.classList.add('page-btn');
        btn.innerText = i;
        if (i === currentPage) {
            btn.disabled = true;
        }
        btn.addEventListener('click', () => {
            loadProducts(i);
        });
        pagination.appendChild(btn);
    }
}
