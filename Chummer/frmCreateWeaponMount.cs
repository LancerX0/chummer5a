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
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using Chummer.Backend.Equipment;

namespace Chummer
{
    public partial class frmCreateWeaponMount : Form
    {
        private readonly List<VehicleMod> _lstMods = new List<VehicleMod>(1);
        private bool _blnLoading = true;
        private readonly Vehicle _objVehicle;
        private readonly Character _objCharacter;
        private WeaponMount _objMount;
        private readonly XmlDocument _xmlDoc;
        private readonly HashSet<string> _setBlackMarketMaps;

        public WeaponMount WeaponMount => _objMount;

        public frmCreateWeaponMount(Vehicle objVehicle, Character objCharacter, WeaponMount objWeaponMount = null)
        {
            _objVehicle = objVehicle;
            _objMount = objWeaponMount;
            _objCharacter = objCharacter;
            _xmlDoc = _objCharacter.LoadData("vehicles.xml");
            _setBlackMarketMaps = _objCharacter.GenerateBlackMarketMappings(_objCharacter.LoadDataXPath("vehicles.xml").SelectSingleNode("/chummer/weaponmountcategories"));
            InitializeComponent();
        }

        private void frmCreateWeaponMount_Load(object sender, EventArgs e)
        {
            XmlNode xmlVehicleNode = _objVehicle.GetNode();
            List<ListItem> lstSize;
            // Populate the Weapon Mount Category list.
            string strSizeFilter = "category = \"Size\" and " + _objCharacter.Settings.BookXPath();
            if (!_objVehicle.IsDrone && _objCharacter.Settings.DroneMods)
                strSizeFilter += " and not(optionaldrone)";
            using (XmlNodeList xmlSizeNodeList = _xmlDoc.SelectNodes("/chummer/weaponmounts/weaponmount[" + strSizeFilter + "]"))
            {
                lstSize = new List<ListItem>(xmlSizeNodeList?.Count ?? 0);
                if (xmlSizeNodeList?.Count > 0)
                {
                    foreach (XmlNode xmlSizeNode in xmlSizeNodeList)
                    {
                        string strId = xmlSizeNode["id"]?.InnerText;
                        if (string.IsNullOrEmpty(strId))
                            continue;

                        XmlNode xmlTestNode = xmlSizeNode.SelectSingleNode("forbidden/vehicledetails");
                        if (xmlTestNode != null && xmlVehicleNode.ProcessFilterOperationNode(xmlTestNode, false))
                        {
                            // Assumes topmost parent is an AND node
                            continue;
                        }

                        xmlTestNode = xmlSizeNode.SelectSingleNode("required/vehicledetails");
                        if (xmlTestNode != null && !xmlVehicleNode.ProcessFilterOperationNode(xmlTestNode, false))
                        {
                            // Assumes topmost parent is an AND node
                            continue;
                        }

                        lstSize.Add(new ListItem(strId, xmlSizeNode["translate"]?.InnerText ?? xmlSizeNode["name"]?.InnerText ?? LanguageManager.GetString("String_Unknown")));
                    }
                }
            }

            cboSize.BeginUpdate();
            cboSize.PopulateWithListItems(lstSize);
            cboSize.Enabled = lstSize.Count > 1;
            cboSize.EndUpdate();

            if (_objMount != null)
            {
                TreeNode objModsParentNode = new TreeNode
                {
                    Tag = "Node_AdditionalMods",
                    Text = LanguageManager.GetString("Node_AdditionalMods")
                };
                treMods.Nodes.Add(objModsParentNode);
                objModsParentNode.Expand();
                foreach (VehicleMod objMod in _objMount.Mods)
                {
                    TreeNode objLoopNode = objMod.CreateTreeNode(null, null, null, null, null, null);
                    if (objLoopNode != null)
                        objModsParentNode.Nodes.Add(objLoopNode);
                }
                _lstMods.AddRange(_objMount.Mods);

                cboSize.SelectedValue = _objMount.SourceIDString;
            }
            if (cboSize.SelectedIndex == -1)
            {
                if (lstSize.Count > 0)
                    cboSize.SelectedIndex = 0;
            }
            else
                RefreshComboBoxes();

            nudMarkup.Visible = AllowDiscounts;
            lblMarkupLabel.Visible = AllowDiscounts;
            lblMarkupPercentLabel.Visible = AllowDiscounts;

            if (_objMount != null)
            {
                List<ListItem> lstVisibility = cboVisibility.Items.Cast<ListItem>().ToList();
                List<ListItem> lstFlexibility = cboFlexibility.Items.Cast<ListItem>().ToList();
                List<ListItem> lstControl = cboControl.Items.Cast<ListItem>().ToList();
                foreach (WeaponMountOption objExistingOption in _objMount.WeaponMountOptions)
                {
                    string strLoopId = objExistingOption.SourceIDString;
                    if (lstVisibility.Any(x => x.Value.ToString() == strLoopId))
                        cboVisibility.SelectedValue = strLoopId;
                    else if (lstFlexibility.Any(x => x.Value.ToString() == strLoopId))
                        cboFlexibility.SelectedValue = strLoopId;
                    else if (lstControl.Any(x => x.Value.ToString() == strLoopId))
                        cboControl.SelectedValue = strLoopId;
                }
            }

            chkBlackMarketDiscount.Visible = _objCharacter.BlackMarketDiscount;

            _blnLoading = false;
            UpdateInfo();
            this.UpdateLightDarkMode();
            this.TranslateWinForm();
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            //TODO: THIS IS UGLY AS SHIT, FIX BETTER

            string strSelectedMount = cboSize.SelectedValue?.ToString();
            if (string.IsNullOrEmpty(strSelectedMount))
                return;
            string strSelectedControl = cboControl.SelectedValue?.ToString();
            if (string.IsNullOrEmpty(strSelectedControl))
                return;
            string strSelectedFlexibility = cboFlexibility.SelectedValue?.ToString();
            if (string.IsNullOrEmpty(strSelectedFlexibility))
                return;
            string strSelectedVisibility = cboVisibility.SelectedValue?.ToString();
            if (string.IsNullOrEmpty(strSelectedVisibility))
                return;

            XmlNode xmlSelectedMount = _xmlDoc.SelectSingleNode("/chummer/weaponmounts/weaponmount[id = " + strSelectedMount.CleanXPath() + "]");
            if (xmlSelectedMount == null)
                return;
            XmlNode xmlSelectedControl = _xmlDoc.SelectSingleNode("/chummer/weaponmounts/weaponmount[id = " + strSelectedControl.CleanXPath() + "]");
            if (xmlSelectedControl == null)
                return;
            XmlNode xmlSelectedFlexibility = _xmlDoc.SelectSingleNode("/chummer/weaponmounts/weaponmount[id = " + strSelectedFlexibility.CleanXPath() + "]");
            if (xmlSelectedFlexibility == null)
                return;
            XmlNode xmlSelectedVisibility = _xmlDoc.SelectSingleNode("/chummer/weaponmounts/weaponmount[id = " + strSelectedVisibility.CleanXPath() + "]");
            if (xmlSelectedVisibility == null)
                return;

            XmlNode xmlForbiddenNode = xmlSelectedMount["forbidden"];
            if (xmlForbiddenNode != null)
            {
                string strStringToCheck = xmlSelectedControl["name"]?.InnerText;
                if (!string.IsNullOrEmpty(strStringToCheck))
                    using (XmlNodeList xmlControlNodeList = xmlForbiddenNode.SelectNodes("control"))
                        if (xmlControlNodeList?.Count > 0)
                            foreach (XmlNode xmlLoopNode in xmlControlNodeList)
                                if (xmlLoopNode.InnerText == strStringToCheck)
                                    return;

                strStringToCheck = xmlSelectedFlexibility["name"]?.InnerText;
                if (!string.IsNullOrEmpty(strStringToCheck))
                {
                    using (XmlNodeList xmlFlexibilityNodeList = xmlForbiddenNode.SelectNodes("flexibility"))
                        if (xmlFlexibilityNodeList?.Count > 0)
                            foreach (XmlNode xmlLoopNode in xmlFlexibilityNodeList)
                                if (xmlLoopNode.InnerText == strStringToCheck)
                                    return;
                }

                strStringToCheck = xmlSelectedVisibility["name"]?.InnerText;
                if (!string.IsNullOrEmpty(strStringToCheck))
                {
                    using (XmlNodeList xmlVisibilityNodeList = xmlForbiddenNode.SelectNodes("visibility"))
                        if (xmlVisibilityNodeList?.Count > 0)
                            foreach (XmlNode xmlLoopNode in xmlVisibilityNodeList)
                                if (xmlLoopNode.InnerText == strStringToCheck)
                                    return;
                }
            }
            XmlNode xmlRequiredNode = xmlSelectedMount["required"];
            if (xmlRequiredNode != null)
            {
                bool blnRequirementsMet = true;
                string strStringToCheck = xmlSelectedControl["name"]?.InnerText;
                if (!string.IsNullOrEmpty(strStringToCheck))
                {
                    using (XmlNodeList xmlControlNodeList = xmlRequiredNode.SelectNodes("control"))
                    {
                        if (xmlControlNodeList?.Count > 0)
                        {
                            foreach (XmlNode xmlLoopNode in xmlControlNodeList)
                            {
                                blnRequirementsMet = false;
                                if (xmlLoopNode.InnerText == strStringToCheck)
                                {
                                    blnRequirementsMet = true;
                                    break;
                                }
                            }
                        }
                    }
                }
                if (!blnRequirementsMet)
                    return;

                strStringToCheck = xmlSelectedFlexibility["name"]?.InnerText;
                if (!string.IsNullOrEmpty(strStringToCheck))
                {
                    using (XmlNodeList xmlFlexibilityNodeList = xmlRequiredNode.SelectNodes("flexibility"))
                    {
                        if (xmlFlexibilityNodeList?.Count > 0)
                        {
                            foreach (XmlNode xmlLoopNode in xmlFlexibilityNodeList)
                            {
                                blnRequirementsMet = false;
                                if (xmlLoopNode.InnerText == strStringToCheck)
                                {
                                    blnRequirementsMet = true;
                                    break;
                                }
                            }
                        }
                    }
                }
                if (!blnRequirementsMet)
                    return;

                strStringToCheck = xmlSelectedVisibility["name"]?.InnerText;
                if (!string.IsNullOrEmpty(strStringToCheck))
                {
                    using (XmlNodeList xmlVisibilityNodeList = xmlRequiredNode.SelectNodes("visibility"))
                    {
                        if (xmlVisibilityNodeList?.Count > 0)
                        {
                            foreach (XmlNode xmlLoopNode in xmlVisibilityNodeList)
                            {
                                blnRequirementsMet = false;
                                if (xmlLoopNode.InnerText == strStringToCheck)
                                {
                                    blnRequirementsMet = true;
                                    break;
                                }
                            }
                        }
                    }
                }
                if (!blnRequirementsMet)
                    return;
            }
            if (_objMount == null)
            {
                _objMount = new WeaponMount(_objCharacter, _objVehicle);
                _objMount.Create(xmlSelectedMount);
            }
            else if (_objMount.SourceIDString != strSelectedMount)
            {
                _objMount.Create(xmlSelectedMount);
            }

            _objMount.DiscountCost = chkBlackMarketDiscount.Checked;

            WeaponMountOption objControlOption = new WeaponMountOption(_objCharacter);
            if (objControlOption.Create(xmlSelectedControl))
            {
                _objMount.WeaponMountOptions.RemoveAll(x => x.Category == "Control");
                _objMount.WeaponMountOptions.Add(objControlOption);
            }
            WeaponMountOption objFlexibilityOption = new WeaponMountOption(_objCharacter);
            if (objFlexibilityOption.Create(xmlSelectedFlexibility))
            {
                _objMount.WeaponMountOptions.RemoveAll(x => x.Category == "Flexibility");
                _objMount.WeaponMountOptions.Add(objFlexibilityOption);
            }
            WeaponMountOption objVisibilityOption = new WeaponMountOption(_objCharacter);
            if (objVisibilityOption.Create(xmlSelectedVisibility))
            {
                _objMount.WeaponMountOptions.RemoveAll(x => x.Category == "Visibilty");
                _objMount.WeaponMountOptions.Add(objVisibilityOption);
            }

            _objMount.Mods.RemoveAll(x => !_lstMods.Contains(x));
            foreach (VehicleMod objMod in _lstMods)
            {
                if (_objMount.Mods.Contains(objMod))
                    continue;
                objMod.WeaponMountParent = _objMount;
                _objMount.Mods.Add(objMod);
            }

            DialogResult = DialogResult.OK;
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void cboSize_SelectedIndexChanged(object sender, EventArgs e)
        {
            RefreshComboBoxes();
            treMods.SelectedNode = null;
            UpdateInfo();
        }

        private void comboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            treMods.SelectedNode = null;
            UpdateInfo();
        }

