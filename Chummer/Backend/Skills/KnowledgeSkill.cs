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
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml;
using System.Xml.XPath;

namespace Chummer.Backend.Skills
{
    public class KnowledgeSkill : Skill
    {
        private ReadOnlyDictionary<string, string> _dicCategoriesSkillMap;  //Categories to their attribute

        private IReadOnlyDictionary<string, string> CategoriesSkillMap
        {
            get
            {
                if (GlobalOptions.LiveCustomData || _dicCategoriesSkillMap == null)
                {
                    Dictionary<string, string> dicReturn = new Dictionary<string, string>();
                    foreach (XPathNavigator objXmlSkill in CharacterObject.LoadDataXPath("skills.xml").Select("/chummer/knowledgeskills/skill"))
                    {
                        string strCategory = objXmlSkill.SelectSingleNode("category")?.Value;
                        if (!string.IsNullOrWhiteSpace(strCategory))
                        {
                            dicReturn[strCategory] = objXmlSkill.SelectSingleNode("attribute")?.Value;
                        }
                    }
                    return _dicCategoriesSkillMap = new ReadOnlyDictionary<string, string>(dicReturn);
                }
                return _dicCategoriesSkillMap;
            }
        }

        public static IReadOnlyList<ListItem> DefaultKnowledgeSkills(Character objCharacter = null, string strLanguage = "")
        {
            List<ListItem> lstReturn = new List<ListItem>();
            if (string.IsNullOrEmpty(strLanguage))
                strLanguage = GlobalOptions.Language;
            XPathNavigator xmlSkillsDocument = XmlManager.LoadXPath("skills.xml", objCharacter?.Options.EnabledCustomDataDirectoryPaths, strLanguage);
            foreach (XPathNavigator xmlSkill in xmlSkillsDocument.Select("/chummer/knowledgeskills/skill"))
            {
                string strName = xmlSkill.SelectSingleNode("name")?.Value ?? string.Empty;
                lstReturn.Add(new ListItem(strName, xmlSkill.SelectSingleNode("translate")?.Value ?? strName));
            }
            lstReturn.Sort(CompareListItems.CompareNames);
            return lstReturn;
        }

        /// <summary>
        /// Load the (possible translated) types of kno skills (Academic, Street...)
        /// </summary>
        /// <param name="objCharacter"></param>
        /// <param name="strLanguage"></param>
        /// <returns></returns>
        public static IReadOnlyList<ListItem> KnowledgeTypes(Character objCharacter = null, string strLanguage = "")
        {
            List<ListItem> lstReturn = new List<ListItem>();
            if (string.IsNullOrEmpty(strLanguage))
                strLanguage = GlobalOptions.Language;
            XPathNavigator xmlSkillsDocument = XmlManager.LoadXPath("skills.xml", objCharacter?.Options.EnabledCustomDataDirectoryPaths, strLanguage);
            foreach (XPathNavigator objXmlCategory in xmlSkillsDocument.Select("/chummer/categories/category[@type = \"knowledge\"]"))
            {
                string strInnerText = objXmlCategory.Value;
                lstReturn.Add(new ListItem(strInnerText, objXmlCategory.SelectSingleNode("@translate")?.Value ?? strInnerText));
            }
            lstReturn.Sort(CompareListItems.CompareNames);
            return lstReturn;
        }

        public override bool IsKnowledgeSkill => true;

        public override bool AllowDelete => (!ForcedName || FreeBase + FreeKarma + RatingModifiers(Attribute) <= 0) && !IsNativeLanguage;

        public override bool AllowNameChange => !ForcedName && (AllowUpgrade || IsNativeLanguage) && (!CharacterObject.Created || (Karma == 0 && Base == 0 && !IsNativeLanguage));

        public override bool AllowTypeChange => (AllowNameChange || string.IsNullOrWhiteSpace(Type)) && !IsNativeLanguage;

        private string _strType = string.Empty;

        private bool _blnIsNativeLanguage;

        public bool ForcedName { get; }

        public KnowledgeSkill(Character objCharacter) : base(objCharacter)
        {
            if (objCharacter == null)
                throw new ArgumentNullException(nameof(objCharacter));
            DefaultAttribute = "LOG";
        }

        public KnowledgeSkill(Character objCharacter, string strForcedName, bool allowUpgrade) : this(objCharacter)
        {
            WritableName = strForcedName;
            ForcedName = true;
            _blnAllowUpgrade = allowUpgrade;
        }

        private bool _blnAllowUpgrade = true;

