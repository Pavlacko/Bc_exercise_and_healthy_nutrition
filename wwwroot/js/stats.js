(async function () {

    const el = (id) => document.getElementById(id);

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

    const clamp = (n, min, max) => Math.max(min, Math.min(max, n));

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

        if (pct > 120) { clsBar = "progress-over"; clsPct = "pct over"; }
        else if (pct > 100) { clsBar = "progress-warn"; clsPct = "pct warn"; }

        barEl.className = "progress-bar " + clsBar;
        pctEl.textContent = `(${pct}%)`;
        pctEl.className = clsPct;
    }


    async function loadToday() {
        const resp = await fetch("/Stats/TodaySummary");
        if (!resp.ok) return;

        const data = await resp.json();

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
        const barW = (barAreaW / items.length) * 0.65;
        const gap = (barAreaW / items.length) * 0.35;

        ctx.beginPath();
        ctx.moveTo(pad, h - pad);
        ctx.lineTo(w - pad, h - pad);
        ctx.strokeStyle = "#cccccc";
        ctx.stroke();

        items.forEach((x, i) => {
            const kcal = Number(x.kcal) || 0;
            const barH = ((h - pad * 2) * kcal) / max;

            const xPos = pad + i * (barW + gap) + gap / 2;
            const yPos = (h - pad) - barH;

            ctx.fillStyle = "#35694c";
            ctx.fillRect(xPos, yPos, barW, barH);

            ctx.fillStyle = "#444";
            ctx.font = "11px Segoe UI";
            const day = String(x.date || "").slice(5);
            ctx.fillText(day, xPos, h - 2);

            ctx.fillStyle = "#222";
            ctx.font = "10px Segoe UI";
            ctx.fillText(String(kcal), xPos, yPos - 4);
        });

        const avg = (items.reduce((s, a) => s + (Number(a.kcal) || 0), 0) / (items.length || 1)).toFixed(1);
        if (hint) hint.textContent = `Priemer: ${avg} kcal / deň`;
    }

    async function loadWeekly() {
        const resp = await fetch("/Stats/WeeklyCalories?days=7");
        if (!resp.ok) return;

        const data = await resp.json();
        drawBars(data);
    }

    window.addEventListener("resize", () => {
        loadWeekly();
    });

    await loadToday();
    await loadWeekly();
})();