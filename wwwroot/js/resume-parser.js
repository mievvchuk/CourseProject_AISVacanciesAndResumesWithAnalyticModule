(function () {
    const form = document.querySelector('.resume-form');
    if (!form) {
        return;
    }

    const fileInput = form.querySelector('input[type="file"][name="ResumeFile"]');
    const status = form.querySelector('.resume-parse-status');
    const token = form.querySelector('input[name="__RequestVerificationToken"]');
    const replaceInput = form.querySelector('input[name="ReplaceFieldsFromFile"]');
    const replaceFieldsFromFile = form.dataset.replaceFieldsFromFile === 'true';

    if (!fileInput || !token) {
        return;
    }

    const setValue = function (name, value, overwrite) {
        if (value === null || value === undefined || value === '') {
            return;
        }

        const field = form.querySelector('[name="' + name + '"]');
        if (!field) {
            return;
        }

        if (!overwrite && field.value && field.value.trim() !== '' && field.value !== '0') {
            return;
        }

        field.value = value;
        field.dispatchEvent(new Event('input', { bubbles: true }));
        field.dispatchEvent(new Event('change', { bubbles: true }));
    };

    const setSelectByText = function (name, value) {
        if (!value) {
            return;
        }

        const field = form.querySelector('select[name="' + name + '"]');
        if (!field) {
            return;
        }

        const option = Array.from(field.options).find(function (item) {
            return item.value === value || item.text === value;
        });

        if (option) {
            field.value = option.value;
            field.dispatchEvent(new Event('change', { bubbles: true }));
        }
    };

    fileInput.addEventListener('change', async function () {
        if (!fileInput.files || fileInput.files.length === 0) {
            return;
        }

        if (status) {
            status.textContent = 'Зчитую дані з файлу...';
        }

        if (replaceInput && replaceFieldsFromFile) {
            replaceInput.value = 'true';
        }

        const data = new FormData();
        data.append('__RequestVerificationToken', token.value);
        data.append('ResumeFile', fileInput.files[0]);

        try {
            const response = await fetch('/Resumes/Parse', {
                method: 'POST',
                body: data,
                credentials: 'same-origin'
            });

            if (!response.ok) {
                throw new Error('parse failed');
            }

            const parsed = await response.json();
            setValue('DesiredPosition', parsed.desiredPosition, replaceFieldsFromFile);
            setValue('CategoryName', parsed.categoryName, replaceFieldsFromFile);
            setValue('Summary', parsed.summary, replaceFieldsFromFile);
            setValue('Education', parsed.education, replaceFieldsFromFile);
            setValue('Experience', parsed.experience, replaceFieldsFromFile);
            setValue('SkillsDescription', parsed.skillsDescription, replaceFieldsFromFile);
            setValue('ExperienceYears', parsed.experienceYears, replaceFieldsFromFile);
            setValue('DesiredSalary', parsed.desiredSalary, replaceFieldsFromFile);
            setSelectByText('EmploymentType', parsed.employmentType);
            setSelectByText('ExperienceLevel', parsed.experienceLevel);
            setSelectByText('EducationLevel', parsed.educationLevel);

            if (replaceInput) {
                replaceInput.value = 'false';
            }

            if (window.jQuery && window.jQuery.validator) {
                window.jQuery(form).valid();
            }

            if (status) {
                status.textContent = parsed.message || 'Дані з файлу підтягнуто.';
            }
        } catch {
            if (status) {
                status.textContent = 'Не вдалося автоматично зчитати файл. Спробуйте DOCX або текстовий PDF.';
            }
        }
    });
})();
