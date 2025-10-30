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
})();



