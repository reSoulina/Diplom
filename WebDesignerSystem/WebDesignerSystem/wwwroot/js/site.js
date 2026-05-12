// site.js – единый файл со всей клиентской логикой

$(document).ready(function () {
    // 1. Автоматическое скрытие алертов через 5 секунд
    setTimeout(function () {
        $('.alert').not('.alert-permanent').fadeOut(500);
    }, 5000);

    // 2. Плавный скролл к якорям
    $('a[href^="#"]').on('click', function (e) {
        if ($(this).attr('href') !== '#') {
            e.preventDefault();
            const target = $($(this).attr('href'));
            if (target.length) {
                $('html, body').animate({
                    scrollTop: target.offset().top - 80
                }, 500);
            }
        }
    });

    // 3. Подтверждение удаления для ссылок с классом .delete-confirm
    $(document).on('click', '.delete-confirm', function (e) {
        if (!confirm('Вы уверены, что хотите удалить этот элемент? Действие необратимо.')) {
            e.preventDefault();
        }
    });

    // 4. Открытие фото в новой вкладке (каталог)
    const imageContainers = document.querySelectorAll('.product-image-container');
    imageContainers.forEach(container => {
        const img = container.querySelector('img');
        if (img) {
            img.style.cursor = 'pointer';
            img.addEventListener('click', (e) => {
                e.stopPropagation();
                window.open(img.src, '_blank');
            });
        } else {
            container.style.cursor = 'pointer';
            container.addEventListener('click', (e) => {
                e.stopPropagation();
                window.open('/images/no-image.png', '_blank');
            });
        }
    });

    // 5. Переключение полей онлайн/офлайн при записи на услугу
    const formatSelect = document.getElementById('formatSelect');
    if (formatSelect) {
        const onlineFields = document.getElementById('onlineFields');
        const offlineFields = document.getElementById('offlineFields');
        const toggleFields = () => {
            if (formatSelect.value === 'online') {
                if (onlineFields) onlineFields.style.display = 'block';
                if (offlineFields) offlineFields.style.display = 'none';
            } else if (formatSelect.value === 'offline') {
                if (onlineFields) onlineFields.style.display = 'none';
                if (offlineFields) offlineFields.style.display = 'block';
            } else {
                if (onlineFields) onlineFields.style.display = 'none';
                if (offlineFields) offlineFields.style.display = 'none';
            }
        };
        formatSelect.addEventListener('change', toggleFields);
        toggleFields(); // установить начальное состояние
    }

    // 6. Предпросмотр изображения при загрузке файла (общий обработчик)
    $(document).on('change', '.file-input-preview', function () {
        const file = this.files[0];
        const previewContainerId = $(this).data('preview-container');
        const previewImgId = $(this).data('preview-image');

        if (file && previewContainerId && previewImgId) {
            const reader = new FileReader();
            reader.onload = function (e) {
                $(`#${previewImgId}`).attr('src', e.target.result);
                $(`#${previewContainerId}`).show();
            };
            reader.readAsDataURL(file);
        } else {
            if (previewContainerId) $(`#${previewContainerId}`).hide();
        }
    });
});

// 7. Утилитные функции (могут быть использованы в любом месте)
function formatPrice(price) {
    return new Intl.NumberFormat('ru-RU', {
        style: 'currency',
        currency: 'RUB',
        minimumFractionDigits: 0
    }).format(price);
}

function showLoading(button) {
    const $button = $(button);
    $button.prop('disabled', true);
    $button.html('<span class="loading-spinner"></span> Загрузка...');
}

function hideLoading(button, originalText) {
    const $button = $(button);
    $button.prop('disabled', false);
    $button.text(originalText);
}