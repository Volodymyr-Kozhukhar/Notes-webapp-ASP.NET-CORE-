const notesBtn = document.getElementById("notesBtn");
const notesTitles = document.getElementById("notes");

notesBtn.addEventListener("click", async () => {
    const response = await fetch("/api/notes");
    const data = await response.json();
    const items = data.items;
    notesTitles.textContent = "";
    for (let item of items) { notesTitles.innerHTML += item.title + "<br>"}
});
