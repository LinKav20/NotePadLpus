using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace NotepdPlus
{
    public partial class Form1 : Form
    {
        /// <summary>
        /// Creating interface and loading settings.
        /// </summary>
        public Form1()
        {
            InitializeComponent();

            // Setting filters for file dialogs.
            openFileDialog1.Filter = "Text files(*.txt)|*.txt|Text files(*.rtf)|*.rtf|Text files(*.*)|*.*|C# code files(*.cs)|*.cs";
            saveFileDialog1.Filter = "Text files(*.txt)|*.txt|Text files(*.rtf)|*.rtf|Text files(*.*)|*.*|C# code files(*.cs)|*.cs";
            fontDialog1.ShowColor = true;
            cbAutoSave.Enabled = true;

            // Creating menu.
            CreateFileMenu();
            CreateEditMenu();
            CreateFormatMenu();
            CreateSettingMenu();
            CreateContextMenu();
            //CreateBackUp();

            // Reading settings and set relevant.
            ReadSettings();
            switch (themeChosen)
            {
                case 0:
                    LightTheme();
                    break;
                case 1:
                    DarkTheme();
                    break;
            }

            cbTheme.SelectedIndex = themeChosen;
            switch (autoSaveTick / 60000)
            {
                case 1:
                    cbAutoSave.SelectedIndex = 0;
                    break;
                case 15:
                    cbAutoSave.SelectedIndex = 1;
                    break;
                case 30:
                    cbAutoSave.SelectedIndex = 2;
                    break;
                case 60:
                    cbAutoSave.SelectedIndex = 3;
                    break;
                default:
                    cbAutoSave.SelectedIndex = 4;
                    break;
            }
            autoSaveTimer.Tick += autoSave_Tick;
        }

        // Lists for saving textboxs and tabPages.
        public Dictionary<TabPage, (RichTextBox, List<string>)> files =
            new Dictionary<TabPage, (RichTextBox, List<string>)>();
        public Dictionary<TabPage, int> index = new Dictionary<TabPage, int>();
        // Colors for changing the theme.
        public Color[] colors = new Color[] { Color.White, Color.Black };
        // Comboboxes for changing parametrs.
        public ToolStripComboBox cbAutoSave = new ToolStripComboBox();
        public ToolStripComboBox cbTheme = new ToolStripComboBox();
        // Timer for autosavimg.
        public Timer autoSaveTimer = new Timer();
        // Parametrs for current value.
        public int autoSaveTick = -1;
        public int themeChosen = 0;
        // Maximum count of steps back of forward.
        const int MAX_COUNT_CHANGES = 50;
        // Check by what way the text was changed.
        public bool textChengedByCode = false;
        // Items for context menu.
        ToolStripMenuItem copyContextMenuItem = new ToolStripMenuItem("Copy");
        ToolStripMenuItem pasteContextMenuItem = new ToolStripMenuItem("Paste");
        ToolStripMenuItem selectAllContextMenuItem = new ToolStripMenuItem("Select all");
        ToolStripMenuItem deleteContextMenuItem = new ToolStripMenuItem("Delete");
        ToolStripMenuItem formatContextMenuItem = new ToolStripMenuItem("Format");
        /// <summary>
        /// Creating context menu.
        /// </summary>
        private void CreateContextMenu()
        {
            contextMenuStrip1.Items.AddRange(new[] { copyContextMenuItem,
                pasteContextMenuItem,
                selectAllContextMenuItem,
                deleteContextMenuItem,
                formatContextMenuItem });
            copyContextMenuItem.Click += copyItem_Click;
            pasteContextMenuItem.Click += pasteItem_Click;
            selectAllContextMenuItem.Click += selectAllItem_Click;
            deleteContextMenuItem.Click += deleteItem_Click;
            formatContextMenuItem.Click += formatItem_Click;
        }
        /// <summary>
        /// Creating back up menu item.
        /// </summary>
        private void CreateBackUp()
        {
            ToolStripMenuItem item = new ToolStripMenuItem("Back Up");
            item.Click += backUpItem_Click;
            menuStrip1.Items.Add(item);
        }
        /// <summary>
        /// Creating settings menu.
        /// </summary>
        private void CreateSettingMenu()
        {
            ToolStripMenuItem fileItem = new ToolStripMenuItem("Settings");
            CreateSettingsAutoSaveItem(fileItem);
            ToolStripSeparator sep = new ToolStripSeparator();
            fileItem.DropDownItems.Add(sep);
            CreateSettingsThemeItem(fileItem);
            menuStrip1.Items.Add(fileItem);
        }
        /// <summary>
        /// Creating format menu.
        /// Lollilol. SO SEXY
        /// </summary>
        private void CreateFormatMenu()
        {
            ToolStripMenuItem fileItem = new ToolStripMenuItem("Format");
            CreateFormatFontItem(fileItem);
            menuStrip1.Items.Add(fileItem);
        }
        /// <summary>
        /// Creating edit menu.
        /// </summary>
        private void CreateEditMenu()
        {
            ToolStripMenuItem fileItem = new ToolStripMenuItem("Edit");
            CreateEditCopyItem(fileItem);
            CreateEditPasteItem(fileItem);
            CreateEditSelectAllItem(fileItem);
            CreateEditDeleteItem(fileItem);
            CreateEditFormatItem(fileItem);
            // Adding items at the menu.
            menuStrip1.Items.Add(fileItem);
        }
        /// <summary>
        /// Creating file menu.
        /// </summary>
        private void CreateFileMenu()
        {
            ToolStripMenuItem fileItem = new ToolStripMenuItem("File");
            CreateFileCreateItem(fileItem);
            CreateFilecreateNewWindow(fileItem);
            CreateFileOpenItem(fileItem);
            CreateFileSaveItem(fileItem);
            CreateFileSaveAsItem(fileItem);
            CreateFileSaveAllItem(fileItem);
            ToolStripSeparator sep = new ToolStripSeparator();
            fileItem.DropDownItems.Add(sep);
            CreateFileStepBackItem(fileItem);
            CreateFileStepForwardItem(fileItem);
            fileItem.DropDownItems.Add(sep);
            CreateFileCloseItem(fileItem);
            menuStrip1.Items.Add(fileItem);
        }
        /// <summary>
        /// Creating close file item.
        /// </summary>
        /// <param name="fileItem">Menu item.</param>
        private void CreateFileCloseItem(ToolStripMenuItem fileItem)
        {
            ToolStripMenuItem item = new ToolStripMenuItem("Exit");
            item.Click += exitItem_Click;
            // Set the hotkey.
            item.ShortcutKeys = Keys.Control | Keys.E;

            fileItem.DropDownItems.Add(item);
        }
        /// <summary>
        /// Creating format font item
        /// </summary>
        /// <param name="fileItem">Menu item.</param>
        private void CreateFormatFontItem(ToolStripMenuItem fileItem)
        {
            ToolStripMenuItem item = new ToolStripMenuItem("Font and color");
            item.Click += fontItem_Click;

            fileItem.DropDownItems.Add(item);
        }
        /// <summary>
        /// Creating edit copy item.
        /// </summary>
        /// <param name="fileItem">Menu item.</param>
        private void CreateEditCopyItem(ToolStripMenuItem fileItem)
        {
            ToolStripMenuItem item = new ToolStripMenuItem("Copy");
            item.Click += copyItem_Click;
            item.ShortcutKeys = Keys.Control | Keys.C;

            fileItem.DropDownItems.Add(item);
        }
        /// <summary>
        /// Creating item for creating file in new window.
        /// </summary>
        /// <param name="fileItem">Menu item.</param>
        private void CreateFilecreateNewWindow(ToolStripMenuItem fileItem)
        {
            ToolStripMenuItem item = new ToolStripMenuItem("Create in the new window");
            item.Click += createNewWindowItem_Click;
            item.ShortcutKeys = Keys.Control | Keys.Shift | Keys.N;

            fileItem.DropDownItems.Add(item);
        }
        /// <summary>
        /// Creating paste item.
        /// </summary>
        /// <param name="fileItem">Menu item.</param>
        private void CreateEditPasteItem(ToolStripMenuItem fileItem)
        {
            ToolStripMenuItem item = new ToolStripMenuItem("Paste");
            item.Click += pasteItem_Click;
            item.ShortcutKeys = Keys.Control | Keys.V;

            fileItem.DropDownItems.Add(item);
        }
        /// <summary>
        /// Creating select item.
        /// </summary>
        /// <param name="fileItem">Menu item.</param>
        private void CreateEditSelectAllItem(ToolStripMenuItem fileItem)
        {
            ToolStripMenuItem item = new ToolStripMenuItem("Select All");
            item.Click += selectAllItem_Click;
            item.ShortcutKeys = Keys.Control | Keys.A;

            fileItem.DropDownItems.Add(item);
        }
        /// <summary>
        /// Creating delete item.
        /// </summary>
        /// <param name="fileItem">Menu item.</param>
        private void CreateEditDeleteItem(ToolStripMenuItem fileItem)
        {
            ToolStripMenuItem item = new ToolStripMenuItem("Delete");
            item.Click += deleteItem_Click;
            item.ShortcutKeys = Keys.Control | Keys.X;

            fileItem.DropDownItems.Add(item);
        }
        /// <summary>
        /// Creating format partle item.
        /// </summary>
        /// <param name="fileItem">Menu item.</param>
        private void CreateEditFormatItem(ToolStripMenuItem fileItem)
        {
            ToolStripMenuItem item = new ToolStripMenuItem("Format");
            item.Click += formatItem_Click;
            item.ShortcutKeys = Keys.Control | Keys.F;

            fileItem.DropDownItems.Add(item);
        }
        /// <summary>
        /// Creating save item.
        /// </summary>
        /// <param name="fileItem">Menu item.</param>
        private void CreateFileSaveItem(ToolStripMenuItem fileItem)
        {
            ToolStripMenuItem item = new ToolStripMenuItem("Save");
            item.Click += saveItem_Click;
            item.ShortcutKeys = Keys.Control | Keys.S;

            fileItem.DropDownItems.Add(item);
        }
        /// <summary>
        /// Creating step back item.
        /// </summary>
        /// <param name="fileItem">Menu item.</param>
        private void CreateFileStepBackItem(ToolStripMenuItem fileItem)
        {
            ToolStripMenuItem item = new ToolStripMenuItem("Step back");
            item.Click += stepBack_Click;
            item.ShortcutKeys = Keys.Control | Keys.Z;

            fileItem.DropDownItems.Add(item);
        }
        /// <summary>
        /// Creating step forward item.
        /// </summary>
        /// <param name="fileItem">Menu item.</param>
        private void CreateFileStepForwardItem(ToolStripMenuItem fileItem)
        {
            ToolStripMenuItem item = new ToolStripMenuItem("Step forward");
            item.Click += stepForward_Click;
            item.ShortcutKeys = Keys.Control | Keys.Shift | Keys.Z;

            fileItem.DropDownItems.Add(item);
        }
        /// <summary>
        /// Creating save as item.
        /// </summary>
        /// <param name="fileItem">Menu item.</param>
        private void CreateFileSaveAsItem(ToolStripMenuItem fileItem)
        {
            ToolStripMenuItem item = new ToolStripMenuItem("Save As");
            item.Click += saveAsItem_Click;
            item.ShortcutKeys = Keys.Control | Keys.Shift | Keys.S;

            fileItem.DropDownItems.Add(item);
        }
        /// <summary>
        /// Func to create file in the new window by click.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void createNewWindowItem_Click(object sender, EventArgs e)
        {
            WriteSettings(true);
            Form1 f = new Form1();
            f.Show();
        }
        /// <summary>
        /// Creating save all item.
        /// </summary>
        /// <param name="fileItem">Menu item.</param>
        private void CreateFileSaveAllItem(ToolStripMenuItem fileItem)
        {
            ToolStripMenuItem item = new ToolStripMenuItem("Save All");
            item.Click += saveAllItem_Click;
            fileItem.DropDownItems.Add(item);
        }
        /// <summary>
        /// Creating open item.
        /// </summary>
        /// <param name="fileItem">Menu item.</param>
        private void CreateFileOpenItem(ToolStripMenuItem fileItem)
        {
            ToolStripMenuItem item = new ToolStripMenuItem("Open");
            item.Click += openItem_Click;
            item.ShortcutKeys = Keys.Control | Keys.O;

            fileItem.DropDownItems.Add(item);
        }
        /// <summary>
        /// Creating create item.
        /// </summary>
        /// <param name="fileItem">Menu item.</param>
        private void CreateFileCreateItem(ToolStripMenuItem fileItem)
        {
            ToolStripMenuItem item = new ToolStripMenuItem("Create");
            item.Click += createItem_Click;
            item.ShortcutKeys = Keys.Control | Keys.N;

            fileItem.DropDownItems.Add(item);
        }
        /// <summary>
        /// Creating theme items with combobox.
        /// </summary>
        /// <param name="fileItem">Menu item.</param>
        private void CreateSettingsThemeItem(ToolStripMenuItem fileItem)
        {
            ToolStripMenuItem item = new ToolStripMenuItem("Theme");
            cbTheme.DropDownStyle = ComboBoxStyle.DropDownList;
            cbTheme.Items.Add("Light");
            cbTheme.Items.Add("Dark");
            cbTheme.SelectedIndex = 0;
            cbTheme.SelectedIndexChanged += theme_Changed;
            fileItem.DropDownItems.Add(item);
            fileItem.DropDownItems.Add(cbTheme);
        }
        /// <summary>
        /// Func to remember chosen theme when the user change it.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void theme_Changed(object sender, EventArgs e)
        {
            switch (cbTheme.SelectedIndex)
            {
                case 0:
                    themeChosen = 0;
                    LightTheme();
                    break;
                case 1:
                    themeChosen = 1;
                    DarkTheme();
                    break;
                default:
                    themeChosen = 0;
                    break;
            }
        }
        /// <summary>
        /// Setting the dark theme.
        /// </summary>
        private void DarkTheme()
        {
            foreach (TabPage tp in tabControl1.TabPages)
            {
                tp.BackColor = Color.DarkGray;
                files[tp].Item1.BackColor = Color.DarkGray;
                //files[tp].Item1.ForeColor = Color.White;
            }
            menuStrip1.BackColor = Color.Black;
            menuStrip1.ForeColor = Color.Lavender;
            tabControl1.Invalidate();
        }
        /// <summary>
        /// Setting the light theme.
        /// </summary>
        private void LightTheme()
        {
            foreach (TabPage tp in tabControl1.TabPages)
            {
                tp.BackColor = Color.WhiteSmoke;
                files[tp].Item1.BackColor = Color.White;
                //files[tp].Item1.ForeColor = Color.Black;
            }
            menuStrip1.BackColor = Color.Lavender;
            menuStrip1.ForeColor = Color.Black;
            tabControl1.Invalidate();
        }
        /// <summary>
        /// Creating autosave item with combobox.
        /// </summary>
        /// <param name="fileItem">Menu item.</param>
        private void CreateSettingsAutoSaveItem(ToolStripMenuItem fileItem)
        {
            ToolStripMenuItem item = new ToolStripMenuItem("Autosave");
            cbAutoSave.DropDownStyle = ComboBoxStyle.DropDownList;
            cbAutoSave.Items.Add("Once per 1 min");
            cbAutoSave.Items.Add("Once per 15 min");
            cbAutoSave.Items.Add("Once per 30 min");
            cbAutoSave.Items.Add("Once per 1 hour");
            cbAutoSave.Items.Add("None");
            cbAutoSave.SelectedIndex = cbAutoSave.Items.Count - 1;
            cbAutoSave.SelectedIndexChanged += autoSave_Changed;
            fileItem.DropDownItems.Add(item);
            fileItem.DropDownItems.Add(cbAutoSave);
        }
        /// <summary>
        /// Read settings.
        /// </summary>
        private void ReadSettings()
        {
            string[] lines = File.ReadAllLines(@"settings/settings.txt");
            themeChosen = Convert.ToInt32(lines[1].Split(':')[1]);
            autoSaveTick = Convert.ToInt32(lines[2].Split(':')[1]);
            if (lines[lines.Length - 1] == "END")
            {
                for (int i = 5; i < lines.Length - 1; i++)
                {
                    try
                    {
                        OpenFile(lines[i]);
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.Message, "Oooopsy",
                       MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            else
            {
                CreateNewFile();
            }
        }
        /// <summary>
        /// Selecting all text by click and hotkey.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void selectAllItem_Click(object sender, EventArgs e)
        {
            try
            {
                files[tabControl1.SelectedTab].Item1.SelectAll();
            }
            catch
            {
                MessageBox.Show("There are no opened files.", "Oooopsy",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        /// <summary>
        /// Exit by click and hotkey.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void exitItem_Click(object sender, EventArgs e)
        {
            SaveAllOpenedFiles();
            WriteSettings(false);
            Environment.Exit(0);
        }
        /// <summary>
        /// Func to remember autosave's parametrs.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void autoSave_Changed(object sender, EventArgs e)
        {
            autoSaveTimer.Stop();
            switch (cbAutoSave.SelectedIndex)
            {
                case 0:
                    autoSaveTick = 1 * 60000;
                    autoSaveTimer.Interval = autoSaveTick;
                    autoSaveTimer.Start();
                    break;
                case 1:
                    autoSaveTick = 15 * 60000;
                    autoSaveTimer.Interval = autoSaveTick;
                    autoSaveTimer.Start();
                    break;
                case 2:
                    autoSaveTick = 30 * 60000;
                    autoSaveTimer.Interval = autoSaveTick;
                    autoSaveTimer.Start();
                    break;
                case 3:
                    autoSaveTick = 60 * 60000;
                    autoSaveTimer.Interval = autoSaveTick;
                    autoSaveTimer.Start();
                    break;
                default:
                    autoSaveTick = -1;
                    autoSaveTimer.Stop();
                    break;
            }
        }
        /// <summary>
        /// Checks if the file has changes from the last saving.
        /// </summary>
        /// <param name="tp">TabPage with needed text.</param>
        /// <returns>True if the file has changes, else false.</returns>
        private bool FileHasChanges(TabPage tp)
        {
            try
            {
                return files[tp].Item2[0] == files[tp].Item1.Text;
            }
            catch
            {
                return true;
            }
        }
        /// <summary>
        /// Saving file by every tick.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void autoSave_Tick(object sender, EventArgs e)
        {
            if (!FileHasChanges(tabControl1.SelectedTab))
            {
                if (tabControl1.SelectedTab.Name == "NewFile")
                {
                    if (saveFileDialog1.ShowDialog() == DialogResult.Cancel)
                        return;
                    string fileName = saveFileDialog1.FileName;
                    SaveFile(fileName);
                }
                else SaveFile(tabControl1.SelectedTab.Name);
            }
        }
        /// <summary>
        /// Deleting selected text by click and hotkey.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void deleteItem_Click(object sender, EventArgs e)
        {
            try
            {
                files[tabControl1.SelectedTab].Item1.Cut();
            }
            catch
            {
                MessageBox.Show("There are no opened files.", "Oooopsy",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        /// <summary>
        /// Set the format selected text be click and hotkey.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void formatItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (fontDialog1.ShowDialog() == DialogResult.Cancel)
                    return;
                files[tabControl1.SelectedTab].Item1.SelectionFont = fontDialog1.Font;
                files[tabControl1.SelectedTab].Item1.SelectionColor = fontDialog1.Color;
            }
            catch
            {
                MessageBox.Show("There are no opened files.", "Oooopsy",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        /// <summary>
        /// Set the font and the color all text be click and hotkey.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void fontItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (fontDialog1.ShowDialog() == DialogResult.Cancel)
                    return;
                if (tabControl1.TabPages.Count > 0)
                {
                    files[tabControl1.SelectedTab].Item1.Font = fontDialog1.Font;
                    files[tabControl1.SelectedTab].Item1.ForeColor = fontDialog1.Color;
                }
            }
            catch
            {
                MessageBox.Show("There are no opened files.", "Oooopsy",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        /// <summary>
        /// Creating new file by clivk and hotkey.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void createItem_Click(object sender, EventArgs e)
        {
            CreateNewFile();
        }
        /// <summary>
        /// Creating new file.
        /// </summary>
        private void CreateNewFile()
        {
            TabPage tp = new TabPage("NewFile      ");
            tp.Name = "NewFile";
            tabControl1.TabPages.Add(tp);

            RichTextBox rtb = new RichTextBox();
            rtb.Dock = DockStyle.Fill;
            tp.Controls.Add(rtb);
            rtb.TextChanged += rtb_TextChanged;
            // Choosing the theme.
            tabControl1.SelectedTab = tp;
            switch (themeChosen)
            {
                case 0:
                    rtb.BackColor = Color.White;
                    break;
                case 1:
                    rtb.BackColor = Color.LightGray;
                    break;
            }
            files.Add(tp, (rtb, new List<string>() { rtb.Text, rtb.Text }));
            files[tp].Item1.ContextMenuStrip = contextMenuStrip1;
            index.Add(tp, 1);
        }
        /// <summary>
        /// Open file by click and hotkey.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void openItem_Click(object sender, EventArgs e)
        {
            openFileDialog1.InitialDirectory = @"..\..\";
            if (openFileDialog1.ShowDialog() == DialogResult.Cancel)
                return;
            string fileName = openFileDialog1.FileName;

            OpenFile(fileName);
        }
        /// <summary>
        /// Open file.
        /// </summary>
        /// <param name="fileName">Path to file.</param>
        private void OpenFile(string fileName)
        {
            string fileText = File.ReadAllText(fileName);
            string[] arrName = fileName.Split(Path.DirectorySeparatorChar);
            TabPage tp = new TabPage(arrName[arrName.Length - 1] + "      ");
            tp.Name = fileName;
            tabControl1.TabPages.Add(tp);
            RichTextBox rtb = new RichTextBox();
            rtb.Dock = DockStyle.Fill;
            rtb.TextChanged += rtb_TextChanged;
            // Choosing the theme.
            switch (themeChosen)
            {
                case 0:
                    rtb.BackColor = Color.White;
                    break;
                case 1:
                    rtb.BackColor = Color.LightGray;
                    break;
            }
            string[] nameFile = arrName[arrName.Length - 1].Split('.');
            // Open each file by correct method.
            if (nameFile[nameFile.Length - 1] == "rtf")
            {
                rtb.LoadFile(fileName);
            }
            else
            {
                rtb.Text = fileText;
            }
            tp.Controls.Add(rtb);
            tabControl1.SelectedTab = tp;
            files.Add(tp, (rtb, new List<string>() { rtb.Text, rtb.Text }));
            files[tp].Item1.ContextMenuStrip = contextMenuStrip1;
            index.Add(tp, 1);
            //if (!Directory.Exists(fileName))
            //{
            //    Directory.CreateDirectory($@"{progPath}\settings\{arrName[arrName.Length - 1]}");
            //}
        }
        /// <summary>
        /// Saving every changes by user.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void rtb_TextChanged(object sender, EventArgs e)
        {
            if (files.Count > 0)
            {
                if (!textChengedByCode)
                {
                    if (files[tabControl1.SelectedTab].Item2.Count > MAX_COUNT_CHANGES)
                    {
                        // Deleting the first unsaved version.
                        files[tabControl1.SelectedTab].Item2.RemoveAt(1);
                        index[tabControl1.SelectedTab]--;
                    }
                    // Deleting all  "future" changes if we step forward after step back.
                    for (int i = index[tabControl1.SelectedTab]; i < files[tabControl1.SelectedTab].Item2.Count; i++)
                        files[tabControl1.SelectedTab].Item2.RemoveAt(i);
                    files[tabControl1.SelectedTab].Item2.Add(files[tabControl1.SelectedTab].Item1.Text);
                    index[tabControl1.SelectedTab]++;
                }
                // Parament to know that the text doest changed by user.
                textChengedByCode = false;
            }
        }
        /// <summary>
        /// Save all files by click and hotkeys.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void saveAllItem_Click(object sender, EventArgs e)
        {
            if (tabControl1.TabPages.Count > 0)
            {
                SaveAllOpenedFiles();
            }
            else
            {
                MessageBox.Show("There are no opened files.", "Oooopsy",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        /// <summary>
        /// Save all files.
        /// </summary>
        private void SaveAllOpenedFiles()
        {
            if (tabControl1.TabPages.Count > 0)
            {
                foreach (TabPage tp in tabControl1.TabPages)
                {
                    tabControl1.SelectedTab = tp;
                    // Cheching if file has unsaved changes.
                    if (!FileHasChanges(tabControl1.SelectedTab))
                    {
                        if (MessageBox.Show($"Would you like to Save the changes in {tp.Text}?",
                    "Confirm", MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question) == DialogResult.Yes)
                        {
                            // Check if this is a new file.
                            if (tp.Name == "NewFile")
                            {
                                if (saveFileDialog1.ShowDialog() == DialogResult.Cancel)
                                    return;
                                string fileName = saveFileDialog1.FileName;
                                SaveFile(fileName);
                            }
                            else SaveFile(tp.Name);
                        }
                    }
                    files[tp].Item2[0] = files[tp].Item1.Text;
                }
            }
        }
        /// <summary>
        /// Save file by click and hotkey.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void saveItem_Click(object sender, EventArgs e)
        {
            if (tabControl1.TabPages.Count > 0)
            {
                string fileName = tabControl1.SelectedTab.Name;
                // Checking is the file save yet.
                if (fileName == "NewFile")
                {
                    if (saveFileDialog1.ShowDialog() == DialogResult.Cancel)
                        return;
                    fileName = saveFileDialog1.FileName;
                }
                SaveFile(fileName);
            }
            else
            {
                MessageBox.Show("There sre no opened files.", "Oooopsy",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        /// <summary>
        /// Save as file by click and hotkey.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void saveAsItem_Click(object sender, EventArgs e)
        {
            if (tabControl1.TabPages.Count > 0)
            {
                if (saveFileDialog1.ShowDialog() == DialogResult.Cancel)
                    return;
                string fileName = saveFileDialog1.FileName;
                SaveFile(fileName);
            }
            else
            {
                MessageBox.Show("There are no opened files.", "Oooopsy",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        /// <summary>
        /// Copy selected text in buffer by click and hotkey.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void copyItem_Click(object sender, EventArgs e)
        {
            try
            {
                Clipboard.SetText(files[tabControl1.SelectedTab].Item1.SelectedText);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ooopsy",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        /// <summary>
        /// Paste buffered text by clivk and hotkey.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void pasteItem_Click(object sender, EventArgs e)
        {
            if (tabControl1.TabPages.Count > 0)
            {
                files[tabControl1.SelectedTab].Item1.Paste();
            }
            else
            {
                MessageBox.Show("There are no opened files.", "Oooopsy",
                   MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        /// <summary>
        /// Return by previous version of the text.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void stepBack_Click(object sender, EventArgs e)
        {
            // If the tabs exits.
            if (tabControl1.TabPages.Count > 0)
            {
                textChengedByCode = true;
                if (index[tabControl1.SelectedTab] > 1)
                    index[tabControl1.SelectedTab] -= 1;
                files[tabControl1.SelectedTab].Item1.Text =
                    files[tabControl1.SelectedTab].Item2[index[tabControl1.SelectedTab]];
            }
            else
            {
                MessageBox.Show("There are no opened files.", "Oooopsy",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        /// <summary>
        /// Function to back ups. Doesnt work.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void backUpItem_Click(object sender, EventArgs e)
        {
            try
            {
                //openFileDialog1.InitialDirectory = $@"{progPath}\settings\{tabControl1.SelectedTab.Name}";
                if (openFileDialog1.ShowDialog() == DialogResult.Cancel)
                    return;
                string fileName = openFileDialog1.FileName;
                string[] arrName = fileName.Split(Path.DirectorySeparatorChar);
                string[] nameFile = arrName[arrName.Length - 1].Split('.');
                if (nameFile[nameFile.Length - 1] == "rtf")
                {
                    files[tabControl1.SelectedTab].Item1.LoadFile(fileName);
                }
                else
                {
                    files[tabControl1.SelectedTab].Item1.Text = File.ReadAllText(fileName);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Oooopsy",
                     MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }
        /// <summary>
        /// Write current settings.
        /// </summary>
        /// <param name="fileInTheNewWindow">Check if this programm opnes by firat time.</param>
        private void WriteSettings(bool fileInTheNewWindow)
        {
            TextWriter tw = new StreamWriter(@"settings/settings.txt");
            tw.WriteLine("SETTINGS");
            tw.WriteLine($"Theme:{themeChosen}");
            tw.WriteLine($"AUTOSAVE:{autoSaveTick}");
            tw.WriteLine("FILES:\nSTART");
            foreach (TabPage tp in tabControl1.TabPages)
            {
                if (tp.Name != "NewFile")
                    tw.WriteLine(tp.Name);
            }
            tw.WriteLine("END");
            if (fileInTheNewWindow)
            {
                tw.Write("fileInTheNewWindow");
            }
            tw.Close();
        }
        /// <summary>
        /// Return by next one version of the text.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void stepForward_Click(object sender, EventArgs e)
        {
            try
            {
                if (tabControl1.TabPages.Count > 0)
                {
                    textChengedByCode = true;
                    if (index[tabControl1.SelectedTab] < MAX_COUNT_CHANGES &&
                        index[tabControl1.SelectedTab] < files[tabControl1.SelectedTab].Item2.Count - 1)
                        index[tabControl1.SelectedTab] += 1;
                    files[tabControl1.SelectedTab].Item1.Text =
                        files[tabControl1.SelectedTab].Item2[index[tabControl1.SelectedTab]];
                }
                else
                {
                    MessageBox.Show("There are no opened files.", "Oooopsy",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch
            {
                MessageBox.Show("You cant go back, only forward /maybe/.", "Oooopsy",
                           MessageBoxButtons.OK, MessageBoxIcon.Error);

            }
        }
        /// <summary>
        /// Save file.
        /// </summary>
        /// <param name="fileName"></param>
        private void SaveFile(string fileName)
        {
            try
            {
                string[] arrName = fileName.Split(Path.DirectorySeparatorChar);
                string[] nameFile = arrName[arrName.Length - 1].Split('.');
                //if (!Directory.Exists(fileName))
                //{
                //    Directory.CreateDirectory($@"\settings\{arrName[arrName.Length - 1]}");
                //}
                //int numOfFile = new DirectoryInfo($@"\settings\{arrName[arrName.Length - 1]}").GetFiles().Length;
                if (nameFile[nameFile.Length - 1] == "rtf")
                {
                    files[tabControl1.SelectedTab].Item1.SaveFile(fileName);
                    //files[tabControl1.SelectedTab].Item1.SaveFile($@"{progPath}\settings\{arrName[arrName.Length - 1]}");
                }
                else
                {
                    File.WriteAllText(fileName, files[tabControl1.SelectedTab].Item1.Text);
                    //File.WriteAllText($@"{progPath}\settings\{arrName[arrName.Length - 1]}",
                    //    files[tabControl1.SelectedTab].Item1.Text);
                }
                tabControl1.SelectedTab.Name = fileName;
                tabControl1.SelectedTab.Text = arrName[arrName.Length - 1] + "      ";
                files[tabControl1.SelectedTab].Item2[0] = files[tabControl1.SelectedTab].Item1.Text;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ooopsy",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        /// <summary>
        /// Drawing cross to closing tap.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tabControl1_DrawItem(object sender, DrawItemEventArgs e)
        {
            // Drawing he cross.
            e.Graphics.DrawString("x", e.Font, Brushes.Red, e.Bounds.Right - 15, e.Bounds.Top + 4);
            e.Graphics.DrawString(this.tabControl1.TabPages[e.Index].Text, e.Font, Brushes.Black, e.Bounds.Left + 12, e.Bounds.Top + 4);
            e.DrawFocusRectangle();
        }
        /// <summary>
        /// Action for closing tap.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tabControl1_MouseDown(object sender, MouseEventArgs e)
        {
            for (int i = 0; i < this.tabControl1.TabPages.Count; i++)
            {
                Rectangle r = tabControl1.GetTabRect(i);
                Rectangle closeButton = new Rectangle(r.Right - 15, r.Top + 4, 9, 7);
                if (closeButton.Contains(e.Location))
                {
                    // Checking if file has insaved changes.
                    if (!FileHasChanges(tabControl1.SelectedTab))
                    {
                        // Users result by closing window.
                        DialogResult d = MessageBox.Show("Would you like to Save this file before closing this tab?",
                        "Confirm", MessageBoxButtons.YesNoCancel,
                        MessageBoxIcon.Question);
                        if (d != DialogResult.Cancel)
                        {
                            if (d == DialogResult.Yes)
                            {
                                // Checkin if this is a new file.
                                if (tabControl1.SelectedTab.Name == "NewFile")
                                {
                                    if (saveFileDialog1.ShowDialog() == DialogResult.Cancel)
                                        return;
                                    string fileName = saveFileDialog1.FileName;
                                    SaveFile(fileName);
                                }
                                else SaveFile(tabControl1.SelectedTab.Name);
                            }
                            // Remove closed tab from the list.
                            this.tabControl1.TabPages.RemoveAt(i);
                            break;
                        }
                    }
                    else
                    {
                        // Remove closed tab from the list.
                        this.tabControl1.TabPages.RemoveAt(i);
                        break;
                    }
                }
            }
        }
        /// <summary>
        /// Saving files before closing programm.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                SaveAllOpenedFiles();
                WriteSettings(false);
            }
            catch
            {
                MessageBox.Show("Smth went wrong", "Oooopsy",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}