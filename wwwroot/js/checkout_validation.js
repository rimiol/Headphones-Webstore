document.getElementById('checkoutForm').addEventListener('submit', function (e) {
    e.preventDefault();

    const fullName = document.getElementById('fullName').value.trim();
    const email = document.getElementById('email').value.trim();
    const phone = document.getElementById('phone').value.trim();
    const postalCode = document.getElementById('postalCode').value.trim();
    const deliveryDate = document.getElementById('deliveryDate').value;
    const errorMessages = document.getElementById('errorMessages');

    let isValid = true;
    errorMessages.innerHTML = ''; 

    if (!/^[А-Яа-яЁё\s]+$/.test(fullName)) {
        showError('Имя и фамилия должны содержать только буквы и пробелы.');
        isValid = false;
    }

    if (!/^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/.test(email)) {
        showError('Введите корректный Email.');
        isValid = false;
    }

    if (!/^\+7\d{10}$/.test(phone)) {
        showError('Телефон должен быть в формате +7XXXXXXXXXX.');
        isValid = false;
    }

    if (!/^\d{6}$/.test(postalCode)) {
        showError('Почтовый индекс должен содержать 6 цифр.');
        isValid = false;
    }

    if (!isValidDeliveryDate(deliveryDate)) {
        showError('Дата доставки должна быть не раньше завтрашнего дня.');
        isValid = false;
    }

    if (isValid) {
        alert('Форма успешно отправлена!');
        e.target.submit();
    }
});

function showError(message) {
    const errorMessages = document.getElementById('errorMessages');
    const error = document.createElement('div');
    error.className = 'error';
    error.style.color = 'red';
    error.textContent = message;
    errorMessages.appendChild(error);
}

function isValidDeliveryDate(date) {
    const today = new Date();
    const tomorrow = new Date(today);
    tomorrow.setDate(today.getDate() + 1);

    const selectedDate = new Date(date);
    return selectedDate >= tomorrow;
}
