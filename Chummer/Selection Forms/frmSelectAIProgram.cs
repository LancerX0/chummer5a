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
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml.XPath;

namespace Chummer
{
    public partial class frmSelectAIProgram : Form
    {
        private string _strSelectedAIProgram = string.Empty;
        private static string _strSelectedCategory = string.Empty;

        private bool _blnLoading = true;
        private bool _blnAddAgain;
        private readonly bool _blnAdvancedProgramAllowed;
        private readonly bool _blnInherentProgram;
        private readonly Character _objCharacter;
        private readonly List<ListItem> _lstCategory = new List<ListItem>();

        private readonly XPathNavigator _xmlBaseChummerNode;
        private readonly XPathNavigator _xmlOptionalAIProgramsNode;

        #region Control Events

        public frmSelectAIProgram(Character objCharacter, bool blnAdvancedProgramAllowed = true, bool blnInherentProgram = false)
        {
            InitializeComponent();
            this.UpdateLightDarkMode();
            this.TranslateWinForm();
            _objCharacter = objCharacter ?? throw new ArgumentNullException(nameof(objCharacter));
            _blnAdvancedProgramAllowed = blnAdvancedProgramAllowed;
            _blnInherentProgram = blnInherentProgram;
            // Load the Programs information.
            _xmlBaseChummerNode = _objCharacter.LoadDataXPath("programs.xml").SelectSingleNodeAndCacheExpression("/chummer");
            if (!_objCharacter.IsCritter) return;
            _xmlOptionalAIProgramsNode = _objCharacter.GetNode().SelectSingleNodeAndCacheExpression("optionalaiprograms");
        }

        private void frmSelectProgram_Load(object sender, EventArgs e)
        {
            // Populate the Category list.
            foreach (XPathNavigator objXmlCategory in _xmlBaseChummerNode.SelectAndCacheExpression("categories/category"))
            {
                string strInnerText = objXmlCategory.Value;
                if (_blnInherentProgram && strInnerText != "Common Programs" && strInnerText != "Hacking Programs")
                    continue;
                if (!_blnAdvancedProgramAllowed && strInnerText == "Advanced Programs")
                    continue;
                // Make sure it is not already in the Category list.
                if (_lstCategory.All(objItem => objItem.Value.ToString() != strInnerText))
                {
                    _lstCategory.Add(new ListItem(strInnerText, objXmlCategory.SelectSingleNodeAndCacheExpression("@translate")?.Value ?? strInnerText));
                }
            }
            _lstCategory.Sort(CompareListItems.CompareNames);

            if (_lstCategory.Count > 0)
            {
                _lstCategory.Insert(0, new ListItem("Show All", LanguageManager.GetString("String_ShowAll")));
            }

            cboCategory.BeginUpdate();
            cboCategory.PopulateWithListItems(_lstCategory);
            if (!string.IsNullOrEmpty(_strSelectedCategory))
                cboCategory.SelectedValue = _strSelectedCategory;
            if (cboCategory.SelectedIndex == -1)
                cboCategory.SelectedIndex = 0;
            cboCategory.EndUpdate();

            _blnLoading = false;

            RefreshList();
        }

        private void cboCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            RefreshList();
        }

        private void chkLimitList_CheckedChanged(object sender, EventArgs e)
        {
            RefreshList();
        }

        private void lstAIPrograms_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateProgramInfo();
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            _blnAddAgain = false;
            AcceptForm();
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void cmdOKAdd_Click(object sender, EventArgs e)
        {
            _blnAddAgain = true;
            AcceptForm();
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            RefreshList();
        }

        private void txtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Down when lstAIPrograms.SelectedIndex + 1 < lstAIPrograms.Items.Count:
                    lstAIPrograms.SelectedIndex += 1;
                    break;

                case Keys.Down:
                    {
                        if (lstAIPrograms.Items.Count > 0)
                        {
                            lstAIPrograms.SelectedIndex = 0;
                        }

                        break;
                    }
                case Keys.Up when lstAIPrograms.SelectedIndex - 1 >= 0:
                    lstAIPrograms.SelectedIndex -= 1;
                    break;

