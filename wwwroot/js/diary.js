(() => {
    const $ = (id) => document.getElementById(id);

    const date = $("diaryDate");
    const food = $("foodSelect");
    const grams = $("gramsInput");
    const addBtn = $("addEntryBtn");
    const err = $("addError");
    const body = $("entriesBody");
    const foodTpl = $("foodOptionsTemplate"); 

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

    const rowHtml = (x) => {
        const options = foodTpl ? foodTpl.innerHTML : "";
        return `
      <tr data-id="${x.id}">
        <td style="min-width:260px;">
          <select class="form-select form-select-sm entryFood">
            ${options}
          </select>
        </td>

        <td style="width:120px;">
          <input class="form-control form-control-sm entryGrams" type="number" min="1" max="5000" value="${x.grams}">
        </td>

        <td class="kcal" style="width:90px;">${x.kcal}</td>
        <td class="p" style="width:70px;">${x.protein}</td>
        <td class="c" style="width:70px;">${x.carbs}</td>
        <td class="f" style="width:70px;">${x.fat}</td>

        <td class="text-end" style="white-space:nowrap; width:170px;">
          <button type="button" class="btn btn-sm btn-outline-main saveBtn">Uložiť</button>
          <button type="button" class="btn btn-sm btn-outline-danger delBtn">Zmazať</button>
        </td>
      </tr>`;
    };

    addBtn?.addEventListener("click", async () => {
        try {
            hideErr();

            const g = Number(grams?.value);
            if (!g || g < 1 || g > 5000) return showErr("Zadaj gramáž 1–5000 g.");

            const payload = {
                foodItemId: Number(food?.value),
                grams: g,
                date: date?.value
            };

            const data = await reqJson("/Diary/AddEntry", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(payload)
            });

            body.insertAdjacentHTML("afterbegin", rowHtml(data));

            const tr = body.querySelector(`tr[data-id="${data.id}"]`);
            const sel = tr?.querySelector(".entryFood");

            const foodId = (data?.foodItemId ?? payload.foodItemId);
            if (sel && foodId != null) sel.value = String(foodId);

        } catch (e) {
            showErr(e.message || "Chyba.");
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
                return;
            }

            if (e.target.closest(".saveBtn")) {
                const g = Number(tr.querySelector(".entryGrams")?.value);
                if (!g || g < 1 || g > 5000) return alert("Zadaj gramáž 1–5000 g.");

                const foodId = Number(tr.querySelector(".entryFood")?.value);

                const data = await reqJson("/Diary/UpdateEntry", {
                    method: "POST",
                    headers: { "Content-Type": "application/json" },
                    body: JSON.stringify({ id, grams: g, foodItemId: foodId })
                });

                tr.querySelector(".kcal").textContent = data.kcal;
                tr.querySelector(".p").textContent = data.protein;
                tr.querySelector(".c").textContent = data.carbs;
                tr.querySelector(".f").textContent = data.fat;
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
                    date: date?.value,
                    kcalGoal: Number($("goalKcal")?.value),
                    proteinGoal: Number($("goalP")?.value),
                    carbsGoal: Number($("goalC")?.value),
                    fatGoal: Number($("goalF")?.value)
                })
            });

            if (goalMsg) {
                goalMsg.style.display = "block";
                goalMsg.className = "text-success mt-2";
                goalMsg.textContent = "Denný cieľ uložený.";
            }
        } catch (e) {
            if (goalMsg) {
                goalMsg.style.display = "block";
                goalMsg.className = "text-danger mt-2";
                goalMsg.textContent = e.message || "Chyba.";
            }
        }
    });

})();