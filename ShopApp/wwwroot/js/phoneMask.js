window.applyPhoneMask = () => {
    const phoneInput = document.getElementById("phone");
    if (phoneInput) {
        const mask = new Inputmask({
            mask: "+7-999-999-99-99",
            showMaskOnHover: false,
            showMaskOnFocus: true,
            placeholder: "_",
            clearIncomplete: true,
            onBeforePaste: (pastedValue, opts) => {
                // Удаляем всё, кроме цифр, и обрезаем до 10 цифр
                const digits = pastedValue.replace(/[^\d]/g, "").slice(0, 10);
                return "+7" + digits;
            },
            onBeforeWrite: (e, buffer, caretPos, opts) => {
                // Гарантируем, что префикс всегда +7
                if (!buffer.join("").startsWith("+7")) {
                    return { buffer: ["+", "7"].concat(buffer.slice(2)) };
                }
            }
        });
        mask.mask(phoneInput);
    }
};