                case Keys.Up:
                    {
                        if (lstAIPrograms.Items.Count > 0)
                        {
                            lstAIPrograms.SelectedIndex = lstAIPrograms.Items.Count - 1;
                        }

                        break;
                    }
            }
        }

        private void txtSearch_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Up)
                txtSearch.Select(txtSearch.Text.Length, 0);
        }

        #endregion Control Events

        #region Properties

        /// <summary>
        /// Whether or not the user wants to add another item after this one.
        /// </summary>
        public bool AddAgain => _blnAddAgain;

        /// <summary>
        /// Program that was selected in the dialogue.
        /// </summary>
        public string SelectedProgram
        {
            get => _strSelectedAIProgram;
            set => _strSelectedAIProgram = value;
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Update the Program's information based on the Program selected.
        /// </summary>
        private void UpdateProgramInfo()
        {
            if (_blnLoading)
                return;

            SuspendLayout();
            tlpRight.Visible = false;
            string strSelectedId = lstAIPrograms.SelectedValue?.ToString();
            if (!string.IsNullOrEmpty(strSelectedId))
            {
                // Retrieve the information for the selected piece of Cyberware.
                XPathNavigator objXmlProgram = _xmlBaseChummerNode.SelectSingleNode("programs/program[id = " + strSelectedId.CleanXPath() + "]");
                if (objXmlProgram != null)
                {
                    string strRequiresProgram = objXmlProgram.SelectSingleNode("require")?.Value;
                    if (string.IsNullOrEmpty(strRequiresProgram))
                    {
                        strRequiresProgram = LanguageManager.GetString("String_None");
                    }
                    else
                    {
                        strRequiresProgram = _xmlBaseChummerNode.SelectSingleNode("/chummer/programs/program[name = " + strRequiresProgram.CleanXPath() + "]/translate")?.Value ?? strRequiresProgram;
                    }

                    lblRequiresProgram.Text = strRequiresProgram;

                    string strSource = objXmlProgram.SelectSingleNode("source")?.Value;
                    if (!string.IsNullOrEmpty(strSource))
                    {
                        string strPage = objXmlProgram.SelectSingleNode("altpage")?.Value ?? objXmlProgram.SelectSingleNode("page")?.Value;
                        if (!string.IsNullOrEmpty(strPage))
                        {
                            SourceString objSource = new SourceString(strSource, strPage, GlobalSettings.Language, GlobalSettings.CultureInfo, _objCharacter);
                            lblSource.Text = objSource.ToString();
                            lblSource.SetToolTip(objSource.LanguageBookTooltip);
                        }
                        else
                        {
                            string strUnknown = LanguageManager.GetString("String_Unknown");
                            lblSource.Text = strUnknown;
                            lblSource.SetToolTip(strUnknown);
                        }
                    }
                    else
                    {
                        string strUnknown = LanguageManager.GetString("String_Unknown");
                        lblSource.Text = strUnknown;
                        lblSource.SetToolTip(strUnknown);
                    }
                    tlpRight.Visible = true;
                }
            }

            lblRequiresProgramLabel.Visible = !string.IsNullOrEmpty(lblRequiresProgram.Text);
            lblSourceLabel.Visible = !string.IsNullOrEmpty(lblSource.Text);
        }

        /// <summary>
        /// Refreshes the displayed lists
        /// </summary>
        private void RefreshList()
        {
            if (_blnLoading)
                return;

            StringBuilder sbdFilter = new StringBuilder('(' + _objCharacter.Settings.BookXPath() + ')');

            string strCategory = cboCategory.SelectedValue?.ToString();
            if (!string.IsNullOrEmpty(strCategory) && strCategory != "Show All" && (GlobalSettings.SearchInCategoryOnly || txtSearch.TextLength == 0))
                sbdFilter.Append(" and category = " + strCategory.CleanXPath());
            else
            {
                StringBuilder sbdCategoryFilter = new StringBuilder();
                foreach (string strItem in _lstCategory.Select(x => x.Value))
                {
                    if (!string.IsNullOrEmpty(strItem))
                        sbdCategoryFilter.Append("category = " + strItem.CleanXPath() + " or ");
                }
                if (sbdCategoryFilter.Length > 0)
                {
                    sbdCategoryFilter.Length -= 4;
                    sbdFilter.Append(" and (" + sbdCategoryFilter + ')');
                }
            }
            if (!string.IsNullOrEmpty(txtSearch.Text))
                sbdFilter.Append(" and " + CommonFunctions.GenerateSearchXPath(txtSearch.Text));

            // Populate the Program list.
            UpdateProgramList(_xmlBaseChummerNode.Select("programs/program[" + sbdFilter + ']'));
        }

        /// <summary>
        /// Update the Program List based on a base program node list.
        /// </summary>
        private void UpdateProgramList(XPathNodeIterator objXmlNodeList)
        {
            string strSpace = LanguageManager.GetString("String_Space");
            List<ListItem> lstPrograms = new List<ListItem>();
            foreach (XPathNavigator objXmlProgram in objXmlNodeList)
            {
                string strId = objXmlProgram.SelectSingleNode("id")?.Value;
                if (string.IsNullOrEmpty(strId))
                    continue;

                if (chkLimitList.Checked)
                {
                    string strRequire = objXmlProgram.SelectSingleNode("require")?.Value;
                    if (!string.IsNullOrEmpty(strRequire))
                    {
                        bool blnAdd = false;
                        foreach (AIProgram objAIProgram in _objCharacter.AIPrograms)
                        {
                            if (objAIProgram.Name == strRequire)
                            {
                                blnAdd = true;
                                break;
                            }
                        }
                        if (!blnAdd)
                            continue;
                    }
                }

                string strName = objXmlProgram.SelectSingleNode("name")?.Value ?? LanguageManager.GetString("String_Unknown");
                // If this is a critter with Optional Programs, see if this Program is allowed.
                if (_xmlOptionalAIProgramsNode?.SelectSingleNode("program") != null && _xmlOptionalAIProgramsNode.SelectSingleNode("program[. = " + strName.CleanXPath() + "]") == null)
                    continue;
                string strDisplayName = objXmlProgram.SelectSingleNode("translate")?.Value ?? strName;
                if (!GlobalSettings.SearchInCategoryOnly && txtSearch.TextLength != 0)
                {
                    string strCategory = objXmlProgram.SelectSingleNode("category")?.Value;
                    if (!string.IsNullOrEmpty(strCategory))
                    {
                        ListItem objFoundItem = _lstCategory.Find(objFind => objFind.Value.ToString() == strCategory);
                        if (!string.IsNullOrEmpty(objFoundItem.Name))
                        {
                            strDisplayName += strSpace + '[' + objFoundItem.Name + ']';
                        }
                    }
                }
                lstPrograms.Add(new ListItem(strId, strDisplayName));
            }

            lstPrograms.Sort(CompareListItems.CompareNames);
            lstAIPrograms.BeginUpdate();
            lstAIPrograms.PopulateWithListItems(lstPrograms);
            lstAIPrograms.EndUpdate();
        }

        /// <summary>
        /// Accept the selected item and close the form.
        /// </summary>
        private void AcceptForm()
        {
            string strSelectedId = lstAIPrograms.SelectedValue?.ToString();
            if (!string.IsNullOrEmpty(strSelectedId))
            {
                XPathNavigator xmlProgram = _xmlBaseChummerNode.SelectSingleNode("programs/program[id = " + strSelectedId.CleanXPath() + "]");
                if (xmlProgram == null)
                    return;

                // Check to make sure requirement is met
                if (!xmlProgram.RequirementsMet(_objCharacter, null, LanguageManager.GetString("String_Program")))
                {
                    return;
                }

                _strSelectedAIProgram = strSelectedId;
                _strSelectedCategory = (GlobalSettings.SearchInCategoryOnly || txtSearch.TextLength == 0) ? cboCategory.SelectedValue?.ToString() : xmlProgram.SelectSingleNode("category")?.Value;

                DialogResult = DialogResult.OK;
            }
        }

        private void OpenSourceFromLabel(object sender, EventArgs e)
        {
            CommonFunctions.OpenPdfFromControl(sender, e);
        }

        #endregion Methods
    }
}
