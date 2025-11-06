(function(){
  async function loadImage(file){
    return new Promise((resolve,reject)=>{
      const img = new Image();
      img.onload = ()=> resolve(img);
      img.onerror = reject;
      img.src = URL.createObjectURL(file);
    });
  }
  function drawToCanvas(img, opts){
    const { targetW, targetH, mime } = opts;
    const canvas = document.createElement('canvas');
    canvas.width = targetW; canvas.height = targetH;
    const ctx = canvas.getContext('2d');
    ctx.drawImage(img, 0, 0, targetW, targetH);
    return new Promise(resolve=> canvas.toBlob(b=> resolve(b), mime || 'image/webp', 0.9));
  }
  window.ImageCropper = {
    async cropResize(file, aspect='16:9', maxW=1920, maxH=1080){
      const img = await loadImage(file);
      const [aw, ah] = aspect.split(':').map(Number);
      const targetRatio = aw/ah;
      const srcRatio = img.width/img.height;
      // compute crop rect preserving aspect
      let sx=0, sy=0, sw=img.width, sh=img.height;
      if (srcRatio > targetRatio){
        // crop width
        sw = Math.round(img.height * targetRatio);
        sx = Math.round((img.width - sw)/2);
      } else {
        // crop height
        sh = Math.round(img.width / targetRatio);
        sy = Math.round((img.height - sh)/2);
      }
      // draw crop to temp canvas then scale down if needed
      const crop = document.createElement('canvas');
      crop.width = sw; crop.height = sh;
      crop.getContext('2d').drawImage(img, sx, sy, sw, sh, 0, 0, sw, sh);
      // scale
      let tw = sw, th = sh;
      const scale = Math.min(1, Math.min(maxW/sw, maxH/sh));
      tw = Math.round(sw*scale); th = Math.round(sh*scale);
      const out = document.createElement('canvas');
      out.width = tw; out.height = th;
      out.getContext('2d').drawImage(crop, 0, 0, sw, sh, 0, 0, tw, th);
      return new Promise(resolve=> out.toBlob(b=> resolve(b), 'image/webp', 0.85));
    }
  };
})();
