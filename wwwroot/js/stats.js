(async function () {
    //generovane pomocou AI
    const todayKcal = document.getElementById("todayKcal");
    const todayP = document.getElementById("todayP");
    const todayC = document.getElementById("todayC");
    const todayF = document.getElementById("todayF");
    const todayCount = document.getElementById("todayCount");

    const goalBox = document.getElementById("goalBox");
    const goalMissing = document.getElementById("goalMissing");

    const gKcal = document.getElementById("gKcal");
    const gP = document.getElementById("gP");
    const gC = document.getElementById("gC");
    const gF = document.getElementById("gF");

 

    const canvas = document.getElementById("weeklyCanvas");
    const hint = document.getElementById("weeklyHint");
    const ctx = canvas.getContext("2d");

    async function loadToday() {
        const resp = await fetch("/Stats/TodaySummary");
        if (!resp.ok) return;

        const data = await resp.json();

        todayKcal.textContent = data.kcal;
        todayP.textContent = data.protein;
        todayC.textContent = data.carbs;
        todayF.textContent = data.fat;
        todayCount.textContent = data.count;

        if (goalBox && goalMissing) {
            if (!data.goal) {
                goalBox.style.display = "none";
                goalMissing.style.display = "block";
            } else {
                goalMissing.style.display = "none";
                goalBox.style.display = "block";

                gKcal.textContent = data.goal.kcalGoal;
                gP.textContent = data.goal.proteinGoal;
                gC.textContent = data.goal.carbsGoal;
                gF.textContent = data.goal.fatGoal;

                const pct = (value, goal) => {
                    if (!goal || goal <= 0) return 0;
                    return Math.min(999, Math.round((value / goal) * 100));
                };

                pKcal.textContent = pct(data.kcal, data.goal.kcalGoal);
                pP.textContent = pct(data.protein, data.goal.proteinGoal);
                pC.textContent = pct(data.carbs, data.goal.carbsGoal);
                pF.textContent = pct(data.fat, data.goal.fatGoal);
            }
        }
    }

    function clearCanvas() {
        ctx.clearRect(0, 0, canvas.width, canvas.height);
    }

    function drawBars(items) {
        clearCanvas();

        const parentWidth = canvas.parentElement.clientWidth;
        canvas.width = parentWidth;

        const w = canvas.width;
        const h = canvas.height;

        const max = Math.max(...items.map(x => x.kcal), 10);
        const pad = 14;

        const barAreaW = w - pad * 2;
        const barW = barAreaW / items.length * 0.65;
        const gap = barAreaW / items.length * 0.35;

        ctx.beginPath();
        ctx.moveTo(pad, h - pad);
        ctx.lineTo(w - pad, h - pad);
        ctx.strokeStyle = "#cccccc";
        ctx.stroke();

        items.forEach((x, i) => {
            const barH = ((h - pad * 2) * x.kcal) / max;
            const xPos = pad + i * (barW + gap) + gap / 2;
            const yPos = (h - pad) - barH;

            ctx.fillStyle = "#35694c";
            ctx.fillRect(xPos, yPos, barW, barH);

            ctx.fillStyle = "#444";
            ctx.font = "11px Segoe UI";
            const day = x.date.slice(5); 
            ctx.fillText(day, xPos, h - 2);

            ctx.fillStyle = "#222";
            ctx.font = "10px Segoe UI";
            ctx.fillText(String(x.kcal), xPos, yPos - 4);
        });

        const avg = (items.reduce((s, a) => s + a.kcal, 0) / items.length).toFixed(1);
        hint.textContent = `Priemer: ${avg} kcal / deň`;
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