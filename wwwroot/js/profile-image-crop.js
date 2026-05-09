(function () {
  const CROP_BOX_SIZE = 240;
  const OUTPUT_SIZE = 800;

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

    const applyFixedLayout = () => {
      panel.style.display = 'grid';
      panel.style.gridTemplateColumns = `${CROP_BOX_SIZE}px minmax(0, 1fr)`;
      panel.style.gap = '1rem';
      panel.style.alignItems = 'start';
      panel.style.marginTop = '0.75rem';
      panel.style.maxWidth = '100%';
      panel.style.overflow = 'hidden';

      cropBox.style.width = `${CROP_BOX_SIZE}px`;
      cropBox.style.height = `${CROP_BOX_SIZE}px`;
      cropBox.style.minWidth = `${CROP_BOX_SIZE}px`;
      cropBox.style.minHeight = `${CROP_BOX_SIZE}px`;
      cropBox.style.maxWidth = `${CROP_BOX_SIZE}px`;
      cropBox.style.maxHeight = `${CROP_BOX_SIZE}px`;
      cropBox.style.overflow = 'hidden';
      cropBox.style.position = 'relative';
      cropBox.style.borderRadius = '12px';
      cropBox.style.backgroundColor = '#eef2f7';
      cropBox.style.border = '1px dashed rgba(98, 105, 118, 0.32)';

      cropImage.style.display = 'block';
      cropImage.style.width = '100%';
      cropImage.style.height = '100%';
      cropImage.style.minWidth = '100%';
      cropImage.style.minHeight = '100%';
      cropImage.style.maxWidth = '100%';
      cropImage.style.maxHeight = '100%';
      cropImage.style.objectFit = 'cover';
      cropImage.style.borderRadius = '12px';
    };

    const getNumber = (value, fallback) => {
      const parsed = Number.parseFloat(value);
      return Number.isFinite(parsed) ? parsed : fallback;
    };

    const getCropArea = () => {
      const scale = getNumber(zoom.value, 1);
      const sourceWidth = sourceImage.naturalWidth;
      const sourceHeight = sourceImage.naturalHeight;

      const cropSize = Math.min(sourceWidth, sourceHeight) / scale;

      const maxX = Math.max(0, sourceWidth - cropSize);
      const maxY = Math.max(0, sourceHeight - cropSize);

      const sx = maxX * (getNumber(positionX.value, 50) / 100);
      const sy = maxY * (getNumber(positionY.value, 50) / 100);

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

    const setPreviewImage = (url) => {
      if (!preview) {
        return;
      }

      preview.classList.remove('d-none');
      preview.nextElementSibling?.classList.add('d-none');

      if (preview.tagName === 'IMG') {
        preview.src = url;
        preview.style.width = '6.75rem';
        preview.style.height = '6.75rem';
        preview.style.maxWidth = '6.75rem';
        preview.style.maxHeight = '6.75rem';
        preview.style.objectFit = 'cover';
        preview.style.display = 'block';
        preview.style.borderRadius = '8px';
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
      img.style.borderRadius = '8px';

      preview.appendChild(img);
    };

    const updatePreview = () => {
      applyFixedLayout();

      const canvas = drawCroppedImage(OUTPUT_SIZE);

      if (!canvas) {
        return;
      }

      const previewUrl = canvas.toDataURL('image/jpeg', 0.9);

      cropImage.src = previewUrl;
      setPreviewImage(previewUrl);
    };

    input.addEventListener('change', () => {
      const file = input.files && input.files[0];

      if (!file || !file.type.startsWith('image/')) {
        return;
      }

      const objectUrl = URL.createObjectURL(file);
      const image = new Image();

      image.onload = () => {
        sourceImage = image;

        panel.classList.remove('d-none');
        applyFixedLayout();

        zoom.value = '1';
        positionX.value = '50';
        positionY.value = '50';

        updatePreview();

        URL.revokeObjectURL(objectUrl);
      };

      image.src = objectUrl;
    });

    [zoom, positionX, positionY].forEach((control) => {
      control.addEventListener('input', updatePreview);
      control.addEventListener('change', updatePreview);
    });

    form.addEventListener('submit', (event) => {
      if (!sourceImage || !input.files || input.files.length === 0) {
        return;
      }

      event.preventDefault();

      const canvas = drawCroppedImage(OUTPUT_SIZE);

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