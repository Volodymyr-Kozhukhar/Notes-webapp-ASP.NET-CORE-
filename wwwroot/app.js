const addForm = document.getElementById("addForm");
const notesList = document.getElementById("notes");
const titleInput = document.getElementById("titleInput");
const tagInput = document.getElementById("tagInput");
const contentInput = document.getElementById("contentInput");
const submitBtn = addForm.querySelector('button[type="submit"]');
const cancelBtn = document.getElementById("cancelEdit");
const reloadBtn = document.getElementById("reloadNotes");
const errorField = document.getElementById("addError");
const statusField = document.getElementById("notesStatus");
let editingId = null;
let busy = false;

function setBusy(value) {
    busy = value;
    for (const control of document.querySelectorAll("button, input, textarea")) {
        control.disabled = value;
    }
}

function resetForm() {
    addForm.reset();
    editingId = null;
    submitBtn.textContent = "Add note";
    cancelBtn.hidden = true;
}

async function request(url, options) {
    let response;
    try {
        response = await fetch(url, options);
    } catch {
        throw new Error("Cannot reach the server. Check your connection and try again.");
    }
    if (!response.ok) {
        if (response.status === 400) throw new Error("Check the required fields and their maximum lengths.");
        if (response.status === 404) throw new Error("This note no longer exists. Reload the notes.");
        throw new Error("Unable to complete the request. Please try again.");
    }
    return response.status === 204 ? null : response.json();
}

async function runAction(action) {
    if (busy) return;
    errorField.textContent = "";
    setBusy(true);
    try {
        await action();
    } catch (error) {
        errorField.textContent = error instanceof Error ? error.message : "An unexpected error occurred.";
    } finally {
        setBusy(false);
    }
}

async function loadNotes() {
    statusField.textContent = "Loading notes…";
    try {
        const data = await request("/api/notes");
        notesList.replaceChildren();
        for (const item of data.items) createNote(item);
        updateStatus();
    } catch (error) {
        statusField.textContent = "Could not refresh notes. Use Reload notes to try again.";
        throw error;
    }
}

function updateStatus() {
    statusField.textContent = notesList.children.length === 0 ? "No notes yet. Add your first note below." : "";
}

addForm.addEventListener("submit", (event) => {
    event.preventDefault();
    void runAction(async () => {
        const note = { title: titleInput.value.trim(), content: contentInput.value.trim(), tag: tagInput.value.trim() };
        if (!note.title || !note.content) throw new Error("Title and content are required.");
        const result = await request(editingId === null ? "/api/notes" : `/api/notes/${editingId}`, {
            method: editingId === null ? "POST" : "PUT",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(note)
        });
        const previous = editingId === null ? null : document.getElementById(`note-${editingId}`);
        createNote(result, previous);
        resetForm();
        updateStatus();
    });
});

function createNote(item, previous = null) {
    const card = document.createElement("article");
    card.className = "note";
    card.id = `note-${item.id}`;
    for (const [className, text] of [["note-title", item.title], ["note-content", item.content], ["note-tag", item.tag ? `Tag: ${item.tag}` : ""]]) {
        const field = document.createElement("div");
        field.className = className;
        field.textContent = text;
        card.appendChild(field);
    }
    const editButton = document.createElement("button");
    editButton.type = "button";
    editButton.textContent = "Edit";
    editButton.addEventListener("click", () => {
        editingId = item.id;
        titleInput.value = item.title;
        contentInput.value = item.content;
        tagInput.value = item.tag ?? "";
        submitBtn.textContent = "Save";
        cancelBtn.hidden = false;
        errorField.textContent = "";
        titleInput.focus();
    });
    const deleteButton = document.createElement("button");
    deleteButton.type = "button";
    deleteButton.textContent = "Delete";
    deleteButton.addEventListener("click", () => {
        if (!window.confirm(`Delete "${item.title}"?`)) return;
        void runAction(async () => {
            await request(`/api/notes/${item.id}`, { method: "DELETE" });
            card.remove();
            if (editingId === item.id) resetForm();
            updateStatus();
        });
    });
    card.append(editButton, deleteButton);
    if (previous) previous.replaceWith(card);
    else notesList.appendChild(card);
}

cancelBtn.addEventListener("click", resetForm);
reloadBtn.addEventListener("click", () => void runAction(loadNotes));
void runAction(loadNotes);
