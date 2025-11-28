editor.registerCommand(
    "text.toUpperCase",
    "To Upper Case",
    "Transform",
    function()
        local text = editor.getSelectedText()
        if text and text ~= "" then
            editor.replaceSelectedText(string.upper(text))
            editor.setStatus("Converted to upper case")
        else
            editor.showMessage("No Selection", "Please select text first")
        end
    end
)