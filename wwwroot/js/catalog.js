document.addEventListener('DOMContentLoaded', function () {
    loadProducts(1);
    fetch('/api/cart/count')
        .then(response => response.json())
        .then(data => updateCartCounter(data.totalItems))
        .catch(() => updateCartCounter(0));
});

function updateCartCounter(count) {
    const cartLink = document.querySelector('a[href="cart.html"]');
    cartLink.innerHTML = count > 0
        ? `Корзина (${count})`
        : 'Корзина';
}

function getSelectedValues(selector) {
    return Array.from(document.querySelectorAll(selector))
        .filter(checkbox => checkbox.checked)
        .map(checkbox => checkbox.value || checkbox.nextSibling.textContent.trim());
}

function getFilters() {
    const connectionCheckboxes = Array.from(document.querySelectorAll('.sidebar-filter-category:nth-child(2) input[type="checkbox"]:checked'));
    const wearingCheckboxes = Array.from(document.querySelectorAll('.sidebar-filter-category:nth-child(3) input[type="checkbox"]:checked'));

    return {
        connectionType: connectionCheckboxes.map(cb => cb.value),
        wearingStyle: wearingCheckboxes.map(cb => cb.value),
        brands: Array.from(document.querySelectorAll('#brand-filters input[type="checkbox"]:checked')).map(cb => cb.value),
        minPrice: document.getElementById('price-from').value ?
            Number(document.getElementById('price-from').value) : null,
        maxPrice: document.getElementById('price-to').value ?
            Number(document.getElementById('price-to').value) : null,
        searchTerm: document.querySelector('.sidebar-search').value.trim()
    };
}

//ОБРАБОТЧИК ПОИСКА
const searchInput = document.querySelector('.sidebar-search');
let searchTimer;

searchInput.addEventListener('input', function () {
    clearTimeout(searchTimer);
    searchTimer = setTimeout(() => loadProducts(1), 300);
});

document.getElementById('apply-filters').addEventListener('click', function () {
    loadProducts(1);
});

function loadProducts(page) {
    const filters = getFilters();
    const queryParams = new URLSearchParams({
        page: page,
        searchTerm: filters.searchTerm,
        ...(filters.connectionType.length > 0 && { connectionType: filters.connectionType.join(',') }),
        ...(filters.wearingStyle.length > 0 && { wearingStyle: filters.wearingStyle.join(',') }),
        ...(filters.brands.length > 0 && { brand: filters.brands.join(',') }),
        ...(filters.minPrice && { minPrice: filters.minPrice }),
        ...(filters.maxPrice && { maxPrice: filters.maxPrice })
    }).toString();

    fetch(`/api/products?${queryParams}`)
        .then(response => {
            if (!response.ok) {
                return response.text().then(text => { throw new Error(text) });
            }
            return response.json();
        })
        .then(data => {
            updateProductList(data.products);
            generatePagination(data.totalPages, page);
        })
        .catch(error => {
            console.error('Error:', error);
            alert('Ошибка: ' + error.message);
        });
}

function updateProductList(products) {
    const container = document.getElementById('productContainer');
    container.innerHTML = products.map(product => `
        <div class="product-item" data-product-id="${product.productId}">
            <img src="${product.imageURL}" alt="${product.name}">
            <p>${product.name}</p>
            <p>Цена: ${product.price} ₽</p>
            <button class="add-to-cart">Добавить в корзину</button>
        </div>
    `).join('');
}

document.getElementById('productContainer').addEventListener('click', function (e) {
    if (e.target.classList.contains('add-to-cart')) {
        const productItem = e.target.closest('.product-item');
        const productId = productItem.dataset.productId;

        fetch('/api/cart/add', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({ productId: parseInt(productId) })
        })
            .then(response => {
                if (!response.ok) throw new Error('Ошибка добавления');
                return response.json();
            })
            .then(data => {
                updateCartCounter(data.totalItems);
            })
            .catch(error => {
                console.error('Error:', error);
                alert(error.message);
            });
    }
});

//ПАГИНАЦИЯ
function generatePagination(totalPages, currentPage) {
    const pagination = document.getElementById('pagination');
    pagination.innerHTML = '';
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