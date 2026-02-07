// site.js
// Базовые JavaScript функции для сайта

$(document).ready(function () {
    // Автоматическое скрытие алертов через 5 секунд
    setTimeout(function () {
        $('.alert').not('.alert-permanent').fadeOut(500);
    }, 5000);

    // Подтверждение важных действий
    $('form').on('submit', function (e) {
        if ($(this).hasClass('confirm-submit')) {
            if (!confirm('Вы уверены, что хотите выполнить это действие?')) {
                e.preventDefault();
            }
        }
    });

    // Плавное появление элементов при прокрутке
    if (typeof IntersectionObserver !== 'undefined') {
        const observer = new IntersectionObserver((entries) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    entry.target.classList.add('fade-in');
                }
            });
        }, { threshold: 0.1 });

        document.querySelectorAll('.animate-on-scroll').forEach(el => observer.observe(el));
    }

    // Валидация форм
    $('form').on('blur', 'input, select, textarea', function () {
        validateField($(this));
    });

    function validateField($field) {
        const value = $field.val();
        const $formGroup = $field.closest('.form-group, .mb-3');

        if ($field.prop('required') && !value) {
            $formGroup.addClass('has-error');
            return false;
        }

        $formGroup.removeClass('has-error');
        return true;
    }

    // Обработка загрузки изображений
    $('input[type="file"]').on('change', function (e) {
        const fileName = e.target.files[0]?.name;
        if (fileName) {
            $(this).next('.custom-file-label').text(fileName);
        }
    });

    // Плавный скролл к якорям
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
});

// Утилитные функции
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

// Обработчик для кнопок с загрузкой
$(document).on('click', '.btn-loading', function () {
    const $btn = $(this);
    const originalText = $btn.text();
    showLoading($btn);

    // Через 3 секунды сбрасываем (на случай если что-то пошло не так)
    setTimeout(() => {
        hideLoading($btn, originalText);
    }, 3000);
});