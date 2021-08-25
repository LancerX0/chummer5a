/*  This file is part of Chummer5a.
 *
 *  Chummer5a is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  Chummer5a is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with Chummer5a.  If not, see <http://www.gnu.org/licenses/>.
 *
 *  You can obtain the full source code for Chummer5a at
 *  https://github.com/chummer5a/chummer5a
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml.XPath;
using Chummer.Backend.Attributes;
using NLog;

namespace Chummer
{
    public partial class frmCharacterOptions : Form
    {
        private static Logger Log { get; } = LogManager.GetCurrentClassLogger();
        private readonly CharacterOptions _objCharacterOptions;
        private CharacterOptions _objReferenceCharacterOptions;
        private readonly List<ListItem> _lstSettings = new List<ListItem>();

        // List of custom data directory infos on the character, in load order. If the character has a directory name for which we have no info, key will be a string instead of an info
        private readonly TypedOrderedDictionary<object, bool> _dicCharacterCustomDataDirectoryInfos = new TypedOrderedDictionary<object, bool>();

        private bool _blnLoading = true;
        private bool _blnSkipLimbCountUpdate;
        private bool _blnDirty;
        private bool _blnSourcebookToggle = true;
        private bool _blnWasRenamed;
        private bool _blnIsLayoutSuspended = true;

        // Used to revert to old selected setting if user cancels out of selecting a different one
        private int _intOldSelectedSettingIndex = -1;

        private readonly HashSet<string> _setPermanentSourcebooks = new HashSet<string>();

        #region Form Events

        public frmCharacterOptions(CharacterOptions objExistingOptions = null)
        {
            InitializeComponent();
            this.UpdateLightDarkMode();
            this.TranslateWinForm();
            _objReferenceCharacterOptions = objExistingOptions ?? OptionsManager.LoadedCharacterOptions[GlobalOptions.DefaultCharacterOption];
            _objCharacterOptions = new CharacterOptions(_objReferenceCharacterOptions);
            RebuildCustomDataDirectoryInfos();
        }

        private void frmCharacterOptions_Load(object sender, EventArgs e)
        {
            SetToolTips();
            PopulateSettingsList();

            List<ListItem> lstBuildMethods = new List<ListItem>(4)
            {
                new ListItem(CharacterBuildMethod.Priority, LanguageManager.GetString("String_Priority")),
                new ListItem(CharacterBuildMethod.SumtoTen, LanguageManager.GetString("String_SumtoTen")),
                new ListItem(CharacterBuildMethod.Karma, LanguageManager.GetString("String_Karma"))
            };
            if (GlobalOptions.LifeModuleEnabled)
                lstBuildMethods.Add(new ListItem(CharacterBuildMethod.LifeModule, LanguageManager.GetString("String_LifeModule")));

            cboBuildMethod.BeginUpdate();
            cboBuildMethod.PopulateWithListItems(lstBuildMethods);
            cboBuildMethod.EndUpdate();

            PopulateOptions();
            SetupDataBindings();

            IsDirty = false;
            _blnLoading = false;
            _blnIsLayoutSuspended = false;
        }

        #endregion Form Events

        #region Control Events

        private void cmdGlobalOptionsCustomData_Click(object sender, EventArgs e)
        {
            using (new CursorWait(this))
            using (frmOptions frmOptions = new frmOptions("tabCustomDataDirectories"))
                frmOptions.ShowDialog(this);
        }

        private void cmdRename_Click(object sender, EventArgs e)
        {
            using (frmSelectText frmSelectName = new frmSelectText
            {
                DefaultString = _objCharacterOptions.Name,
                Description = LanguageManager.GetString("Message_CharacterOptions_SettingRename")
            })
            {
                frmSelectName.ShowDialog(this);
                if (frmSelectName.DialogResult != DialogResult.OK)
                    return;
                _objCharacterOptions.Name = frmSelectName.SelectedValue;
            }

            using (new CursorWait(this))
            {
                bool blnDoResumeLayout = !_blnIsLayoutSuspended;
                if (blnDoResumeLayout)
                {
                    _blnIsLayoutSuspended = true;
                    SuspendLayout();
                }

                if (cboSetting.SelectedIndex >= 0)
                {
                    int intCurrentSelectedSettingIndex = cboSetting.SelectedIndex;
                    ListItem objNewListItem = new ListItem(_lstSettings[intCurrentSelectedSettingIndex].Value, _objCharacterOptions.DisplayName);
                    _blnLoading = true;
                    cboSetting.BeginUpdate();
                    _lstSettings[intCurrentSelectedSettingIndex] = objNewListItem;
                    cboSetting.PopulateWithListItems(_lstSettings);
                    cboSetting.SelectedIndex = intCurrentSelectedSettingIndex;
                    cboSetting.EndUpdate();
                    _blnLoading = false;
                }

                _blnWasRenamed = true;
                IsDirty = true;
                if (blnDoResumeLayout)
                {
                    _blnIsLayoutSuspended = false;
                    ResumeLayout();
                }
                _intOldSelectedSettingIndex = cboSetting.SelectedIndex;
            }
        }

        private void cmdDelete_Click(object sender, EventArgs e)
        {
            // Verify that the user wants to delete this setting
            if (Program.MainForm.ShowMessageBox(
                string.Format(GlobalOptions.CultureInfo, LanguageManager.GetString("Message_CharacterOptions_ConfirmDelete"),
                    _objReferenceCharacterOptions.Name),
                LanguageManager.GetString("MessageTitle_Options_ConfirmDelete"),
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                return;

            if (!Utils.SafeDeleteFile(Path.Combine(Application.StartupPath, "settings", _objReferenceCharacterOptions.FileName), true))
                return;

            using (new CursorWait(this))
            {
                OptionsManager.LoadedCharacterOptions.Remove(_objReferenceCharacterOptions.DictionaryKey);
                KeyValuePair<string, CharacterOptions> kvpReplacementOption =
                    OptionsManager.LoadedCharacterOptions.First(x => x.Value.BuiltInOption
                                                                     && x.Value.BuildMethod ==
                                                                     _objReferenceCharacterOptions.BuildMethod);
                foreach (Character objCharacter in Program.MainForm.OpenCharacters.Where(x =>
                    x.CharacterOptionsKey == _objReferenceCharacterOptions.FileName))
                    objCharacter.CharacterOptionsKey = kvpReplacementOption.Key;
                bool blnDoResumeLayout = !_blnIsLayoutSuspended;
                if (blnDoResumeLayout)
                {
                    _blnIsLayoutSuspended = true;
                    SuspendLayout();
                }

                _objReferenceCharacterOptions = kvpReplacementOption.Value;
                _objCharacterOptions.CopyValues(_objReferenceCharacterOptions);
                RebuildCustomDataDirectoryInfos();
                IsDirty = false;
                PopulateSettingsList();
                if (blnDoResumeLayout)
                {
                    _blnIsLayoutSuspended = false;
                    ResumeLayout();
                }
            }
        }

        private void cmdSaveAs_Click(object sender, EventArgs e)
        {
            string strSelectedName;
            string strSelectedFullFileName;
            do
            {
                do
                {
                    using (frmSelectText frmSelectName = new frmSelectText
                    {
                        DefaultString = _objCharacterOptions.BuiltInOption
                            ? string.Empty
                            : _objCharacterOptions.FileName.TrimEndOnce(".xml"),
                        Description = LanguageManager.GetString("Message_CharacterOptions_SelectSettingName")
                    })
                    {
                        frmSelectName.ShowDialog(this);
                        if (frmSelectName.DialogResult != DialogResult.OK)
                            return;
                        strSelectedName = frmSelectName.SelectedValue;
                    }

                    if (OptionsManager.LoadedCharacterOptions.Any(x => x.Value.Name == strSelectedName))
                    {
                        DialogResult eCreateDuplicateSetting = Program.MainForm.ShowMessageBox(
                            string.Format(LanguageManager.GetString("Message_CharacterOptions_DuplicateSettingName"),
                                strSelectedName),
                            LanguageManager.GetString("MessageTitle_CharacterOptions_DuplicateFileName"),
                            MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);
                        switch (eCreateDuplicateSetting)
                        {
                            case DialogResult.Cancel:
                                return;

                            case DialogResult.No:
                                strSelectedName = string.Empty;
                                break;
                        }
                    }
                } while (string.IsNullOrWhiteSpace(strSelectedName));

                string strBaseFileName = strSelectedName.FastEscape(Path.GetInvalidFileNameChars()).TrimEndOnce(".xml");
                // Make sure our file name isn't too long, otherwise we run into problems on Windows
                // We can assume that Chummer's startup path plus 16 is within the limit, otherwise the user would have had problems installing Chummer with its data files in the first place
                int intStartupPathLimit = Application.StartupPath.Length + 16;
                if (strBaseFileName.Length > intStartupPathLimit)
                    strBaseFileName = strBaseFileName.Substring(0, intStartupPathLimit);
                strSelectedFullFileName = strBaseFileName + ".xml";
                int intMaxNameLength = char.MaxValue - Application.StartupPath.Length - "settings".Length - 6;
                uint uintAccumulator = 1;
                string strSeparator = "_";
                while (OptionsManager.LoadedCharacterOptions.Any(x => x.Value.FileName == strSelectedFullFileName))
                {
                    strSelectedFullFileName = strBaseFileName + strSeparator + uintAccumulator.ToString(GlobalOptions.InvariantCultureInfo) + ".xml";
                    if (strSelectedFullFileName.Length > intMaxNameLength)
                    {
                        Program.MainForm.ShowMessageBox(
                            LanguageManager.GetString("Message_CharacterOptions_SettingFileNameTooLongError"),
                            LanguageManager.GetString("MessageTitle_CharacterOptions_SettingFileNameTooLongError"),
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        strSelectedName = string.Empty;
                        break;
                    }
                    if (uintAccumulator == uint.MaxValue)
                    {
                        uintAccumulator = 0;
                        strSeparator += "_";
                    }
                    uintAccumulator += 1;
                }
            } while (string.IsNullOrWhiteSpace(strSelectedName));

            using (new CursorWait(this))
            {
                _objCharacterOptions.Name = strSelectedName;
                if (!_objCharacterOptions.Save(strSelectedFullFileName, true))
                    return;
                bool blnDoResumeLayout = !_blnIsLayoutSuspended;
                if (blnDoResumeLayout)
                {
                    _blnIsLayoutSuspended = true;
                    SuspendLayout();
                }

                CharacterOptions objNewCharacterOptions = new CharacterOptions();
                objNewCharacterOptions.CopyValues(_objCharacterOptions);
                OptionsManager.LoadedCharacterOptions.Add(
                    objNewCharacterOptions.DictionaryKey,
                    objNewCharacterOptions);
                _objReferenceCharacterOptions = objNewCharacterOptions;
                IsDirty = false;
                PopulateSettingsList();
                if (blnDoResumeLayout)
                {
                    _blnIsLayoutSuspended = false;
                    ResumeLayout();
                }
            }
        }

        private void cmdSave_Click(object sender, EventArgs e)
        {
            using (new CursorWait(this))
            {
                if (_objReferenceCharacterOptions.BuildMethod != _objCharacterOptions.BuildMethod)
                {
                    StringBuilder sbdConflictingCharacters = new StringBuilder();
                    foreach (Character objCharacter in Program.MainForm.OpenCharacters)
                    {
                        if (!objCharacter.Created && objCharacter.Options == _objReferenceCharacterOptions)
                            sbdConflictingCharacters.AppendLine(objCharacter.CharacterName);
                    }
                    if (sbdConflictingCharacters.Length > 0)
                    {
                        Program.MainForm.ShowMessageBox(this,
                            LanguageManager.GetString("Message_CharacterOptions_OpenCharacterOnBuildMethodChange") +
                            sbdConflictingCharacters,
                            LanguageManager.GetString("MessageTitle_CharacterOptions_OpenCharacterOnBuildMethodChange"),
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }
                }
                if (!_objCharacterOptions.Save())
                    return;
                bool blnDoResumeLayout = !_blnIsLayoutSuspended;
                if (blnDoResumeLayout)
                {
                    _blnIsLayoutSuspended = true;
                    SuspendLayout();
                }

                _objReferenceCharacterOptions.CopyValues(_objCharacterOptions);
                IsDirty = false;
                if (blnDoResumeLayout)
                {
                    _blnIsLayoutSuspended = false;
                    ResumeLayout();
                }
            }
        }

        private void cboSetting_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_blnLoading)
                return;
            string strSelectedFile = cboSetting.SelectedValue?.ToString();
            if (string.IsNullOrEmpty(strSelectedFile) || !OptionsManager.LoadedCharacterOptions.TryGetValue(strSelectedFile, out CharacterOptions objNewOption))
                return;

            if (IsDirty)
            {
                string text = LanguageManager.GetString("Message_CharacterOptions_UnsavedDirty");
                string caption = LanguageManager.GetString("MessageTitle_CharacterOptions_UnsavedDirty");

                if (Program.MainForm.ShowMessageBox(text, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Question) !=
                    DialogResult.Yes)
                {
                    _blnLoading = true;
                    cboSetting.SelectedIndex = _intOldSelectedSettingIndex;
                    _blnLoading = false;
                    return;
                }
                IsDirty = false;
            }

            using (new CursorWait(this))
            {
                _blnLoading = true;
                bool blnDoResumeLayout = !_blnIsLayoutSuspended;
                if (blnDoResumeLayout)
                {
                    _blnIsLayoutSuspended = true;
                    SuspendLayout();
                }

                if (_blnWasRenamed && _intOldSelectedSettingIndex >= 0)
                {
                    int intCurrentSelectedSettingIndex = cboSetting.SelectedIndex;
                    ListItem objNewListItem =
                        new ListItem(_lstSettings[_intOldSelectedSettingIndex].Value, _objReferenceCharacterOptions.DisplayName);
                    cboSetting.BeginUpdate();
                    _lstSettings[_intOldSelectedSettingIndex] = objNewListItem;
                    cboSetting.PopulateWithListItems(_lstSettings);
                    cboSetting.SelectedIndex = intCurrentSelectedSettingIndex;
                    cboSetting.EndUpdate();
                }

                _objReferenceCharacterOptions = objNewOption;
                _objCharacterOptions.CopyValues(objNewOption);
                RebuildCustomDataDirectoryInfos();
                PopulateOptions();
                _blnLoading = false;
                IsDirty = false;
                if (blnDoResumeLayout)
                {
                    _blnIsLayoutSuspended = false;
                    ResumeLayout();
                }
                _intOldSelectedSettingIndex = cboSetting.SelectedIndex;
            }
        }

        private void cmdRestoreDefaults_Click(object sender, EventArgs e)
        {
            // Verify that the user wants to reset these values.
            if (Program.MainForm.ShowMessageBox(
                LanguageManager.GetString("Message_Options_RestoreDefaults"),
                LanguageManager.GetString("MessageTitle_Options_RestoreDefaults"),
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            using (new CursorWait(this))
            {
                _blnLoading = true;
                bool blnDoResumeLayout = !_blnIsLayoutSuspended;
                if (blnDoResumeLayout)
                {
                    _blnIsLayoutSuspended = true;
                    SuspendLayout();
                }

                if (_blnWasRenamed && cboSetting.SelectedIndex >= 0)
                {
                    int intCurrentSelectedSettingIndex = cboSetting.SelectedIndex;
                    ListItem objNewListItem =
                        new ListItem(_lstSettings[intCurrentSelectedSettingIndex].Value, _objReferenceCharacterOptions.DisplayName);
                    cboSetting.BeginUpdate();
                    _lstSettings[intCurrentSelectedSettingIndex] = objNewListItem;
                    cboSetting.PopulateWithListItems(_lstSettings);
                    cboSetting.SelectedIndex = intCurrentSelectedSettingIndex;
                    cboSetting.EndUpdate();
                }

                _objCharacterOptions.CopyValues(_objReferenceCharacterOptions);
                RebuildCustomDataDirectoryInfos();
                PopulateOptions();
                _blnLoading = false;
                IsDirty = false;
                if (blnDoResumeLayout)
                {
                    _blnIsLayoutSuspended = false;
                    ResumeLayout();
                }
                _intOldSelectedSettingIndex = cboSetting.SelectedIndex;
            }
        }

        private void cboLimbCount_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_blnLoading || _blnSkipLimbCountUpdate)
                return;

            string strLimbCount = cboLimbCount.SelectedValue?.ToString();
            if (string.IsNullOrEmpty(strLimbCount))
            {
                _objCharacterOptions.LimbCount = 6;
                _objCharacterOptions.ExcludeLimbSlot = string.Empty;
            }
            else
            {
                int intSeparatorIndex = strLimbCount.IndexOf('<');
                if (intSeparatorIndex == -1)
                {
                    if (int.TryParse(strLimbCount, NumberStyles.Any, GlobalOptions.InvariantCultureInfo, out int intLimbCount))
                        _objCharacterOptions.LimbCount = intLimbCount;
                    else
                    {
                        Utils.BreakIfDebug();
                        _objCharacterOptions.LimbCount = 6;
                    }
                    _objCharacterOptions.ExcludeLimbSlot = string.Empty;
                }
                else
                {
                    if (int.TryParse(strLimbCount.Substring(0, intSeparatorIndex), NumberStyles.Any,
                        GlobalOptions.InvariantCultureInfo, out int intLimbCount))
                    {
                        _objCharacterOptions.LimbCount = intLimbCount;
                        _objCharacterOptions.ExcludeLimbSlot = intSeparatorIndex + 1 < strLimbCount.Length ? strLimbCount.Substring(intSeparatorIndex + 1) : string.Empty;
                    }
                    else
                    {
                        Utils.BreakIfDebug();
                        _objCharacterOptions.LimbCount = 6;
                        _objCharacterOptions.ExcludeLimbSlot = string.Empty;
                    }
                }
            }
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void frmCharacterOptions_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (IsDirty && Program.MainForm.ShowMessageBox(LanguageManager.GetString("Message_CharacterOptions_UnsavedDirty"),
                LanguageManager.GetString("MessageTitle_CharacterOptions_UnsavedDirty"), MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                e.Cancel = true;
            }
        }

        private void cmdEnableSourcebooks_Click(object sender, EventArgs e)
        {
            _blnLoading = true;
            foreach (TreeNode objNode in treSourcebook.Nodes)
            {
                string strBookCode = objNode.Tag.ToString();
                if (!_setPermanentSourcebooks.Contains(strBookCode))
                {
                    objNode.Checked = _blnSourcebookToggle;
                    if (_blnSourcebookToggle)
                        _objCharacterOptions.Books.Add(strBookCode);
                    else
                        _objCharacterOptions.Books.Remove(strBookCode);
                }
            }
            _blnLoading = false;
            _objCharacterOptions.RecalculateBookXPath();
            _objCharacterOptions.OnPropertyChanged(nameof(CharacterOptions.Books));
            _blnSourcebookToggle = !_blnSourcebookToggle;
        }

        private void treSourcebook_AfterCheck(object sender, TreeViewEventArgs e)
        {
            if (_blnLoading)
                return;
            TreeNode objNode = e.Node;
            if (objNode == null)
                return;
            string strBookCode = objNode.Tag.ToString();
            if (string.IsNullOrEmpty(strBookCode) || (_setPermanentSourcebooks.Contains(strBookCode) && !objNode.Checked))
            {
                _blnLoading = true;
                objNode.Checked = !objNode.Checked;
                _blnLoading = false;
                return;
            }
            if (objNode.Checked)
                _objCharacterOptions.Books.Add(strBookCode);
            else
                _objCharacterOptions.Books.Remove(strBookCode);
            _objCharacterOptions.RecalculateBookXPath();
            _objCharacterOptions.OnPropertyChanged(nameof(CharacterOptions.Books));
        }

        private void cmdIncreaseCustomDirectoryLoadOrder_Click(object sender, EventArgs e)
        {
            TreeNode nodSelected = treCustomDataDirectories.SelectedNode;
            if (nodSelected == null)
                return;
            int intIndex = nodSelected.Index;
            if (intIndex <= 0)
                return;
            _dicCharacterCustomDataDirectoryInfos.Reverse(intIndex - 1, 2);
            _objCharacterOptions.CustomDataDirectoryKeys.Reverse(intIndex - 1, 2);
            _objCharacterOptions.OnPropertyChanged(nameof(CharacterOptions.CustomDataDirectoryKeys));
            PopulateCustomDataDirectoryTreeView();
        }

        private void cmdToTopCustomDirectoryLoadOrder_Click(object sender, EventArgs e)
        {
            TreeNode nodSelected = treCustomDataDirectories.SelectedNode;
            if (nodSelected == null)
                return;
            int intIndex = nodSelected.Index;
            if (intIndex <= 0)
                return;
            for (int i = intIndex; i > 0; --i)
            {
                _dicCharacterCustomDataDirectoryInfos.Reverse(i - 1, 2);
                _objCharacterOptions.CustomDataDirectoryKeys.Reverse(i - 1, 2);
            }
            _objCharacterOptions.OnPropertyChanged(nameof(CharacterOptions.CustomDataDirectoryKeys));
            PopulateCustomDataDirectoryTreeView();
        }

        private void cmdDecreaseCustomDirectoryLoadOrder_Click(object sender, EventArgs e)
        {
            TreeNode nodSelected = treCustomDataDirectories.SelectedNode;
            if (nodSelected == null)
                return;
            int intIndex = nodSelected.Index;
            if (intIndex >= _dicCharacterCustomDataDirectoryInfos.Count - 1)
                return;
            _dicCharacterCustomDataDirectoryInfos.Reverse(intIndex, 2);
            _objCharacterOptions.CustomDataDirectoryKeys.Reverse(intIndex, 2);
            _objCharacterOptions.OnPropertyChanged(nameof(CharacterOptions.CustomDataDirectoryKeys));
            PopulateCustomDataDirectoryTreeView();
        }

        private void cmdToBottomCustomDirectoryLoadOrder_Click(object sender, EventArgs e)
        {
            TreeNode nodSelected = treCustomDataDirectories.SelectedNode;
            if (nodSelected == null)
                return;
            int intIndex = nodSelected.Index;
            if (intIndex >= _dicCharacterCustomDataDirectoryInfos.Count - 1)
                return;
            for (int i = intIndex; i < _dicCharacterCustomDataDirectoryInfos.Count - 1; ++i)
            {
                _dicCharacterCustomDataDirectoryInfos.Reverse(i, 2);
                _objCharacterOptions.CustomDataDirectoryKeys.Reverse(i, 2);
            }
            _objCharacterOptions.OnPropertyChanged(nameof(CharacterOptions.CustomDataDirectoryKeys));
            PopulateCustomDataDirectoryTreeView();
        }

        private void treCustomDataDirectories_AfterCheck(object sender, TreeViewEventArgs e)
        {
            TreeNode objNode = e.Node;
            if (objNode == null)
                return;
            int intIndex = objNode.Index;
            _dicCharacterCustomDataDirectoryInfos[_dicCharacterCustomDataDirectoryInfos[intIndex].Key] = objNode.Checked;
            if (objNode.Tag is CustomDataDirectoryInfo objCustomDataDirectoryInfo
                && _objCharacterOptions.CustomDataDirectoryKeys.ContainsKey(objCustomDataDirectoryInfo.CharacterOptionsSaveKey))
            {
                _objCharacterOptions.CustomDataDirectoryKeys[objCustomDataDirectoryInfo.CharacterOptionsSaveKey] = objNode.Checked;
                _objCharacterOptions.OnPropertyChanged(nameof(CharacterOptions.CustomDataDirectoryKeys));
            }
            else if (objNode.Tag is string strCustomDataDirectoryKey
                     && _objCharacterOptions.CustomDataDirectoryKeys.ContainsKey(strCustomDataDirectoryKey))
            {
                _objCharacterOptions.CustomDataDirectoryKeys[strCustomDataDirectoryKey] = objNode.Checked;
                _objCharacterOptions.OnPropertyChanged(nameof(CharacterOptions.CustomDataDirectoryKeys));
            }
        }

        private void txtPriorities_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = !char.IsControl(e.KeyChar)
                        && e.KeyChar != 'A' && e.KeyChar != 'B' && e.KeyChar != 'C' && e.KeyChar != 'D' && e.KeyChar != 'E'
                        && e.KeyChar != 'a' && e.KeyChar != 'b' && e.KeyChar != 'c' && e.KeyChar != 'd' && e.KeyChar != 'e';
            switch (e.KeyChar)
            {
                case 'a':
                    e.KeyChar = 'A';
                    break;

                case 'b':
                    e.KeyChar = 'B';
                    break;

                case 'c':
                    e.KeyChar = 'C';
                    break;

                case 'd':
                    e.KeyChar = 'D';
                    break;

                case 'e':
                    e.KeyChar = 'E';
                    break;
            }
        }

        private void txtPriorities_TextChanged(object sender, EventArgs e)
        {
            txtPriorities.ForeColor = txtPriorities.Text.Length == 5 ? ColorManager.WindowText : ColorManager.ErrorColor;
        }

        private void txtContactPoints_TextChanged(object sender, EventArgs e)
        {
            string strExpression = txtContactPoints.Text;
            if (!string.IsNullOrEmpty(strExpression))
            {
                foreach (string strCharAttributeName in AttributeSection.AttributeStrings)
                {
                    strExpression = strExpression
                        .Replace('{' + strCharAttributeName + '}', "0")
                        .Replace('{' + strCharAttributeName + "Unaug}", "0")
                        .Replace('{' + strCharAttributeName + "Base}", "0");
                }
                CommonFunctions.EvaluateInvariantXPath(strExpression, out bool blnSuccess);
                if (!blnSuccess)
                {
                    txtContactPoints.ForeColor = ColorManager.ErrorColor;
                    return;
                }
            }
            txtContactPoints.ForeColor = ColorManager.WindowText;
        }

        private void txtKnowledgePoints_TextChanged(object sender, EventArgs e)
        {
            string strExpression = txtKnowledgePoints.Text;
            if (!string.IsNullOrEmpty(strExpression))
            {
                foreach (string strCharAttributeName in AttributeSection.AttributeStrings)
                {
                    strExpression = strExpression
                        .Replace('{' + strCharAttributeName + '}', "0")
                        .Replace('{' + strCharAttributeName + "Unaug}", "0")
                        .Replace('{' + strCharAttributeName + "Base}", "0");
                }
                CommonFunctions.EvaluateInvariantXPath(strExpression, out bool blnSuccess);
                if (!blnSuccess)
                {
                    txtKnowledgePoints.ForeColor = ColorManager.ErrorColor;
                    return;
                }
            }
            txtKnowledgePoints.ForeColor = ColorManager.WindowText;
        }

        private void txtNuyenExpression_TextChanged(object sender, EventArgs e)
        {
            string strExpression = txtNuyenExpression.Text.Replace("{Karma}", "0");
            if (!string.IsNullOrEmpty(strExpression))
            {
                foreach (string strCharAttributeName in AttributeSection.AttributeStrings)
                {
                    strExpression = strExpression
                        .Replace('{' + strCharAttributeName + '}', "0")
                        .Replace('{' + strCharAttributeName + "Unaug}", "0")
                        .Replace('{' + strCharAttributeName + "Base}", "0");
                }
                CommonFunctions.EvaluateInvariantXPath(strExpression, out bool blnSuccess);
                if (!blnSuccess)
                {
                    txtNuyenExpression.ForeColor = ColorManager.ErrorColor;
                    return;
                }
            }
            txtNuyenExpression.ForeColor = ColorManager.WindowText;
        }

        private void chkGrade_CheckedChanged(object sender, EventArgs e)
        {
            if (!(sender is CheckBox chkGrade))
                return;

            string strGrade = chkGrade.Tag.ToString();
            if (chkGrade.Checked)
            {
                if (_objCharacterOptions.BannedWareGrades.Contains(strGrade))
                {
                    _objCharacterOptions.BannedWareGrades.Remove(strGrade);
                    _objCharacterOptions.OnPropertyChanged(nameof(CharacterOptions.BannedWareGrades));
                }
            }
            else if (!_objCharacterOptions.BannedWareGrades.Contains(strGrade))
            {
                _objCharacterOptions.BannedWareGrades.Add(strGrade);
                _objCharacterOptions.OnPropertyChanged(nameof(CharacterOptions.BannedWareGrades));
            }
        }

        private void cboPriorityTable_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_blnLoading)
                return;
            string strNewPriorityTable = cboPriorityTable.SelectedValue?.ToString();
            if (string.IsNullOrWhiteSpace(strNewPriorityTable))
                return;
            _objCharacterOptions.PriorityTable = strNewPriorityTable;
        }

        private void treCustomDataDirectories_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (!(e.Node?.Tag is CustomDataDirectoryInfo objSelected))
            {
                gpbDirectoryInfo.Visible = false;
                return;
            }

            gpbDirectoryInfo.SuspendLayout();
            txtDirectoryDescription.Text = objSelected.DisplayDescription;
            lblDirectoryVersion.Text = objSelected.MyVersion.ToString();
            lblDirectoryAuthors.Text = objSelected.DisplayAuthors;
            lblDirectoryName.Text = objSelected.Name;

            if (objSelected.DependenciesList.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                foreach (var dependency in objSelected.DependenciesList)
                    sb.AppendLine(dependency.DisplayName);
                lblDependencies.Text = sb.ToString();
            }
            else
            {
                //Make sure all old information is discarded
                lblDependencies.Text = string.Empty;
            }

            if (objSelected.IncompatibilitiesList.Count > 0)
            {
                //We only need a Stringbuilder if we got anything
                StringBuilder sb = new StringBuilder();
                foreach (var exclusivity in objSelected.IncompatibilitiesList)
                    sb.AppendLine(exclusivity.DisplayName);
                lblIncompatibilities.Text = sb.ToString();
            }
            else
            {
                lblIncompatibilities.Text = string.Empty;
            }
            gpbDirectoryInfo.Visible = true;
            gpbDirectoryInfo.ResumeLayout();
        }

        #endregion Control Events

        #region Methods

        private void PopulateSourcebookTreeView()
        {
            // Load the Sourcebook information.
            // Put the Sourcebooks into a List so they can first be sorted.
            object objOldSelected = treSourcebook.SelectedNode?.Tag;
            treSourcebook.BeginUpdate();
            treSourcebook.Nodes.Clear();
            _setPermanentSourcebooks.Clear();
            foreach (XPathNavigator objXmlBook in XmlManager.LoadXPath("books.xml", _objCharacterOptions.EnabledCustomDataDirectoryPaths).Select("/chummer/books/book"))
            {
                if (objXmlBook.SelectSingleNode("hide") != null)
                    continue;
                string strCode = objXmlBook.SelectSingleNode("code")?.Value;
                if (string.IsNullOrEmpty(strCode))
                    continue;
                bool blnChecked = _objCharacterOptions.Books.Contains(strCode);
                if (objXmlBook.SelectSingleNode("permanent") != null)
                {
                    _setPermanentSourcebooks.Add(strCode);
                    _objCharacterOptions.Books.Add(strCode);
                    blnChecked = true;
                }
                TreeNode objNode = new TreeNode
                {
                    Text = objXmlBook.SelectSingleNode("translate")?.Value ?? objXmlBook.SelectSingleNode("name")?.Value ?? string.Empty,
                    Tag = strCode,
                    Checked = blnChecked
                };
                treSourcebook.Nodes.Add(objNode);
            }

            treSourcebook.Sort();
            if (objOldSelected != null)
                treSourcebook.SelectedNode = treSourcebook.FindNodeByTag(objOldSelected);
            treSourcebook.EndUpdate();
        }

        private void PopulateCustomDataDirectoryTreeView()
        {
            object objOldSelected = treCustomDataDirectories.SelectedNode?.Tag;
            treCustomDataDirectories.BeginUpdate();
            if (_dicCharacterCustomDataDirectoryInfos.Count != treCustomDataDirectories.Nodes.Count)
            {
                treCustomDataDirectories.Nodes.Clear();

                foreach (KeyValuePair<object, bool> kvpInfo in _dicCharacterCustomDataDirectoryInfos)
                {
                    TreeNode objNode = new TreeNode
                    {
                        Tag = kvpInfo.Key,
                        Checked = kvpInfo.Value
                    };
                    if (kvpInfo.Key is CustomDataDirectoryInfo objInfo)
                    {
                        objNode.Text = objInfo.DisplayName;
                        if (objNode.Checked)
                        {
                            // check dependencies and exclusivities only if they could exist at all instead of calling and running into empty an foreach.
                            string missingDirectories = string.Empty;
                            if (objInfo.DependenciesList.Count > 0)
                                missingDirectories = objInfo.CheckDependency(_objCharacterOptions);

                            string prohibitedDirectories = string.Empty;
                            if (objInfo.IncompatibilitiesList.Count > 0)
                                prohibitedDirectories = objInfo.CheckIncompatibility(_objCharacterOptions);

                            if (!string.IsNullOrEmpty(missingDirectories) || !string.IsNullOrEmpty(prohibitedDirectories))
                            {
                                objNode.ToolTipText = CustomDataDirectoryInfo.BuildIncompatibilityDependencyString(missingDirectories, prohibitedDirectories);
                                objNode.ForeColor = ColorManager.ErrorColor;
                            }
                        }
                    }
                    else
                    {
                        objNode.Text = kvpInfo.Key.ToString();
                        objNode.ForeColor = ColorManager.GrayText;
                        objNode.ToolTipText = LanguageManager.GetString("MessageTitle_FileNotFound");
                    }
                    treCustomDataDirectories.Nodes.Add(objNode);
                }
            }
            else
            {
                for (int i = 0; i < treCustomDataDirectories.Nodes.Count; ++i)
                {
                    TreeNode objNode = treCustomDataDirectories.Nodes[i];
                    KeyValuePair<object, bool> kvpInfo = _dicCharacterCustomDataDirectoryInfos[i];
                    if (!kvpInfo.Key.Equals(objNode.Tag))
                        objNode.Tag = kvpInfo.Key;
                    if (kvpInfo.Value != objNode.Checked)
                        objNode.Checked = kvpInfo.Value;
                    if (kvpInfo.Key is CustomDataDirectoryInfo objInfo)
                    {
                        objNode.Text = objInfo.DisplayName;
                        if (objNode.Checked)
                        {
                            // check dependencies and exclusivities only if they could exist at all instead of calling and running into empty an foreach.
                            string missingDirectories = string.Empty;
                            if (objInfo.DependenciesList.Count > 0)
                                missingDirectories = objInfo.CheckDependency(_objCharacterOptions);

                            string prohibitedDirectories = string.Empty;
                            if (objInfo.IncompatibilitiesList.Count > 0)
                                prohibitedDirectories = objInfo.CheckIncompatibility(_objCharacterOptions);

                            if (!string.IsNullOrEmpty(missingDirectories) || !string.IsNullOrEmpty(prohibitedDirectories))
                            {
                                objNode.ToolTipText = CustomDataDirectoryInfo.BuildIncompatibilityDependencyString(missingDirectories, prohibitedDirectories);
                                objNode.ForeColor = ColorManager.ErrorColor;
                            }
                            else
                            {
                                objNode.ToolTipText = string.Empty;
                                objNode.ForeColor = ColorManager.WindowText;
                            }
                        }
                        else
                        {
                            objNode.ToolTipText = string.Empty;
                            objNode.ForeColor = ColorManager.WindowText;
                        }
                    }
                    else
                    {
                        objNode.Text = kvpInfo.Key.ToString();
                        objNode.ForeColor = ColorManager.GrayText;
                        objNode.ToolTipText = LanguageManager.GetString("MessageTitle_FileNotFound");
                    }
                }
            }

            if (objOldSelected != null)
                treCustomDataDirectories.SelectedNode = treCustomDataDirectories.FindNodeByTag(objOldSelected);
            treCustomDataDirectories.ShowNodeToolTips = true;
            treCustomDataDirectories.EndUpdate();
        }

        /// <summary>
        /// Set the values for all of the controls based on the Options for the selected Setting.
        /// </summary>
        private void PopulateOptions()
        {
            bool blnDoResumeLayout = !_blnIsLayoutSuspended;
            if (blnDoResumeLayout)
            {
                _blnIsLayoutSuspended = true;
                SuspendLayout();
            }
            PopulateSourcebookTreeView();
            PopulatePriorityTableList();
            PopulateLimbCountList();
            PopulateAllowedGrades();
            PopulateCustomDataDirectoryTreeView();
            if (blnDoResumeLayout)
            {
                _blnIsLayoutSuspended = false;
                ResumeLayout();
            }
        }

        private void PopulatePriorityTableList()
        {
            List<ListItem> lstPriorityTables = new List<ListItem>();

            foreach (XPathNavigator objXmlNode in XmlManager.LoadXPath("priorities.xml", _objCharacterOptions.EnabledCustomDataDirectoryPaths)
                .Select("/chummer/prioritytables/prioritytable"))
            {
                string strName = objXmlNode.Value;
                if (!string.IsNullOrEmpty(strName))
                    lstPriorityTables.Add(new ListItem(objXmlNode.Value, objXmlNode.SelectSingleNode("@translate")?.Value ?? strName));
            }

            string strOldSelected = _objCharacterOptions.PriorityTable;

            bool blnOldLoading = _blnLoading;
            _blnLoading = true;
            cboPriorityTable.BeginUpdate();
            cboPriorityTable.PopulateWithListItems(lstPriorityTables);
            if (!string.IsNullOrEmpty(strOldSelected))
                cboPriorityTable.SelectedValue = strOldSelected;
            if (cboPriorityTable.SelectedIndex == -1 && lstPriorityTables.Count > 0)
                cboPriorityTable.SelectedValue = _objReferenceCharacterOptions.PriorityTable;
            if (cboPriorityTable.SelectedIndex == -1 && lstPriorityTables.Count > 0)
                cboPriorityTable.SelectedIndex = 0;
            cboPriorityTable.EndUpdate();
            _blnLoading = blnOldLoading;
            string strSelectedTable = cboPriorityTable.SelectedValue?.ToString();
            if (!string.IsNullOrWhiteSpace(strSelectedTable) && _objCharacterOptions.PriorityTable != strSelectedTable)
                _objCharacterOptions.PriorityTable = strSelectedTable;
        }

        private void PopulateLimbCountList()
        {
            List<ListItem> lstLimbCount = new List<ListItem>();

            foreach (XPathNavigator objXmlNode in XmlManager.LoadXPath("options.xml", _objCharacterOptions.EnabledCustomDataDirectoryPaths)
                .Select("/chummer/limbcounts/limb"))
            {
                string strExclude = objXmlNode.SelectSingleNode("exclude")?.Value ?? string.Empty;
                if (!string.IsNullOrEmpty(strExclude))
                    strExclude = '<' + strExclude;
                lstLimbCount.Add(new ListItem(objXmlNode.SelectSingleNode("limbcount")?.Value + strExclude, objXmlNode.SelectSingleNode("translate")?.Value ?? objXmlNode.SelectSingleNode("name")?.Value ?? string.Empty));
            }

            string strLimbSlot = _objCharacterOptions.LimbCount.ToString(GlobalOptions.InvariantCultureInfo);
            if (!string.IsNullOrEmpty(_objCharacterOptions.ExcludeLimbSlot))
                strLimbSlot += '<' + _objCharacterOptions.ExcludeLimbSlot;

            _blnSkipLimbCountUpdate = true;
            cboLimbCount.BeginUpdate();
            cboLimbCount.PopulateWithListItems(lstLimbCount);
            if (!string.IsNullOrEmpty(strLimbSlot))
                cboLimbCount.SelectedValue = strLimbSlot;
            if (cboLimbCount.SelectedIndex == -1 && lstLimbCount.Count > 0)
                cboLimbCount.SelectedIndex = 0;

            cboLimbCount.EndUpdate();
            _blnSkipLimbCountUpdate = false;
        }

        private void PopulateAllowedGrades()
        {
            List<ListItem> lstGrades = new List<ListItem>();

            foreach (XPathNavigator objXmlNode in XmlManager.LoadXPath("bioware.xml", _objCharacterOptions.EnabledCustomDataDirectoryPaths)
                .Select("/chummer/grades/grade[not(hide)]"))
            {
                string strName = objXmlNode.SelectSingleNode("name")?.Value;
                if (!string.IsNullOrEmpty(strName) && strName != "None")
                {
                    string strBook = objXmlNode.SelectSingleNode("source")?.Value;
                    if (!string.IsNullOrEmpty(strBook) && treSourcebook.Nodes.Cast<TreeNode>().All(x => x.Tag.ToString() != strBook))
                        continue;
                    if (lstGrades.Any(x => strName.Contains(x.Value.ToString())))
                        continue;
                    ListItem objExistingCoveredGrade = lstGrades.FirstOrDefault(x => x.Value.ToString().Contains(strName));
                    if (objExistingCoveredGrade.Value != null)
                        lstGrades.Remove(objExistingCoveredGrade);
                    lstGrades.Add(new ListItem(strName, objXmlNode.SelectSingleNode("translate")?.Value ?? strName));
                }
            }
            foreach (XPathNavigator objXmlNode in XmlManager.LoadXPath("cyberware.xml", _objCharacterOptions.EnabledCustomDataDirectoryPaths)
                .Select("/chummer/grades/grade[not(hide)]"))
            {
                string strName = objXmlNode.SelectSingleNode("name")?.Value;
                if (!string.IsNullOrEmpty(strName) && strName != "None")
                {
                    string strBook = objXmlNode.SelectSingleNode("source")?.Value;
                    if (!string.IsNullOrEmpty(strBook) && treSourcebook.Nodes.Cast<TreeNode>().All(x => x.Tag.ToString() != strBook))
                        continue;
                    if (lstGrades.Any(x => strName.Contains(x.Value.ToString())))
                        continue;
                    ListItem objExistingCoveredGrade = lstGrades.FirstOrDefault(x => x.Value.ToString().Contains(strName));
                    if (objExistingCoveredGrade.Value != null)
                        lstGrades.Remove(objExistingCoveredGrade);
                    lstGrades.Add(new ListItem(strName, objXmlNode.SelectSingleNode("translate")?.Value ?? strName));
                }
            }

            flpAllowedCyberwareGrades.SuspendLayout();
            flpAllowedCyberwareGrades.Controls.Clear();
            foreach (ListItem objGrade in lstGrades)
            {
                CheckBox chkGrade = new CheckBox
                {
                    UseVisualStyleBackColor = true,
                    Text = objGrade.Name,
                    Tag = objGrade.Value,
                    AutoSize = true,
                    Anchor = AnchorStyles.Left,
                    Checked = !_objCharacterOptions.BannedWareGrades.Contains(objGrade.Value.ToString())
                };
                chkGrade.CheckedChanged += chkGrade_CheckedChanged;
                flpAllowedCyberwareGrades.Controls.Add(chkGrade);
            }
            flpAllowedCyberwareGrades.ResumeLayout();
        }

        private void RebuildCustomDataDirectoryInfos()
        {
            _dicCharacterCustomDataDirectoryInfos.Clear();
            foreach (KeyValuePair<string, bool> kvpCustomDataDirectory in _objCharacterOptions.CustomDataDirectoryKeys)
            {
                CustomDataDirectoryInfo objLoopInfo = GlobalOptions.CustomDataDirectoryInfos.FirstOrDefault(x => x.CharacterOptionsSaveKey == kvpCustomDataDirectory.Key);
                if (objLoopInfo != default)
                {
                    _dicCharacterCustomDataDirectoryInfos.Add(objLoopInfo, kvpCustomDataDirectory.Value);
                }
                else
                {
                    _dicCharacterCustomDataDirectoryInfos.Add(kvpCustomDataDirectory.Key, kvpCustomDataDirectory.Value);
                }
            }
        }

        private void SetToolTips()
        {
            chkUnarmedSkillImprovements.SetToolTip(LanguageManager.GetString("Tip_OptionsUnarmedSkillImprovements").WordWrap());
            chkIgnoreArt.SetToolTip(LanguageManager.GetString("Tip_OptionsIgnoreArt").WordWrap());
            chkIgnoreComplexFormLimit.SetToolTip(LanguageManager.GetString("Tip_OptionsIgnoreComplexFormLimit").WordWrap());
            chkCyberlegMovement.SetToolTip(LanguageManager.GetString("Tip_OptionsCyberlegMovement").WordWrap());
            chkDontDoubleQualityPurchases.SetToolTip(LanguageManager.GetString("Tip_OptionsDontDoubleQualityPurchases").WordWrap());
            chkDontDoubleQualityRefunds.SetToolTip(LanguageManager.GetString("Tip_OptionsDontDoubleQualityRefunds").WordWrap());
            chkStrictSkillGroups.SetToolTip(LanguageManager.GetString("Tip_OptionStrictSkillGroups").WordWrap());
            chkAllowInitiation.SetToolTip(LanguageManager.GetString("Tip_OptionsAllowInitiation").WordWrap());
            chkUseCalculatedPublicAwareness.SetToolTip(LanguageManager.GetString("Tip_PublicAwareness").WordWrap());
        }

        private void SetupDataBindings()
        {
            cmdRename.DoOneWayNegatableDataBinding("Enabled", _objCharacterOptions, nameof(CharacterOptions.BuiltInOption));
            cmdDelete.DoOneWayNegatableDataBinding("Enabled", _objCharacterOptions, nameof(CharacterOptions.BuiltInOption));

            cboBuildMethod.DoDataBinding("SelectedValue", _objCharacterOptions, nameof(CharacterOptions.BuildMethod));
            lblPriorityTable.DoOneWayDataBinding("Visible", _objCharacterOptions, nameof(CharacterOptions.BuildMethodUsesPriorityTables));
            cboPriorityTable.DoOneWayDataBinding("Visible", _objCharacterOptions, nameof(CharacterOptions.BuildMethodUsesPriorityTables));
            lblPriorities.DoOneWayDataBinding("Visible", _objCharacterOptions, nameof(CharacterOptions.BuildMethodIsPriority));
            txtPriorities.DoOneWayDataBinding("Visible", _objCharacterOptions, nameof(CharacterOptions.BuildMethodIsPriority));
            txtPriorities.DoDataBinding("Text", _objCharacterOptions, nameof(CharacterOptions.PriorityArray));
            lblSumToTen.DoOneWayDataBinding("Visible", _objCharacterOptions, nameof(CharacterOptions.BuildMethodIsSumtoTen));
            nudSumToTen.DoOneWayDataBinding("Visible", _objCharacterOptions, nameof(CharacterOptions.BuildMethodIsSumtoTen));
            nudSumToTen.DoDataBinding("Value", _objCharacterOptions, nameof(CharacterOptions.SumtoTen));
            nudStartingKarma.DoDataBinding("Value", _objCharacterOptions, nameof(CharacterOptions.BuildKarma));
            nudMaxNuyenKarma.DoDataBinding("Value", _objCharacterOptions, nameof(CharacterOptions.NuyenMaximumBP));
            nudMaxAvail.DoDataBinding("Value", _objCharacterOptions, nameof(CharacterOptions.MaximumAvailability));
            nudQualityKarmaLimit.DoDataBinding("Value", _objCharacterOptions, nameof(CharacterOptions.QualityKarmaLimit));
            txtContactPoints.DoDataBinding("Text", _objCharacterOptions, nameof(CharacterOptions.ContactPointsExpression));
            txtKnowledgePoints.DoDataBinding("Text", _objCharacterOptions, nameof(CharacterOptions.KnowledgePointsExpression));
            txtNuyenExpression.DoDataBinding("Text", _objCharacterOptions, nameof(CharacterOptions.ChargenKarmaToNuyenExpression));

            chkEnforceCapacity.DoDataBinding("Checked", _objCharacterOptions, nameof(CharacterOptions.EnforceCapacity));
            chkLicenseEachRestrictedItem.DoDataBinding("Checked", _objCharacterOptions, nameof(CharacterOptions.LicenseRestricted));
            chkReverseAttributePriorityOrder.DoDataBinding("Checked", _objCharacterOptions, nameof(CharacterOptions.ReverseAttributePriorityOrder));
            chkDronemods.DoDataBinding("Checked", _objCharacterOptions, nameof(CharacterOptions.DroneMods));
            chkDronemodsMaximumPilot.DoDataBinding("Checked", _objCharacterOptions, nameof(CharacterOptions.DroneModsMaximumPilot));
            chkRestrictRecoil.DoDataBinding("Checked", _objCharacterOptions, nameof(CharacterOptions.RestrictRecoil));
            chkStrictSkillGroups.DoDataBinding("Checked", _objCharacterOptions, nameof(CharacterOptions.StrictSkillGroupsInCreateMode));
            chkAllowPointBuySpecializationsOnKarmaSkills.DoDataBinding("Checked", _objCharacterOptions, nameof(CharacterOptions.AllowPointBuySpecializationsOnKarmaSkills));
            chkAllowFreeGrids.DoDataBinding("Checked", _objCharacterOptions, nameof(CharacterOptions.AllowFreeGrids));
            chkEnemyKarmaQualityLimit.DoDataBinding("Checked", _objCharacterOptions, nameof(CharacterOptions.EnemyKarmaQualityLimit));

            chkDontUseCyberlimbCalculation.DoDataBinding("Checked", _objCharacterOptions, nameof(CharacterOptions.DontUseCyberlimbCalculation));
            chkCyberlegMovement.DoDataBinding("Checked", _objCharacterOptions, nameof(CharacterOptions.CyberlegMovement));
            chkCyberlimbAttributeBonusCap.DoDataBinding("Checked", _objCharacterOptions, nameof(CharacterOptions.CyberlimbAttributeBonusCapOverride));
            nudCyberlimbAttributeBonusCap.DoOneWayDataBinding("Enabled", _objCharacterOptions, nameof(CharacterOptions.CyberlimbAttributeBonusCapOverride));
            nudCyberlimbAttributeBonusCap.DoDataBinding("Value", _objCharacterOptions, nameof(CharacterOptions.CyberlimbAttributeBonusCap));
            chkRedlinerLimbsSkull.DoNegatableDataBinding("Checked", _objCharacterOptions, nameof(CharacterOptions.RedlinerExcludesSkull));
            chkRedlinerLimbsTorso.DoNegatableDataBinding("Checked", _objCharacterOptions, nameof(CharacterOptions.RedlinerExcludesTorso));
            chkRedlinerLimbsArms.DoNegatableDataBinding("Checked", _objCharacterOptions, nameof(CharacterOptions.RedlinerExcludesArms));
            chkRedlinerLimbsLegs.DoNegatableDataBinding("Checked", _objCharacterOptions, nameof(CharacterOptions.RedlinerExcludesLegs));

            nudNuyenDecimalsMaximum.DoDataBinding("Value", _objCharacterOptions, nameof(CharacterOptions.MaxNuyenDecimals));
            nudNuyenDecimalsMinimum.DoDataBinding("Value", _objCharacterOptions, nameof(CharacterOptions.MinNuyenDecimals));
            nudEssenceDecimals.DoDataBinding("Value", _objCharacterOptions, nameof(CharacterOptions.EssenceDecimals));
            chkDontRoundEssenceInternally.DoDataBinding("Checked", _objCharacterOptions, nameof(CharacterOptions.DontRoundEssenceInternally));

            chkMoreLethalGameplay.DoDataBinding("Checked", _objCharacterOptions, nameof(CharacterOptions.MoreLethalGameplay));
            chkNoArmorEncumbrance.DoDataBinding("Checked", _objCharacterOptions, nameof(CharacterOptions.NoArmorEncumbrance));
            chkIgnoreArt.DoDataBinding("Checked", _objCharacterOptions, nameof(CharacterOptions.IgnoreArt));
            chkIgnoreComplexFormLimit.DoDataBinding("Checked", _objCharacterOptions, nameof(CharacterOptions.IgnoreComplexFormLimit));
            chkUnarmedSkillImprovements.DoDataBinding("Checked", _objCharacterOptions, nameof(CharacterOptions.UnarmedImprovementsApplyToWeapons));
            chkMysAdPp.DoDataBinding("Checked", _objCharacterOptions, nameof(CharacterOptions.MysAdeptAllowPpCareer));
            chkMysAdPp.DoOneWayNegatableDataBinding("Enabled", _objCharacterOptions, nameof(CharacterOptions.MysAdeptSecondMAGAttribute));
            chkPrioritySpellsAsAdeptPowers.DoDataBinding("Checked", _objCharacterOptions, nameof(CharacterOptions.PrioritySpellsAsAdeptPowers));
            chkPrioritySpellsAsAdeptPowers.DoOneWayNegatableDataBinding("Enabled", _objCharacterOptions, nameof(CharacterOptions.MysAdeptSecondMAGAttribute));
            chkMysAdeptSecondMAGAttribute.DoDataBinding("Checked", _objCharacterOptions, nameof(CharacterOptions.MysAdeptSecondMAGAttribute));
            chkMysAdeptSecondMAGAttribute.DoOneWayDataBinding("Enabled", _objCharacterOptions, nameof(CharacterOptions.MysAdeptSecondMAGAttributeEnabled));
            chkUsePointsOnBrokenGroups.DoDataBinding("Checked", _objCharacterOptions, nameof(CharacterOptions.UsePointsOnBrokenGroups));
            chkSpecialKarmaCost.DoDataBinding("Checked", _objCharacterOptions, nameof(CharacterOptions.SpecialKarmaCostBasedOnShownValue));
            chkUseCalculatedPublicAwareness.DoDataBinding("Checked", _objCharacterOptions, nameof(CharacterOptions.UseCalculatedPublicAwareness));
            chkAlternateMetatypeAttributeKarma.DoDataBinding("Checked", _objCharacterOptions, nameof(CharacterOptions.AlternateMetatypeAttributeKarma));
            chkCompensateSkillGroupKarmaDifference.DoDataBinding("Checked", _objCharacterOptions, nameof(CharacterOptions.CompensateSkillGroupKarmaDifference));
            chkFreeMartialArtSpecialization.DoDataBinding("Checked", _objCharacterOptions, nameof(CharacterOptions.FreeMartialArtSpecialization));
            chkIncreasedImprovedAbilityModifier.DoDataBinding("Checked", _objCharacterOptions, nameof(CharacterOptions.IncreasedImprovedAbilityMultiplier));
            chkAllowTechnomancerSchooling.DoDataBinding("Checked", _objCharacterOptions, nameof(CharacterOptions.AllowTechnomancerSchooling));
            chkAllowSkillRegrouping.DoDataBinding("Checked", _objCharacterOptions, nameof(CharacterOptions.AllowSkillRegrouping));
            chkDontDoubleQualityPurchases.DoDataBinding("Checked", _objCharacterOptions, nameof(CharacterOptions.DontDoubleQualityPurchases));
            chkDontDoubleQualityRefunds.DoDataBinding("Checked", _objCharacterOptions, nameof(CharacterOptions.DontDoubleQualityRefunds));
            chkDroneArmorMultiplier.DoDataBinding("Checked", _objCharacterOptions, nameof(CharacterOptions.DroneArmorMultiplierEnabled));
            nudDroneArmorMultiplier.DoOneWayDataBinding("Enabled", _objCharacterOptions, nameof(CharacterOptions.DroneArmorMultiplierEnabled));
            nudDroneArmorMultiplier.DoDataBinding("Value", _objCharacterOptions, nameof(CharacterOptions.DroneArmorMultiplier));
            chkESSLossReducesMaximumOnly.DoDataBinding("Checked", _objCharacterOptions, nameof(CharacterOptions.ESSLossReducesMaximumOnly));
            chkExceedNegativeQualities.DoDataBinding("Checked", _objCharacterOptions, nameof(CharacterOptions.ExceedNegativeQualities));
            chkExceedNegativeQualitiesLimit.DoOneWayDataBinding("Enabled", _objCharacterOptions, nameof(CharacterOptions.ExceedNegativeQualities));
            chkExceedNegativeQualitiesLimit.DoDataBinding("Checked", _objCharacterOptions, nameof(CharacterOptions.ExceedNegativeQualitiesLimit));
            chkExceedPositiveQualities.DoDataBinding("Checked", _objCharacterOptions, nameof(CharacterOptions.ExceedPositiveQualities));
            chkExceedPositiveQualitiesCostDoubled.DoOneWayDataBinding("Enabled", _objCharacterOptions, nameof(CharacterOptions.ExceedPositiveQualities));
            chkExceedPositiveQualitiesCostDoubled.DoDataBinding("Checked", _objCharacterOptions, nameof(CharacterOptions.ExceedPositiveQualitiesCostDoubled));
            chkExtendAnyDetectionSpell.DoDataBinding("Checked", _objCharacterOptions, nameof(CharacterOptions.ExtendAnyDetectionSpell));
            chkAllowCyberwareESSDiscounts.DoDataBinding("Checked", _objCharacterOptions, nameof(CharacterOptions.AllowCyberwareESSDiscounts));
            chkAllowInitiation.DoDataBinding("Checked", _objCharacterOptions, nameof(CharacterOptions.AllowInitiationInCreateMode));

            // Karma options.
            nudMetatypeCostsKarmaMultiplier.DoDataBinding("Value", _objCharacterOptions, nameof(CharacterOptions.MetatypeCostsKarmaMultiplier));
            nudKarmaNuyenPerWftM.DoDataBinding("Value", _objCharacterOptions, nameof(CharacterOptions.NuyenPerBPWftM));
            nudKarmaNuyenPerWftP.DoDataBinding("Value", _objCharacterOptions, nameof(CharacterOptions.NuyenPerBPWftP));
            nudKarmaAttribute.DoDataBinding("Value", _objCharacterOptions, nameof(CharacterOptions.KarmaAttribute));
            nudKarmaQuality.DoDataBinding("Value", _objCharacterOptions, nameof(CharacterOptions.KarmaQuality));
            nudKarmaSpecialization.DoDataBinding("Value", _objCharacterOptions, nameof(CharacterOptions.KarmaSpecialization));
            nudKarmaKnowledgeSpecialization.DoDataBinding("Value", _objCharacterOptions, nameof(CharacterOptions.KarmaKnowledgeSpecialization));
            nudKarmaNewKnowledgeSkill.DoDataBinding("Value", _objCharacterOptions, nameof(CharacterOptions.KarmaNewKnowledgeSkill));
            nudKarmaNewActiveSkill.DoDataBinding("Value", _objCharacterOptions, nameof(CharacterOptions.KarmaNewActiveSkill));
            nudKarmaNewSkillGroup.DoDataBinding("Value", _objCharacterOptions, nameof(CharacterOptions.KarmaNewSkillGroup));
            nudKarmaImproveKnowledgeSkill.DoDataBinding("Value", _objCharacterOptions, nameof(CharacterOptions.KarmaImproveKnowledgeSkill));
            nudKarmaImproveActiveSkill.DoDataBinding("Value", _objCharacterOptions, nameof(CharacterOptions.KarmaImproveActiveSkill));
            nudKarmaImproveSkillGroup.DoDataBinding("Value", _objCharacterOptions, nameof(CharacterOptions.KarmaImproveSkillGroup));
            nudKarmaSpell.DoDataBinding("Value", _objCharacterOptions, nameof(CharacterOptions.KarmaSpell));
            nudKarmaNewComplexForm.DoDataBinding("Value", _objCharacterOptions, nameof(CharacterOptions.KarmaNewComplexForm));
            nudKarmaNewAIProgram.DoDataBinding("Value", _objCharacterOptions, nameof(CharacterOptions.KarmaNewAIProgram));
            nudKarmaNewAIAdvancedProgram.DoDataBinding("Value", _objCharacterOptions, nameof(CharacterOptions.KarmaNewAIAdvancedProgram));
            nudKarmaMetamagic.DoDataBinding("Value", _objCharacterOptions, nameof(CharacterOptions.KarmaMetamagic));
            nudKarmaContact.DoDataBinding("Value", _objCharacterOptions, nameof(CharacterOptions.KarmaContact));
            nudKarmaEnemy.DoDataBinding("Value", _objCharacterOptions, nameof(CharacterOptions.KarmaEnemy));
            nudKarmaCarryover.DoDataBinding("Value", _objCharacterOptions, nameof(CharacterOptions.KarmaCarryover));
            nudKarmaSpirit.DoDataBinding("Value", _objCharacterOptions, nameof(CharacterOptions.KarmaSpirit));
            nudKarmaSpiritFettering.DoDataBinding("Value", _objCharacterOptions, nameof(CharacterOptions.KarmaSpiritFettering));
            nudKarmaTechnique.DoDataBinding("Value", _objCharacterOptions, nameof(CharacterOptions.KarmaTechnique));
            nudKarmaInitiation.DoDataBinding("Value", _objCharacterOptions, nameof(CharacterOptions.KarmaInitiation));
            nudKarmaInitiationFlat.DoDataBinding("Value", _objCharacterOptions, nameof(CharacterOptions.KarmaInitiationFlat));
            nudKarmaJoinGroup.DoDataBinding("Value", _objCharacterOptions, nameof(CharacterOptions.KarmaJoinGroup));
            nudKarmaLeaveGroup.DoDataBinding("Value", _objCharacterOptions, nameof(CharacterOptions.KarmaLeaveGroup));
            nudKarmaMysticAdeptPowerPoint.DoDataBinding("Value", _objCharacterOptions, nameof(CharacterOptions.KarmaMysticAdeptPowerPoint));

            // Focus costs
            nudKarmaAlchemicalFocus.DoDataBinding("Value", _objCharacterOptions, nameof(CharacterOptions.KarmaAlchemicalFocus));
            nudKarmaBanishingFocus.DoDataBinding("Value", _objCharacterOptions, nameof(CharacterOptions.KarmaBanishingFocus));
            nudKarmaBindingFocus.DoDataBinding("Value", _objCharacterOptions, nameof(CharacterOptions.KarmaBindingFocus));
            nudKarmaCenteringFocus.DoDataBinding("Value", _objCharacterOptions, nameof(CharacterOptions.KarmaCenteringFocus));
            nudKarmaCounterspellingFocus.DoDataBinding("Value", _objCharacterOptions, nameof(CharacterOptions.KarmaCounterspellingFocus));
            nudKarmaDisenchantingFocus.DoDataBinding("Value", _objCharacterOptions, nameof(CharacterOptions.KarmaDisenchantingFocus));
            nudKarmaFlexibleSignatureFocus.DoDataBinding("Value", _objCharacterOptions, nameof(CharacterOptions.KarmaFlexibleSignatureFocus));
            nudKarmaMaskingFocus.DoDataBinding("Value", _objCharacterOptions, nameof(CharacterOptions.KarmaMaskingFocus));
            nudKarmaPowerFocus.DoDataBinding("Value", _objCharacterOptions, nameof(CharacterOptions.KarmaPowerFocus));
            nudKarmaQiFocus.DoDataBinding("Value", _objCharacterOptions, nameof(CharacterOptions.KarmaQiFocus));
            nudKarmaRitualSpellcastingFocus.DoDataBinding("Value", _objCharacterOptions, nameof(CharacterOptions.KarmaRitualSpellcastingFocus));
            nudKarmaSpellcastingFocus.DoDataBinding("Value", _objCharacterOptions, nameof(CharacterOptions.KarmaSpellcastingFocus));
            nudKarmaSpellShapingFocus.DoDataBinding("Value", _objCharacterOptions, nameof(CharacterOptions.KarmaSpellShapingFocus));
            nudKarmaSummoningFocus.DoDataBinding("Value", _objCharacterOptions, nameof(CharacterOptions.KarmaSummoningFocus));
            nudKarmaSustainingFocus.DoDataBinding("Value", _objCharacterOptions, nameof(CharacterOptions.KarmaSustainingFocus));
            nudKarmaWeaponFocus.DoDataBinding("Value", _objCharacterOptions, nameof(CharacterOptions.KarmaWeaponFocus));

            _objCharacterOptions.PropertyChanged += OptionsChanged;
        }

        private void PopulateSettingsList()
        {
            string strSelect = string.Empty;
            if (!_blnLoading)
                strSelect = cboSetting.SelectedValue?.ToString();
            cboSetting.BeginUpdate();
            _lstSettings.Clear();
            foreach (KeyValuePair<string, CharacterOptions> kvpCharacterOptionsEntry in OptionsManager.LoadedCharacterOptions)
            {
                _lstSettings.Add(new ListItem(kvpCharacterOptionsEntry.Key, kvpCharacterOptionsEntry.Value.DisplayName));
                if (_objReferenceCharacterOptions == kvpCharacterOptionsEntry.Value)
                    strSelect = kvpCharacterOptionsEntry.Key;
            }
            _lstSettings.Sort(CompareListItems.CompareNames);
            cboSetting.PopulateWithListItems(_lstSettings);
            if (!string.IsNullOrEmpty(strSelect))
                cboSetting.SelectedValue = strSelect;
            if (cboSetting.SelectedIndex == -1 && _lstSettings.Count > 0)
                cboSetting.SelectedValue = cboSetting.FindStringExact(GlobalOptions.DefaultCharacterOption);
            if (cboSetting.SelectedIndex == -1 && _lstSettings.Count > 0)
                cboSetting.SelectedIndex = 0;
            cboSetting.EndUpdate();
            _intOldSelectedSettingIndex = cboSetting.SelectedIndex;
        }

        private void OptionsChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!_blnLoading)
            {
                IsDirty = !_objCharacterOptions.Equals(_objReferenceCharacterOptions);
                cmdSaveAs.Enabled = IsDirty && IsAllTextBoxesLegal;
                cmdSave.Enabled = cmdSaveAs.Enabled && !_objCharacterOptions.BuiltInOption;
                switch (e.PropertyName)
                {
                    case nameof(CharacterOptions.EnabledCustomDataDirectoryPaths):
                        PopulateOptions();
                        break;

                    case nameof(CharacterOptions.PriorityTable):
                        PopulatePriorityTableList();
                        break;
                }
            }
            else
            {
                switch (e.PropertyName)
                {
                    case nameof(CharacterOptions.BuiltInOption):
                        cmdSave.Enabled = cmdSaveAs.Enabled
                                          && !_objCharacterOptions.BuiltInOption;
                        break;

                    case nameof(CharacterOptions.PriorityArray):
                    case nameof(CharacterOptions.BuildMethod):
                        cmdSaveAs.Enabled = IsDirty && IsAllTextBoxesLegal;
                        cmdSave.Enabled = cmdSaveAs.Enabled
                                          && !_objCharacterOptions.BuiltInOption;
                        break;
                }
            }
        }

        private bool IsAllTextBoxesLegal
        {
            get
            {
                if (_objCharacterOptions.BuildMethod == CharacterBuildMethod.Priority && _objCharacterOptions.PriorityArray.Length != 5)
                    return false;

                string strContactPointsExpression = _objCharacterOptions.ContactPointsExpression;
                string strKnowledgePointsExpression = _objCharacterOptions.KnowledgePointsExpression;
                string strNuyenExpression = _objCharacterOptions.ChargenKarmaToNuyenExpression.Replace("{Karma}", "0").Replace("{PriorityNuyen}", "0");
                if (string.IsNullOrEmpty(strContactPointsExpression) && string.IsNullOrEmpty(strKnowledgePointsExpression) && string.IsNullOrEmpty(strNuyenExpression))
                    return true;
                foreach (string strCharAttributeName in AttributeSection.AttributeStrings)
                {
                    if (!string.IsNullOrEmpty(strContactPointsExpression))
                        strContactPointsExpression = strContactPointsExpression
                            .Replace('{' + strCharAttributeName + '}', "0")
                            .Replace('{' + strCharAttributeName + "Unaug}", "0")
                            .Replace('{' + strCharAttributeName + "Base}", "0");
                    if (!string.IsNullOrEmpty(strKnowledgePointsExpression))
                        strKnowledgePointsExpression = strKnowledgePointsExpression
                            .Replace('{' + strCharAttributeName + '}', "0")
                            .Replace('{' + strCharAttributeName + "Unaug}", "0")
                            .Replace('{' + strCharAttributeName + "Base}", "0");
                    if (!string.IsNullOrEmpty(strNuyenExpression))
                        strNuyenExpression = strNuyenExpression
                            .Replace('{' + strCharAttributeName + '}', "0")
                            .Replace('{' + strCharAttributeName + "Unaug}", "0")
                            .Replace('{' + strCharAttributeName + "Base}", "0");
                }

                if (!string.IsNullOrEmpty(strContactPointsExpression))
                {
                    CommonFunctions.EvaluateInvariantXPath(strContactPointsExpression, out bool blnSuccess);
                    if (!blnSuccess)
                        return false;
                }
                if (!string.IsNullOrEmpty(strKnowledgePointsExpression))
                {
                    CommonFunctions.EvaluateInvariantXPath(strKnowledgePointsExpression, out bool blnSuccess);
                    if (!blnSuccess)
                        return false;
                }
                if (!string.IsNullOrEmpty(strNuyenExpression))
                {
                    CommonFunctions.EvaluateInvariantXPath(strNuyenExpression, out bool blnSuccess);
                    if (!blnSuccess)
                        return false;
                }

                return true;
            }
        }

        private bool IsDirty
        {
            get => _blnDirty;
            set
            {
                if (_blnDirty != value)
                {
                    _blnDirty = value;
                    cmdOK.Text = LanguageManager.GetString(value ? "String_Cancel" : "String_OK");
                    if (!value)
                    {
                        _blnWasRenamed = false;
                        cmdSaveAs.Enabled = false;
                        cmdSave.Enabled = false;
                    }
                }
            }
        }

        #endregion Methods
    }
}
