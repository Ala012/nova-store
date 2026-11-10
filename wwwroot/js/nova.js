/* ==========================================================================
   NOVA — Interactions
   ========================================================================== */
(function () {
    'use strict';

    var $ = function (sel, scope) { return (scope || document).querySelector(sel); };
    var $$ = function (sel, scope) { return Array.prototype.slice.call((scope || document).querySelectorAll(sel)); };

    /* ---------- Helpers ---------- */
    function money(value) {
        return Number(value || 0).toLocaleString('fr-FR', {
            minimumFractionDigits: 3,
            maximumFractionDigits: 3
        }) + ' DT';
    }

    function getToken() {
        var input = document.querySelector('input[name="__RequestVerificationToken"]');
        return input ? input.value : '';
    }

    function toast(message, type) {
        var stack = $('#toastStack');
        if (!stack) return;
        var el = document.createElement('div');
        el.className = 'nova-toast toast-' + (type || 'success');
        el.setAttribute('role', 'status');
        var icon = type === 'error' ? 'bi-exclamation-triangle' : type === 'info' ? 'bi-info-circle' : 'bi-check2-circle';
        el.innerHTML = '<i class="bi ' + icon + '"></i><span></span><button type="button" class="toast-close" aria-label="Fermer"><i class="bi bi-x"></i></button>';
        el.querySelector('span').textContent = message;
        stack.appendChild(el);
        bindToast(el);
        window.setTimeout(function () { dismissToast(el); }, 4200);
    }

    function dismissToast(el) {
        if (!el || el.classList.contains('is-hiding')) return;
        el.classList.add('is-hiding');
        window.setTimeout(function () { el.remove(); }, 320);
    }

    function bindToast(el) {
        var close = el.querySelector('.toast-close');
        if (close) close.addEventListener('click', function () { dismissToast(el); });
        window.setTimeout(function () { dismissToast(el); }, 5200);
    }

    /* ---------- Theme ---------- */
    function initTheme() {
        var toggle = $('#themeToggle');
        if (!toggle) return;
        toggle.addEventListener('click', function () {
            var next = document.documentElement.getAttribute('data-theme') === 'dark' ? 'light' : 'dark';
            document.documentElement.setAttribute('data-theme', next);
            try { localStorage.setItem('nova-theme', next); } catch (e) { }
        });
    }

    /* ---------- Header / scroll ---------- */
    function initScroll() {
        var header = $('#siteHeader');
        var top = $('#backToTop');
        function onScroll() {
            var y = window.scrollY || window.pageYOffset;
            if (header) header.classList.toggle('is-scrolled', y > 8);
            if (top) top.classList.toggle('is-visible', y > 520);
        }
        window.addEventListener('scroll', onScroll, { passive: true });
        onScroll();
        if (top) top.addEventListener('click', function () { window.scrollTo({ top: 0, behavior: 'smooth' }); });
    }

    /* ---------- Search shortcut ---------- */
    function initSearch() {
        var input = $('#globalSearch');
        if (!input) return;
        document.addEventListener('keydown', function (e) {
            var tag = (e.target.tagName || '').toLowerCase();
            var typing = tag === 'input' || tag === 'textarea' || tag === 'select' || e.target.isContentEditable;
            if (e.key === '/' && !typing) {
                e.preventDefault();
                input.focus();
            }
            if (e.key === 'Escape' && document.activeElement === input) input.blur();
        });
    }

    /* ---------- Category drawer ---------- */
    function initDrawer() {
        var drawerEl = $('#categoriesDrawer');
        if (!drawerEl || !window.bootstrap) return;
        var drawer = bootstrap.Offcanvas.getOrCreateInstance(drawerEl);
        ['#toggleMenu', '#categoriesNavBtn'].forEach(function (sel) {
            var btn = $(sel);
            if (btn) btn.addEventListener('click', function () { drawer.show(); });
        });
    }

    /* ---------- Reveal on scroll ---------- */
    function initReveal() {
        var items = $$('.reveal');
        if (!items.length) return;
        if (!('IntersectionObserver' in window)) {
            items.forEach(function (el) { el.classList.add('is-visible'); });
            return;
        }
        var observer = new IntersectionObserver(function (entries) {
            entries.forEach(function (entry) {
                if (!entry.isIntersecting) return;
                var el = entry.target;
                var delay = Number(el.dataset.revealDelay || 0);
                window.setTimeout(function () { el.classList.add('is-visible'); }, delay);
                observer.unobserve(el);
            });
        }, { threshold: 0.08, rootMargin: '0px 0px -40px 0px' });
        items.forEach(function (el, i) {
            if (!el.dataset.revealDelay && i < 8) el.dataset.revealDelay = String((i % 4) * 70);
            observer.observe(el);
        });
    }

    /* ---------- Toasts ---------- */
    function initToasts() {
        $$('.nova-toast').forEach(bindToast);
    }

    /* ---------- Confirm dialogs ---------- */
    function initConfirm() {
        document.addEventListener('submit', function (e) {
            var form = e.target;
            var message = form.getAttribute('data-confirm');
            if (!message) {
                var btn = form.querySelector('[data-confirm], button[type="submit"][data-confirm]');
                if (btn) message = btn.getAttribute('data-confirm');
            }
            if (message && !window.confirm(message)) e.preventDefault();
        });
        document.addEventListener('click', function (e) {
            var btn = e.target.closest('button[data-confirm], a[data-confirm]');
            if (!btn) return;
            var form = btn.closest('form');
            if (form) return; // handled on submit
            if (!window.confirm(btn.getAttribute('data-confirm'))) e.preventDefault();
        });
    }

    /* ---------- Password visibility ---------- */
    function initPasswords() {
        $$('[data-password-toggle]').forEach(function (btn) {
            btn.addEventListener('click', function () {
                var wrap = btn.closest('.input-icon');
                var input = wrap ? wrap.querySelector('input') : null;
                if (!input) return;
                var show = input.type === 'password';
                input.type = show ? 'text' : 'password';
                var icon = btn.querySelector('i');
                if (icon) icon.className = show ? 'bi bi-eye-slash' : 'bi bi-eye';
            });
        });
    }

    /* ---------- Demo accounts ---------- */
    function initDemoAccounts() {
        $$('[data-demo-email]').forEach(function (btn) {
            btn.addEventListener('click', function () {
                var form = btn.closest('form');
                if (!form) return;
                var email = form.querySelector('input[type="email"], input[name="Email"]');
                var pass = form.querySelector('input[name="Password"]');
                if (email) email.value = btn.dataset.demoEmail || '';
                if (pass) pass.value = btn.dataset.demoPassword || '';
                if (email) email.dispatchEvent(new Event('input', { bubbles: true }));
                toast('Compte démo pré-rempli. Cliquez sur « Se connecter ».', 'info');
            });
        });
    }

    /* ---------- Newsletter ---------- */
    function initNewsletter() {
        var form = $('#newsletterForm');
        if (!form) return;
        form.addEventListener('submit', function (e) {
            e.preventDefault();
            var input = form.querySelector('input[type="email"]');
            if (!input || !input.value) return;
            toast('Merci ! Vous êtes inscrit(e) à la newsletter NOVA.', 'success');
            form.reset();
        });
    }

    /* ---------- Quantity stepper (product page) ---------- */
    function initSteppers() {
        $$('[data-qty-stepper]').forEach(function (stepper) {
            var input = stepper.querySelector('input[name="quantity"]');
            if (!input) return;
            var max = parseInt(input.getAttribute('max') || '99', 10);
            var min = parseInt(input.getAttribute('min') || '1', 10);
            var minus = stepper.querySelector('[data-qty-minus]');
            var plus = stepper.querySelector('[data-qty-plus]');
            if (minus) minus.addEventListener('click', function () {
                input.value = String(Math.max(min, (parseInt(input.value, 10) || 1) - 1));
            });
            if (plus) plus.addEventListener('click', function () {
                input.value = String(Math.min(max, (parseInt(input.value, 10) || 1) + 1));
            });
            input.addEventListener('change', function () {
                var v = parseInt(input.value, 10) || min;
                input.value = String(Math.min(max, Math.max(min, v)));
            });
        });
    }

    /* ---------- Star picker ---------- */
    function initStarPickers() {
        $$('[data-star-picker]').forEach(function (picker) {
            var hidden = picker.querySelector('input[type="hidden"]');
            var stars = $$('[data-star]', picker);
            function paint(value) {
                stars.forEach(function (star) {
                    star.classList.toggle('is-active', Number(star.dataset.star) <= value);
                });
            }
            stars.forEach(function (star) {
                star.addEventListener('click', function () {
                    var value = Number(star.dataset.star);
                    if (hidden) hidden.value = String(value);
                    paint(value);
                });
                star.addEventListener('mouseenter', function () { paint(Number(star.dataset.star)); });
            });
            picker.addEventListener('mouseleave', function () {
                paint(Number(hidden ? hidden.value : 5));
            });
            paint(Number(hidden ? hidden.value : 5));
        });
    }

    /* ---------- Image preview (admin product form) ---------- */
    function initImagePreview() {
        var input = $('[data-image-input]');
        var preview = $('#imagePreview');
        if (!input || !preview) return;
        input.addEventListener('change', function () {
            var file = input.files && input.files[0];
            if (!file) return;
            if (!/^image\//.test(file.type)) {
                toast('Merci de choisir un fichier image.', 'error');
                input.value = '';
                return;
            }
            preview.src = URL.createObjectURL(file);
        });
    }

    /* ---------- Shop sidebar (mobile) ---------- */
    function initSidebar() {
        var toggle = $('#toggleSidebar');
        var sidebar = $('#sidebar');
        if (!toggle || !sidebar) return;
        toggle.addEventListener('click', function () { sidebar.classList.toggle('is-open'); });
        document.addEventListener('click', function (e) {
            if (!sidebar.classList.contains('is-open')) return;
            if (sidebar.contains(e.target) || toggle.contains(e.target)) return;
            sidebar.classList.remove('is-open');
        });
    }

    /* ---------- Cart badge ---------- */
    function setCartBadge(count) {
        var holder = $('#cartBadge');
        if (!holder) return;
        if (count > 0) {
            holder.innerHTML = '<span class="nav-badge">' + count + '</span>';
        } else {
            holder.innerHTML = '';
        }
    }

    function setWishlistBadge(count) {
        var holder = $('#wishlistBadge');
        if (!holder) return;
        if (count > 0) {
            holder.innerHTML = '<span class="nav-badge nav-badge-pink">' + count + '</span>';
        } else {
            holder.innerHTML = '';
        }
    }

    /* ---------- AJAX cart ---------- */
    function initCart() {
        var summary = $('[data-cart-summary]');
        if (!summary) return;
        var freeAt = Number(summary.dataset.freeShipping || 299);
        var fee = Number(summary.dataset.shipping || 7);

        function refreshSummary(subtotal, count) {
            var shipping = subtotal <= 0 ? 0 : (subtotal >= freeAt ? 0 : fee);
            var total = subtotal + shipping;
            var subEl = $('[data-cart-subtotal]');
            var shipEl = $('[data-cart-shipping]');
            var totalEl = $('[data-cart-total]');
            if (subEl) subEl.textContent = money(subtotal);
            if (shipEl) shipEl.textContent = shipping === 0 ? 'Offerte' : money(shipping);
            if (totalEl) totalEl.textContent = money(total);
            setCartBadge(count);

            var note = summary.querySelector('.summary-note span');
            if (note) {
                note.innerHTML = shipping === 0
                    ? '<span>Vous bénéficiez de la <strong>livraison offerte</strong>.</span>'
                    : '<span>Plus que <strong>' + money(Math.max(0, freeAt - subtotal)) + '</strong> pour la livraison offerte.</span>';
            }

            if (count === 0) {
                window.location.reload();
            }
        }

        function post(url, id) {
            return fetch(url, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/x-www-form-urlencoded',
                    'X-Requested-With': 'XMLHttpRequest',
                    'RequestVerificationToken': getToken()
                },
                body: 'id=' + encodeURIComponent(id)
            }).then(function (r) {
                if (!r.ok) throw new Error('HTTP ' + r.status);
                return r.json();
            });
        }

        $$('[data-cart-action]').forEach(function (btn) {
            btn.addEventListener('click', function () {
                var id = btn.dataset.id;
                var action = btn.dataset.cartAction;
                var url = action === 'plus' ? '/Panier/PlusProduct'
                    : action === 'minus' ? '/Panier/MinusProduct'
                        : '/Panier/RemoveProduct';
                var row = document.querySelector('[data-cart-row="' + id + '"]');
                btn.disabled = true;
                post(url, id).then(function (data) {
                    if (!data.ok && data.message) toast(data.message, 'error');
                    if (data.removed || action === 'remove') {
                        if (row) {
                            row.classList.add('is-removing');
                            window.setTimeout(function () { row.remove(); }, 220);
                        }
                    } else if (row) {
                        var qty = row.querySelector('[data-cart-qty]');
                        var line = row.querySelector('[data-cart-line]');
                        if (qty) qty.textContent = data.quantity;
                        if (line) line.textContent = money(data.lineTotal);
                    }
                    refreshSummary(Number(data.subtotal || 0), Number(data.count || 0));
                    if (data.message && data.ok) toast(data.message, 'success');
                }).catch(function () {
                    toast('Impossible de mettre à jour le panier.', 'error');
                }).finally(function () {
                    btn.disabled = false;
                });
            });
        });
    }

    /* ---------- AJAX wishlist ---------- */
    function initWishlist() {
        $$('[data-wishlist-form]').forEach(function (form) {
            form.addEventListener('submit', function (e) {
                e.preventDefault();
                var btn = form.querySelector('button[type="submit"]');
                var icon = btn ? btn.querySelector('i') : null;

                fetch(form.action, {
                    method: 'POST',
                    headers: {
                        'X-Requested-With': 'XMLHttpRequest',
                        'RequestVerificationToken': getToken()
                    },
                    body: new FormData(form)
                }).then(function (r) {
                    if (!r.ok) throw new Error('HTTP ' + r.status);
                    return r.json();
                }).then(function (data) {
                    if (!data.ok) return;
                    if (icon) {
                        icon.className = data.inWishlist ? 'bi bi-heart-fill' : 'bi bi-heart';
                    }
                    if (btn) btn.classList.toggle('is-active', data.inWishlist);
                    setWishlistBadge(data.count);
                    toast(data.message, 'success');
                    var item = form.closest('.wishlist-item');
                    if (item && !data.inWishlist) {
                        item.style.transition = 'opacity .25s ease, transform .25s ease';
                        item.style.opacity = '0';
                        item.style.transform = 'scale(.96)';
                        window.setTimeout(function () { item.remove(); }, 260);
                    }
                }).catch(function () {
                    form.submit();
                });
            });
        });
    }

    /* ---------- Boot ---------- */
    document.addEventListener('DOMContentLoaded', function () {
        initTheme();
        initScroll();
        initSearch();
        initDrawer();
        initReveal();
        initToasts();
        initConfirm();
        initPasswords();
        initDemoAccounts();
        initNewsletter();
        initSteppers();
        initStarPickers();
        initImagePreview();
        initSidebar();
        initCart();
        initWishlist();
    });
})();
