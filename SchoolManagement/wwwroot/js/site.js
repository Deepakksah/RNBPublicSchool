// Global JavaScript helper for School Management System
document.addEventListener('DOMContentLoaded', function () {
    // 1. Persistent Sidebar Collapse / Unhide
    const toggleBtn = document.getElementById('sidebarToggle');
    const sidebar = document.getElementById('appSidebar');
    const backdrop = document.getElementById('sidebarBackdrop');

    if (window.innerWidth >= 992) {
        const isCollapsed = localStorage.getItem('school_sidebar_collapsed') === 'true';
        if (isCollapsed) {
            document.body.classList.add('sidebar-collapsed');
        }
    }

    if (toggleBtn) {
        toggleBtn.addEventListener('click', function (e) {
            e.preventDefault();
            if (window.innerWidth >= 992) {
                document.body.classList.toggle('sidebar-collapsed');
                const isNowCollapsed = document.body.classList.contains('sidebar-collapsed');
                localStorage.setItem('school_sidebar_collapsed', isNowCollapsed);
            } else {
                if (sidebar && backdrop) {
                    sidebar.classList.toggle('show');
                    backdrop.classList.toggle('show');
                }
            }
        });
    }

    if (backdrop && sidebar) {
        backdrop.addEventListener('click', function () {
            sidebar.classList.remove('show');
            backdrop.classList.remove('show');
        });
    }

    // 2. Initialize Toasts
    var toastElList = [].slice.call(document.querySelectorAll('.toast'));
    toastElList.forEach(function (toastEl) {
        var toast = new bootstrap.Toast(toastEl, { delay: 4500 });
        toast.show();
    });

    // Auto-hide alert banners after 4s
    setTimeout(function () {
        var alerts = document.querySelectorAll('.alert-dismissible');
        alerts.forEach(function (alert) {
            var bsAlert = new bootstrap.Alert(alert);
            bsAlert.close();
        });
    }, 4500);

    // 3. Initialize Global Live Auto-Search on All Search Inputs Across Every Page
    initGlobalAutoSearch();

    // 4. Initialize Global Seamless AJAX Form Interceptors (No Page Reload on Add/Edit/Delete/Assign)
    initGlobalAjaxForms();
});

// Toast notification helper
function showToast(message, type = 'success') {
    let toastContainer = document.getElementById('toastNotificationContainer');
    if (!toastContainer) {
        toastContainer = document.createElement('div');
        toastContainer.id = 'toastNotificationContainer';
        toastContainer.className = 'toast-container position-fixed top-0 end-0 p-3';
        toastContainer.style.zIndex = '1150';
        document.body.appendChild(toastContainer);
    }

    const bgClass = type === 'success' ? 'bg-success' : type === 'error' ? 'bg-danger' : 'bg-warning';
    const iconClass = type === 'success' ? 'bi-check-circle-fill' : type === 'error' ? 'bi-exclamation-octagon-fill' : 'bi-exclamation-triangle-fill';

    const toastHtml = `
        <div class="toast align-items-center text-white ${bgClass} border-0 show shadow mb-2" role="alert" aria-live="assertive" aria-atomic="true">
            <div class="d-flex">
                <div class="toast-body d-flex align-items-center gap-2 py-2 px-3 small">
                    <i class="bi ${iconClass} fs-6"></i>
                    <div>${message}</div>
                </div>
                <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>
            </div>
        </div>`;

    toastContainer.insertAdjacentHTML('beforeend', toastHtml);
    const newToast = toastContainer.lastElementChild;
    const bsToast = new bootstrap.Toast(newToast, { delay: 4000 });
    bsToast.show();
    setTimeout(() => {
        if (newToast && newToast.parentNode) newToast.remove();
    }, 4500);
}

// Helper to extract clean searchable text from a table row (ignoring unselected <option> elements in dropdowns)
function getRowSearchableText(row) {
    let text = '';
    row.querySelectorAll('td').forEach(td => {
        const select = td.querySelector('select');
        if (select) {
            const selectedOpt = select.options[select.selectedIndex];
            text += ' ' + (selectedOpt ? selectedOpt.text : '');
        } else {
            text += ' ' + td.innerText;
        }
    });
    return text.toLowerCase();
}

