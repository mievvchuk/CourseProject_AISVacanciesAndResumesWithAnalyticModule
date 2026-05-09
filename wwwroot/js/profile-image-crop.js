(function () {
  document.querySelectorAll('.js-profile-image-form').forEach((form) => {
    const input = form.querySelector('.js-profile-image-input');
    const panel = form.querySelector('.js-profile-crop-panel');
    const cropBox = form.querySelector('.profile-crop-box');
    const cropImage = form.querySelector('.js-profile-crop-image');
    const preview = form.querySelector('.js-profile-image-preview');
    const zoom = form.querySelector('.js-profile-crop-zoom');
    const positionX = form.querySelector('.js-profile-crop-x');
    const positionY = form.querySelector('.js-profile-crop-y');

    let sourceImage = null;

    if (!input || !panel || !cropBox || !cropImage || !zoom || !positionX || !positionY) {
      return;
    }

    const lockCropLayout = () => {
      panel.style.display = 'grid';
      panel.style.gridTemplateColumns = '240px minmax(0, 1fr)';
      panel.style.gap = '1rem';
      panel.style.alignItems = 'start';
      panel.style.marginTop = '0.75rem';

      cropBox.style.width = '240px';
      cropBox.style.height = '240px';
      cropBox.style.maxWidth = '240px';
      cropBox.style.maxHeight = '240px';
      cropBox.style.overflow = 'hidden';
      cropBox.style.position = 'relative';
      cropBox.style.borderRadius = '12px';

      cropImage.style.display = 'block';
      cropImage.style.width = '100%';
      cropImage.style.height = '100%';
      cropImage.style.maxWidth = '100%';
      cropImage.style.maxHeight = '100%';
      cropImage.style.objectFit = 'cover';
    };

    const getCropArea = () => {
      const scale = Number.parseFloat(zoom.value) || 1;
      const sourceWidth = sourceImage.naturalWidth;
      const sourceHeight = sourceImage.naturalHeight;

      const cropSize = Math.min(sourceWidth, sourceHeight) / scale;

      const maxX = Math.max(0, sourceWidth - cropSize);
      const maxY = Math.max(0, sourceHeight - cropSize);

      const sx = maxX * ((Number.parseFloat(positionX.value) || 50) / 100);
      const sy = maxY * ((Number.parseFloat(positionY.value) || 50) / 100);

      return {
        sx,
        sy,
        size: cropSize
      };
    };

    const drawCroppedImage = (outputSize) => {
      if (!sourceImage) {
        return null;
      }

      const canvas = document.createElement('canvas');
      canvas.width = outputSize;
      canvas.height = outputSize;

      const context = canvas.getContext('2d');

      if (!context) {
        return null;
      }

      const crop = getCropArea();

      context.imageSmoothingEnabled = true;
      context.imageSmoothingQuality = 'high';

      context.drawImage(
        sourceImage,
        crop.sx,
        crop.sy,
        crop.size,
        crop.size,
        0,
        0,
        outputSize,
        outputSize
      );

      return canvas;
    };

    const updateHeaderPreview = (url) => {
      if (!preview) {
        return;
      }

      preview.classList.remove('d-none');
      preview.nextElementSibling?.classList.add('d-none');

      if (preview.tagName === 'IMG') {
        preview.src = url;
        return;
      }

      preview.innerHTML = '';

      const img = document.createElement('img');
      img.src = url;
      img.alt = 'Попередній перегляд фото';
      img.style.width = '100%';
      img.style.height = '100%';
      img.style.objectFit = 'cover';
      img.style.display = 'block';

      preview.appendChild(img);
    };

    const updatePreview = () => {
      lockCropLayout();

      const canvas = drawCroppedImage(800);

      if (!canvas) {
        return;
      }

      const previewUrl = canvas.toDataURL('image/jpeg', 0.9);

      cropImage.src = previewUrl;
      updateHeaderPreview(previewUrl);
    };

    input.addEventListener('change', () => {
      const file = input.files && input.files[0];

      if (!file || !file.type.startsWith('image/')) {
        return;
      }

      const objectUrl = URL.createObjectURL(file);
      sourceImage = new Image();

      sourceImage.onload = () => {
        panel.classList.remove('d-none');
        lockCropLayout();

        zoom.value = '1';
        positionX.value = '50';
        positionY.value = '50';

        updatePreview();

        URL.revokeObjectURL(objectUrl);
      };

      sourceImage.src = objectUrl;
    });

    [zoom, positionX, positionY].forEach((control) => {
      control.addEventListener('input', updatePreview);
    });

    form.addEventListener('submit', (event) => {
      if (!sourceImage || !input.files || input.files.length === 0) {
        return;
      }

      event.preventDefault();

      const canvas = drawCroppedImage(800);

      if (!canvas) {
        form.submit();
        return;
      }

      canvas.toBlob((blob) => {
        if (!blob) {
          form.submit();
          return;
        }

        const originalName = input.files[0].name.replace(/\.[^.]+$/, '');
        const croppedFile = new File([blob], `${originalName}-cropped.jpg`, {
          type: 'image/jpeg'
        });

        const transfer = new DataTransfer();
        transfer.items.add(croppedFile);
        input.files = transfer.files;

        form.submit();
      }, 'image/jpeg', 0.92);
    });
  });
})();