document.addEventListener('DOMContentLoaded', function () {
    loadProducts(1);
});

// Добавляем обработчик поиска
const searchInput = document.querySelector('.sidebar-search');
let searchTimer;

searchInput.addEventListener('input', function () {
    clearTimeout(searchTimer);
    searchTimer = setTimeout(() => loadProducts(1), 300);
});

// Модифицируем функцию загрузки товаро
function loadProducts(page) {

    const searchTerm = document.querySelector('.sidebar-search').value.trim();

    fetch(`/api/products?page=${page}&searchTerm=${encodeURIComponent(searchTerm)}`)
        .then(response => {
            if (!response.ok) {
                return response.json().then(err => Promise.reject(err));
            }
            return response.json();
        })
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
        .catch(error => {
            console.error('Ошибка:', error);
            alert(error.error || 'Произошла ошибка при загрузке товаров');
        });
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