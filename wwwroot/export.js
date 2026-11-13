window.skillExport = {
    download(fileName, content, mimeType) {
        const blob = new Blob([content], { type: mimeType });
        const url = URL.createObjectURL(blob);
        const link = document.createElement("a");
        link.href = url;
        link.download = fileName;
        link.click();
        URL.revokeObjectURL(url);
    }
};
window.organizeView = {
    load() {
        return sessionStorage.getItem("skill-organize-view");
    },
    save(value) {
        sessionStorage.setItem("skill-organize-view", value);
    },
    clear() {
        sessionStorage.removeItem("skill-organize-view");
    }
};