// ----------------------------------------------------
// Global Live Auto-Search & Instant Filtering
// ----------------------------------------------------
function initGlobalAutoSearch() {
    // Find all search inputs across all pages
    const searchInputs = document.querySelectorAll('input[type="search"], input[name*="search" i], input[id*="search" i], input[placeholder*="search" i]');

    searchInputs.forEach(input => {
        let debounceTimer;

        input.addEventListener('input', function (e) {
            const query = e.target.value.trim().toLowerCase();

            // 1. Instant (0ms) in-memory table row filtering
            const card = input.closest('.card') || document.querySelector('.card');
            const table = card ? card.querySelector('table') : document.querySelector('table');

            if (table && table.querySelectorAll('tbody tr').length > 0) {
                const rows = table.querySelectorAll('tbody tr:not(.no-filter):not(.auto-search-empty-row)');
                let matchCount = 0;

                rows.forEach(row => {
                    const rowSearchText = getRowSearchableText(row);
                    if (query === '' || rowSearchText.indexOf(query) !== -1) {
                        row.style.display = '';
                        matchCount++;
                    } else {
                        row.style.display = 'none';
                    }
                });

                // Manage "No matching records" placeholder
                let emptyRow = table.querySelector('.auto-search-empty-row');
                if (matchCount === 0 && query !== '') {
                    if (!emptyRow) {
                        const colCount = table.querySelectorAll('thead th').length || 6;
                        const tr = document.createElement('tr');
                        tr.className = 'auto-search-empty-row';
                        tr.innerHTML = `<td colspan="${colCount}" class="text-center py-4 text-muted small"><i class="bi bi-search me-1"></i> No records matching "<strong>${query}</strong>"</td>`;
                        table.querySelector('tbody').appendChild(tr);
                    } else {
                        emptyRow.style.display = '';
                        emptyRow.querySelector('strong').innerText = query;
                    }
                } else if (emptyRow) {
                    emptyRow.style.display = 'none';
                }
            }

            // 2. Debounced Background AJAX Sync if server form exists
            clearTimeout(debounceTimer);
            const form = input.closest('form');
            if (form && form.method.toLowerCase() === 'get') {
                debounceTimer = setTimeout(() => {
                    const url = new URL(form.action || window.location.href, window.location.origin);
                    const formData = new FormData(form);
                    for (const [k, v] of formData.entries()) {
                        url.searchParams.set(k, v);
                    }

                    // Update browser URL silently
                    window.history.replaceState({}, '', url.toString());

                    // If table exists, update seamlessly
                    const tableContainer = card ? card.querySelector('.table-responsive') : document.querySelector('.table-responsive');
                    if (tableContainer) {
                        fetch(url.toString(), { credentials: 'same-origin' })
                            .then(res => res.text())
                            .then(html => {
                                const parser = new DOMParser();
                                const doc = parser.parseFromString(html, 'text/html');
                                const newTable = doc.querySelector('.table-responsive') || doc.querySelector('table');
                                if (newTable && tableContainer) {
                                    tableContainer.innerHTML = newTable.innerHTML;
                                }
                            })
                            .catch(() => {});
                    }
                }, 350);
            }
        });

        // Prevent unwanted full page reload on pressing Enter
        const form = input.closest('form');
        if (form && form.method.toLowerCase() === 'get') {
            form.addEventListener('submit', function (e) {
                e.preventDefault();
                input.dispatchEvent(new Event('input'));
            });
        }
    });

    // Also auto-filter on dropdown changes (Class, Section, Status)
    const filterSelects = document.querySelectorAll('select[id*="filter" i], select[name*="class" i], select[name*="section" i], select[name*="status" i]');
    filterSelects.forEach(sel => {
        sel.addEventListener('change', function () {
            const form = sel.closest('form');
            if (form && form.method.toLowerCase() === 'get') {
                form.submit();
            }
        });
    });
}

// ----------------------------------------------------
// Global AJAX Form Submissions (No Full Page Reload)
// ----------------------------------------------------
function initGlobalAjaxForms() {
    // Intercept forms for quick actions like AssignClassTeacher, Quick Delete, and Status Toggle
    document.addEventListener('submit', function (e) {
        const form = e.target;
        if (!form || form.method.toLowerCase() !== 'post') return;

        // Check if form is an in-place action
        const isInlineAction = form.action.includes('AssignClassTeacher') || 
                               form.action.includes('CreateTeacherAccount') ||
                               form.action.includes('GenerateAllTeacherAccounts') ||
                               form.classList.contains('ajax-form');

        if (isInlineAction) {
            e.preventDefault();
            const submitBtn = form.querySelector('button[type="submit"]');
            const origHtml = submitBtn ? submitBtn.innerHTML : '';
            if (submitBtn) {
                submitBtn.disabled = true;
                submitBtn.innerHTML = '<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span>';
            }

            const formData = new FormData(form);
            fetch(form.action, {
                method: 'POST',
                body: formData,
                credentials: 'same-origin'
            })
            .then(res => {
                if (res.redirected) {
                    return fetch(res.url, { credentials: 'same-origin' }).then(r => r.text());
                }
                return res.text();
            })
            .then(html => {
                // Parse and update DOM smoothly
                const parser = new DOMParser();
                const doc = parser.parseFromString(html, 'text/html');
                
                // Close any open modals
                const modal = form.closest('.modal');
                if (modal) {
                    const bsModal = bootstrap.Modal.getInstance(modal);
                    if (bsModal) bsModal.hide();
                }

                // Smoothly replace the main content without full page reload!
                const newContent = doc.querySelector('.page-content') || doc.querySelector('.app-main') || doc.body;
                const currentContent = document.querySelector('.page-content') || document.querySelector('.app-main');
                if (newContent && currentContent) {
                    currentContent.innerHTML = newContent.innerHTML;
                    // Re-bind listeners for new DOM elements
                    initGlobalAutoSearch();
                }

                showToast('Action completed successfully!', 'success');
            })
            .catch(err => {
                showToast('Failed to complete action: ' + err.message, 'error');
            })
            .finally(() => {
                if (submitBtn) {
                    submitBtn.disabled = false;
                    submitBtn.innerHTML = origHtml;
                }
            });
        }
    });
}
