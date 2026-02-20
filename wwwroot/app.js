const addForm = document.getElementById("addForm");
const notesTitles = document.getElementById("notes");
const titleInput = document.getElementById("titleInput");
const tagInput = document.getElementById("tagInput");

const errorField = document.getElementById("addError");

window.addEventListener('load', async () => {
    await loadNotes();
});


addForm.addEventListener("submit", async (e) => {
    e.preventDefault();

    errorField.textContent = "";

    const title = titleInput.value
    const tag = tagInput.value

    if (!title.trim()) {
        errorField.textContent = "Title is required";
        return;
    }

    const noteObject = { title: title, tag: tag };

    const response = await fetch("/api/notes", {
        method: "POST",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify(noteObject)
    });

    if (!response.ok) {
        const msg = await response.text();
        errorField.textContent = msg || response.statusText;
        return;
    }

    const created = await response.json();

    titleInput.value = "";
    tagInput.value = "";
    createNote(created);
});

async function loadNotes() {
    const response = await fetch("/api/notes");

    const data = await response.json();
    const items = data.items;

    renderNotes(items);
}

function renderNotes(items) {
    notesTitles.innerHTML = "";
    for (item of items) {
        createNote(item);
    }
}

function createNote(item) {

    var divElement = document.createElement("div");

    var spanElement = document.createElement("span");
    spanElement.textContent = item.title;

    var bttn = document.createElement("button");
    bttn.id = item.id;
    bttn.textContent = "Delete"

    divElement.appendChild(spanElement);
    divElement.appendChild(bttn);

    notesTitles.appendChild(divElement);

    bttn.addEventListener("click", async () => {
        const response = await fetch("/api/notes/" + item.id, { method: "DELETE" })
        if (!response.ok)
        {
            return;
        }
        divElement.remove();
    });
}