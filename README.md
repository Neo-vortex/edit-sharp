# EditSharp

A simple terminal-based text editor in C# inspired by [Microsoft's edit](https://github.com/microsoft/edit) — a similar project built in Rust.

## 🧠 Why This Project?

Microsoft recently released a minimalist, terminal-based editor written in Rust.  
As a long-time C# and .NET developer, I was stunned by the decision to go with Rust — not C#, the main language of .NET.

This project is my way of exploring how difficult (or not) it is to build a similar editor using C# and [Terminal.Gui](https://github.com/gui-cs/Terminal.Gui).

## ✨ Features

- 📁 Basic file operations (new, open, save, save as, close)
- ✍️ Text editing with word wrap
- 🧾 Toggleable line numbers and status bar
- 🎨 Custom color scheme (inspired by Midnight Commander?)
- 🔌 Simple plugin system with menu integration
- 🪟 Plugin-based dialog creation support
- 📊 Status bar shows encoding, line ending, line count, and character count

## ⚠️ Current Limitations

- ❌ No automated tests yet
- ⚙️ Plugin system is still basic and needs more flexible API access
- 🧼 Code cleanup and refactoring are pending for a future milestone
- 🏗️ Not currently optimized for full Native AOT (Ahead-of-Time) compilation

## 🚧 Status

This is a **beta version**. It works, but there's a lot of room for improvement. Feedback and contributions are welcome!

---

Built with ❤️ in C#.
