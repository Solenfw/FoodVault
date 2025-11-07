(function(){
    const PAGE_SIZE = 12;
    const DEBOUNCE_MS = 200;
    const TRIGGER_RATIO = 0.8; // 80%

    let page = 1;
    let loading = false;
    let hasMore = true;
    let cache = [];
    let lastCall = 0;

    const grid = document.getElementById('recipeGrid');
    const loader = document.getElementById('recipeLoader');
    const errorBox = document.getElementById('recipeError');
    const emptyBox = document.getElementById('recipeEmpty');

    function debounce(fn, wait){
        return function(){
            const now = Date.now();
            if (now - lastCall < wait) return;
            lastCall = now;
            return fn.apply(this, arguments);
        };
    }

    function showLoader(show){ if (loader) loader.style.display = show ? '' : 'none'; }
    function showError(msg){ if (errorBox){ errorBox.textContent = msg || 'Failed to load'; errorBox.style.display = ''; } }
    function clearError(){ if (errorBox){ errorBox.style.display = 'none'; errorBox.textContent = ''; } }

    function renderSkeleton(count){
        if (!grid) return;
        for (let i=0;i<count;i++){
            const div = document.createElement('div');
            div.className = 'skeleton-card';
            div.innerHTML = '<div class="sk-img shimmer"></div><div class="sk-title shimmer"></div><div class="sk-meta shimmer"></div>';
            grid.appendChild(div);
        }
    }
    function clearSkeleton(){
        grid?.querySelectorAll('.skeleton-card').forEach(n=>n.remove());
    }

    function renderItems(items){
        if (!grid) return;
        const frag = document.createDocumentFragment();
        // Get existing recipe IDs to avoid duplicates
        const existingIds = new Set();
        // Check all cards by data attribute or link
        grid.querySelectorAll('.col').forEach(col => {
            const card = col.querySelector('.card');
            if (card) {
                const link = card.querySelector('a[href*="/Recipe/Details/"]');
                if (link) {
                    const match = link.href.match(/\/Recipe\/Details\/([^\/\?]+)/);
                    if (match) existingIds.add(match[1]);
                }
                const hiddenInput = card.querySelector('input[type="hidden"][name="id"]');
                if (hiddenInput && hiddenInput.value) {
                    existingIds.add(hiddenInput.value);
                }
            }
        });
        
        items.forEach(r=>{
            // Skip if recipe already exists
            if (existingIds.has(r.id)) return;
            
            const card = document.createElement('div');
            card.className = 'card h-100 shadow-sm';
            
            // Image
            if (r.imageUrl){
                const img = document.createElement('img');
                img.src = r.imageUrl;
                img.className = 'card-img-top object-fit-cover';
                img.style.aspectRatio = '16/9';
                img.style.maxHeight = '200px';
                img.alt = r.title;
                card.appendChild(img);
            } else {
                const placeholder = document.createElement('div');
                placeholder.className = 'card-img-top bg-light d-flex align-items-center justify-content-center';
                placeholder.style.aspectRatio = '16/9';
                placeholder.style.maxHeight = '200px';
                placeholder.innerHTML = '<i class="bi bi-image text-muted" style="font-size: 3rem;"></i>';
                card.appendChild(placeholder);
            }
            
            // Body
            const body = document.createElement('div');
            body.className = 'card-body d-flex flex-column';
            body.innerHTML = `<h5 class="card-title">${r.title}</h5><p class="card-text text-muted small flex-grow-1">${r.description||''}</p>`;
            
            // Footer with buttons
            const buttonContainer = document.createElement('div');
            buttonContainer.className = 'mt-auto d-flex gap-2';
            
            const viewBtn = document.createElement('a');
            viewBtn.className = 'btn btn-sm btn-primary flex-grow-1';
            viewBtn.href = `/Recipe/Details/${r.id}`;
            viewBtn.textContent = 'View Details';
            buttonContainer.appendChild(viewBtn);
            
            // Delete button if user owns the recipe
            if (r.canDelete) {
                const deleteForm = document.createElement('form');
                deleteForm.action = '/Recipe/Delete';
                deleteForm.method = 'post';
                deleteForm.className = 'd-inline';
                
                // Anti-forgery token - need to get from page
                const tokenInput = document.createElement('input');
                tokenInput.type = 'hidden';
                tokenInput.name = '__RequestVerificationToken';
                const existingToken = document.querySelector('input[name="__RequestVerificationToken"]');
                if (existingToken) {
                    tokenInput.value = existingToken.value;
                }
                deleteForm.appendChild(tokenInput);
                
                const idInput = document.createElement('input');
                idInput.type = 'hidden';
                idInput.name = 'id';
                idInput.value = r.id;
                deleteForm.appendChild(idInput);
                
                const deleteBtn = document.createElement('button');
                deleteBtn.type = 'submit';
                deleteBtn.className = 'btn btn-sm btn-danger';
                deleteBtn.title = 'Xóa công thức';
                deleteBtn.textContent = '🗑️';
                deleteBtn.onclick = function() {
                    return confirm('Bạn có chắc muốn xóa công thức này không?');
                };
                deleteForm.appendChild(deleteBtn);
                buttonContainer.appendChild(deleteForm);
            }
            
            body.appendChild(buttonContainer);
            card.appendChild(body);
            
            const col = document.createElement('div');
            col.className = 'col';
            col.appendChild(card);
            frag.appendChild(col);
        });
        grid.appendChild(frag);
    }

    async function fetchPage(){
        if (loading || !hasMore) return;
        loading = true; clearError(); showLoader(true); renderSkeleton(3);
        try{
            const res = await fetch(`/api/recipes?page=${page}&pageSize=${PAGE_SIZE}`);
            if (!res.ok) throw new Error('Network error');
            const json = await res.json();
            hasMore = !!json.hasMore;
            cache = cache.concat(json.data || []);
            clearSkeleton();
            renderItems(json.data || []);
            page = json.nextPage || (page+1);
            if ((!cache || cache.length===0) && !hasMore) {
                if (emptyBox) emptyBox.style.display = '';
            }
        } catch(e){
            showError('Không thể tải công thức. Vui lòng thử lại.');
        } finally{
            showLoader(false); loading = false;
        }
    }

    function onScroll(){
        if (!hasMore || loading) return;
        const pos = window.scrollY + window.innerHeight;
        const doc = document.documentElement.scrollHeight;
        if (pos / doc >= TRIGGER_RATIO){ fetchPage(); }
    }

    document.addEventListener('DOMContentLoaded', function(){
        // Check if infinite scroll is disabled for this page - exit early if disabled
        if (grid && grid.dataset.disableInfiniteScroll === 'true') {
            return; // Exit completely, don't set up any listeners
        }
        
        // Double check - if grid doesn't exist or is disabled, exit
        if (!grid || grid.dataset.disableInfiniteScroll === 'true') {
            return;
        }
        
        // Check if there are already recipes rendered from server
        const existingCards = grid.querySelectorAll('.col .card').length || 0;
        
        // Only load more if:
        // 1. No recipes were rendered server-side, OR
        // 2. Server rendered fewer than PAGE_SIZE recipes (meaning there might be more)
        if (existingCards === 0 || existingCards < PAGE_SIZE) {
            // Initial load if server didn't render enough
            fetchPage();
        }
        
        // Always listen for scroll in case we need more
        window.addEventListener('scroll', debounce(onScroll, DEBOUNCE_MS), { passive: true });
    });
})();


