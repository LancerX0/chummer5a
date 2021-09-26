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
using System.Globalization;
using System.Linq;
using System.Xml;

namespace Chummer.Backend.Skills
{
    /// <summary>
    /// Type of Specialization
    /// </summary>
    public class SkillSpecialization : IHasName, IHasXmlNode
    {
        private Guid _guiID;
        private string _strName;
        private readonly bool _blnFree;
        private readonly bool _blnExpertise;
        private readonly Character _objCharacter;

        #region Constructor, Create, Save, Load, and Print Methods

        public SkillSpecialization(Character objCharacter, string strName, bool blnFree = false, bool blnExpertise = false)
        {
            _objCharacter = objCharacter;
            _strName = _objCharacter.ReverseTranslateExtra(strName, GlobalSettings.Language, "skills.xml");
            _guiID = Guid.NewGuid();
            _blnFree = blnFree;
            _blnExpertise = blnExpertise;
        }

        /// <summary>
        /// Save the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        public void Save(XmlTextWriter objWriter)
        {
            if (objWriter == null)
                return;
            objWriter.WriteStartElement("spec");
            objWriter.WriteElementString("guid", _guiID.ToString("D", GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("name", _strName);
            objWriter.WriteElementString("free", _blnFree.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("expertise", _blnExpertise.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteEndElement();
        }

        /// <summary>
        /// Re-create a saved SkillSpecialization from an XmlNode;
        /// </summary>
        /// <param name="objCharacter">Character to load for.</param>
        /// <param name="xmlNode">XmlNode to load.</param>
        public static SkillSpecialization Load(Character objCharacter, XmlNode xmlNode)
        {
            string strName = string.Empty;
            if (!xmlNode.TryGetStringFieldQuickly("name", ref strName) || string.IsNullOrEmpty(strName))
                return null;
            if (!xmlNode.TryGetField("guid", Guid.TryParse, out Guid guiTemp))
                guiTemp = Guid.NewGuid();

            return new SkillSpecialization(objCharacter, strName, xmlNode["free"]?.InnerText == bool.TrueString, xmlNode["expertise"]?.InnerText == bool.TrueString)
            {
                _guiID = guiTemp
            };
        }

        /// <summary>
        /// Print the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        /// <param name="objCulture">Culture in which to print.</param>
        /// <param name="strLanguageToPrint">Language in which to print.</param>
        public void Print(XmlTextWriter objWriter, CultureInfo objCulture, string strLanguageToPrint)
        {
            if (objWriter == null)
                return;
            objWriter.WriteStartElement("skillspecialization");
            objWriter.WriteElementString("guid", _guiID.ToString("D", GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("name", DisplayName(strLanguageToPrint));
            objWriter.WriteElementString("free", _blnFree.ToString(GlobalSettings.InvariantCultureInfo));
            objWriter.WriteElementString("expertise", _blnExpertise.ToString(GlobalSettings.InvariantCultureInfo));
            int intSpecializationBonus = _objCharacter.Improvements.Any(x => x.ImproveType == Improvement.ImprovementType.DisableSpecializationEffects
                                                                             && x.ImprovedName == Name && string.IsNullOrEmpty(x.Condition) && x.Enabled)
                ? 0
                : SpecializationBonus;
            objWriter.WriteElementString("specbonus", intSpecializationBonus.ToString(objCulture));
            objWriter.WriteEndElement();
        }

        #endregion Constructor, Create, Save, Load, and Print Methods

        #region Properties

        /// <summary>
        /// Internal identifier which will be used to identify this Spell in the Improvement system.
        /// </summary>
        public string InternalId => _guiID.ToString("D", GlobalSettings.InvariantCultureInfo);

        /// <summary>
        /// Skill Specialization's name.
        /// </summary>
        public string DisplayName(string strLanguage)
        {
            if (strLanguage.Equals(GlobalSettings.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                return Name;

            return GetNode(strLanguage)?.Attributes?["translate"]?.InnerText ?? Name;
        }

        /// <summary>
        /// The name of the object as it should be displayed in lists in the program's current language.
        /// </summary>
        public string CurrentDisplayName => DisplayName(GlobalSettings.Language);

        /// <summary>
        /// The Skill to which this specialization belongs
        /// </summary>
        public Skill Parent { get; set; }

        private XmlNode _objCachedMyXmlNode;
        private string _strCachedXmlNodeLanguage = string.Empty;

        public XmlNode GetNode()
        {
            return GetNode(GlobalSettings.Language);
        }

        public XmlNode GetNode(string strLanguage)
        {
            if (_objCachedMyXmlNode == null || strLanguage != _strCachedXmlNodeLanguage || GlobalSettings.LiveCustomData)
            {
                _objCachedMyXmlNode = Parent?.GetNode(strLanguage)?.SelectSingleNode("specs/spec[. = " + Name.CleanXPath() + "]");
                _strCachedXmlNodeLanguage = strLanguage;
            }
            return _objCachedMyXmlNode;
        }

        /// <summary>
        /// Skill Specialization's name.
        /// </summary>
        public string Name
        {
            get => _strName;
            set
            {
                if (_strName != value)
                {
                    _strName = value;
                    _objCachedMyXmlNode = null;
                }
            }
        }

        /// <summary>
        /// Is this a forced specialization (true) or player entered (false)
        /// </summary>
        public bool Free => _blnFree;

        /// <summary>
        /// Does this specialization give an extra bonus on top of the normal bonus that specializations give (used by SASS' Inspired and by 6e)
        /// </summary>
        public bool Expertise => _blnExpertise;

        /// <summary>
        /// The bonus this specialization gives to relevant dicepools
        /// </summary>
        public int SpecializationBonus => _objCharacter.Settings.SpecializationBonus + (Expertise ? 1 : 0);

        #endregion Properties
    }
}
