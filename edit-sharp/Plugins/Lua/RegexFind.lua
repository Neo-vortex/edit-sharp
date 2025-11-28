-- ============================================
-- regex_tools.lua - Regex find and replace
-- ============================================

-- Find matches using regex
editor.registerCommand(
    "regex.find",
    "Find with Regex",
    "Regex",
    function()
        -- Get regex pattern from user
        local pattern = editor.getInput("Regex Find", "Enter regex pattern:", "")
        
        if not pattern or pattern == "" then
            return
        end
        
        local text = editor.getText()
        if not text then
            editor.showMessage("Error", "No text in editor")
            return
        end
        
        -- Find all matches
        local matches = {}
        local count = 0
        
        for match in string.gmatch(text, pattern) do
            count = count + 1
            table.insert(matches, match)
            if count > 100 then  -- Limit to first 100 matches
                break
            end
        end
        
        if count == 0 then
            editor.showMessage("Regex Find", "No matches found")
        else
            local result = string.format("Found %d match(es):\n\n%s", 
                count, 
                table.concat(matches, "\n"):sub(1, 500))  -- Show first 500 chars
            
            if #table.concat(matches, "\n") > 500 then
                result = result .. "\n\n... (truncated)"
            end
            
            editor.showMessage("Regex Find Results", result)
        end
        
        editor.setStatus(string.format("Regex find: %d matches", count))
    end
)

-- Replace using regex
editor.registerCommand(
    "regex.replace",
    "Replace with Regex",
    "Regex",
    function()
        -- Get regex pattern
        local pattern = editor.getInput("Regex Replace", "Enter regex pattern to find:", "")
        
        if not pattern or pattern == "" then
            return
        end
        
        -- Get replacement text
        local replacement = editor.getInput("Regex Replace", "Enter replacement text:", "")
        
        if not replacement then
            return  -- User cancelled
        end
        
        local text = editor.getText()
        if not text then
            editor.showMessage("Error", "No text in editor")
            return
        end
        
        -- Perform replacement
        local newText, count = string.gsub(text, pattern, replacement)
        
        if count > 0 then
            -- Confirm before replacing
            if editor.confirm("Confirm Replace", 
                string.format("Replace %d occurrence(s)?", count)) then
                editor.setText(newText)
                editor.setStatus(string.format("Replaced %d occurrence(s)", count))
            end
        else
            editor.showMessage("Regex Replace", "No matches found")
        end
    end
)

-- ============================================
-- find_replace.lua - Simple find and replace
-- ============================================

-- Simple find
editor.registerCommand(
    "find.simple",
    "Find Text",
    "Find",
    function()
        local searchText = editor.getInput("Find", "Enter text to find:", "")
        
        if not searchText or searchText == "" then
            return
        end
        
        local text = editor.getText()
        if not text then
            editor.showMessage("Error", "No text in editor")
            return
        end
        
        -- Count occurrences
        local count = 0
        local startPos = 1
        
        while true do
            local pos = string.find(text, searchText, startPos, true)
            if not pos then break end
            count = count + 1
            startPos = pos + 1
        end
        
        if count > 0 then
            editor.showMessage("Find Results", 
                string.format("Found %d occurrence(s) of '%s'", count, searchText))
        else
            editor.showMessage("Find Results", "Text not found")
        end
        
        editor.setStatus(string.format("Found %d occurrence(s)", count))
    end
)

-- Simple replace
editor.registerCommand(
    "find.replace",
    "Replace Text",
    "Find",
    function()
        local searchText = editor.getInput("Replace", "Enter text to find:", "")
        
        if not searchText or searchText == "" then
            return
        end
        
        local replaceText = editor.getInput("Replace", "Enter replacement text:", "")
        
        if not replaceText then
            return  -- User cancelled
        end
        
        local text = editor.getText()
        if not text then
            editor.showMessage("Error", "No text in editor")
            return
        end
        
        -- Count occurrences first
        local count = 0
        local startPos = 1
        
        while true do
            local pos = string.find(text, searchText, startPos, true)
            if not pos then break end
            count = count + 1
            startPos = pos + 1
        end
        
        if count > 0 then
            if editor.confirm("Confirm Replace", 
                string.format("Replace %d occurrence(s) of '%s' with '%s'?", 
                    count, searchText, replaceText)) then
                
                -- Perform replacement (plain text, not pattern)
                local newText = string.gsub(text, searchText:gsub("[%^%$%(%)%%%.%[%]%*%+%-%?]", "%%%1"), replaceText)
                editor.setText(newText)
                editor.setStatus(string.format("Replaced %d occurrence(s)", count))
            end
        else
            editor.showMessage("Replace", "Text not found")
        end
    end
)

