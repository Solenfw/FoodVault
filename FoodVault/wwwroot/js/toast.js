window.toast = (function(){
  const MAX = 5;
  let region = null;
  const icons = {
    success: '<svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M20 6L9 17l-5-5"/></svg>',
    error: '<svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="10"/><line x1="15" y1="9" x2="9" y2="15"/><line x1="9" y1="9" x2="15" y2="15"/></svg>',
    warning: '<svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M10.29 3.86L1.82 18a2 2 0 001.71 3h16.94a2 2 0 001.71-3L13.71 3.86a2 2 0 00-3.42 0z"/><line x1="12" y1="9" x2="12" y2="13"/><line x1="12" y1="17" x2="12" y2="17"/></svg>',
    info: '<svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="10"/><line x1="12" y1="16" x2="12" y2="12"/><line x1="12" y1="8" x2="12.01" y2="8"/></svg>'
  };

  function ensureRegion(position){
    if (!region){
      region = document.getElementById('toastRegion');
      if (!region){
        region = document.createElement('div');
        region.id = 'toastRegion';
        document.body.appendChild(region);
      }
      region.className = 'fv-toasts' + (position==='bottom-right' ? ' bottom' : '');
    }
    return region;
  }

  function createToast(type, message, opts){
    const duration = Math.max(1000, Math.min(15000, (opts && opts.duration) || 3500));
    const position = (opts && opts.position) || 'top-right';
    const container = ensureRegion(position);
    // Enforce max stack
    const existing = container.querySelectorAll('.fv-toast');
    if (existing.length >= MAX){
      container.removeChild(existing[0]);
    }
    const el = document.createElement('div');
    el.className = `fv-toast ${type}`;
    el.innerHTML = `
      <div class="fv-icon">${icons[type] || ''}</div>
      <div class="fv-content">${message}</div>
      <button class="fv-close" aria-label="Close">×</button>
      <div class="fv-progress"><i></i></div>
    `;
    const bar = el.querySelector('.fv-progress > i');
    let closing = false;
    function close(){
      if (closing) return; closing = true;
      el.classList.remove('show'); el.classList.add('hide');
      setTimeout(()=>{ el.remove(); }, 220);
    }
    el.querySelector('.fv-close').addEventListener('click', close);
    el.addEventListener('click', close);
    container.appendChild(el);
    requestAnimationFrame(()=>{
      el.classList.add('show');
      // progress
      let start;
      function step(ts){
        if (!start) start = ts;
        const p = Math.min(1, (ts - start) / duration);
        bar.style.width = (p*100).toFixed(1) + '%';
        if (p < 1 && !closing){ requestAnimationFrame(step); } else { close(); }
      }
      requestAnimationFrame(step);
    });
    return { close };
  }

  function api(type){
    return function(message, opts){ return createToast(type, message, opts); };
  }

  return {
    success: api('success'),
    error: api('error'),
    warning: api('warning'),
    info: api('info')
  };
})();


