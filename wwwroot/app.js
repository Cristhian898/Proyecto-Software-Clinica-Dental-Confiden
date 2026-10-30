// Preferencias de visualización: modo claro/oscuro y tamaño de letra (se guardan en el navegador)
(function () {
  var r = document.documentElement;
  function get(k, d) { try { return localStorage.getItem(k) || d; } catch (e) { return d; } }
  function set(k, v) { try { localStorage.setItem(k, v); } catch (e) { } }
  function apply() {
    var t = get('cd-theme', 'light');
    r.setAttribute('data-bs-theme', t);
    r.setAttribute('data-theme', t);
    r.style.fontSize = get('cd-font', '100') + '%';
  }
  window.cdTheme = function () { set('cd-theme', get('cd-theme', 'light') === 'dark' ? 'light' : 'dark'); apply(); };
  window.cdFont = function () { var z = get('cd-font', '100'); set('cd-font', z === '100' ? '112' : z === '112' ? '125' : '100'); apply(); };
  apply();
  document.addEventListener('DOMContentLoaded', apply);
  window.addEventListener('load', function () { if (window.Blazor && Blazor.addEventListener) Blazor.addEventListener('enhancedload', apply); });
})();
