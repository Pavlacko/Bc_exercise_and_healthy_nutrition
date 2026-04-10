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
            row.className = "friend-row";

            const profileButton = u.canViewProfile
                ? `<a href="/Stats/User/${u.id}" class="btn btn-outline-main btn-sm friend-btn">Zobraziť profil</a>`
                : "";

            row.innerHTML = `
                <div class="friend-row-left">
                    <div class="friend-name">${u.meno}</div>
                <div class="friend-email">${u.email}</div>
                </div>
                <div class="friend-actions">
                    ${profileButton}
                    <button class="btn btn-main btn-sm friend-btn" onclick="sendRequest(${u.id})">
                        Pridať priateľa
                    </button>
                </div>
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

async function removeFriend(friendId) {
    const confirmed = confirm("Naozaj chceš odstrániť tohto priateľa?");
    if (!confirmed) return;

    const resp = await fetch(`/Friends/RemoveFriend?friendId=${friendId}`, {
        method: "POST"
    });

    if (resp.ok) {
        location.reload();
    } else {
        alert("Nepodarilo sa odstrániť priateľa.");
    }
}