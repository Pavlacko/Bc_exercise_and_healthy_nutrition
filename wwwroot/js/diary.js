(function () {
    //generovane pomocou AI
    const dateInput = document.getElementById("diaryDate");
    const foodSelect = document.getElementById("foodSelect");
    const gramsInput = document.getElementById("gramsInput");
    const addBtn = document.getElementById("addEntryBtn");
    const addError = document.getElementById("addError");
    const entriesBody = document.getElementById("entriesBody");

    const sumCount = document.getElementById("sumCount");
    const sumKcal = document.getElementById("sumKcal");
    const sumP = document.getElementById("sumP");
    const sumC = document.getElementById("sumC");
    const sumF = document.getElementById("sumF");

    function showError(msg) {
        addError.style.display = "block";
        addError.textContent = msg;
    }

    function hideError() {
        addError.style.display = "none";
        addError.textContent = "";
    }

    async function refreshSummary() {
        const d = dateInput.value;
        const resp = await fetch(`/Diary/Summary?date=${encodeURIComponent(d)}`);
        if (!resp.ok) return;

        const data = await resp.json();
        sumCount.textContent = data.count;
        sumKcal.textContent = data.kcal;
        sumP.textContent = data.protein;
        sumC.textContent = data.carbs;
        sumF.textContent = data.fat;
    }

    function escapeHtml(str) {
        return String(str)
            .replaceAll("&", "&amp;")
            .replaceAll("<", "&lt;")
            .replaceAll(">", "&gt;")
            .replaceAll('"', "&quot;")
            .replaceAll("'", "&#039;");
    }

    function addRow(item) {
        const tr = document.createElement("tr");
        tr.setAttribute("data-id", item.id);

        tr.innerHTML = `
            <td>${escapeHtml(item.foodName)}</td>
            <td>${item.grams}</td>
            <td>${item.kcal}</td>
            <td>${item.protein}</td>
            <td>${item.carbs}</td>
            <td>${item.fat}</td>
            <td><button class="btn btn-sm btn-danger delBtn">Zmazať</button></td>
        `;

        entriesBody.prepend(tr);
    }

    async function addEntry() {
        hideError();

        const grams = Number(gramsInput.value);
        if (!grams || grams < 1 || grams > 5000) {
            showError("Zadaj gramáž 1–5000 g.");
            return;
        }

        const payload = {
            foodItemId: Number(foodSelect.value),
            grams: grams,
            date: dateInput.value
        };

        const resp = await fetch("/Diary/AddEntry", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(payload)
        });

        if (!resp.ok) {
            const txt = await resp.text();
            showError(txt || "Nepodarilo sa pridať záznam.");
            return;
        }

        const data = await resp.json();
        addRow(data);
        await refreshSummary();
    }

    async function deleteEntry(id) {
        const resp = await fetch("/Diary/DeleteEntry", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ id })
        });

        if (!resp.ok) return;

        const tr = entriesBody.querySelector(`tr[data-id="${id}"]`);
        if (tr) tr.remove();

        await refreshSummary();
    }

    addBtn.addEventListener("click", addEntry);

    entriesBody.addEventListener("click", async function (e) {
        const tr = e.target.closest("tr");
        if (!tr) return;

        const id = Number(tr.dataset.id);
        const date = dateInput.value;

        if (e.target.closest(".delBtn")) {
            await deleteEntry(id);
            return;
        }

        if (e.target.closest(".saveBtn")) {
            const foodItemId = Number(tr.querySelector(".entryFood").value);
            const grams = Number(tr.querySelector(".entryGrams").value);

            if (!grams || grams < 1 || grams > 5000) {
                alert("Zadaj gramáž 1–5000 g");
                return;
            }

            const resp = await fetch("/Diary/UpdateEntry", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({
                    id,
                    foodItemId,
                    grams,
                    date
                })
            });

            if (!resp.ok) {
                alert("Nepodarilo sa uložiť zmenu");
                return;
            }

            const data = await resp.json();

            tr.querySelector(".kcal").textContent = data.kcal;
            tr.querySelector(".p").textContent = data.protein;
            tr.querySelector(".c").textContent = data.carbs;
            tr.querySelector(".f").textContent = data.fat;

            await refreshSummary();
        }
    });

    dateInput.addEventListener("change", function () {
        window.location.href = `/Diary/Index?date=${encodeURIComponent(dateInput.value)}`;
    });

    refreshSummary();

    const saveGoalBtn = document.getElementById("saveGoalBtn");

    if (saveGoalBtn) {
        saveGoalBtn.addEventListener("click", async () => {
            const payload = {
                date: dateInput.value,
                kcalGoal: Number(document.getElementById("goalKcal").value),
                proteinGoal: Number(document.getElementById("goalP").value),
                carbsGoal: Number(document.getElementById("goalC").value),
                fatGoal: Number(document.getElementById("goalF").value)
            };

            const resp = await fetch("/Diary/SaveGoal", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(payload)
            });

            const goalMsg = document.getElementById("goalMsg");
            goalMsg.style.display = "block";

            if (!resp.ok) {
                goalMsg.className = "text-danger mt-2";
                goalMsg.textContent = await resp.text() || "Chyba pri ukladaní cieľa.";
                return;
            }

            goalMsg.className = "text-success mt-2";
            goalMsg.textContent = "Denný cieľ uložený.";
        });
    }
})();