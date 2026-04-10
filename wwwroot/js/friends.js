const searchInput = document.getElementById("searchInput");
const searchResults = document.getElementById("searchResults");

if (searchInput) {
    searchInput.addEventListener("input", async () => {
        const query = searchInput.value.trim();

        if (query.length < 2) {
            searchResults.innerHTML = "";
            return;
        }

        const resp = await fetch(`/Friends/Search?query=${encodeURIComponent(query)}`);
        if (!resp.ok) {
            searchResults.innerHTML = "<div class='text-danger'>Nepodarilo sa načítať výsledky.</div>";
            return;
        }

        const data = await resp.json();
        searchResults.innerHTML = "";

        if (!data.length) {
            searchResults.innerHTML = "<div class='text-muted'>Nenašiel sa žiadny používateľ.</div>";
            return;
        }

        data.forEach(u => {
            const row = document.createElement("div");
            row.className = "d-flex justify-content-between align-items-center py-2 border-bottom";

            row.innerHTML = `
                <div>
                    <div class="fw-semibold">${u.meno}</div>
                    <div class="text-muted small">${u.email}</div>
                </div>
                <button class="btn btn-sm btn-primary" onclick="sendRequest(${u.id})">
                    Pridať
                </button>
            `;

            searchResults.appendChild(row);
        });
    });
}

async function sendRequest(id) {
    const resp = await fetch("/Friends/SendRequest", {
        method: "POST",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify({ receiverId: id })
    });

    if (resp.ok) {
        alert("Žiadosť odoslaná.");
        location.reload();
        return;
    }

    const text = await resp.text();
    alert(text || "Nepodarilo sa odoslať žiadosť.");
}

async function accept(id) {
    const resp = await fetch(`/Friends/Accept?id=${id}`, {
        method: "POST"
    });

    if (resp.ok) {
        location.reload();
    } else {
        alert("Nepodarilo sa prijať žiadosť.");
    }
}

async function reject(id) {
    const resp = await fetch(`/Friends/Reject?id=${id}`, {
        method: "POST"
    });

    if (resp.ok) {
        location.reload();
    } else {
        alert("Nepodarilo sa odmietnuť žiadosť.");
    }
}