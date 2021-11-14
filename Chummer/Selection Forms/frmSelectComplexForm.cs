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
using System.Windows.Forms;
using System.Xml.XPath;

namespace Chummer
{
    public partial class frmSelectComplexForm : Form
    {
        private string _strSelectedComplexForm = string.Empty;

        private bool _blnLoading = true;
        private bool _blnAddAgain;
        private readonly Character _objCharacter;

        private readonly XPathNavigator _xmlBaseComplexFormsNode;
        private readonly XPathNavigator _xmlOptionalComplexFormNode;

        //private bool _blnBiowireEnabled = false;

        #region Control Events

        public frmSelectComplexForm(Character objCharacter)
        {
            InitializeComponent();
            this.UpdateLightDarkMode();
            this.TranslateWinForm();
            _objCharacter = objCharacter ?? throw new ArgumentNullException(nameof(objCharacter));
            // Load the Complex Form information.
            _xmlBaseComplexFormsNode = _objCharacter.LoadDataXPath("complexforms.xml").SelectSingleNode("/chummer/complexforms");

            _xmlOptionalComplexFormNode = _objCharacter.GetNode();
            if (_xmlOptionalComplexFormNode == null) return;
            if (_objCharacter.MetavariantGuid != Guid.Empty)
            {
                XPathNavigator xmlMetavariantNode = _xmlOptionalComplexFormNode.SelectSingleNode("metavariants/metavariant[id = "
                                                                                                 + _objCharacter.MetavariantGuid.ToString("D", GlobalSettings.InvariantCultureInfo).CleanXPath()
                                                                                                 + "]");
                if (xmlMetavariantNode != null)
                    _xmlOptionalComplexFormNode = xmlMetavariantNode;
            }

            _xmlOptionalComplexFormNode = _xmlOptionalComplexFormNode.SelectSingleNode("optionalcomplexforms");
        }

        private void frmSelectComplexForm_Load(object sender, EventArgs e)
        {
            _blnLoading = false;
            BuildComplexFormList();
        }