        private void chkFreeItem_CheckedChanged(object sender, EventArgs e)
        {
            UpdateInfo();
        }

        private void chkBlackMarketDiscount_CheckedChanged(object sender, EventArgs e)
        {
            UpdateInfo();
        }

        public bool FreeCost => chkFreeItem.Checked;

        public decimal Markup => nudMarkup.Value;

        public bool AllowDiscounts { get; set; }

        private void nudMarkup_ValueChanged(object sender, EventArgs e)
        {
            UpdateInfo();
        }

        private void UpdateInfo()
        {
            if (_blnLoading)
                return;

            XmlNode xmlSelectedMount = null;
            string strSelectedMount = cboSize.SelectedValue?.ToString();
            if (string.IsNullOrEmpty(strSelectedMount))
                cmdOK.Enabled = false;
            else
            {
                xmlSelectedMount = _xmlDoc.SelectSingleNode("/chummer/weaponmounts/weaponmount[id = " + strSelectedMount.CleanXPath() + "]");
                if (xmlSelectedMount == null)
                    cmdOK.Enabled = false;
                else
                {
                    string strSelectedControl = cboControl.SelectedValue?.ToString();
                    if (string.IsNullOrEmpty(strSelectedControl))
                        cmdOK.Enabled = false;
                    else if (_xmlDoc.SelectSingleNode("/chummer/weaponmounts/weaponmount[id = " + strSelectedControl.CleanXPath() + "]") == null)
                        cmdOK.Enabled = false;
                    else
                    {
                        string strSelectedFlexibility = cboFlexibility.SelectedValue?.ToString();
                        if (string.IsNullOrEmpty(strSelectedFlexibility))
                            cmdOK.Enabled = false;
                        else if (_xmlDoc.SelectSingleNode("/chummer/weaponmounts/weaponmount[id = " + strSelectedFlexibility.CleanXPath() + "]") == null)
                            cmdOK.Enabled = false;
                        else
                        {
                            string strSelectedVisibility = cboVisibility.SelectedValue?.ToString();
                            if (string.IsNullOrEmpty(strSelectedVisibility))
                                cmdOK.Enabled = false;
                            else if (_xmlDoc.SelectSingleNode("/chummer/weaponmounts/weaponmount[id = " + strSelectedVisibility.CleanXPath() + "]") == null)
                                cmdOK.Enabled = false;
                            else
                                cmdOK.Enabled = true;
                        }
                    }
                }
            }

            string[] astrSelectedValues = { cboVisibility.SelectedValue?.ToString(), cboFlexibility.SelectedValue?.ToString(), cboControl.SelectedValue?.ToString() };

            cmdDeleteMod.Enabled = false;
            string strSelectedModId = treMods.SelectedNode?.Tag.ToString();
            if (!string.IsNullOrEmpty(strSelectedModId) && strSelectedModId.IsGuid())
            {
                VehicleMod objMod = _lstMods.FirstOrDefault(x => x.InternalId == strSelectedModId);
                if (objMod != null)
                {
                    cmdDeleteMod.Enabled = !objMod.IncludedInVehicle;
                    lblSlots.Text = objMod.CalculatedSlots.ToString(GlobalSettings.InvariantCultureInfo);
                    lblAvailability.Text = objMod.DisplayTotalAvail;

                    if (chkFreeItem.Checked)
                    {
                        lblCost.Text = (0.0m).ToString(_objCharacter.Settings.NuyenFormat, GlobalSettings.CultureInfo) + '¥';
                    }
                    else
                    {
                        int intTotalSlots = 0;
                        xmlSelectedMount.TryGetInt32FieldQuickly("slots", ref intTotalSlots);
                        foreach (string strSelectedId in astrSelectedValues)
                        {
                            if (!string.IsNullOrEmpty(strSelectedId))
                            {
                                XmlNode xmlLoopNode = _xmlDoc.SelectSingleNode("/chummer/weaponmounts/weaponmount[id = " + strSelectedId.CleanXPath() + "]");
                                if (xmlLoopNode == null)
                                    continue;
                                int intLoopSlots = 0;
                                if (xmlLoopNode.TryGetInt32FieldQuickly("slots", ref intLoopSlots))
                                {
                                    intTotalSlots += intLoopSlots;
                                }
                            }
                        }
                        foreach (VehicleMod objLoopMod in _lstMods)
                        {
                            intTotalSlots += objLoopMod.CalculatedSlots;
                        }
                        lblCost.Text = (objMod.TotalCostInMountCreation(intTotalSlots) * (1 + (nudMarkup.Value / 100.0m))).ToString(_objCharacter.Settings.NuyenFormat, GlobalSettings.CultureInfo) + '¥';
                    }

                    objMod.SetSourceDetail(lblSource);
                    lblCostLabel.Visible = !string.IsNullOrEmpty(lblCost.Text);
                    lblSlotsLabel.Visible = !string.IsNullOrEmpty(lblSlots.Text);
                    lblAvailabilityLabel.Visible = !string.IsNullOrEmpty(lblAvailability.Text);
                    lblSourceLabel.Visible = !string.IsNullOrEmpty(lblSource.Text);
                    return;
                }
            }

            if (xmlSelectedMount == null)
            {
                lblCost.Text = string.Empty;
                lblSlots.Text = string.Empty;
                lblAvailability.Text = string.Empty;
                lblCostLabel.Visible = false;
                lblSlotsLabel.Visible = false;
                lblAvailabilityLabel.Visible = false;
                return;
            }
            // Cost.
            bool blnCanBlackMarketDiscount = _setBlackMarketMaps.Contains(xmlSelectedMount.SelectSingleNode("category")?.Value);
            chkBlackMarketDiscount.Enabled = blnCanBlackMarketDiscount;
            if (!chkBlackMarketDiscount.Checked)
            {
                chkBlackMarketDiscount.Checked = GlobalSettings.AssumeBlackMarket && blnCanBlackMarketDiscount;
            }
            else if (!blnCanBlackMarketDiscount)
            {
                //Prevent chkBlackMarketDiscount from being checked if the category doesn't match.
                chkBlackMarketDiscount.Checked = false;
            }

            decimal decCost = 0;
            if (!chkFreeItem.Checked)
                xmlSelectedMount.TryGetDecFieldQuickly("cost", ref decCost);
            int intSlots = 0;
            xmlSelectedMount.TryGetInt32FieldQuickly("slots", ref intSlots);

            string strAvail = xmlSelectedMount["avail"]?.InnerText ?? string.Empty;
            char chrAvailSuffix = strAvail.Length > 0 ? strAvail[strAvail.Length - 1] : ' ';
            if (chrAvailSuffix == 'F' || chrAvailSuffix == 'R')
                strAvail = strAvail.Substring(0, strAvail.Length - 1);
            else
                chrAvailSuffix = ' ';
            int.TryParse(strAvail, NumberStyles.Any, GlobalSettings.InvariantCultureInfo, out int intAvail);

            foreach (string strSelectedId in astrSelectedValues)
            {
                if (string.IsNullOrEmpty(strSelectedId))
                    continue;
                XmlNode xmlLoopNode = _xmlDoc.SelectSingleNode("/chummer/weaponmounts/weaponmount[id = " + strSelectedId.CleanXPath() + "]");
                if (xmlLoopNode == null)
                    continue;
                if (!chkFreeItem.Checked)
                {
                    decimal decLoopCost = 0;
                    if (xmlLoopNode.TryGetDecFieldQuickly("cost", ref decLoopCost))
                        decCost += decLoopCost;
                }

                int intLoopSlots = 0;
                if (xmlLoopNode.TryGetInt32FieldQuickly("slots", ref intLoopSlots))
                    intSlots += intLoopSlots;

                string strLoopAvail = xmlLoopNode["avail"]?.InnerText ?? string.Empty;
                char chrLoopAvailSuffix = strLoopAvail.Length > 0 ? strLoopAvail[strLoopAvail.Length - 1] : ' ';
                switch (chrLoopAvailSuffix)
                {
                    case 'F':
                        strLoopAvail = strLoopAvail.Substring(0, strLoopAvail.Length - 1);
                        chrAvailSuffix = 'F';
                        break;

                    case 'R':
                        {
                            strLoopAvail = strLoopAvail.Substring(0, strLoopAvail.Length - 1);
                            if (chrAvailSuffix == ' ')
                                chrAvailSuffix = 'R';
                            break;
                        }
                }
                if (int.TryParse(strLoopAvail, NumberStyles.Any, GlobalSettings.InvariantCultureInfo, out int intLoopAvail))
                    intAvail += intLoopAvail;
            }
            foreach (VehicleMod objMod in _lstMods)
            {
                intSlots += objMod.CalculatedSlots;
                AvailabilityValue objLoopAvail = objMod.TotalAvailTuple();
                char chrLoopAvailSuffix = objLoopAvail.Suffix;
                if (chrLoopAvailSuffix == 'F')
                    chrAvailSuffix = 'F';
                else if (chrAvailSuffix != 'F' && chrLoopAvailSuffix == 'R')
                    chrAvailSuffix = 'R';
                intAvail += objLoopAvail.Value;
            }
            if (!chkFreeItem.Checked)
            {
                foreach (VehicleMod objMod in _lstMods)
                {
                    decCost += objMod.TotalCostInMountCreation(intSlots);
                }
            }

            if (chkBlackMarketDiscount.Checked)
                decCost *= 0.9m;

            string strAvailText = intAvail.ToString(GlobalSettings.CultureInfo);
            switch (chrAvailSuffix)
            {
                case 'F':
                    strAvailText += LanguageManager.GetString("String_AvailForbidden");
                    break;

                case 'R':
                    strAvailText += LanguageManager.GetString("String_AvailRestricted");
                    break;
            }

            decCost *= 1 + (nudMarkup.Value / 100.0m);
            lblCost.Text = decCost.ToString(_objCharacter.Settings.NuyenFormat, GlobalSettings.CultureInfo) + '¥';
            lblSlots.Text = intSlots.ToString(GlobalSettings.CultureInfo);
            lblAvailability.Text = strAvailText;
            lblCostLabel.Visible = !string.IsNullOrEmpty(lblCost.Text);
            lblSlotsLabel.Visible = !string.IsNullOrEmpty(lblSlots.Text);
            lblAvailabilityLabel.Visible = !string.IsNullOrEmpty(lblAvailability.Text);

            string strSource = xmlSelectedMount["source"]?.InnerText ?? LanguageManager.GetString("String_Unknown");
            string strPage = xmlSelectedMount["altpage"]?.InnerText ?? xmlSelectedMount["page"]?.InnerText ?? LanguageManager.GetString("String_Unknown");
            SourceString objSourceString = new SourceString(strSource, strPage, GlobalSettings.Language, GlobalSettings.CultureInfo, _objCharacter);
            objSourceString.SetControl(lblSource);
            lblSourceLabel.Visible = !string.IsNullOrEmpty(lblSource.Text);
        }

