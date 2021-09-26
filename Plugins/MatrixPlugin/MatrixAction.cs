using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.XPath;
using Chummer;
using Chummer.Backend.Attributes;

namespace MatrixPlugin
{
    /// <summary>
    /// Class with a representation of matrix actions
    /// it contains both skillcheck (action skillcheck and defence check)
    /// </summary>
    public class MatrixAction : INotifyPropertyChanged
    {
        /// <summary>
        /// Inner class with a representation of skillcheck
        /// as a Skill + Attribute [Limit]
        /// it also has a modifiers for dicepool of skillcheck and limit
        /// </summary>
        private class Action
        {
            public string Skill { get; set; } = "";
            public string Attribute { get; set; } = "";
            public int Modifier { get; set; } = 0;
            public string Limit { get; set; } = "";
            public int LimitModifier { get; set; } = 0;

            public Action()
            {
            }
        }

        private static readonly string[] SkillNames = { "Computer", "Software", "Cybercombat", "Hacking", "Electronic Warfare" };
        
        private readonly Action action;
        private readonly Action defenceAction;

        public MatrixAction()
        {
            action = new Action();
            defenceAction = new Action();
        }

        public MatrixAction(XPathNavigator xmlAction)
        {
            action = new Action();
            defenceAction = new Action();
            Name = extractStringFromXmlNode(xmlAction, "name");
            Type = extractStringFromXmlNode(xmlAction,"type");
            Description = extractStringFromXmlNode(xmlAction,"test/bonusstring");

            string limit = extractStringFromXmlNode(xmlAction, "test/limit");

            //Parsing SkillCheck as
            //Skill + Attribute|MatrixAttribute + Modifier vs. Skill + MatrixAttribute + Modifier
            string[] SkillCheck = xmlAction.SelectSingleNode("test/dice").Value.Split('.');
            string actionCheck = SkillCheck[0];
            string defenceCheck = SkillCheck.Length > 1 ? SkillCheck[0] : "";
            //Parse Skills
            foreach (string skill in SkillNames)
            {
                if (actionCheck.Contains(skill))
                    ActionSkill = skill;
                if (defenceCheck.Contains(skill))
                    DefenceSkill = skill;
            }
            //Parse MatrixAttributes
            foreach (string skill in MatrixAttributes.MatrixAttributeStrings)
            {
                if (actionCheck.Contains(skill))
                    ActionSkill = skill;
                if (defenceCheck.Contains(skill))
                    DefenceSkill = skill;
                if (limit.Contains(skill))
                    Limit = skill;
            }
            //Parse MentalAttributes
            foreach (string attr in AttributeSection.MentalAttributes)
            {
                if (actionCheck.Contains(attr))
                    ActionAttribute = attr;
                if (defenceCheck.Contains(attr))
                    DefenceAttribute = attr;
            }
            //Parse Modifiers
            // as [-] Digits
            if (Regex.IsMatch(actionCheck, "([-]{0,1})[ ]([\\d]+)"))
            {
                Match match = Regex.Match(actionCheck, "([-]{0,1})[ ]([\\d]+)");
                string result = match.Groups[1].Value + match.Groups[2].Value;
                ActionModifier = int.Parse(result);
            }
            if (Regex.IsMatch(defenceCheck, "([-]{0,1})[ ]([\\d]+)"))
            {
                Match match = Regex.Match(defenceCheck, "([-]{0,1})[ ]([\\d]+)");
                string result = match.Groups[1].Value + match.Groups[2].Value;
                DefenceModifier = int.Parse(result);
            }
        }

        private string extractStringFromXmlNode(XPathNavigator xmlAction,string path)
        {
            var xmlType = xmlAction.SelectSingleNode(path);
            if (xmlType != null)
                return xmlType.Value;
            return "";
        }

        #region Properties
        public string Name { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public string ActionSkill
        {
            get => action.Skill;
            set
            {
                action.Skill = value;
                OnPropertyChanged();
            }
        }
        public string ActionAttribute
        {
            get => action.Attribute;
            set
            {
                action.Attribute = value;
                OnPropertyChanged();
            }
        }
        public int ActionModifier
        {
            get => action.Modifier;
            set
            {
                action.Modifier = value;
                OnPropertyChanged();
            }
        }
        public string Limit
        {
            get => action.Limit;
            set
            {
                action.Limit = value;
                OnPropertyChanged();
            }
        }
        public int LimitModifier
        {
            get => action.LimitModifier;
            set
            {
                action.LimitModifier = value;
                OnPropertyChanged();
            }
        }
        public string DefenceSkill
        {
            get => defenceAction.Skill;
            set
            {
                defenceAction.Skill = value;
                OnPropertyChanged();
            }
        }
        public string DefenceAttribute
        {
            get => defenceAction.Attribute;
            set
            {
                defenceAction.Attribute = value;
                OnPropertyChanged();
            }
        }
        public int DefenceModifier
        {
            get => defenceAction.Modifier;
            set
            {
                defenceAction.Modifier = value;
                OnPropertyChanged();
            }
        }
        #endregion
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

    }
}
