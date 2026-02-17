const addForm = document.getElementById("addForm");
const notesTitles = document.getElementById("notes");
const titleInput = document.getElementById("titleInput");
const tagInput = document.getElementById("tagInput");

const errorField = document.getElementById("addError");

async function loadNotes(){
    const response = await fetch("/api/notes");

    const data = await response.json();
    const items = data.items;


    notesTitles.innerHTML = "";
    for (let item of items) { notesTitles.innerHTML += item.title + "<br>" }
}

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
        errorField.textContent = response.statusText;
        return;
    }

    titleInput.value = "";
    tagInput.value = "";
    await loadNotes();
});
