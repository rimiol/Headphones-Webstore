document.getElementById('addProductForm').addEventListener('submit', async function (e) {
    e.preventDefault();

    resetErrors();
    const formMessage = document.getElementById('formMessage');
    hideElement(formMessage);

    const priceInput = document.getElementById('price');
    const price = parseFloat(priceInput.value);
    if (!validatePrice(price)) {
        showError('priceError', 'Введите корректную цену (больше 0)');
        priceInput.focus();
        return;
    }

    const formData = getFormData();

    try {
        const response = await fetch('/api/AddItem', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'X-Requested-With': 'XMLHttpRequest'
            },
            body: JSON.stringify(formData)
        });

        const data = await handleResponse(response);

        if (!response.ok) {
            handleApiError(response.status, data);
            return;
        }

        handleSuccess(data);

    } catch (error) {
        handleNetworkError(error);
    }
});

function resetErrors() {
    document.querySelectorAll('.error-message').forEach(el => {
        el.textContent = '';
        el.style.display = 'none';
    });
}

function getFormData() {
    return {
        name: document.getElementById('name').value.trim(),
        description: document.getElementById('description').value.trim(),
        imageURL: document.getElementById('imageURL').value.trim(),
        price: parseFloat(document.getElementById('price').value),
        connectionType: document.getElementById('connectionType').value.trim(),
        wearingStyle: document.getElementById('wearingStyle').value.trim(),
        brand: document.getElementById('brand').value.trim()
    };
}

async function handleResponse(response) {
    const contentType = response.headers.get('content-type');

    if (!contentType?.includes('application/json')) {
        return {
            message: `Ошибка сервера (${response.status})`,
            details: await response.text()
        };
    }
    return response.json();
}

function handleApiError(status, data) {
    switch (status) {
        case 400:
            handleValidationErrors(data);
            break;
        case 404:
            showFieldError('imageURLError', data.message);
            break;
        case 409:
            showFieldError('nameError', data.message);
            break;
        case 500:
            showError('formMessage', `${data.message}: ${data.details}`);
            break;
        default:
            showError('formMessage', `Ошибка ${status}: ${data.message || 'Неизвестная ошибка'}`);
    }
}

function handleValidationErrors(data) {
    if (data.errors) {
        data.errors.forEach(error => {
            const fieldMatch = error.match(/'(.*?)'/);
            const field = fieldMatch ? fieldMatch[1] : 'form';
            showError(`${field}Error`, error);
        });
    } else {
        showFieldError(`${data.type}Error`, data.message);
    }
}

function validatePrice(price) {
    return !isNaN(price) && price > 0;
}

function showFieldError(fieldId, message) {
    const errorElement = document.getElementById(fieldId);
    if (errorElement) {
        errorElement.textContent = message;
        showElement(errorElement);
        scrollToElement(errorElement);
    } else {
        showError('formMessage', message);
    }
}

function handleSuccess(data) {
    document.getElementById('addProductForm').reset();
    showSuccess(`Товар успешно добавлен! ID: ${data.productId}`);
}

function handleNetworkError(error) {
    console.error('Network Error:', error);
    showError('formMessage', 'Ошибка сети. Проверьте соединение');
}

function showError(elementId, message) {
    const element = document.getElementById(elementId);
    if (element) {
        element.textContent = message;
        showElement(element);
        scrollToElement(element);
    }
}

function showSuccess(message) {
    const formMessage = document.getElementById('formMessage');
    formMessage.textContent = message;
    formMessage.style.color = '#28a745';
    showElement(formMessage);
    window.scrollTo({ top: 0, behavior: 'smooth' });
}

function showElement(element) {
    element.style.display = 'block';
}

function hideElement(element) {
    element.style.display = 'none';
}

function scrollToElement(element) {
    element.scrollIntoView({
        behavior: 'smooth',
        block: 'center',
        inline: 'nearest'
    });
}