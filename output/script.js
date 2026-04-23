(() => {
  // ========= CONFIG =========
  const STORAGE_KEY = "mm_enabledRestaurants";     // Persistenz-Key Sichtbarkeit
  const ORDER_KEY   = "mm_restaurantOrder";        // === NEW: Persistenz-Key Reihenfolge ===
  const NON_RESTO_IDS = new Set(["fixed-header", "navigation", "all", "restaurantSettingsPanel"]);
  // Optional hübschere Namen im Panel:
  const LABEL_MAP = {
    // "hausmannskost": "Hausmannskost",
    // "radatz": "Radatz",
    // ...
  };

  // ========= UTIL =========
  function getRestaurantNodes() {
    // Bevorzugt: explizit markierte Restaurants
    let nodes = Array.from(document.querySelectorAll(".resto[id]"));
    if (nodes.length) return nodes;

    // Fallback: alle DIVs mit id, offensichtliche ausschließen
    nodes = Array.from(document.querySelectorAll("div[id]"))
      .filter(n => !NON_RESTO_IDS.has(n.id) && n.id !== "restaurantSettingsToggle");
    return nodes;
  }

  function loadEnabled(validIds) {
    try {
      const raw = localStorage.getItem(STORAGE_KEY);
      if (!raw) return new Set(validIds); // Default: alles sichtbar
      const parsed = JSON.parse(raw);
      return new Set(parsed.filter(id => validIds.includes(id)));
    } catch {
      return new Set(validIds);
    }
  }

  function saveEnabled(enabledSet) {
    localStorage.setItem(STORAGE_KEY, JSON.stringify([...enabledSet]));
  }

  function applyVisibility(nodes, enabledSet) {
    for (const n of nodes) {
      // leeres display = nutzt vorhandenes CSS/Default; "none" = ausblenden
      n.style.display = enabledSet.has(n.id) ? "" : "none";
    }
  }

  // ========= NEW: Reihenfolge laden/speichern/anwenden =========

  function loadOrder(validIds) {
    try {
      const raw = localStorage.getItem(ORDER_KEY);
      if (!raw) return null;
      const parsed = JSON.parse(raw);
      if (!Array.isArray(parsed)) return null;
      // nur IDs behalten, die es wirklich gibt
      return parsed.filter(id => validIds.includes(id));
    } catch {
      return null;
    }
  }

  function saveOrder(orderIds) {
    try {
      localStorage.setItem(ORDER_KEY, JSON.stringify(orderIds));
    } catch {
      // egal
    }
  }

  function applyOrderToNodes(orderIds, nodes) {
    if (!nodes.length || !orderIds || !orderIds.length) return;
    const parent = nodes[0].parentNode;
    if (!parent) return;

    const map = new Map(nodes.map(n => [n.id, n]));

    // zuerst die bekannte Reihenfolge
    for (const id of orderIds) {
      const node = map.get(id);
      if (node && node.parentNode === parent) {
        parent.appendChild(node);
      }
    }
    // dann neue/unbekannte hinten anhängen
    for (const n of nodes) {
      if (!orderIds.includes(n.id)) {
        parent.appendChild(n);
      }
    }
  }

  // ========= PANEL UI (ohne Inline-Styles) =========
  function buildPanel(ids, enabledSet, onChange) {
    // Panel
    const panel = document.createElement("div");
    panel.id = "restaurantSettingsPanel";
    panel.hidden = true;

    // Titel
    const title = document.createElement("div");
    title.className = "rsp-title";
    title.textContent = "Restaurants auswählen";
    panel.appendChild(title);

    // Controls
    const controls = document.createElement("div");
    controls.className = "rsp-controls";

    const btnAllOn = document.createElement("button");
    btnAllOn.type = "button";
    btnAllOn.textContent = "Alle an";

    const btnAllOff = document.createElement("button");
    btnAllOff.type = "button";
    btnAllOff.textContent = "Alle aus";

    const btnClose = document.createElement("button");
    btnClose.type = "button";
    btnClose.textContent = "Schließen";

    controls.append(btnAllOn, btnAllOff, btnClose);
    panel.appendChild(controls);

    // Liste
    const list = document.createElement("div");
    list.className = "rsp-list";
    panel.appendChild(list);

    const boxes = new Map();

    // === NEW: Funktion um nach Klick auf ⬆/⬇ die Reihenfolge zu aktualisieren ===
    function updateOrderFromList() {
      // neue IDs anhand der Reihenfolge der Items in der Liste
      ids.length = 0;
      const items = Array.from(list.querySelectorAll(".rsp-item"));
      for (const item of items) {
        ids.push(item.dataset.id);
      }
      saveOrder(ids);
      // Menükarten auf der Seite in die gleiche Reihenfolge bringen
      const nodes = getRestaurantNodes();
      applyOrderToNodes(ids, nodes);
    }

    function moveItem(labelEl, direction) {
      const items = Array.from(list.querySelectorAll(".rsp-item"));
      const idx = items.indexOf(labelEl);
      if (idx === -1) return;
      const newIdx = idx + direction;
      if (newIdx < 0 || newIdx >= items.length) return;

      const targetItem = items[newIdx];
      if (direction < 0) {
        // nach oben: vor target einfügen
        list.insertBefore(labelEl, targetItem);
      } else {
        // nach unten: hinter target einfügen
        list.insertBefore(labelEl, targetItem.nextSibling);
      }
      updateOrderFromList();
    }

    // Checkboxen + Move-Buttons
    for (const id of ids) {
      const label = document.createElement("label");
      label.className = "rsp-item";
      label.dataset.id = id;               // === NEW: ID hier ablegen ===

      const cb = document.createElement("input");
      cb.type = "checkbox";
      cb.checked = enabledSet.has(id);
      cb.addEventListener("change", () => {
        if (cb.checked) enabledSet.add(id);
        else enabledSet.delete(id);
        saveEnabled(enabledSet);
        onChange(enabledSet);
      });

      const txt = document.createElement("span");
      txt.textContent = LABEL_MAP[id] || id;

      // === NEW: Move-Buttons nur im Settings-Panel ===
      const moveWrapper = document.createElement("span");
      moveWrapper.className = "rsp-moveButtons";

      const btnUp = document.createElement("button");
      btnUp.type = "button";
      btnUp.textContent = "⬆";
      btnUp.title = "Dieses Restaurant nach oben";
      btnUp.addEventListener("click", (ev) => {
        ev.preventDefault();
        ev.stopPropagation();
        moveItem(label, -1);
      });

      const btnDown = document.createElement("button");
      btnDown.type = "button";
      btnDown.textContent = "⬇";
      btnDown.title = "Dieses Restaurant nach unten";
      btnDown.addEventListener("click", (ev) => {
        ev.preventDefault();
        ev.stopPropagation();
        moveItem(label, +1);
      });

      moveWrapper.append(btnUp, btnDown);

      label.append(cb, txt, moveWrapper);
      list.appendChild(label);
      boxes.set(id, cb);
    }

    // Button-Callbacks
    btnAllOn.addEventListener("click", () => {
      enabledSet = new Set(ids);
      saveEnabled(enabledSet);
      onChange(enabledSet);
      sync();
    });

    btnAllOff.addEventListener("click", () => {
      enabledSet = new Set();
      saveEnabled(enabledSet);
      onChange(enabledSet);
      sync();
    });

    btnClose.addEventListener("click", () => {
      panel.hidden = true;
    });

    function sync() {
      for (const [id, cb] of boxes) cb.checked = enabledSet.has(id);
    }

    document.body.appendChild(panel);

    // Toggle-Button
    const toggle = document.createElement("button");
    toggle.id = "restaurantSettingsToggle";
    toggle.type = "button";
    toggle.textContent = "⚙️ Restaurants";
    toggle.title = "Sichtbarkeit konfigurieren";
    toggle.addEventListener("click", () => (panel.hidden = !panel.hidden));
    document.body.appendChild(toggle);

    return { panel, sync };
  }

  // ========= BOOT =========
  const nodes = getRestaurantNodes();
  let ids = nodes.map(n => n.id);

  // === NEW: gespeicherte Reihenfolge anwenden (Settings + Seite) ===
  const storedOrder = loadOrder(ids);
  if (storedOrder && storedOrder.length) {
    applyOrderToNodes(storedOrder, nodes);
    // ids so anpassen, dass neue Restaurants am Ende hängen
    ids = [...storedOrder, ...ids.filter(id => !storedOrder.includes(id))];
  }

  let enabled = loadEnabled(ids);
  applyVisibility(nodes, enabled);

  const ui = buildPanel(ids, enabled, (set) => applyVisibility(nodes, set));

  // Sync über Tab-Grenzen hinweg (wenn gleiche Seite in zweitem Tab offen ist)
  window.addEventListener("storage", (ev) => {
    if (ev.key === STORAGE_KEY) {
      try {
        const arr = JSON.parse(ev.newValue ?? "[]");
        enabled = new Set(arr.filter(id => ids.includes(id)));
        applyVisibility(nodes, enabled);
        ui?.sync?.();
      } catch { /* noop */ }
    }
    // Optional: Reihenfolge-Änderung zwischen Tabs könnte man hier auch noch syncen
  });
})();