        private void cmdAddMod_Click(object sender, EventArgs e)
        {
            bool blnAddAgain;

            XmlNode xmlSelectedMount = null;
            string strSelectedMount = cboSize.SelectedValue?.ToString();
            if (!string.IsNullOrEmpty(strSelectedMount))
                xmlSelectedMount = _xmlDoc.SelectSingleNode("/chummer/weaponmounts/weaponmount[id = " + strSelectedMount.CleanXPath() + "]");

            int intSlots = 0;
            xmlSelectedMount.TryGetInt32FieldQuickly("slots", ref intSlots);

            string[] astrSelectedValues = { cboVisibility.SelectedValue?.ToString(), cboFlexibility.SelectedValue?.ToString(), cboControl.SelectedValue?.ToString() };
            foreach (string strSelectedId in astrSelectedValues)
            {
                if (!string.IsNullOrEmpty(strSelectedId))
                {
                    XmlNode xmlLoopNode = _xmlDoc.SelectSingleNode("/chummer/weaponmounts/weaponmount[id = " + strSelectedId.CleanXPath() + "]");
                    if (xmlLoopNode != null)
                    {
                        intSlots += Convert.ToInt32(xmlLoopNode["slots"]?.InnerText, GlobalSettings.InvariantCultureInfo);
                    }
                }
            }
            foreach (VehicleMod objMod in _lstMods)
            {
                intSlots += objMod.CalculatedSlots;
            }

            string strSpace = LanguageManager.GetString("String_Space");
            TreeNode objModsParentNode = treMods.FindNode("Node_AdditionalMods");
            do
            {
                using (frmSelectVehicleMod frmPickVehicleMod = new frmSelectVehicleMod(_objCharacter, _objVehicle, _objMount?.Mods)
                {
                    // Pass the selected vehicle on to the form.
                    VehicleMountMods = true,
                    WeaponMountSlots = intSlots
                })
                {
                    frmPickVehicleMod.ShowDialog(this);

                    // Make sure the dialogue window was not canceled.
                    if (frmPickVehicleMod.DialogResult == DialogResult.Cancel)
                        break;

                    blnAddAgain = frmPickVehicleMod.AddAgain;
                    XmlDocument objXmlDocument = _objCharacter.LoadData("vehicles.xml");
                    XmlNode objXmlMod = objXmlDocument.SelectSingleNode("/chummer/weaponmountmods/mod[id = " + frmPickVehicleMod.SelectedMod.CleanXPath() + "]");

                    VehicleMod objMod = new VehicleMod(_objCharacter)
                    {
                        DiscountCost = frmPickVehicleMod.BlackMarketDiscount
                    };
                    objMod.Create(objXmlMod, frmPickVehicleMod.SelectedRating, _objVehicle, frmPickVehicleMod.Markup);
                    // Check the item's Cost and make sure the character can afford it.
                    decimal decOriginalCost = _objVehicle.TotalCost;
                    if (frmPickVehicleMod.FreeCost)
                        objMod.Cost = "0";

                    // Do not allow the user to add a new Vehicle Mod if the Vehicle's Capacity has been reached.
                    if (_objCharacter.Settings.EnforceCapacity)
                    {
                        bool blnOverCapacity = false;
                        if (_objCharacter.Settings.BookEnabled("R5"))
                        {
                            if (_objVehicle.IsDrone && _objCharacter.Settings.DroneMods)
                            {
                                if (_objVehicle.DroneModSlotsUsed > _objVehicle.DroneModSlots)
                                    blnOverCapacity = true;
                            }
                            else
                            {
                                int intUsed = _objVehicle.CalcCategoryUsed(objMod.Category);
                                int intAvail = _objVehicle.CalcCategoryAvail(objMod.Category);
                                if (intUsed > intAvail)
                                    blnOverCapacity = true;
                            }
                        }
                        else if (_objVehicle.Slots < _objVehicle.SlotsUsed)
                        {
                            blnOverCapacity = true;
                        }

                        if (blnOverCapacity)
                        {
                            Program.MainForm.ShowMessageBox(this, LanguageManager.GetString("Message_CapacityReached"), LanguageManager.GetString("MessageTitle_CapacityReached"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                            continue;
                        }
                    }

                    if (_objCharacter.Created)
                    {
                        decimal decCost = _objVehicle.TotalCost - decOriginalCost;

                        // Multiply the cost if applicable.
                        char chrAvail = objMod.TotalAvailTuple().Suffix;
                        switch (chrAvail)
                        {
                            case 'R' when _objCharacter.Settings.MultiplyRestrictedCost:
                                decCost *= _objCharacter.Settings.RestrictedCostMultiplier;
                                break;

                            case 'F' when _objCharacter.Settings.MultiplyForbiddenCost:
                                decCost *= _objCharacter.Settings.ForbiddenCostMultiplier;
                                break;
                        }

                        if (decCost > _objCharacter.Nuyen)
                        {
                            Program.MainForm.ShowMessageBox(this, LanguageManager.GetString("Message_NotEnoughNuyen"),
                                LanguageManager.GetString("MessageTitle_NotEnoughNuyen"),
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                            continue;
                        }

                        // Create the Expense Log Entry.
                        ExpenseLogEntry objExpense = new ExpenseLogEntry(_objCharacter);
                        objExpense.Create(decCost * -1,
                            LanguageManager.GetString("String_ExpensePurchaseVehicleMod") +
                            strSpace + objMod.DisplayNameShort(GlobalSettings.Language), ExpenseType.Nuyen, DateTime.Now);
                        _objCharacter.ExpenseEntries.AddWithSort(objExpense);
                        _objCharacter.Nuyen -= decCost;

                        ExpenseUndo objUndo = new ExpenseUndo();
                        objUndo.CreateNuyen(NuyenExpenseType.AddVehicleWeaponMountMod, objMod.InternalId);
                        objExpense.Undo = objUndo;
                    }

                    _lstMods.Add(objMod);
                    intSlots += objMod.CalculatedSlots;

                    TreeNode objNewNode = objMod.CreateTreeNode(null, null, null, null, null, null);

                    if (objModsParentNode == null)
                    {
                        objModsParentNode = new TreeNode
                        {
                            Tag = "Node_AdditionalMods",
                            Text = LanguageManager.GetString("Node_AdditionalMods")
                        };
                        treMods.Nodes.Add(objModsParentNode);
                        objModsParentNode.Expand();
                    }

                    objModsParentNode.Nodes.Add(objNewNode);
                    treMods.SelectedNode = objNewNode;
                }
            }
            while (blnAddAgain);
        }

        private void cmdDeleteMod_Click(object sender, EventArgs e)
        {
            TreeNode objSelectedNode = treMods.SelectedNode;
            if (objSelectedNode == null)
                return;
            string strSelectedId = objSelectedNode.Tag.ToString();
            if (!string.IsNullOrEmpty(strSelectedId) && strSelectedId.IsGuid())
            {
                VehicleMod objMod = _lstMods.FirstOrDefault(x => x.InternalId == strSelectedId);
                if (objMod != null && !objMod.IncludedInVehicle)
                {
                    if (!CommonFunctions.ConfirmDelete(LanguageManager.GetString("Message_DeleteVehicle")))
                        return;

                    _lstMods.Remove(objMod);
                    foreach (Weapon objLoopWeapon in objMod.Weapons)
                    {
                        objLoopWeapon.DeleteWeapon();
                    }
                    foreach (Cyberware objLoopCyberware in objMod.Cyberware)
                    {
                        objLoopCyberware.DeleteCyberware();
                    }
                    TreeNode objParentNode = objSelectedNode.Parent;
                    objSelectedNode.Remove();
                    if (objParentNode.Nodes.Count == 0)
                        objParentNode.Remove();
                }
            }
        }

        private void treMods_AfterSelect(object sender, TreeViewEventArgs e)
        {
            UpdateInfo();
        }

        private void RefreshComboBoxes()
        {
            XmlNode xmlRequiredNode = null;
            XmlNode xmlForbiddenNode = null;
            string strSelectedMount = cboSize.SelectedValue?.ToString();
            if (!string.IsNullOrEmpty(strSelectedMount))
            {
                XmlNode xmlSelectedMount = _xmlDoc.SelectSingleNode("/chummer/weaponmounts/weaponmount[id = " + strSelectedMount.CleanXPath() + "]");
                if (xmlSelectedMount != null)
                {
                    xmlForbiddenNode = xmlSelectedMount.SelectSingleNode("forbidden/weaponmountdetails");
                    xmlRequiredNode = xmlSelectedMount.SelectSingleNode("required/weaponmountdetails");
                }
            }

            XmlNode xmlVehicleNode = _objVehicle.GetNode();
            List<ListItem> lstVisibility;
            List<ListItem> lstFlexibility;
            List<ListItem> lstControl;
            // Populate the Weapon Mount Category list.
            string strFilter = "category != \"Size\" and " + _objCharacter.Settings.BookXPath();
            if (!_objVehicle.IsDrone && _objCharacter.Settings.DroneMods)
                strFilter += " and not(optionaldrone)";
            using (XmlNodeList xmlWeaponMountOptionNodeList = _xmlDoc.SelectNodes("/chummer/weaponmounts/weaponmount[" + strFilter + "]"))
            {
                lstVisibility = new List<ListItem>(xmlWeaponMountOptionNodeList?.Count ?? 0);
                lstFlexibility = new List<ListItem>(xmlWeaponMountOptionNodeList?.Count ?? 0);
                lstControl = new List<ListItem>(xmlWeaponMountOptionNodeList?.Count ?? 0);
                if (xmlWeaponMountOptionNodeList?.Count > 0)
                {
                    foreach (XmlNode xmlWeaponMountOptionNode in xmlWeaponMountOptionNodeList)
                    {
                        string strId = xmlWeaponMountOptionNode["id"]?.InnerText;
                        if (string.IsNullOrEmpty(strId))
                            continue;

                        XmlNode xmlTestNode = xmlWeaponMountOptionNode.SelectSingleNode("forbidden/vehicledetails");
                        if (xmlTestNode != null && xmlVehicleNode.ProcessFilterOperationNode(xmlTestNode, false))
                        {
                            // Assumes topmost parent is an AND node
                            continue;
                        }

                        xmlTestNode = xmlWeaponMountOptionNode.SelectSingleNode("required/vehicledetails");
                        if (xmlTestNode != null && !xmlVehicleNode.ProcessFilterOperationNode(xmlTestNode, false))
                        {
                            // Assumes topmost parent is an AND node
                            continue;
                        }

                        string strName = xmlWeaponMountOptionNode["name"]?.InnerText ?? LanguageManager.GetString("String_Unknown");
                        bool blnAddItem = true;
                        switch (xmlWeaponMountOptionNode["category"]?.InnerText)
                        {
                            case "Visibility":
                                {
                                    XmlNodeList xmlNodeList = xmlForbiddenNode?.SelectNodes("visibility");
                                    if (xmlNodeList?.Count > 0)
                                    {
                                        foreach (XmlNode xmlLoopNode in xmlNodeList)
                                        {
                                            if (xmlLoopNode.InnerText == strName)
                                            {
                                                blnAddItem = false;
                                                break;
                                            }
                                        }
                                    }

                                    if (xmlRequiredNode != null)
                                    {
                                        blnAddItem = false;
                                        xmlNodeList = xmlRequiredNode.SelectNodes("visibility");
                                        if (xmlNodeList?.Count > 0)
                                        {
                                            foreach (XmlNode xmlLoopNode in xmlNodeList)
                                            {
                                                if (xmlLoopNode.InnerText == strName)
                                                {
                                                    blnAddItem = true;
                                                    break;
                                                }
                                            }
                                        }
                                    }

                                    if (blnAddItem)
                                        lstVisibility.Add(new ListItem(strId, xmlWeaponMountOptionNode["translate"]?.InnerText ?? strName));
                                }
                                break;

                            case "Flexibility":
                                {
                                    XmlNodeList xmlNodeList = xmlForbiddenNode?.SelectNodes("flexibility");
                                    if (xmlNodeList?.Count > 0)
                                    {
                                        foreach (XmlNode xmlLoopNode in xmlNodeList)
                                        {
                                            if (xmlLoopNode.InnerText == strName)
                                            {
                                                blnAddItem = false;
                                                break;
                                            }
                                        }
                                    }

                                    if (xmlRequiredNode != null)
                                    {
                                        blnAddItem = false;
                                        xmlNodeList = xmlRequiredNode.SelectNodes("flexibility");
                                        if (xmlNodeList?.Count > 0)
                                        {
                                            foreach (XmlNode xmlLoopNode in xmlNodeList)
                                            {
                                                if (xmlLoopNode.InnerText == strName)
                                                {
                                                    blnAddItem = true;
                                                    break;
                                                }
                                            }
                                        }
                                    }

                                    if (blnAddItem)
                                        lstFlexibility.Add(new ListItem(strId, xmlWeaponMountOptionNode["translate"]?.InnerText ?? strName));
                                }
                                break;

                            case "Control":
                                {
                                    XmlNodeList xmlNodeList = xmlForbiddenNode?.SelectNodes("control");
                                    if (xmlNodeList?.Count > 0)
                                    {
                                        foreach (XmlNode xmlLoopNode in xmlNodeList)
                                        {
                                            if (xmlLoopNode.InnerText == strName)
                                            {
                                                blnAddItem = false;
                                                break;
                                            }
                                        }
                                    }

                                    if (xmlRequiredNode != null)
                                    {
                                        blnAddItem = false;
                                        xmlNodeList = xmlRequiredNode.SelectNodes("control");
                                        if (xmlNodeList?.Count > 0)
                                        {
                                            foreach (XmlNode xmlLoopNode in xmlNodeList)
                                            {
                                                if (xmlLoopNode.InnerText == strName)
                                                {
                                                    blnAddItem = true;
                                                    break;
                                                }
                                            }
                                        }
                                    }

                                    if (blnAddItem)
                                        lstControl.Add(new ListItem(strId, xmlWeaponMountOptionNode["translate"]?.InnerText ?? strName));
                                }
                                break;

                            default:
                                Utils.BreakIfDebug();
                                break;
                        }
                    }
                }
            }

            bool blnOldLoading = _blnLoading;
            _blnLoading = true;
            string strOldVisibility = cboVisibility.SelectedValue?.ToString();
            string strOldFlexibility = cboFlexibility.SelectedValue?.ToString();
            string strOldControl = cboControl.SelectedValue?.ToString();
            cboVisibility.BeginUpdate();
            cboVisibility.PopulateWithListItems(lstVisibility);
            cboVisibility.Enabled = lstVisibility.Count > 1;
            if (!string.IsNullOrEmpty(strOldVisibility))
                cboVisibility.SelectedValue = strOldVisibility;
            if (cboVisibility.SelectedIndex == -1 && lstVisibility.Count > 0)
                cboVisibility.SelectedIndex = 0;
            cboVisibility.EndUpdate();

            cboFlexibility.BeginUpdate();
            cboFlexibility.PopulateWithListItems(lstFlexibility);
            cboFlexibility.Enabled = lstFlexibility.Count > 1;
            if (!string.IsNullOrEmpty(strOldFlexibility))
                cboFlexibility.SelectedValue = strOldFlexibility;
            if (cboFlexibility.SelectedIndex == -1 && lstFlexibility.Count > 0)
                cboFlexibility.SelectedIndex = 0;
            cboFlexibility.EndUpdate();

            cboControl.BeginUpdate();
            cboControl.PopulateWithListItems(lstControl);
            cboControl.Enabled = lstControl.Count > 1;
            if (!string.IsNullOrEmpty(strOldControl))
                cboControl.SelectedValue = strOldControl;
            if (cboControl.SelectedIndex == -1 && lstControl.Count > 0)
                cboControl.SelectedIndex = 0;
            cboControl.EndUpdate();

            _blnLoading = blnOldLoading;
        }

        private void lblSource_Click(object sender, EventArgs e)
        {
            CommonFunctions.OpenPdfFromControl(sender, e);
        }
    }
}
