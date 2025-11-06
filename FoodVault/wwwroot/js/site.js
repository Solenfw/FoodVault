(function () {
	// Initialize any toasts already present in DOM (rendered by _Toast partial)
	const container = document.getElementById('toastRegion');
	if (container) {
		const renderedToasts = document.querySelectorAll('.toast');
		renderedToasts.forEach(t => {
			container.appendChild(t);
			const toast = new bootstrap.Toast(t);
			toast.show();
		});
	}

	// Expose a simple API to show a toast dynamically
	window.showToast = function (message, variant = 'primary', autohide = true, delay = 3500) {
		if (!container) return;
		const toastEl = document.createElement('div');
		toastEl.className = `toast align-items-center text-bg-${variant} border-0`;
		toastEl.setAttribute('role', 'alert');
		toastEl.setAttribute('aria-live', 'polite');
		toastEl.setAttribute('aria-atomic', 'true');
		toastEl.dataset.bsAutohide = autohide ? 'true' : 'false';
		toastEl.dataset.bsDelay = String(delay);
		toastEl.innerHTML = `
			<div class="d-flex">
				<div class="toast-body">${message}</div>
				<button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>
			</div>`;
		container.appendChild(toastEl);
		new bootstrap.Toast(toastEl).show();
	};

	// Swipe interactions for recipe list
	(function initSwipe() {
		const THRESHOLD = 100;
		let startX = 0, startY = 0, currentX = 0, currentY = 0, dragging = false, lockedToSwipe = false, active = null, foreground = null, favForm = null, delForm = null;
		let pendingTimer = null, pendingType = null; // 'fav' | 'del'

		function onStart(e) {
			const touch = e.touches ? e.touches[0] : e;
			active = e.currentTarget;
			foreground = active.querySelector('.swipe-foreground');
			favForm = active.querySelector('.swipe-fav-form');
			delForm = active.querySelector('.swipe-delete-form');
			startX = touch.clientX;
			startY = touch.clientY;
			currentX = startX;
			currentY = startY;
			dragging = true;
			lockedToSwipe = false;
			foreground.style.transition = 'none';
		}

		function onMove(e) {
			if (!dragging || !foreground) return;
			const touch = e.touches ? e.touches[0] : e;
			currentX = touch.clientX;
			currentY = touch.clientY;
			const dx = currentX - startX;
			const dy = currentY - startY;
			// Avoid conflict with vertical scroll: only lock to swipe if horizontal dominates
			if (!lockedToSwipe) {
				if (Math.abs(dy) > Math.abs(dx)) { return; }
				lockedToSwipe = true;
			}
			// prevent scrolling while swiping horizontally
			e.preventDefault();
			foreground.style.transform = `translateX(${dx}px)`;
		}

		function onEnd() {
			if (!dragging || !foreground) return;
			dragging = false;
			const dx = currentX - startX;
			foreground.style.transition = 'transform .28s cubic-bezier(.2,.8,.2,1.2)'; // spring-like
			if (dx > THRESHOLD) {
				triggerAction('fav');
			} else if (dx < -THRESHOLD) {
				triggerAction('del');
			} else {
				foreground.style.transform = 'translateX(0)';
			}
		}

		function vibrate(pattern) {
			if (navigator.vibrate) { try { navigator.vibrate(pattern); } catch(_){} }
		}

		function showUndoToast(message, onUndo, onExpire) {
			const container = document.getElementById('toastRegion');
			if (!container) { onExpire(); return; }
			const toastEl = document.createElement('div');
			toastEl.className = 'toast align-items-center text-bg-dark border-0';
			toastEl.setAttribute('role', 'alert');
			toastEl.setAttribute('aria-live', 'polite');
			toastEl.setAttribute('aria-atomic', 'true');
			toastEl.dataset.bsAutohide = 'true';
			toastEl.dataset.bsDelay = '3000';
			toastEl.innerHTML = `
				<div class="d-flex">
					<div class="toast-body">${message}</div>
					<button type="button" class="btn btn-sm btn-light me-2 my-auto" data-undo>Undo</button>
					<button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>
				</div>`;
			container.appendChild(toastEl);
			const toast = new bootstrap.Toast(toastEl);
			let undone = false;
			toastEl.querySelector('[data-undo]')?.addEventListener('click', function(){
				undone = true;
				toast.hide();
				onUndo();
			});
			toastEl.addEventListener('hidden.bs.toast', function(){
				if (!undone) onExpire();
				toastEl.remove();
			});
			toast.show();
		}

		function triggerAction(type) {
			if (!foreground) return;
			pendingType = type; // 'fav' or 'del'
			if (type === 'fav') {
				foreground.style.transform = 'translateX(120%)';
				vibrate(20);
				if (pendingTimer) clearTimeout(pendingTimer);
				showUndoToast('Đã thêm vào yêu thích', undo, commit);
			} else {
				foreground.style.transform = 'translateX(-120%)';
				vibrate([10, 40, 10]);
				if (pendingTimer) clearTimeout(pendingTimer);
				showUndoToast('Sẽ xóa công thức', undo, commit);
			}
			// fallback commit after 3s in case toast not available
			pendingTimer = setTimeout(commit, 3000);
		}

		function undo() {
			if (pendingTimer) { clearTimeout(pendingTimer); pendingTimer = null; }
			if (foreground) {
				foreground.style.transition = 'transform .28s cubic-bezier(.2,.8,.2,1.2)';
				foreground.style.transform = 'translateX(0)';
			}
			pendingType = null;
		}

		function commit() {
			if (pendingTimer) { clearTimeout(pendingTimer); pendingTimer = null; }
			if (!pendingType) { return; }
			if (pendingType === 'fav' && favForm) { favForm.submit(); }
			if (pendingType === 'del' && delForm) { delForm.submit(); }
			pendingType = null;
		}

		function attach(el) {
			el.addEventListener('touchstart', onStart, { passive: true });
			el.addEventListener('touchmove', onMove, { passive: false });
			el.addEventListener('touchend', onEnd, { passive: true });
			el.addEventListener('mousedown', onStart);
			document.addEventListener('mousemove', onMove, { passive: false });
			document.addEventListener('mouseup', onEnd);
		}

		document.addEventListener('DOMContentLoaded', function () {
			document.querySelectorAll('.swipe-item').forEach(attach);
		});
	})();
})();



