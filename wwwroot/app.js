const addForm = document.getElementById("addForm");
const notesTitles = document.getElementById("notes");
const titleInput = document.getElementById("titleInput");
const tagInput = document.getElementById("tagInput");
const contentInput = document.getElementById("contentInput");
const submitBtn = addForm.querySelector('button[type="submit"]');

const errorField = document.getElementById("addError");

let editingId = null;

window.addEventListener('load', async () => {
    await loadNotes();
});


addForm.addEventListener("submit", async (e) => {
    e.preventDefault();

    errorField.textContent = "";

    const title = titleInput.value
    const tag = tagInput.value
    const content = contentInput.value

    if (!title.trim() || !content.trim()) {
        errorField.textContent = "Title and content are required";
        return;
    }

    const noteObject = { title: title, content: content, tag: tag };
    let response;

    let isEditing = editingId !== null;


    if (!isEditing) {
        response = await fetch("/api/notes", {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify(noteObject)
        });
    } else {
        response = await fetch("/api/notes/" + editingId, {
            method: "PUT",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify(noteObject)
        });
    }

    if (!response.ok) {
        const msg = await response.text();
        errorField.textContent = msg || response.statusText;
        return;
    }

    const result = await response.json();

    titleInput.value = "";
    tagInput.value = "";
    contentInput.value = "";
    if (isEditing) {
        const noteDiv = document.getElementById("note-" + editingId);
        if (!noteDiv) return;

        noteDiv.querySelector(".note-title").textContent = result.title;
        noteDiv.querySelector(".note-content").textContent = result.content;

        editingId = null;
        submitBtn.textContent = "Add note";

    }
    else {
        createNote(result);
    }
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
    divElement.className = "note";
    divElement.id = "note-" + item.id;

    var divElementTitle = document.createElement("div");
    divElementTitle.textContent = item.title;
    divElementTitle.className = "note-title";

    var divElementContent = document.createElement("div");
    divElementContent.textContent = item.content;
    divElementContent.className = "note-content";

    var deleteBttn = document.createElement("button");
    deleteBttn.id = item.id;
    deleteBttn.textContent = "Delete"

    var editBttn = document.createElement("button");
    editBttn.id = item.id;
    editBttn.textContent = "Edit"

    divElement.appendChild(divElementTitle);
    divElement.appendChild(divElementContent);
    divElement.appendChild(deleteBttn);
    divElement.appendChild(editBttn);

    notesTitles.appendChild(divElement);

    deleteBttn.addEventListener("click", async () => {
        const response = await fetch("/api/notes/" + item.id, { method: "DELETE" })
        if (!response.ok)
        {
            return;
        }
        divElement.remove();
    });

    editBttn.addEventListener("click", async () => {
        editingId = item.id;
        titleInput.value = item.title;
        contentInput.value = item.content;
        tagInput.value = item.tag ?? "";
        submitBtn.textContent = "Save";
        titleInput.focus();
    });
}