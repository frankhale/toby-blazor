const { app, BrowserWindow } = require("electron");

let win;

function createWindow() {
  win = new BrowserWindow({
    width: 1024,
    height: 768,
    icon: __dirname + "/toby.png"
  });
  win.removeMenu();
  win.loadURL("https://localhost:5001");
  //win.webContents.openDevTools();

  win.on("closed", () => {
    win = null;
  });
}

app.on("ready", createWindow);
app.on("window-all-closed", () => {
  if (process.platform !== "darwin") {
    app.quit();
  }
});
app.on("activate", () => {
  if (win === null) {
    createWindow();
  }
});
