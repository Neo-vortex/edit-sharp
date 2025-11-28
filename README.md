# EditSharp

A modern terminal-based text editor in C# inspired by [Microsoft's edit](https://github.com/microsoft/edit) — a similar project built in Rust.


<img width="839" height="542" alt="image" src="https://github.com/user-attachments/assets/1eec3210-c66a-4c27-8614-5186bafcee8a" />
<img width="671" height="450" alt="image" src="https://github.com/user-attachments/assets/181317af-3a66-4ce1-93cc-d5d3c28da58a" />

<img width="845" height="552" alt="image" src="https://github.com/user-attachments/assets/3b7ed66c-18aa-4328-b823-1e41a37a3199" />
<img width="847" height="547" alt="image" src="https://github.com/user-attachments/assets/f19468e3-32ed-4546-8850-5aa03e9bfda2" />
<img width="846" height="560" alt="image" src="https://github.com/user-attachments/assets/18afa5da-b424-400c-be93-d69151e1e189" />
<img width="843" height="536" alt="image" src="https://github.com/user-attachments/assets/75c79817-5e51-4bf6-a389-0fa0fb1bf581" />
<img width="833" height="539" alt="image" src="https://github.com/user-attachments/assets/c253b782-50fe-4d95-9e9f-4ca091e9e8c0" />



## 🎯 Quick Start

### Download
Choose your edition from the  [Releases](https://github.com/Neo-vortex/edit-sharp/releases) page:
- **Trimmed AOT**: Ultra-lightweight, instant startup (~5-10MB)
- **Full Edition**: Feature-complete with plugin support (~20-30MB)

### Installation

**Linux/macOS:**
```bash
tar -xzf edit-sharp-*.tar.gz
chmod +x edit-sharp
./edit-sharp [filename]
```

**Windows:**
```cmd
# Extract the .zip file, then:
edit-sharp.exe [filename]
```

## 📦 Two Editions

### 🪶 Trimmed AOT Edition

**Perfect for:** Daily text editing, fast startup, minimal resource usage

**Features:**
- ✅ Native AOT compiled - instant startup
- ✅ Smallest file size (~5-10MB)
- ✅ No runtime dependencies
- ✅ All core editing features
- ✅ Line numbers, status bar, word wrap
- ✅ File operations (new, open, save, save as)
- ❌ No plugin support
- ❌ No Command Palette

**Build Configuration:** `Release-Trimmed`

### 🔌 Full Edition

**Perfect for:** Power users, plugin developers, extensibility

**Features:**
- ✅ Full Lua plugin support
- ✅ .NET plugin support
- ✅ Command Palette (Ctrl+Shift+P)
- ✅ Extensible with custom commands
- ✅ All core editing features
- ✅ Regex find/replace (via plugins)
- ✅ Text transformations (via plugins)
- ⚠️ Larger file size (~20-30MB)
- ⚠️ Slightly slower startup

**Build Configuration:** `Release-Full`

## 🎮 Usage

### Basic Keyboard Shortcuts

| Shortcut | Action |
|----------|--------|
| `Ctrl+N` | New File |
| `Ctrl+O` | Open File |
| `Ctrl+S` | Save File |
| `Ctrl+Shift+S` | Save As |
| `Ctrl+Q` | Exit |

### Full Edition Only

| Shortcut | Action |
|----------|--------|
| `Ctrl+Shift+P` | Open Command Palette |

## 🔌 Plugin Development (Full Edition Only)

### Lua Plugins

Create a `plugins/` folder next to the executable and add `.lua` files.

**Example Plugin:**
```lua
-- hello.lua
editor.registerCommand(
    "example.hello",
    "Say Hello",
    "Examples",
    function()
        local name = editor.getInput("Name", "Enter your name:", "")
        if name and name ~= "" then
            editor.showMessage("Hello", "Hello, " .. name .. "!")
        end
    end
)
```

### Lua API

**Text Operations:**
- `editor.getText()` - Get all text
- `editor.setText(text)` - Set all text
- `editor.getSelectedText()` - Get selection
- `editor.replaceSelectedText(text)` - Replace selection
- `editor.insertText(text)` - Insert at cursor

**User Input:**
- `editor.getInput(title, prompt, default)` - Single-line input
- `editor.getMultiLineInput(title, prompt, default)` - Multi-line input
- `editor.confirm(title, message)` - Yes/No confirmation

**UI:**
- `editor.showMessage(title, message)` - Show message dialog
- `editor.setStatus(message)` - Update status bar

**Commands:**
- `editor.registerCommand(id, name, category, function)` - Register command

### Example Plugins Included

1. **text_transform.lua** - Upper/lower/title case, reverse
2. **line_tools.lua** - Sort, deduplicate, trim
3. **regex_tools.lua** - Regex find/replace
4. **find_replace.lua** - Simple find/replace
5. **snippets.lua** - Lorem ipsum, date insertion
6. **word_stats.lua** - Word count statistics

## 🛠️ Building from Source

### Requirements
- .NET 10.0 SDK or later
- Terminal.Gui 1.19.0 (auto-installed)
- MoonSharp 2.0.0 (Full edition only, auto-installed)

### Build Commands

**Linux/macOS:**
```bash
chmod +x build.sh
./build.sh
```

**Windows:**
```powershell
.\build.ps1
```

**Manual Build:**
```bash
# Trimmed AOT Edition
dotnet publish -c Release-Trimmed -r linux-x64 --self-contained

# Full Edition
dotnet publish -c Release-Full -r linux-x64 --self-contained
```

### Build Configurations

| Configuration | AOT | Trimmed | Plugins |
|--------------|-----|---------|---------|
| `Release-Trimmed` | ✅ | ✅ | ❌ |
| `Release-Full` | ❌ | ❌ | ✅ |
| `Release` | ❌ | ❌ | ✅ (same as Release-Full) |
| `Debug` | ❌ | ❌ | ✅ |

## 📊 Performance Comparison

Tested on Linux x64:

| Metric | Trimmed AOT | Full Edition |
|--------|-------------|--------------|
| Binary Size | ~8 MB | ~25 MB |
| Startup Time | ~50ms | ~200ms |
| Memory (idle) | ~15 MB | ~30 MB |
| Plugin Support | ❌ | ✅ |

## 🤝 Contributing

Contributions welcome! Please ensure:
1. Code works in both Trimmed and Full builds
2. Use `#if PLUGIN_SUPPORT` for plugin-specific code
3. Test both editions before submitting PR

## 📝 License

MIT License - See LICENSE file

## 🐛 Known Issues

**Trimmed AOT Edition:**
- Cannot load plugins (by design)
- No Command Palette (by design)

**Full Edition:**
- Larger binary size
- Slower startup than Trimmed edition

## 🚀 Roadmap

- [ ] Syntax highlighting (Full edition)
- [ ] More built-in Lua plugins
- [ ] Configuration file support
- [ ] Theme customization
- [ ] Undo/Redo support
- [ ] Search/Replace UI (without plugins)


## 💬 Support

- GitHub Issues: Report bugs or request features
- Discussions: Ask questions or share plugins

---

Made with ❤️ using .NET and Terminal.Gui
