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
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using System.Xml;
using NLog;

namespace Chummer
{
    /// <summary>
    /// An Art.
    /// </summary>
    [DebuggerDisplay("{DisplayName(GlobalOptions.DefaultLanguage)}")]
    public class Art : IHasInternalId, IHasName, IHasXmlNode, IHasNotes, ICanRemove, IHasSource
    {
        private static Logger Log { get; } = LogManager.GetCurrentClassLogger();
        private Guid _guiID;
        private Guid _guiSourceID;
        private SourceString _objCachedSourceDetail;
        private string _strName = string.Empty;
        private string _strSource = string.Empty;
        private string _strPage = string.Empty;
        private XmlNode _nodBonus;
        private int _intGrade;
        private Improvement.ImprovementSource _objImprovementSource = Improvement.ImprovementSource.Art;
        private string _strNotes = string.Empty;
        private Color _colNotes = ColorManager.HasNotesColor;

        private readonly Character _objCharacter;

        #region Constructor, Create, Save, Load, and Print Methods

        public Art(Character objCharacter)
        {
            // Create the GUID for the new art.
            _guiID = Guid.NewGuid();
            _objCharacter = objCharacter;
        }

        /// Create an Art from an XmlNode.
        /// <param name="objXmlArtNode">XmlNode to create the object from.</param>
        /// <param name="objSource">Source of the Improvement.</param>
        public void Create(XmlNode objXmlArtNode, Improvement.ImprovementSource objSource)
        {
            if (!objXmlArtNode.TryGetField("id", Guid.TryParse, out _guiSourceID))
            {
                Log.Warn(new object[] { "Missing id field for xmlnode", objXmlArtNode });
                Utils.BreakIfDebug();
            }
            if (objXmlArtNode.TryGetStringFieldQuickly("name", ref _strName))
                _objCachedMyXmlNode = null;
            objXmlArtNode.TryGetStringFieldQuickly("source", ref _strSource);
            objXmlArtNode.TryGetStringFieldQuickly("page", ref _strPage);
            _objImprovementSource = objSource;
            if (!objXmlArtNode.TryGetMultiLineStringFieldQuickly("altnotes", ref _strNotes))
                objXmlArtNode.TryGetMultiLineStringFieldQuickly("notes", ref _strNotes);

            String sNotesColor = ColorTranslator.ToHtml(ColorManager.HasNotesColor);
            objXmlArtNode.TryGetStringFieldQuickly("notesColor", ref sNotesColor);
            _colNotes = ColorTranslator.FromHtml(sNotesColor);

            objXmlArtNode.TryGetInt32FieldQuickly("grade", ref _intGrade);
            _nodBonus = objXmlArtNode["bonus"];
            if (_nodBonus != null)
            {
                if (!ImprovementManager.CreateImprovements(_objCharacter, objSource, _guiID.ToString("D", GlobalOptions.InvariantCultureInfo), _nodBonus, 1, DisplayNameShort(GlobalOptions.Language)))
                {
                    _guiID = Guid.Empty;
                    return;
                }
                if (!string.IsNullOrEmpty(ImprovementManager.SelectedValue))
                    _strName += LanguageManager.GetString("String_Space") + '(' + ImprovementManager.SelectedValue + ')';
            }
            /*
            if (string.IsNullOrEmpty(_strNotes))
            {
                _strNotes = CommonFunctions.GetTextFromPdf(_strSource + ' ' + _strPage, _strName);
                if (string.IsNullOrEmpty(_strNotes))
                {
                    _strNotes = CommonFunctions.GetTextFromPdf(Source + ' ' + DisplayPage(GlobalOptions.Language), CurrentDisplayName);
                }
            }
            */
        }

        /// <summary>
        /// Save the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        public void Save(XmlTextWriter objWriter)
        {
            if (objWriter == null)
                return;
            objWriter.WriteStartElement("art");
            objWriter.WriteElementString("sourceid", SourceIDString);
            objWriter.WriteElementString("guid", InternalId);
            objWriter.WriteElementString("name", _strName);
            objWriter.WriteElementString("source", _strSource);
            objWriter.WriteElementString("page", _strPage);
            objWriter.WriteElementString("grade", _intGrade.ToString(GlobalOptions.InvariantCultureInfo));
            if (_nodBonus != null)
                objWriter.WriteRaw(_nodBonus.OuterXml);
            else
                objWriter.WriteElementString("bonus", string.Empty);
            objWriter.WriteElementString("improvementsource", _objImprovementSource.ToString());
            objWriter.WriteElementString("notes", System.Text.RegularExpressions.Regex.Replace(_strNotes, @"[\u0000-\u0008\u000B\u000C\u000E-\u001F]", ""));
            objWriter.WriteElementString("notesColor", ColorTranslator.ToHtml(_colNotes));
            objWriter.WriteEndElement();

            if (Grade >= 0)
                _objCharacter.SourceProcess(_strSource);
        }

