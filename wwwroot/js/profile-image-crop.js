(function () {
  document.querySelectorAll('.js-profile-image-form').forEach((form) => {
    const input = form.querySelector('.js-profile-image-input');
    const panel = form.querySelector('.js-profile-crop-panel');
    const image = form.querySelector('.js-profile-crop-image');
    const preview = form.querySelector('.js-profile-image-preview');
    const zoom = form.querySelector('.js-profile-crop-zoom');
    const positionX = form.querySelector('.js-profile-crop-x');
    const positionY = form.querySelector('.js-profile-crop-y');
    let sourceImage = null;

    if (!input || !panel || !image || !zoom || !positionX || !positionY) {
      return;
    }

    const updatePreview = () => {
      const transform = `translate(-${positionX.value}%, -${positionY.value}%) scale(${zoom.value})`;
      image.style.transform = transform;
      image.style.left = `${positionX.value}%`;
      image.style.top = `${positionY.value}%`;
    };

    const setPreviewImage = (url) => {
      image.src = url;
      if (preview) {
        if (preview.tagName === 'IMG') {
          preview.src = url;
        } else {
          preview.innerHTML = '';
          const img = document.createElement('img');
          img.src = url;
          img.alt = 'Попередній перегляд фото';
          preview.appendChild(img);
        }
      }
    };

    input.addEventListener('change', () => {
      const file = input.files && input.files[0];
      if (!file || !file.type.startsWith('image/')) {
        return;
      }

      const url = URL.createObjectURL(file);
      sourceImage = new Image();
      sourceImage.onload = () => {
        setPreviewImage(url);
        panel.classList.remove('d-none');
        zoom.value = '1';
        positionX.value = '50';
        positionY.value = '50';
        updatePreview();
      };
      sourceImage.src = url;
    });

    [zoom, positionX, positionY].forEach((control) => {
      control.addEventListener('input', updatePreview);
    });

    form.addEventListener('submit', (event) => {
      if (!sourceImage || !input.files || input.files.length === 0) {
        return;
      }

      event.preventDefault();
      const outputSize = 800;
      const canvas = document.createElement('canvas');
      canvas.width = outputSize;
      canvas.height = outputSize;
      const context = canvas.getContext('2d');
      const scale = Number.parseFloat(zoom.value);
      const sourceSize = Math.min(sourceImage.naturalWidth, sourceImage.naturalHeight) / scale;
      const maxX = Math.max(0, sourceImage.naturalWidth - sourceSize);
      const maxY = Math.max(0, sourceImage.naturalHeight - sourceSize);
      const sx = maxX * (Number.parseFloat(positionX.value) / 100);
      const sy = maxY * (Number.parseFloat(positionY.value) / 100);

      context.drawImage(sourceImage, sx, sy, sourceSize, sourceSize, 0, 0, outputSize, outputSize);
      canvas.toBlob((blob) => {
        if (!blob) {
          form.submit();
          return;
        }

        const originalName = input.files[0].name.replace(/\.[^.]+$/, '');
        const croppedFile = new File([blob], `${originalName}-cropped.jpg`, { type: 'image/jpeg' });
        const transfer = new DataTransfer();
        transfer.items.add(croppedFile);
        input.files = transfer.files;
        form.submit();
      }, 'image/jpeg', 0.92);
    });
  });
})();
