using Chummer;
using Chummer.Plugins;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using NLog;
using Microsoft.ApplicationInsights.Channel;
using System.Diagnostics;
using System.Xml.XPath;
using System.Xml.Serialization;
using System.Xml;

namespace MatrixPlugin
{
    [DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
    public class MatrixPlugin : IPlugin
    {

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private List<MatrixAction> Actions;
        private List<Program> Softwares;

        public override string ToString()
        {
            return "Matrix Helper";
        }

        public void CustomInitialize(frmChummerMain mainControl)
        {
            try
            {
                Actions = new List<MatrixAction>();
                XmlSerializer xmlSerializerAction = new XmlSerializer(typeof(MatrixAction));
                XPathNavigator navActions = XmlManager.LoadXPath("actions.xml");
                foreach (XPathNavigator xAction in navActions.Select("/chummer/actions/action"))
                {
                    XmlReader reader = xAction.ReadSubtree();
                    MatrixAction item = (MatrixAction)xmlSerializerAction.Deserialize(reader);
                    if (!string.IsNullOrEmpty(item.ActionSkill) && !string.IsNullOrEmpty(item.ActionAttribute))
                        Actions.Add(item);
                }

                Softwares = new List<Program>();
                XPathNavigator navSoftwares = XmlManager.LoadXPath("..\\Plugins\\MatrixPlugin\\data\\program.xml");
                XPathNodeIterator iterator = navSoftwares.Select("/chummer/gears/gear");
                XmlSerializer xmlSerializerProgram = new XmlSerializer(typeof(Program));
                foreach (XPathNavigator xSoftware in iterator)
                {
                    XmlReader reader = xSoftware.ReadSubtree();
                    Program item = (Program)xmlSerializerProgram.Deserialize(reader);
                    Softwares.Add(item);
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
            }



            return;
        }

        public void Dispose()
        {
            return;
        }

        public async Task<bool> DoCharacterList_DragDrop(object sender, System.Windows.Forms.DragEventArgs dragEventArgs, System.Windows.Forms.TreeView treCharacterList)
        {
            return true;
        }

        public async Task<ICollection<System.Windows.Forms.TreeNode>> GetCharacterRosterTreeNode(frmCharacterRoster frmCharRoster, bool forceUpdate)
        {
            return null;
        }

        public IEnumerable<System.Windows.Forms.ToolStripMenuItem> GetMenuItems(System.Windows.Forms.ToolStripMenuItem menu)
        {
            yield break;
        }

        public System.Windows.Forms.UserControl GetOptionsControl()
        {
            return null;
        }

        public Assembly GetPluginAssembly()
        {
            try
            {
                return this.GetType().Assembly;
            }

            catch (Exception e)
            {
                Log.Error(e);
            }
            return null;
        }


        public string GetSaveToFileElement(Character input)
        {
            return null;
        }

        public IEnumerable<System.Windows.Forms.TabPage> GetTabPages(frmCareer input)
        {
            MatrixLogic logic = new MatrixLogic(input.CharacterObject, Actions, Softwares);
            MatrixForm FormFrom = new MatrixForm(logic);
            yield return FormFrom.MatrixTabPage;
        }

        public IEnumerable<System.Windows.Forms.TabPage> GetTabPages(frmCreate input)
        {
            yield break;
        }

        public void LoadFileElement(Character input, string fileElement)
        {
            return;
        }

        public bool ProcessCommandLine(string parameter)
        {
            return true;
        }

        public bool SetCharacterRosterNode(System.Windows.Forms.TreeNode objNode)
        {
            return true;
        }

        public void SetIsUnitTest(bool isUnitTest)
        {
            return;
        }

        public ITelemetry SetTelemetryInitialize(ITelemetry telemetry)
        {
            return telemetry;
        }

        private string GetDebuggerDisplay()
        {
            return ToString();
        }
    }
}
