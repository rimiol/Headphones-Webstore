document.getElementById('addProductForm').addEventListener('submit', async function (e) {
    e.preventDefault();

    // Сброс сообщений
    document.querySelectorAll('.error-message').forEach(el => el.textContent = '');
    const formMessage = document.getElementById('formMessage');
    formMessage.textContent = '';

    // Сбор данных
    const formData = {
        name: document.getElementById('name').value.trim(),
        description: document.getElementById('description').value.trim(),
        imageURL: document.getElementById('imageURL').value.trim(),
        price: parseFloat(document.getElementById('price').value),
        connectionType: document.getElementById('connectionType').value,
        wearingStyle: document.getElementById('wearingStyle').value,
        brand: document.getElementById('brand').value
    };

    try {
        const response = await fetch('/api/AddItem', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(formData)
        });

        const data = await response.json();

        if (!response.ok) {
            // Детализированная обработка ошибок
            if (data.type && data.message) {
                const errorFieldId = `${data.type}Error`;
                const errorField = document.getElementById(errorFieldId);

                if (errorField) {
                    errorField.textContent = data.message;
                    errorField.style.display = 'block';
                } else {
                    formMessage.textContent = data.message;
                }
            } else {
                formMessage.textContent = 'Неизвестная ошибка сервера';
            }
            formMessage.style.color = 'red';
            return;
        }

        // Успешное добавление
        document.getElementById('addProductForm').reset();
        formMessage.textContent = `${data.message} ID: ${data.productId}`;
        formMessage.style.color = 'green';

    } catch (error) {
        console.error('Ошибка:', error);
        formMessage.textContent = 'Ошибка сети или сервера. Проверьте консоль для деталей.';
        formMessage.style.color = 'red';
    }
});