        /// <summary>
        /// Is the skill allowed to be upgraded through karma or points?
        /// </summary>
        public bool AllowUpgrade
        {
            get => !IsNativeLanguage && _blnAllowUpgrade;
            set => _blnAllowUpgrade = value;
        }

        public string WritableName
        {
            get => CurrentDisplayName;
            set
            {
                if (ForcedName)
                    return;
                if (string.Equals(CurrentDisplayName, value, StringComparison.CurrentCulture))
                    return;
                LoadSkillFromData(value);
                Name = value;
                OnPropertyChanged();
            }
        }

        private void LoadSkillFromData(string strInputSkillName)
        {
            string strSkillName = GetSkillNameFromData(strInputSkillName);
            XPathNavigator xmlSkillNode = CharacterObject.LoadDataXPath("skills.xml").SelectSingleNode("/chummer/knowledgeskills/skill[name = " + strSkillName.CleanXPath() + "]");

            if (xmlSkillNode == null)
            {
                SkillId = Guid.Empty;
                return;
            }

            SkillId = xmlSkillNode.TryGetField("id", Guid.TryParse, out Guid guidTemp)
                ? guidTemp
                : Guid.Empty;

            string strCategory = xmlSkillNode.SelectSingleNode("category")?.Value;

            if (!string.IsNullOrEmpty(strCategory))
            {
                Type = strCategory;
            }

            string strAttribute = xmlSkillNode.SelectSingleNode("attribute")?.Value;

            if (!string.IsNullOrEmpty(strAttribute))
            {
                DefaultAttribute = CharacterObject.GetAttribute(strAttribute) != null ? strAttribute : "LOG";
            }
        }