        /// <summary>
        /// Load the Metamagic from the XmlNode.
        /// </summary>
        /// <param name="objNode">XmlNode to load.</param>
        public void Load(XmlNode objNode)
        {
            if (!objNode.TryGetField("guid", Guid.TryParse, out _guiID))
            {
                _guiID = Guid.NewGuid();
            }
            if (objNode.TryGetStringFieldQuickly("name", ref _strName))
                _objCachedMyXmlNode = null;
            if (!objNode.TryGetGuidFieldQuickly("sourceid", ref _guiSourceID))
            {
                XmlNode node = GetNode(GlobalOptions.Language);
                node?.TryGetGuidFieldQuickly("id", ref _guiSourceID);
            }
            objNode.TryGetStringFieldQuickly("source", ref _strSource);
            objNode.TryGetStringFieldQuickly("page", ref _strPage);
            _nodBonus = objNode["bonus"];
            if (objNode["improvementsource"] != null)
                _objImprovementSource = Improvement.ConvertToImprovementSource(objNode["improvementsource"].InnerText);

            objNode.TryGetInt32FieldQuickly("grade", ref _intGrade);
            objNode.TryGetMultiLineStringFieldQuickly("notes", ref _strNotes);

            String sNotesColor = ColorTranslator.ToHtml(ColorManager.HasNotesColor);
            objNode.TryGetStringFieldQuickly("notesColor", ref sNotesColor);
            _colNotes = ColorTranslator.FromHtml(sNotesColor);
        }

        /// <summary>
        /// Print the object's XML to the XmlWriter.
        /// </summary>
        /// <param name="objWriter">XmlTextWriter to write with.</param>
        /// <param name="strLanguageToPrint">Language in which to print</param>
        public void Print(XmlTextWriter objWriter, string strLanguageToPrint)
        {
            if (objWriter == null)
                return;
            objWriter.WriteStartElement("art");
            objWriter.WriteElementString("guid", InternalId);
            objWriter.WriteElementString("sourceid", SourceIDString);
            objWriter.WriteElementString("name", DisplayNameShort(strLanguageToPrint));
            objWriter.WriteElementString("fullname", DisplayName(strLanguageToPrint));
            objWriter.WriteElementString("name_english", Name);
            objWriter.WriteElementString("source", _objCharacter.LanguageBookShort(Source, strLanguageToPrint));
            objWriter.WriteElementString("page", DisplayPage(strLanguageToPrint));
            objWriter.WriteElementString("improvementsource", SourceType.ToString());
            if (GlobalOptions.PrintNotes)
                objWriter.WriteElementString("notes", Notes);
            objWriter.WriteEndElement();
        }

        #endregion Constructor, Create, Save, Load, and Print Methods

        #region Properties

        /// <summary>
        /// Internal identifier which will be used to identify this Metamagic in the Improvement system.
        /// </summary>
        public string InternalId => _guiID.ToString("D", GlobalOptions.InvariantCultureInfo);

        public SourceString SourceDetail
        {
            get
            {
                if (_objCachedSourceDetail == default)
                    _objCachedSourceDetail = new SourceString(Source,
                        DisplayPage(GlobalOptions.Language), GlobalOptions.Language, GlobalOptions.CultureInfo,
                        _objCharacter);
                return _objCachedSourceDetail;
            }
        }

        /// <summary>
        /// Identifier of the object within data files.
        /// </summary>
        public Guid SourceID
        {
            get => _guiSourceID;
            set
            {
                if (_guiSourceID == value) return;
                _guiSourceID = value;
                _objCachedMyXmlNode = null;
            }
        }

        /// <summary>
        /// String-formatted identifier of the <inheritdoc cref="SourceID"/> from the data files.
        /// </summary>
        public string SourceIDString => _guiSourceID.ToString("D", GlobalOptions.InvariantCultureInfo);

        /// <summary>
        /// Bonus node from the XML file.
        /// </summary>
        public XmlNode Bonus
        {
            get => _nodBonus;
            set => _nodBonus = value;
        }

        /// <summary>
        /// ImprovementSource Type.
        /// </summary>
        public Improvement.ImprovementSource SourceType
        {
            get => _objImprovementSource;
            set => _objImprovementSource = value;
        }

        /// <summary>
        /// Metamagic name.
        /// </summary>
        public string Name
        {
            get => _strName;
            set
            {
                if (_strName != value)
                    _objCachedMyXmlNode = null;
                _strName = value;
            }
        }

