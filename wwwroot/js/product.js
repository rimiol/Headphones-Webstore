document.addEventListener('DOMContentLoaded', function () {
    const productId = getProductIdFromURL();
    
    if (!productId) {
        showError('Не указан идентификатор товара');
        return;
    }
    
    console.log('Загрузка данных для товара ID:', productId);
    loadProductData(productId);
});

// Получение ID товара из URL
function getProductIdFromURL() {
    const params = new URLSearchParams(window.location.search);
    const productId = params.get('productId');
    
    if (!productId) {
        console.error('Product ID не найден в URL');
        return null;
    }
    
    console.log('Получен Product ID из URL:', productId);
    return parseInt(productId);
}

// Загрузка данных товара
function loadProductData(productId) {
    showLoader();
    
    fetch(`/api/products/${productId}`, {
        credentials: 'include'
    })
    .then(response => {
        console.log('Статус ответа:', response.status);
        
        if (!response.ok) {
            return response.json().then(err => {
                throw new Error(err.error || 'Ошибка сервера');
            });
        }
        return response.json();
    })
    .then(product => {
        console.log('Получены данные товара:', product);
        updateProductPage(product);
    })
    .catch(error => {
        console.error('Ошибка загрузки:', error);
        showError(error.message);
    })
    .finally(() => {
        hideLoader();
    });
}

// Обновление страницы товара
function updateProductPage(product) {
    try {
        // Основная информация
        document.title = `${product.name} | United Audio`;
        setElementContent('.product-title', product.name);
        setElementContent('.product-price', `${product.price.toLocaleString()} ₽`);
        setImageSource('.product-main-image', product.imageURL);

        // Описание товара
        const descriptionContainer = document.querySelector('.product-description');
        if (product.description) {
            descriptionContainer.innerHTML = product.description;
        } else {
            descriptionContainer.innerHTML = '<p class="no-description">Описание товара отсутствует</p>';
        }

        // Обновление характеристик
        setElementContent('.description-title', `Технические характеристики ${product.name}`);
        updateSpecificationTable(product);

    } catch (error) {
        console.error('Ошибка обновления страницы:', error);
        showError('Ошибка отображения данных');
    }
}

// Обновление таблицы характеристик
function updateSpecificationTable(product) {
    const specs = {
        'Бренд': 'Sony',
        'Тип подключения': 'Беспроводные',
        'Способ ношения': 'Накладные',
        'Импеданс': product.impedance ? `${product.impedance} Ом` : '38',
        'Вес': product.weight ? `${product.weight} г` : '250'
    };

    const tbody = document.querySelector('.product-characteristics tbody');
    tbody.innerHTML = '';

    for (const [key, value] of Object.entries(specs)) {
        tbody.innerHTML += `
            <tr>
                <td>${key}</td>
                <td>${value}</td>
            </tr>
        `;
    }
}

// Обработчик добавления в корзину
document.querySelector('.product-submit-btn').addEventListener('click', function() {
    const productId = getProductIdFromURL();
    
    if (!productId) {
        showError('Не удалось определить товар');
        return;
    }

    addToCart(productId);
});

// Функция добавления в корзину
function addToCart(productId) {
    showLoader();
    
    fetch('/api/cart/add', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        credentials: 'include',
        body: JSON.stringify({ productId: productId })
    })
    .then(response => {
        if (!response.ok) {
            return response.json().then(err => {
                throw new Error(err.error || 'Ошибка добавления');
            });
        }
        return response.json();
    })
    .then(data => {
        showTempAlert('Товар успешно добавлен в корзину!');
        updateCartCounter(data.totalItems);
    })
    .catch(error => {
        console.error('Ошибка:', error);
        showTempAlert(error.message, true);
    })
    .finally(() => {
        hideLoader();
    });
}

// Вспомогательные функции
function setElementContent(selector, content) {
    const element = document.querySelector(selector);
    if (element) element.textContent = content;
}

function setImageSource(selector, src) {
    const img = document.querySelector(selector);
    if (img) {
        img.src = src;
        img.onerror = () => {
            img.src = 'images/placeholder.jpg';
            console.warn('Изображение не найдено, установлен заглушка');
        };
    }
}

function showLoader() {
    document.getElementById('loader').style.display = 'block';
}

function hideLoader() {
    document.getElementById('loader').style.display = 'none';
}

function showTempAlert(message, isError = false) {
    const alert = document.createElement('div');
    alert.className = `temp-alert ${isError ? 'error' : 'success'}`;
    alert.textContent = message;
    
    document.body.appendChild(alert);
    setTimeout(() => alert.remove(), 3000);
}

function showError(message) {
    document.querySelector('main').innerHTML = `
        <div class="error-container">
            <h2>Ошибка</h2>
            <p>${message}</p>
            <a href="catalog.html" class="back-link">Вернуться в каталог</a>
        </div>
    `;
}