-- ============================================
-- insert_custom.lua - Insert custom text with template
-- ============================================

-- Insert custom snippet
editor.registerCommand(
    "snippet.custom",
    "Insert Custom Snippet",
    "Snippets",
    function()
        local snippet = editor.getMultiLineInput(
            "Custom Snippet",
            "Enter text to insert (multi-line supported):",
            ""
        )
        
        if snippet and snippet ~= "" then
            editor.insertText(snippet)
            editor.setStatus("Inserted custom snippet")
        end
    end
)

-- Insert with variable substitution
editor.registerCommand(
    "snippet.withVariables",
    "Insert Snippet with Variables",
    "Snippets",
    function()
        local name = editor.getInput("Variable: Name", "Enter your name:", "")
        if not name or name == "" then return end
        
        local email = editor.getInput("Variable: Email", "Enter your email:", "")
        if not email or email == "" then return end
        
        local template = string.format([[
/**
 * Author: %s
 * Email: %s
 * Date: %s
 */
]], name, email, os.date("%Y-%m-%d"))
        
        editor.insertText(template)
        editor.setStatus("Inserted header with variables")
    end
)

-- ============================================
-- line_numbers.lua - Go to line number
-- ============================================

editor.registerCommand(
    "navigate.gotoLine",
    "Go to Line",
    "Navigate",
    function()
        local lineNum = editor.getInput("Go to Line", "Enter line number:", "1")
        
        if not lineNum or lineNum == "" then
            return
        end
        
        local num = tonumber(lineNum)
        if not num then
            editor.showMessage("Error", "Invalid line number")
            return
        end
        
        -- Note: TextView cursor manipulation would need TextView access
        editor.setStatus(string.format("Go to line: %d (feature in progress)", num))
        editor.showMessage("Info", "Direct line navigation requires TextView access")
    end
)

-- ============================================
-- bulk_operations.lua - Bulk text operations
-- ============================================

-- Prepend text to each line
editor.registerCommand(
    "bulk.prependLines",
    "Prepend to Each Line",
    "Bulk Edit",
    function()
        local prefix = editor.getInput("Prepend to Lines", "Enter text to prepend:", "")
        
        if not prefix or prefix == "" then
            return
        end
        
        local text = editor.getSelectedText()
        local scope = "selection"
        
        if not text or text == "" then
            text = editor.getText()
            scope = "document"
        end
        
        if text then
            local lines = {}
            for line in text:gmatch("[^\r\n]+") do
                table.insert(lines, prefix .. line)
            end
            
            local result = table.concat(lines, "\n")
            
            if scope == "selection" then
                editor.replaceSelectedText(result)
            else
                editor.setText(result)
            end
            
            editor.setStatus(string.format("Prepended to %s", scope))
        end
    end
)

-- Append text to each line
editor.registerCommand(
    "bulk.appendLines",
    "Append to Each Line",
    "Bulk Edit",
    function()
        local suffix = editor.getInput("Append to Lines", "Enter text to append:", "")
        
        if not suffix or suffix == "" then
            return
        end
        
        local text = editor.getSelectedText()
        local scope = "selection"
        
        if not text or text == "" then
            text = editor.getText()
            scope = "document"
        end
        
        if text then
            local lines = {}
            for line in text:gmatch("[^\r\n]+") do
                table.insert(lines, line .. suffix)
            end
            
            local result = table.concat(lines, "\n")
            
            if scope == "selection" then
                editor.replaceSelectedText(result)
            else
                editor.setText(result)
            end
            
            editor.setStatus(string.format("Appended to %s", scope))
        end
    end
)

editor.setStatus("Advanced input plugins loaded!")