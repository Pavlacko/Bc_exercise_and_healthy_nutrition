const input = document.getElementById("searchInput");
const results = document.getElementById("searchResults");

input.addEventListener("input", async () => {
    const res = await fetch(`/Friends/Search?query=${input.value}`);
    const data = await res.json();

    results.innerHTML = "";

    data.forEach(u => {
        const div = document.createElement("div");

        div.innerHTML = `
            <div class="d-flex justify-content-between">
                <span>${u.meno}</span>
                <button onclick="sendRequest(${u.id})" class="btn btn-sm btn-primary">
                    Pridať
                </button>
            </div>
        `;

        results.appendChild(div);
    });
});

async function sendRequest(id) {
    await fetch("/Friends/SendRequest", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ receiverId: id })
    });

    alert("Žiadosť odoslaná");
}

async function accept(id) {
    await fetch(`/Friends/Accept?id=${id}`, { method: "POST" });
    location.reload();
}

async function reject(id) {
    await fetch(`/Friends/Reject?id=${id}`, { method: "POST" });
    location.reload();
}