        /// <summary>
        /// The name of the object as it should be displayed on printouts (translated name only).
        /// </summary>
        public string DisplayNameShort(string strLanguage)
        {
            // Get the translated name if applicable.
            if (strLanguage.Equals(GlobalOptions.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                return Name;

            return GetNode(strLanguage)?["translate"]?.InnerText ?? Name;
        }

        /// <summary>
        /// The name of the object as it should be displayed in lists. Name (Extra).
        /// </summary>
        public string DisplayName(string strLanguage)
        {
            string strReturn = DisplayNameShort(strLanguage);

            return strReturn;
        }

        public string CurrentDisplayName => DisplayName(GlobalOptions.Language);

        /// <summary>
        /// The initiate grade where the art was learned.
        /// </summary>
        public int Grade
        {
            get => _intGrade;
            set => _intGrade = value;
        }

        /// <summary>
        /// Sourcebook.
        /// </summary>
        public string Source
        {
            get => _strSource;
            set => _strSource = value;
        }

        /// <summary>
        /// Sourcebook Page Number.
        /// </summary>
        public string Page
        {
            get => _strPage;
            set => _strPage = value;
        }

        /// <summary>
        /// Sourcebook Page Number using a given language file.
        /// Returns Page if not found or the string is empty.
        /// </summary>
        /// <param name="strLanguage">Language file keyword to use.</param>
        /// <returns></returns>
        public string DisplayPage(string strLanguage)
        {
            if (strLanguage.Equals(GlobalOptions.DefaultLanguage, StringComparison.OrdinalIgnoreCase))
                return Page;
            string s = GetNode(strLanguage)?["altpage"]?.InnerText ?? Page;
            return !string.IsNullOrWhiteSpace(s) ? s : Page;
        }

        /// <summary>
        /// Notes.
        /// </summary>
        public string Notes
        {
            get => _strNotes;
            set => _strNotes = value;
        }

        /// <summary>
        /// Forecolor to use for Notes in treeviews.
        /// </summary>
        public Color NotesColor
        {
            get => _colNotes;
            set => _colNotes = value;
        }

        private XmlNode _objCachedMyXmlNode;
        private string _strCachedXmlNodeLanguage = string.Empty;

        public XmlNode GetNode()
        {
            return GetNode(GlobalOptions.Language);
        }

        public XmlNode GetNode(string strLanguage)
        {
            if (_objCachedMyXmlNode == null || strLanguage != _strCachedXmlNodeLanguage || GlobalOptions.LiveCustomData)
            {
                _objCachedMyXmlNode = _objCharacter.LoadData("metamagic.xml", strLanguage)
                    .SelectSingleNode(SourceID == Guid.Empty
                        ? "/chummer/arts/art[name = " + Name.CleanXPath() + ']'
                        : string.Format(GlobalOptions.InvariantCultureInfo,
                            "/chummer/arts/art[id = {0} or id = {1}]",
                            SourceIDString.CleanXPath(), SourceIDString.ToUpperInvariant().CleanXPath()));
                _strCachedXmlNodeLanguage = strLanguage;
            }
            return _objCachedMyXmlNode;
        }

        #endregion Properties

        #region UI Methods

        public TreeNode CreateTreeNode(ContextMenuStrip cmsArt, bool blnAddCategory = false)
        {
            if (Grade == -1 && !string.IsNullOrEmpty(Source) && !_objCharacter.Options.BookEnabled(Source))
                return null;

            string strText = CurrentDisplayName;
            if (blnAddCategory)
                strText = LanguageManager.GetString("Label_Art") + LanguageManager.GetString("String_Space") + strText;
            TreeNode objNode = new TreeNode
            {
                Name = InternalId,
                Text = strText,
                Tag = this,
                ContextMenuStrip = cmsArt,
                ForeColor = PreferredColor,
                ToolTipText = Notes.WordWrap()
            };

            return objNode;
        }

        public Color PreferredColor
        {
            get
            {
                if (!string.IsNullOrEmpty(Notes))
                {
                    return Grade == -1
                        ? ColorManager.GenerateCurrentModeDimmedColor(NotesColor)
                        : ColorManager.GenerateCurrentModeColor(NotesColor);
                }
                return Grade == -1
                    ? ColorManager.GrayText
                    : ColorManager.WindowText;
            }
        }

        #endregion UI Methods

        public bool Remove(bool blnConfirmDelete = true)
        {
            if (Grade <= 0)
                return false;
            if (blnConfirmDelete && !CommonFunctions.ConfirmDelete(LanguageManager.GetString("Message_DeleteArt")))
                return false;

            ImprovementManager.RemoveImprovements(_objCharacter, _objImprovementSource, InternalId);
            return _objCharacter.Arts.Remove(this);
        }

        public void SetSourceDetail(Control sourceControl)
        {
            if (_objCachedSourceDetail.Language != GlobalOptions.Language)
                _objCachedSourceDetail = default;
            SourceDetail.SetControl(sourceControl);
        }
    }
}
