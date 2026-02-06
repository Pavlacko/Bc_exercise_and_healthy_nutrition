(() => {
  const $ = (id) => document.getElementById(id);

  const date = $("diaryDate");
  const food = $("foodSelect");
  const grams = $("gramsInput");
  const addBtn = $("addEntryBtn");
  const err = $("addError");
  const body = $("entriesBody");

  const sum = {
    count: $("sumCount"),
    kcal: $("sumKcal"),
    p: $("sumP"),
    c: $("sumC"),
    f: $("sumF")
  };

  const esc = (s) => String(s)
    .replace(/&/g, "&amp;").replace(/</g, "&lt;")
    .replace(/>/g, "&gt;").replace(/"/g, "&quot;")
    .replace(/'/g, "&#39;");

  const showErr = (m) => { if (!err) return; err.style.display = "block"; err.textContent = m; };
  const hideErr = () => { if (!err) return; err.style.display = "none"; err.textContent = ""; };

  const reqJson = async (url, opt) => {
    const r = await fetch(url, opt);
    if (!r.ok) throw new Error((await r.text()) || "Request failed");
    const ct = r.headers.get("content-type") || "";
    return ct.includes("application/json") ? r.json() : null;
  };

  const refreshSummary = async () => {
    const d = date?.value;
    if (!d) return;
    const s = await reqJson(`/Diary/Summary?date=${encodeURIComponent(d)}`);
    if (!s) return;
    sum.count.textContent = s.count;
    sum.kcal.textContent = s.kcal;
    sum.p.textContent = s.protein;
    sum.c.textContent = s.carbs;
    sum.f.textContent = s.fat;
  };

  const rowHtml = (x) => `
    <tr data-id="${x.id}">
      <td>${esc(x.foodName)}</td>
      <td style="max-width:110px;">
        <input class="form-control form-control-sm entryGrams" type="number" min="1" max="5000" value="${x.grams}">
      </td>
      <td class="kcal">${x.kcal}</td>
      <td class="p">${x.protein}</td>
      <td class="c">${x.carbs}</td>
      <td class="f">${x.fat}</td>
      <td style="white-space:nowrap;">
        <button type="button" class="btn btn-sm btn-warning saveBtn">Uložiť</button>
        <button type="button" class="btn btn-sm btn-danger delBtn">Zmazať</button>
      </td>
    </tr>`;

  addBtn?.addEventListener("click", async () => {
    try {
      hideErr();
      const g = Number(grams.value);
      if (!g || g < 1 || g > 5000) return showErr("Zadaj gramáž 1–5000 g.");

      const data = await reqJson("/Diary/AddEntry", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ foodItemId: Number(food.value), grams: g, date: date.value })
      });

      body.insertAdjacentHTML("afterbegin", rowHtml(data));
      await refreshSummary();
    } catch (e) {
      showErr(e.message);
    }
  });

  body?.addEventListener("click", async (e) => {
    const tr = e.target.closest("tr");
    if (!tr) return;
    const id = Number(tr.dataset.id);

    try {
      if (e.target.closest(".delBtn")) {
        await reqJson("/Diary/DeleteEntry", {
          method: "POST",
          headers: { "Content-Type": "application/json" },
          body: JSON.stringify({ id })
        });
        tr.remove();
        return refreshSummary();
      }

      if (e.target.closest(".saveBtn")) {
        const g = Number(tr.querySelector(".entryGrams")?.value);
        if (!g || g < 1 || g > 5000) return alert("Zadaj gramáž 1–5000 g.");

        const data = await reqJson("/Diary/UpdateEntry", {
          method: "POST",
          headers: { "Content-Type": "application/json" },
          body: JSON.stringify({ id, grams: g })
        });

        tr.querySelector(".kcal").textContent = data.kcal;
        tr.querySelector(".p").textContent = data.protein;
        tr.querySelector(".c").textContent = data.carbs;
        tr.querySelector(".f").textContent = data.fat;
        await refreshSummary();
      }
    } catch (e2) {
      alert(e2.message || "Chyba.");
    }
  });

  date?.addEventListener("change", () => {
    location.href = `/Diary/Index?date=${encodeURIComponent(date.value)}`;
  });

  $("saveGoalBtn")?.addEventListener("click", async () => {
    const goalMsg = $("goalMsg");
    try {
      await reqJson("/Diary/SaveGoal", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
          date: date.value,
          kcalGoal: Number($("goalKcal").value),
          proteinGoal: Number($("goalP").value),
          carbsGoal: Number($("goalC").value),
          fatGoal: Number($("goalF").value)
        })
      });
      if (goalMsg) { goalMsg.style.display = "block"; goalMsg.className = "text-success mt-2"; goalMsg.textContent = "Denný cieľ uložený."; }
    } catch (e) {
      if (goalMsg) { goalMsg.style.display = "block"; goalMsg.className = "text-danger mt-2"; goalMsg.textContent = e.message || "Chyba."; }
    }
  });

  refreshSummary().catch(() => {});
})();