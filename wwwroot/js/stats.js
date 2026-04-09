(async function () {
    const el = (id) => document.getElementById(id);

    const statsDate = el("statsDate");
    const statsRange = el("statsRange");
    const summaryDate = el("summaryDate");
    const chartTitle = el("chartTitle");

    const todayKcal = el("todayKcal");
    const todayP = el("todayP");
    const todayC = el("todayC");
    const todayF = el("todayF");
    const todayCount = el("todayCount");

    const goalMissing = el("goalMissing");

    const gKcal = el("gKcal");
    const gP = el("gP");
    const gC = el("gC");
    const gF = el("gF");

    const barKcal = el("barKcal");
    const barP = el("barP");
    const barC = el("barC");
    const barF = el("barF");

    const pctKcalEl = el("pctKcal");
    const pctPEl = el("pctP");
    const pctCEl = el("pctC");
    const pctFEl = el("pctF");

    const canvas = el("weeklyCanvas");
    const hint = el("weeklyHint");
    const ctx = canvas ? canvas.getContext("2d") : null;

    function formatDateSk(dateStr) {
        const d = new Date(dateStr);
        if (Number.isNaN(d.getTime())) return dateStr;

        const day = String(d.getDate()).padStart(2, "0");
        const month = String(d.getMonth() + 1).padStart(2, "0");
        const year = d.getFullYear();
        return `${day}.${month}.${year}`;
    }

    function getSelectedDate() {
        return statsDate?.value || new Date().toISOString().slice(0, 10);
    }

    function getSelectedRange() {
        return Number(statsRange?.value || 7);
    }

    function updateChartTitle() {
        if (!chartTitle) return;

        const days = getSelectedRange();
        if (days === 7) chartTitle.textContent = "Kalórie posledných 7 dní";
        else if (days === 30) chartTitle.textContent = "Kalórie posledných 30 dní";
        else chartTitle.textContent = "Kalórie posledného roka";
    }

    function setBar(barEl, pctEl, value, goal) {
        if (!barEl || !pctEl) return;

        const v = Number(value) || 0;
        const g = Number(goal) || 0;

        if (g <= 0) {
            barEl.className = "progress-bar progress-none";
            barEl.style.width = "0%";
            pctEl.textContent = "(—)";
            pctEl.className = "pct";
            return;
        }

        const pct = Math.round((v / g) * 100);
        const w = Math.max(0, Math.min(160, pct));
        barEl.style.width = w + "%";

        let clsBar = "progress-ok";
        let clsPct = "pct ok";

        if (pct > 120) {
            clsBar = "progress-over";
            clsPct = "pct over";
        } else if (pct > 100) {
            clsBar = "progress-warn";
            clsPct = "pct warn";
        }

        barEl.className = "progress-bar " + clsBar;
        pctEl.textContent = `(${pct}%)`;
        pctEl.className = clsPct;
    }

    async function loadSummary() {
        const date = getSelectedDate();
        const resp = await fetch(`/Stats/TodaySummary?date=${encodeURIComponent(date)}`);
        if (!resp.ok) return;

        const data = await resp.json();

        if (summaryDate) summaryDate.textContent = formatDateSk(data.date);
        if (todayKcal) todayKcal.textContent = data.kcal;
        if (todayP) todayP.textContent = data.protein;
        if (todayC) todayC.textContent = data.carbs;
        if (todayF) todayF.textContent = data.fat;
        if (todayCount) todayCount.textContent = data.count;

        const goal = data.goal;

        if (!goal) {
            if (goalMissing) goalMissing.style.display = "block";

            if (gKcal) gKcal.textContent = "0";
            if (gP) gP.textContent = "0";
            if (gC) gC.textContent = "0";
            if (gF) gF.textContent = "0";

            setBar(barKcal, pctKcalEl, 0, 0);
            setBar(barP, pctPEl, 0, 0);
            setBar(barC, pctCEl, 0, 0);
            setBar(barF, pctFEl, 0, 0);
            return;
        }

        if (goalMissing) goalMissing.style.display = "none";

        if (gKcal) gKcal.textContent = goal.kcalGoal;
        if (gP) gP.textContent = goal.proteinGoal;
        if (gC) gC.textContent = goal.carbsGoal;
        if (gF) gF.textContent = goal.fatGoal;

        setBar(barKcal, pctKcalEl, data.kcal, goal.kcalGoal);
        setBar(barP, pctPEl, data.protein, goal.proteinGoal);
        setBar(barC, pctCEl, data.carbs, goal.carbsGoal);
        setBar(barF, pctFEl, data.fat, goal.fatGoal);
    }

    function clearCanvas() {
        if (!ctx || !canvas) return;
        ctx.clearRect(0, 0, canvas.width, canvas.height);
    }

    function drawBars(items) {
        if (!ctx || !canvas) return;

        const parentWidth = canvas.parentElement?.clientWidth ?? canvas.width;
        canvas.width = parentWidth;

        clearCanvas();

        const w = canvas.width;
        const h = canvas.height;
        const pad = 14;

        const max = Math.max(...items.map(x => Number(x.kcal) || 0), 10);
        const barAreaW = w - pad * 2;
        const slotW = barAreaW / items.length;
        const barW = Math.max(2, slotW * 0.65);
        const gap = slotW * 0.35;

        ctx.beginPath();
        ctx.moveTo(pad, h - pad);
        ctx.lineTo(w - pad, h - pad);
        ctx.strokeStyle = "#cccccc";
        ctx.stroke();

        items.forEach((x, i) => {
            const kcal = Number(x.kcal) || 0;
            const barH = ((h - pad * 2) * kcal) / max;

            const xPos = pad + i * slotW + gap / 2;
            const yPos = (h - pad) - barH;

            ctx.fillStyle = "#35694c";
            ctx.fillRect(xPos, yPos, barW, barH);

            const dateText = String(x.date || "");
            let label = dateText.slice(5);

            if (items.length > 31) {
                label = dateText.slice(2, 7);
            }

            if (items.length <= 31 || i % 30 === 0) {
                ctx.fillStyle = "#444";
                ctx.font = "10px Segoe UI";
                ctx.fillText(label, xPos, h - 2);
            }

            if (items.length <= 31 || i % 30 === 0) {
                ctx.fillStyle = "#222";
                ctx.font = "10px Segoe UI";
                ctx.fillText(String(kcal), xPos, yPos - 4);
            }
        });

        const avg = (items.reduce((s, a) => s + (Number(a.kcal) || 0), 0) / (items.length || 1)).toFixed(1);
        if (hint) hint.textContent = `Priemer: ${avg} kcal / deň`;
    }

    async function loadChart() {
        const date = getSelectedDate();
        const days = getSelectedRange();

        updateChartTitle();

        const resp = await fetch(`/Stats/CaloriesChart?date=${encodeURIComponent(date)}&days=${days}`);
        if (!resp.ok) return;

        const data = await resp.json();
        drawBars(data);
    }

    async function reloadAll() {
        await loadSummary();
        await loadChart();
    }

    statsDate?.addEventListener("change", reloadAll);
    statsRange?.addEventListener("change", loadChart);

    window.addEventListener("resize", () => {
        loadChart();
    });

    updateChartTitle();
    await reloadAll();
})();