        private string GetSkillNameFromData(string strInputSkillName)
        {
            if (GlobalOptions.Language.Equals(GlobalOptions.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
            {
                return strInputSkillName;
            }

            XPathNavigator xmlSkillTranslationNode = CharacterObject.LoadDataXPath("skills.xml").SelectSingleNode("/chummer/knowledgeskills/skill[translate = " + strInputSkillName.CleanXPath() + "]");

            if (xmlSkillTranslationNode == null)
            {
                return CharacterObject.ReverseTranslateExtra(strInputSkillName, GlobalOptions.Language, "skills.xml");
            }

            return xmlSkillTranslationNode.SelectSingleNode("name")?.Value ?? strInputSkillName;
        }

        public override string SkillCategory => Type;

        public override string DisplayPool => IsNativeLanguage ? LanguageManager.GetString("Skill_NativeLanguageShort") : base.DisplayPool;

        // ReSharper disable once InconsistentNaming
        private int _intCachedCyberwareRating = int.MinValue;

        protected override void ResetCachedCyberwareRating()
        {
            _intCachedCyberwareRating = int.MinValue;
        }

        /// <summary>
        /// The attributeValue this skill have from Skilljacks + Knowsoft
        /// </summary>
        /// <returns>Artificial skill attributeValue</returns>
        public override int CyberwareRating
        {
            get
            {
                if (_intCachedCyberwareRating != int.MinValue)
                    return _intCachedCyberwareRating;

                string strTranslatedName = CurrentDisplayName;
                int intMaxHardwire = -1;
                foreach (Improvement objImprovement in CharacterObject.Improvements)
                {
                    if (objImprovement.ImproveType == Improvement.ImprovementType.Hardwire &&
                        (objImprovement.ImprovedName == DictionaryKey ||
                         objImprovement.ImprovedName == strTranslatedName) && objImprovement.Enabled)
                    {
                        intMaxHardwire = Math.Max(intMaxHardwire, objImprovement.Value.StandardRound());
                    }
                }
                if (intMaxHardwire >= 0)
                {
                    return _intCachedCyberwareRating = intMaxHardwire;
                }

                int intMaxSkillsoftRating = ImprovementManager.ValueOf(CharacterObject, Improvement.ImprovementType.SkillsoftAccess).StandardRound();
                if (intMaxSkillsoftRating <= 0)
                    return _intCachedCyberwareRating = 0;
                int intMax = 0;
                foreach (Improvement objSkillsoftImprovement in CharacterObject.Improvements)
                {
                    if (objSkillsoftImprovement.ImproveType == Improvement.ImprovementType.Skillsoft &&
                        objSkillsoftImprovement.ImprovedName == InternalId && objSkillsoftImprovement.Enabled)
                    {
                        intMax = Math.Max(intMax, objSkillsoftImprovement.Value.StandardRound());
                    }
                }

                return _intCachedCyberwareRating = Math.Min(intMax, intMaxSkillsoftRating);
            }
        }

        public string Type
        {
            get => _strType;
            set
            {
                if (value == _strType) return;
                _strType = value;

                //2018-22-03: Causes any attempt to alter the Type for skills with names that match
                //default skills to reset to the default Type for that skill. If we want to disable
                //that behavior, better to disable it via the control.
                /*
                if (!LoadSkill())
                {
                    if (s_CategoriesSkillMap.TryGetValue(value, out string strNewAttributeValue))
                    {
                        AttributeObject = CharacterObject.GetAttribute(strNewAttributeValue);
                    }
                }
                */
                if (CategoriesSkillMap.TryGetValue(value, out string strNewAttributeValue))
                {
                    DefaultAttribute = strNewAttributeValue;
                }

                OnPropertyChanged();
                if (!IsLanguage)
                    IsNativeLanguage = false;
            }
        }

        public override bool IsLanguage => Type == "Language";

        public override bool IsNativeLanguage
        {
            get => _blnIsNativeLanguage;
            set
            {
                if (_blnIsNativeLanguage != value)
                {
                    _blnIsNativeLanguage = value;
                    if (value)
                    {
                        Base = 0;
                        Karma = 0;
                        BuyWithKarma = false;
                        Specializations.Clear();
                    }
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// How much karma this costs. Return value during career mode is undefined
        /// </summary>
        /// <returns></returns>
        public override int CurrentKarmaCost
        {
            get
            {
                int intTotalBaseRating = TotalBaseRating;
                decimal decCost = intTotalBaseRating * (intTotalBaseRating + 1);
                int intLower = Base + FreeKarma + RatingModifiers(Attribute);
                decCost -= intLower * (intLower + 1);

                decCost /= 2;
                decCost *= CharacterObject.Options.KarmaImproveKnowledgeSkill;
                // We have bought the first level with karma, too
                if (intLower == 0 && decCost > 0)
                    decCost += CharacterObject.Options.KarmaNewKnowledgeSkill - CharacterObject.Options.KarmaImproveKnowledgeSkill;

                decimal decMultiplier = 1.0m;
                decimal decExtra = 0;
                int intSpecCount = 0;
                foreach (SkillSpecialization objSpec in Specializations)
                {
                    if (!objSpec.Free && BuyWithKarma)
                        intSpecCount += 1;
                }
                decimal decSpecCost = CharacterObject.Options.KarmaKnowledgeSpecialization * intSpecCount;
                decimal decExtraSpecCost = 0;
                decimal decSpecCostMultiplier = 1.0m;
                foreach (Improvement objLoopImprovement in CharacterObject.Improvements)
                {
                    if (objLoopImprovement.Minimum <= intTotalBaseRating &&
                        (string.IsNullOrEmpty(objLoopImprovement.Condition) || (objLoopImprovement.Condition == "career") == CharacterObject.Created || (objLoopImprovement.Condition == "create") != CharacterObject.Created) && objLoopImprovement.Enabled)
                    {
                        if (objLoopImprovement.ImprovedName == DictionaryKey || string.IsNullOrEmpty(objLoopImprovement.ImprovedName))
                        {
                            switch (objLoopImprovement.ImproveType)
                            {
                                case Improvement.ImprovementType.KnowledgeSkillKarmaCost:
                                    decExtra += objLoopImprovement.Value * (Math.Min(intTotalBaseRating, objLoopImprovement.Maximum == 0 ? int.MaxValue : objLoopImprovement.Maximum) - Math.Max(intLower, objLoopImprovement.Minimum - 1));
                                    break;

                                case Improvement.ImprovementType.KnowledgeSkillKarmaCostMultiplier:
                                    decMultiplier *= objLoopImprovement.Value / 100.0m;
                                    break;
                            }
                        }
                        else if (objLoopImprovement.ImprovedName == SkillCategory)
                        {
                            switch (objLoopImprovement.ImproveType)
                            {
                                case Improvement.ImprovementType.SkillCategoryKarmaCost:
                                    decExtra += objLoopImprovement.Value * (Math.Min(intTotalBaseRating, objLoopImprovement.Maximum == 0 ? int.MaxValue : objLoopImprovement.Maximum) - Math.Max(intLower, objLoopImprovement.Minimum - 1));
                                    break;

                                case Improvement.ImprovementType.SkillCategoryKarmaCostMultiplier:
                                    decMultiplier *= objLoopImprovement.Value / 100.0m;
                                    break;

                                case Improvement.ImprovementType.SkillCategorySpecializationKarmaCost:
                                    decExtraSpecCost += objLoopImprovement.Value * intSpecCount;
                                    break;

                                case Improvement.ImprovementType.SkillCategorySpecializationKarmaCostMultiplier:
                                    decSpecCostMultiplier *= objLoopImprovement.Value / 100.0m;
                                    break;
                            }
                        }
                    }
                }
                if (decMultiplier != 1.0m)
                    decCost *= decMultiplier;

                if (decSpecCostMultiplier != 1.0m)
                    decSpecCost *= decSpecCostMultiplier;
                decCost += decExtra;
                decCost += decSpecCost + decExtraSpecCost; //Spec

                return Math.Max(decCost.StandardRound(), 0);
            }
        }

        /// <summary>
        /// Karma price to upgrade. Returns negative if impossible. Minimum value is always 1.
        /// </summary>
        /// <returns>Price in karma</returns>
        public override int UpgradeKarmaCost
        {
            get
            {
                int intTotalBaseRating = TotalBaseRating;
                if (intTotalBaseRating >= RatingMaximum)
                {
                    return -1;
                }
                int intOptionsCost;
                int intValue;
                if (intTotalBaseRating == 0)
                {
                    intOptionsCost = CharacterObject.Options.KarmaNewKnowledgeSkill;
                    intValue = intOptionsCost;
                }
                else
                {
                    intOptionsCost = CharacterObject.Options.KarmaNewKnowledgeSkill;
                    intValue = (intTotalBaseRating + 1) * intOptionsCost;
                }

                decimal decMultiplier = 1.0m;
                decimal decExtra = 0;
                int intMinOverride = int.MaxValue;
                foreach (Improvement objLoopImprovement in CharacterObject.Improvements)
                {
                    if ((objLoopImprovement.Maximum == 0 || intTotalBaseRating + 1 <= objLoopImprovement.Maximum) && objLoopImprovement.Minimum <= intTotalBaseRating + 1 &&
                        (string.IsNullOrEmpty(objLoopImprovement.Condition) || (objLoopImprovement.Condition == "career") == CharacterObject.Created || (objLoopImprovement.Condition == "create") != CharacterObject.Created) && objLoopImprovement.Enabled)
                    {
                        if (objLoopImprovement.ImprovedName == DictionaryKey || string.IsNullOrWhiteSpace(objLoopImprovement.ImprovedName))
                        {
                            switch (objLoopImprovement.ImproveType)
                            {
                                case Improvement.ImprovementType.KnowledgeSkillKarmaCost:
                                    decExtra += objLoopImprovement.Value;
                                    break;

                                case Improvement.ImprovementType.KnowledgeSkillKarmaCostMultiplier:
                                    decMultiplier *= objLoopImprovement.Value / 100.0m;
                                    break;
                            }
                        }
                        else if (objLoopImprovement.ImprovedName == SkillCategory)
                        {
                            switch (objLoopImprovement.ImproveType)
                            {
                                case Improvement.ImprovementType.SkillCategoryKarmaCost:
                                    decExtra += objLoopImprovement.Value;
                                    break;

                                case Improvement.ImprovementType.SkillCategoryKarmaCostMultiplier:
                                    decMultiplier *= objLoopImprovement.Value / 100.0m;
                                    break;
                            }
                        }

                        if ((objLoopImprovement.ImprovedName == DictionaryKey ||
                            string.IsNullOrWhiteSpace(objLoopImprovement.ImprovedName) ||
                            objLoopImprovement.ImprovedName == SkillCategory) && objLoopImprovement.ImproveType ==
                            Improvement.ImprovementType.KnowledgeSkillKarmaCostMinimum)
                        {
                            intMinOverride = Math.Min(intMinOverride, objLoopImprovement.Value.StandardRound());
                        }
                    }
                }
                if (decMultiplier != 1.0m)
                    intValue = (intValue * decMultiplier + decExtra).StandardRound();
                else
                    intValue += decExtra.StandardRound();
                return Math.Max(intValue, intMinOverride != int.MaxValue ? intMinOverride : Math.Min(1, intOptionsCost));
            }
        }

        /// <summary>
        /// How much Sp this costs. Price during career mode is undefined
        /// </summary>
        /// <returns></returns>
        public override int CurrentSpCost
        {
            get
            {
                int intPointCost = BasePoints + (string.IsNullOrWhiteSpace(Specialization) || BuyWithKarma ? 0 : 1);

                decimal decExtra = 0;
                decimal decMultiplier = 1.0m;
                foreach (Improvement objLoopImprovement in CharacterObject.Improvements)
                {
                    if (objLoopImprovement.Minimum <= BasePoints &&
                        (string.IsNullOrEmpty(objLoopImprovement.Condition) || (objLoopImprovement.Condition == "career") == CharacterObject.Created || (objLoopImprovement.Condition == "create") != CharacterObject.Created) && objLoopImprovement.Enabled)
                    {
                        if (objLoopImprovement.ImprovedName == DictionaryKey || string.IsNullOrEmpty(objLoopImprovement.ImprovedName))
                        {
                            switch (objLoopImprovement.ImproveType)
                            {
                                case Improvement.ImprovementType.KnowledgeSkillPointCost:
                                    decExtra += objLoopImprovement.Value * (Math.Min(BasePoints, objLoopImprovement.Maximum == 0 ? int.MaxValue : objLoopImprovement.Maximum) - objLoopImprovement.Minimum);
                                    break;

                                case Improvement.ImprovementType.KnowledgeSkillPointCostMultiplier:
                                    decMultiplier *= objLoopImprovement.Value / 100.0m;
                                    break;
                            }
                        }
                        else if (objLoopImprovement.ImprovedName == SkillCategory)
                        {
                            switch (objLoopImprovement.ImproveType)
                            {
                                case Improvement.ImprovementType.SkillCategoryPointCost:
                                    decExtra += objLoopImprovement.Value * (Math.Min(BasePoints, objLoopImprovement.Maximum == 0 ? int.MaxValue : objLoopImprovement.Maximum) - objLoopImprovement.Minimum);
                                    break;

                                case Improvement.ImprovementType.SkillCategoryPointCostMultiplier:
                                    decMultiplier *= objLoopImprovement.Value / 100.0m;
                                    break;
                            }
                        }
                    }
                }
                if (decMultiplier != 1.0m)
                    intPointCost = (intPointCost * decMultiplier + decExtra).StandardRound();
                else
                    intPointCost += decExtra.StandardRound();

                return Math.Max(intPointCost, 0);
            }
        }

        public override void WriteToDerived(XmlTextWriter objWriter)
        {
            objWriter.WriteElementString("type", Type);
            objWriter.WriteElementString("isnativelanguage", IsNativeLanguage.ToString(GlobalOptions.InvariantCultureInfo));
            if (ForcedName)
                objWriter.WriteElementString("forced", null);
        }

        public void Load(XmlNode xmlNode)
        {
            if (xmlNode == null)
                return;
            string strTemp = Name;
            if (xmlNode.TryGetStringFieldQuickly("name", ref strTemp))
                Name = strTemp;
            if (xmlNode.TryGetField("id", Guid.TryParse, out Guid guiTemp))
                SkillId = guiTemp;
            else if (xmlNode.TryGetField("suid", Guid.TryParse, out Guid guiTemp2))
                SkillId = guiTemp2;

            // Legacy shim
            if (SkillId.Equals(Guid.Empty))
            {
                XPathNavigator objDataNode = CharacterObject.LoadDataXPath("skills.xml").SelectSingleNode("/chummer/knowledgeskills/skill[name = " + Name.CleanXPath() + "]");
                if (objDataNode.TryGetField("id", Guid.TryParse, out Guid guidTemp))
                    SkillId = guidTemp;
            }

            string strCategoryString = string.Empty;
            if ((xmlNode.TryGetStringFieldQuickly("type", ref strCategoryString) && !string.IsNullOrEmpty(strCategoryString))
                || (xmlNode.TryGetStringFieldQuickly("skillcategory", ref strCategoryString) && !string.IsNullOrEmpty(strCategoryString)))
            {
                Type = strCategoryString;
            }

            // Legacy sweep for native language skills
            if (!xmlNode.TryGetBoolFieldQuickly("isnativelanguage", ref _blnIsNativeLanguage) && IsLanguage && CharacterObject.LastSavedVersion <= new Version(5, 212, 72))
            {
                int intKarma = 0;
                int intBase = 0;
                xmlNode.TryGetInt32FieldQuickly("karma", ref intKarma);
                xmlNode.TryGetInt32FieldQuickly("base", ref intBase);
                if (intKarma == 0 && intBase == 0 && CharacterObject.SkillsSection.KnowledgeSkills.Count(x => x.IsNativeLanguage) < 1 + ImprovementManager.ValueOf(CharacterObject, Improvement.ImprovementType.NativeLanguageLimit))
                    _blnIsNativeLanguage = true;
            }
        }
    }
}
