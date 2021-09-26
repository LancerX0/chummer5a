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
using System.Windows.Forms;

namespace Chummer
{
    public partial class frmDiceHits : Form
    {
        #region Control Events

        public frmDiceHits()
        {
            InitializeComponent();
            this.UpdateLightDarkMode();
            this.TranslateWinForm();
        }

        private void frmDiceHits_Load(object sender, EventArgs e)
        {
            string strSpace = LanguageManager.GetString("String_Space");
            lblDice.Text = LanguageManager.GetString("String_DiceHits_HitsOn") + strSpace + Dice.ToString(GlobalSettings.CultureInfo)
                           + LanguageManager.GetString("String_D6") + LanguageManager.GetString("String_Colon") + strSpace;
            nudDiceResult.Maximum = Dice * 6;
            nudDiceResult.Minimum = 6;
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void cmdRoll_Click(object sender, EventArgs e)
        {
            using (new CursorWait(this))
            {
                int intResult = 0;
                for (int i = 0; i < Dice; ++i)
                {
                    intResult += GlobalSettings.RandomGenerator.NextD6ModuloBiasRemoved();
                }
                nudDiceResult.ValueAsInt = intResult;
            }
        }

        #endregion Control Events

        #region Properties

        private int _intDice;

        /// <summary>
        /// Number of dice that are rolled for the lifestyle.
        /// </summary>
        public int Dice
        {
            get => _intDice;
            set
            {
                if (_intDice == value)
                    return;
                _intDice = value;
                nudDiceResult.SuspendLayout();
                nudDiceResult.MinimumAsInt = int.MinValue; // Temporarily set this to avoid crashing if we shift from something with more than 6 dice to something with less.
                nudDiceResult.MaximumAsInt = value * 6;
                nudDiceResult.MinimumAsInt = value;
                nudDiceResult.ResumeLayout();
            }
        }

        /// <summary>
        /// Description text.
        /// </summary>
        public string Description
        {
            set => lblDescription.Text = value;
        }

        /// <summary>
        /// Dice roll result.
        /// </summary>
        public int Result => nudDiceResult.ValueAsInt;

        #endregion Properties
    }
}