        private void lstComplexForms_SelectedIndexChanged(object sender, EventArgs e)
        {
            string strSelectedComplexFormId = lstComplexForms.SelectedValue?.ToString();
            if (_blnLoading || string.IsNullOrEmpty(strSelectedComplexFormId))
            {
                tlpRight.Visible = false;
                return;
            }

            // Display the Complex Form information.
            XPathNavigator xmlComplexForm = _xmlBaseComplexFormsNode.SelectSingleNode("complexform[id = " + strSelectedComplexFormId.CleanXPath() + "]");
            if (xmlComplexForm == null)
            {
                tlpRight.Visible = false;
                return;
            }

            SuspendLayout();
            switch (xmlComplexForm.SelectSingleNode("duration")?.Value)
            {
                case "P":
                    lblDuration.Text = LanguageManager.GetString("String_SpellDurationPermanent");
                    break;

                case "S":
                    lblDuration.Text = LanguageManager.GetString("String_SpellDurationSustained");
                    break;

                case "Special":
                    lblDuration.Text = LanguageManager.GetString("String_SpellDurationSpecial");
                    break;

                default:
                    lblDuration.Text = LanguageManager.GetString("String_SpellDurationInstant");
                    break;
            }

            switch (xmlComplexForm.SelectSingleNode("target")?.Value)
            {
                case "Persona":
                    lblTarget.Text = LanguageManager.GetString("String_ComplexFormTargetPersona");
                    break;

                case "Device":
                    lblTarget.Text = LanguageManager.GetString("String_ComplexFormTargetDevice");
                    break;

                case "File":
                    lblTarget.Text = LanguageManager.GetString("String_ComplexFormTargetFile");
                    break;

                case "Self":
                    lblTarget.Text = LanguageManager.GetString("String_SpellRangeSelf");
                    break;

                case "Sprite":
                    lblTarget.Text = LanguageManager.GetString("String_ComplexFormTargetSprite");
                    break;

                case "Host":
                    lblTarget.Text = LanguageManager.GetString("String_ComplexFormTargetHost");
                    break;

                case "IC":
                    lblTarget.Text = LanguageManager.GetString("String_ComplexFormTargetIC");
                    break;

                default:
                    lblTarget.Text = LanguageManager.GetString("String_None");
                    break;
            }

            string strFv = xmlComplexForm.SelectSingleNode("fv")?.Value.Replace('/', '÷') ?? string.Empty;
            if (!GlobalSettings.Language.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
            {
                strFv = strFv.CheapReplace("L", () => LanguageManager.GetString("String_ComplexFormLevel"))
                    .CheapReplace("Overflow damage", () => LanguageManager.GetString("String_SpellOverflowDamage"))
                    .CheapReplace("Damage Value", () => LanguageManager.GetString("String_SpellDamageValue"))
                    .CheapReplace("Toxin DV", () => LanguageManager.GetString("String_SpellToxinDV"))
                    .CheapReplace("Disease DV", () => LanguageManager.GetString("String_SpellDiseaseDV"))
                    .CheapReplace("Radiation Power", () => LanguageManager.GetString("String_SpellRadiationPower"));
            }

            lblFV.Text = strFv;

            string strSource = xmlComplexForm.SelectSingleNode("source")?.Value ??
                               LanguageManager.GetString("String_Unknown");
            string strPage = xmlComplexForm.SelectSingleNode("altpage")?.Value ??
                             xmlComplexForm.SelectSingleNode("page")?.Value ??
                             LanguageManager.GetString("String_Unknown");
            SourceString objSource = new SourceString(strSource, strPage, GlobalSettings.Language,
                GlobalSettings.CultureInfo, _objCharacter);
            lblSource.Text = objSource.ToString();
            lblSource.SetToolTip(objSource.LanguageBookTooltip);
            lblDurationLabel.Visible = !string.IsNullOrEmpty(lblDuration.Text);
            lblSourceLabel.Visible = !string.IsNullOrEmpty(lblSource.Text);
            lblFVLabel.Visible = !string.IsNullOrEmpty(lblFV.Text);
            lblSourceLabel.Visible = !string.IsNullOrEmpty(lblSource.Text);
            tlpRight.Visible = true;
            ResumeLayout();
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
            BuildComplexFormList();
        }

        private void txtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (lstComplexForms.SelectedIndex == -1 && lstComplexForms.Items.Count > 0)
            {
                lstComplexForms.SelectedIndex = 0;
            }
            switch (e.KeyCode)
            {
                case Keys.Down:
                    {
                        int intNewIndex = lstComplexForms.SelectedIndex + 1;
                        if (intNewIndex >= lstComplexForms.Items.Count)
                            intNewIndex = 0;
                        if (lstComplexForms.Items.Count > 0)
                            lstComplexForms.SelectedIndex = intNewIndex;
                        break;
                    }
                case Keys.Up:
                    {
                        int intNewIndex = lstComplexForms.SelectedIndex - 1;
                        if (intNewIndex <= 0)
                            intNewIndex = lstComplexForms.Items.Count - 1;
                        if (lstComplexForms.Items.Count > 0)
                            lstComplexForms.SelectedIndex = intNewIndex;
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
        /// Complex Form that was selected in the dialogue.
        /// </summary>
        public string SelectedComplexForm => _strSelectedComplexForm;

        #endregion Properties

        #region Methods

        private void BuildComplexFormList()
        {
            if (_blnLoading)
                return;

            string strFilter = '(' + _objCharacter.Settings.BookXPath() + ')';
            if (!string.IsNullOrEmpty(txtSearch.Text))
                strFilter += " and " + CommonFunctions.GenerateSearchXPath(txtSearch.Text);
            // Populate the Complex Form list.
            List<ListItem> lstComplexFormItems = new List<ListItem>();
            foreach (XPathNavigator xmlComplexForm in _xmlBaseComplexFormsNode.Select("complexform[" + strFilter + ']'))
            {
                string strId = xmlComplexForm.SelectSingleNodeAndCacheExpression("id")?.Value;
                if (string.IsNullOrEmpty(strId))
                    continue;

                if (!xmlComplexForm.RequirementsMet(_objCharacter))
                    continue;

                string strName = xmlComplexForm.SelectSingleNodeAndCacheExpression("name")?.Value ?? LanguageManager.GetString("String_Unknown");
                // If this is a Sprite with Optional Complex Forms, see if this Complex Form is allowed.
                if (_xmlOptionalComplexFormNode?.SelectSingleNodeAndCacheExpression("complexform") != null && _xmlOptionalComplexFormNode.SelectSingleNodeAndCacheExpression("complexform[. = " + strName.CleanXPath() + "]") == null)
                    continue;

                lstComplexFormItems.Add(new ListItem(strId, xmlComplexForm.SelectSingleNode("translate")?.Value ?? strName));
            }

            lstComplexFormItems.Sort(CompareListItems.CompareNames);
            _blnLoading = true;
            string strOldSelected = lstComplexForms.SelectedValue?.ToString();
            lstComplexForms.BeginUpdate();
            lstComplexForms.PopulateWithListItems(lstComplexFormItems);
            _blnLoading = false;
            if (!string.IsNullOrEmpty(strOldSelected))
                lstComplexForms.SelectedValue = strOldSelected;
            else
                lstComplexForms.SelectedIndex = -1;
            lstComplexForms.EndUpdate();
        }

        /// <summary>
        /// Accept the selected item and close the form.
        /// </summary>
        private void AcceptForm()
        {
            string strSelectedItem = lstComplexForms.SelectedValue?.ToString();
            if (!string.IsNullOrEmpty(strSelectedItem))
            {
                _strSelectedComplexForm = strSelectedItem;
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
