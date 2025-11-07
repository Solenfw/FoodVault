(function(){
  const MAX_SIZE = 5 * 1024 * 1024;
  const ALLOWED = ['image/jpeg','image/png','image/webp'];

  function init(root){
    const input = root.querySelector('[data-input]');
    const pick = root.querySelector('[data-pick]');
    const previews = document.querySelector('[data-previews]');
    const container = root.dataset.container || 'uploads';
    const basePrefix = (root.dataset.prefix || '').replace(/\/$/, '');
    const aspect = root.dataset.aspect || '16:9';
    const deleteOld = (root.dataset.deleteOld || 'false') === 'true';
    let oldPaths = [];
    try { oldPaths = JSON.parse(root.dataset.oldPaths || '[]'); } catch(_) {}

    function choose(){ input.click(); }
    pick.addEventListener('click', choose);
    root.addEventListener('dragover', e=>{ e.preventDefault(); root.classList.add('dragover'); });
    root.addEventListener('dragleave', ()=> root.classList.remove('dragover'));
    root.addEventListener('drop', e=>{ e.preventDefault(); root.classList.remove('dragover'); handleFiles(e.dataTransfer.files); });
    input.addEventListener('change', ()=> handleFiles(input.files));

    // paste support
    window.addEventListener('paste', e=>{
      if (document.activeElement && (document.activeElement.tagName === 'INPUT' || document.activeElement.tagName === 'TEXTAREA')) return;
      const items = e.clipboardData?.files; if (items && items.length) handleFiles(items);
    });

    async function handleFiles(fileList){
      const files = Array.from(fileList);
      for (const file of files){
        if (!ALLOWED.includes(file.type) || file.size > MAX_SIZE){ toast && toast.error('File không hợp lệ hoặc quá lớn'); continue; }
        const thumb = document.createElement('div'); thumb.className='fv-thumb';
        const img = document.createElement('img'); thumb.appendChild(img);
        const bar = document.createElement('div'); bar.className='fv-progress'; bar.innerHTML='<i></i>'; thumb.appendChild(bar);
        const remove = document.createElement('button'); remove.className='fv-remove'; remove.textContent='×'; remove.addEventListener('click', ()=> thumb.remove()); thumb.appendChild(remove);
        previews.appendChild(thumb);
        // preview
        img.src = URL.createObjectURL(file);
        // crop/resize/compress to WebP using ImageCropper
        let mainBlob;
        try {
          mainBlob = await (window.ImageCropper ? ImageCropper.cropResize(file, aspect, 1920, 1080) : file);
        } catch(_) { mainBlob = file; }
        // upload main
        const p = basePrefix ? basePrefix + '/' : '';
        const mainRes = await uploadFile(mainBlob, bar.querySelector('i'), container, p);
        // thumbnails
        const sizes = [ [150,150], [300,300], [800,600] ];
        for (const [w,h] of sizes){
          try {
            const b = await (window.ImageCropper ? ImageCropper.cropResize(file, `${w}:${h}`, w, h) : file);
            await uploadFile(b, null, container, p + 'thumbs');
          } catch(_) {}
        }
        // delete old paths if configured
        if (deleteOld && Array.isArray(oldPaths) && oldPaths.length){
          oldPaths.forEach(path=>{ try { fetch(`/api/images?container=${encodeURIComponent(container)}&path=${encodeURIComponent(path)}`, { method: 'DELETE' }); } catch(_){} });
          oldPaths = [];
        }
      }
    }

    async function uploadFile(file, progressEl, container, prefix){
      const form = new FormData(); form.append('file', file, 'upload.webp');
      const xhr = new XMLHttpRequest();
      const qs = `?container=${encodeURIComponent(container)}&prefix=${encodeURIComponent(prefix||'')}`;
      xhr.open('POST', '/api/images/upload' + qs);
      xhr.upload.onprogress = e=>{ if (e.lengthComputable && progressEl){ progressEl.style.width = ((e.loaded/e.total)*100).toFixed(1)+'%'; } };
      return new Promise((resolve)=>{
        xhr.onreadystatechange = function(){ if (xhr.readyState===4){ resolve(xhr.responseText ? JSON.parse(xhr.responseText) : null); } };
        xhr.send(form);
      });
    }
  }

  document.addEventListener('DOMContentLoaded', function(){
    document.querySelectorAll('[data-upload]').forEach(init);
  });
})();


