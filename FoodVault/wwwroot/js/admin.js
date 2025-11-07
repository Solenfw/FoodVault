/**
 * Admin Dashboard JavaScript
 * Quản lý các chức năng của trang Admin Dashboard
 */

(function() {
    'use strict';

    // ========================================
    // BIẾN TOÀN CỤC
    // ========================================
    let userGrowthChart = null;
    let systemHealthInterval = null;
    let clockInterval = null;

    // ========================================
    // 1. SIDEBAR TOGGLE
    // ========================================
    
    /**
     * Khởi tạo chức năng toggle sidebar
     */
    function initSidebarToggle() {
        const sidebarToggle = document.getElementById('sidebarToggle');
        const adminSidebar = document.querySelector('.admin-sidebar');
        
        if (!sidebarToggle || !adminSidebar) {
            return;
        }

        // Khôi phục trạng thái từ localStorage
        const isCollapsed = localStorage.getItem('sidebarCollapsed') === 'true';
        if (isCollapsed) {
            adminSidebar.classList.add('collapsed');
            document.body.classList.add('sidebar-collapsed');
        }

        // Xử lý click toggle button
        sidebarToggle.addEventListener('click', function(e) {
            e.preventDefault();
            e.stopPropagation();
            
            adminSidebar.classList.toggle('collapsed');
            document.body.classList.toggle('sidebar-collapsed');
            
            // Lưu trạng thái vào localStorage
            const isNowCollapsed = adminSidebar.classList.contains('collapsed');
            localStorage.setItem('sidebarCollapsed', isNowCollapsed.toString());
            
            // Dispatch event để các component khác có thể lắng nghe
            window.dispatchEvent(new CustomEvent('sidebarToggle', { 
                detail: { collapsed: isNowCollapsed } 
            }));
        });

        // Xử lý click outside để đóng sidebar trên mobile
        if (window.innerWidth <= 768) {
            document.addEventListener('click', function(e) {
                if (!adminSidebar.contains(e.target) && 
                    !sidebarToggle.contains(e.target) && 
                    !adminSidebar.classList.contains('collapsed')) {
                    adminSidebar.classList.add('collapsed');
                    document.body.classList.add('sidebar-collapsed');
                    localStorage.setItem('sidebarCollapsed', 'true');
                }
            });
        }

        // Xử lý resize window
        window.addEventListener('resize', function() {
            if (window.innerWidth > 768) {
                // Desktop: xóa collapsed class
                adminSidebar.classList.remove('collapsed');
                document.body.classList.remove('sidebar-collapsed');
            }
        });
    }

    // ========================================
    // 2. SUBMENU TOGGLE
    // ========================================
    
    /**
     * Khởi tạo chức năng toggle submenu
     */
    function initSubmenuToggle() {
        const submenuToggles = document.querySelectorAll('.nav-item.has-submenu > .nav-link');
        
        submenuToggles.forEach(toggle => {
            toggle.addEventListener('click', function(e) {
                e.preventDefault();
                
                const navItem = this.closest('.nav-item.has-submenu');
                const submenu = navItem.querySelector('.submenu');
                
                if (!navItem || !submenu) {
                    return;
                }

                // Kiểm tra xem submenu đang mở hay đóng
                const isOpen = navItem.classList.contains('open');
                
                // Đóng tất cả submenu khác (optional - có thể bỏ nếu muốn nhiều submenu mở cùng lúc)
                // document.querySelectorAll('.nav-item.has-submenu.open').forEach(item => {
                //     if (item !== navItem) {
                //         item.classList.remove('open');
                //         const otherSubmenu = item.querySelector('.submenu');
                //         if (otherSubmenu) {
                //             otherSubmenu.style.maxHeight = '0';
                //         }
                //     }
                // });

                // Toggle class và animation
                if (isOpen) {
                    // Đóng submenu
                    navItem.classList.remove('open');
                    submenu.style.maxHeight = '0';
                } else {
                    // Mở submenu
                    navItem.classList.add('open');
                    // Tính toán chiều cao thực tế của submenu
                    submenu.style.maxHeight = submenu.scrollHeight + 'px';
                }
            });
        });

        // Auto-expand submenu nếu có link active trong đó
        const activeSubmenuLink = document.querySelector('.submenu-link.active');
        if (activeSubmenuLink) {
            const navItem = activeSubmenuLink.closest('.nav-item.has-submenu');
            if (navItem) {
                navItem.classList.add('open');
                const submenu = navItem.querySelector('.submenu');
                if (submenu) {
                    submenu.style.maxHeight = submenu.scrollHeight + 'px';
                }
            }
        }
    }

    // ========================================
    // 3. USER GROWTH CHART
    // ========================================
    
    /**
     * Khởi tạo biểu đồ tăng trưởng người dùng
     */
    function initUserGrowthChart() {
        const canvas = document.getElementById('userGrowthChart');
        
        if (!canvas) {
            console.warn('Không tìm thấy canvas cho User Growth Chart');
            return;
        }

        // Kiểm tra Chart.js đã được load chưa
        if (typeof Chart === 'undefined') {
            console.error('Chart.js chưa được load. Vui lòng kiểm tra lại.');
            return;
        }

        // Fetch dữ liệu từ API
        fetch('/admin/dashboard/GetUserGrowth?days=30')
            .then(response => {
                if (!response.ok) {
                    throw new Error(`HTTP error! status: ${response.status}`);
                }
                return response.json();
            })
            .then(data => {
                createUserGrowthChart(canvas, data);
            })
            .catch(error => {
                console.error('Lỗi khi tải dữ liệu User Growth:', error);
                // Hiển thị thông báo lỗi trên canvas
                const ctx = canvas.getContext('2d');
                ctx.fillStyle = '#94a3b8';
                ctx.font = '14px Arial';
                ctx.textAlign = 'center';
                ctx.fillText('Không thể tải dữ liệu', canvas.width / 2, canvas.height / 2);
            });
    }

    /**
     * Tạo biểu đồ Chart.js với dữ liệu
     */
    function createUserGrowthChart(canvas, data) {
        const ctx = canvas.getContext('2d');
        
        // Gradient cho fill
        const gradient = ctx.createLinearGradient(0, 0, 0, canvas.height);
        gradient.addColorStop(0, 'rgba(79, 70, 229, 0.3)');
        gradient.addColorStop(1, 'rgba(79, 70, 229, 0.05)');

        userGrowthChart = new Chart(ctx, {
            type: 'line',
            data: {
                labels: data.labels || [],
                datasets: [{
                    label: 'Số người dùng mới',
                    data: data.data || [],
                    borderColor: 'rgb(79, 70, 229)',
                    backgroundColor: gradient,
                    tension: 0.4, // Smooth curves
                    fill: true,
                    pointRadius: 4,
                    pointHoverRadius: 6,
                    pointBackgroundColor: 'rgb(79, 70, 229)',
                    pointBorderColor: '#fff',
                    pointBorderWidth: 2,
                    pointHoverBackgroundColor: 'rgb(99, 102, 241)',
                    pointHoverBorderColor: '#fff',
                    pointHoverBorderWidth: 3
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                interaction: {
                    intersect: false,
                    mode: 'index'
                },
                plugins: {
                    legend: {
                        display: false
                    },
                    tooltip: {
                        backgroundColor: 'rgba(15, 23, 42, 0.95)',
                        titleColor: '#e2e8f0',
                        bodyColor: '#cbd5e1',
                        borderColor: '#334155',
                        borderWidth: 1,
                        padding: 12,
                        displayColors: false,
                        callbacks: {
                            title: function(context) {
                                return 'Ngày: ' + context[0].label;
                            },
                            label: function(context) {
                                return 'Người dùng mới: ' + context.parsed.y;
                            }
                        }
                    }
                },
                scales: {
                    y: {
                        beginAtZero: true,
                        ticks: {
                            color: '#94a3b8',
                            stepSize: 1,
                            precision: 0
                        },
                        grid: {
                            color: 'rgba(51, 65, 85, 0.3)',
                            lineWidth: 1
                        },
                        border: {
                            color: 'rgba(51, 65, 85, 0.5)'
                        }
                    },
                    x: {
                        ticks: {
                            color: '#94a3b8',
                            maxRotation: 45,
                            minRotation: 0
                        },
                        grid: {
                            color: 'rgba(51, 65, 85, 0.3)',
                            lineWidth: 1
                        },
                        border: {
                            color: 'rgba(51, 65, 85, 0.5)'
                        }
                    }
                },
                animation: {
                    duration: 1000,
                    easing: 'easeInOutQuart'
                }
            }
        });
    }

    // ========================================
    // 4. SYSTEM HEALTH AUTO-REFRESH
    // ========================================
    
    /**
     * Khởi tạo auto-refresh cho System Health
     */
    function initSystemHealthRefresh() {
        // Load ngay lập tức
        loadSystemHealth();

        // Refresh mỗi 30 giây
        systemHealthInterval = setInterval(function() {
            loadSystemHealth();
        }, 30000);
    }

    /**
     * Tải và cập nhật dữ liệu System Health
     */
    function loadSystemHealth() {
        fetch('/admin/dashboard/GetSystemHealth')
            .then(response => {
                if (!response.ok) {
                    throw new Error(`HTTP error! status: ${response.status}`);
                }
                return response.json();
            })
            .then(data => {
                updateSystemHealthBars(data);
            })
            .catch(error => {
                console.error('Lỗi khi tải System Health:', error);
                // Hiển thị thông báo lỗi (optional)
                showSystemHealthError();
            });
    }

    /**
     * Cập nhật các progress bars với dữ liệu mới
     */
    function updateSystemHealthBars(data) {
        const cpuValue = Math.round(data.cpuUsage || 0);
        const memoryValue = Math.round(data.memoryUsage || 0);
        const diskValue = Math.round(data.diskUsage || 0);

        // Cập nhật CPU
        updateProgressBar('cpu', cpuValue);
        
        // Cập nhật Memory
        updateProgressBar('memory', memoryValue);
        
        // Cập nhật Disk
        updateProgressBar('disk', diskValue);

        // Cập nhật timestamp nếu có
        if (data.timestamp) {
            const timestampEl = document.getElementById('system-health-timestamp');
            if (timestampEl) {
                timestampEl.textContent = 'Cập nhật: ' + new Date(data.timestamp).toLocaleTimeString('vi-VN');
            }
        }
    }

    /**
     * Cập nhật một progress bar cụ thể
     */
    function updateProgressBar(type, value) {
        const valueEl = document.getElementById(type + '-value');
        const progressEl = document.getElementById(type + '-progress');
        
        if (!valueEl || !progressEl) {
            return;
        }

        // Cập nhật giá trị
        valueEl.textContent = value;

        // Cập nhật width và text
        progressEl.style.width = value + '%';
        progressEl.textContent = value + '%';
        
        // Cập nhật màu sắc dựa trên threshold
        progressEl.classList.remove('bg-success', 'bg-warning', 'bg-danger');
        if (value < 50) {
            progressEl.classList.add('bg-success');
        } else if (value >= 50 && value <= 80) {
            progressEl.classList.add('bg-warning');
        } else {
            progressEl.classList.add('bg-danger');
        }
    }

    /**
     * Hiển thị thông báo lỗi cho System Health
     */
    function showSystemHealthError() {
        // Optional: hiển thị toast notification hoặc update UI
        console.warn('Không thể tải System Health data');
    }

    // ========================================
    // 5. REAL-TIME CLOCK
    // ========================================
    
    /**
     * Khởi tạo đồng hồ real-time
     */
    function initRealTimeClock() {
        const clockElement = document.getElementById('admin-clock');
        
        if (!clockElement) {
            // Tạo element nếu chưa có
            const header = document.querySelector('.admin-header');
            if (header) {
                const clock = document.createElement('div');
                clock.id = 'admin-clock';
                clock.className = 'admin-clock';
                clock.style.cssText = 'color: var(--admin-text-secondary); font-size: 0.875rem; font-family: monospace;';
                header.querySelector('.header-actions')?.appendChild(clock) || header.appendChild(clock);
            } else {
                return;
            }
        }

        // Cập nhật ngay lập tức
        updateClock();

        // Cập nhật mỗi giây
        clockInterval = setInterval(function() {
            updateClock();
        }, 1000);
    }

    /**
     * Cập nhật hiển thị đồng hồ
     */
    function updateClock() {
        const clockElement = document.getElementById('admin-clock');
        if (!clockElement) {
            return;
        }

        const now = new Date();
        
        // Format: HH:mm:ss DD/MM/YYYY
        const hours = String(now.getHours()).padStart(2, '0');
        const minutes = String(now.getMinutes()).padStart(2, '0');
        const seconds = String(now.getSeconds()).padStart(2, '0');
        const day = String(now.getDate()).padStart(2, '0');
        const month = String(now.getMonth() + 1).padStart(2, '0');
        const year = now.getFullYear();

        const timeString = `${hours}:${minutes}:${seconds} ${day}/${month}/${year}`;
        clockElement.textContent = timeString;
    }

    // ========================================
    // KHỞI TẠO KHI DOM READY
    // ========================================
    
    /**
     * Khởi tạo tất cả các chức năng khi DOM sẵn sàng
     */
    function init() {
        // Kiểm tra nếu đang ở trang admin
        if (!document.querySelector('.admin-sidebar')) {
            return;
        }

        // Khởi tạo các chức năng
        initSidebarToggle();
        initSubmenuToggle();
        initUserGrowthChart();
        initSystemHealthRefresh();
        initRealTimeClock();
    }

    // Chờ DOM load xong
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        // DOM đã sẵn sàng
        init();
    }

    // ========================================
    // CLEANUP KHI TRANG ĐƯỢC UNLOAD
    // ========================================
    
    window.addEventListener('beforeunload', function() {
        // Clear intervals
        if (systemHealthInterval) {
            clearInterval(systemHealthInterval);
        }
        if (clockInterval) {
            clearInterval(clockInterval);
        }
        
        // Destroy chart nếu có
        if (userGrowthChart) {
            userGrowthChart.destroy();
        }
    });

})();

