/*  Tool for performing model manager tasks in Enterprise Architect for the IEC CIM standard
    Copyright (C) 2018  Martin E. Miller

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU Affero General Public License as
    published by the Free Software Foundation, either version 3 of the
    License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Affero General Public License for more details.

    You should have received a copy of the GNU Affero General Public License
    along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

using System;
using System.Windows.Forms;

namespace CIMModelManager
{
    /// <summary>
    /// Adds drop-down menus to the Enterprise Architect menu bar
    /// </summary>
    public class Main
    {
        private const string M_ToolName = "-&CIM Model Manager";
        private const string M_MenuFindEmptyDescriptions = "Find all empty normative &descriptions";
        private const string M_MenuFindSelectedEmptyDescriptions = "Find empty descriptions within selected package";
        private const string M_MenuSpacer = "-";

        public String EA_Connect(EA.Repository Repository)
        {
            // No special processing req'd
            return "";
        }

        public void EA_ShowHelp(EA.Repository Repository, string Location, string MenuName, string ItemName)
        {
            MessageBox.Show("Help for: " + MenuName + "/" + ItemName);
        }

        public object EA_GetMenuItems(EA.Repository Repository, string Location, string MenuName)
        {
            /* nb example of out parameter:
			object item;
			EA.ObjectType t = Repository.GetTreeSelectedItem(out item);
			EA.Package r = (EA.Package) item;
			*/

            switch (MenuName)
            {
                case "":
                    return M_ToolName;

                case M_ToolName:
                    string[] ar = { M_MenuFindEmptyDescriptions };
                    return ar;
            }
            return "";
        }

        private bool IsProjectOpen(EA.Repository Repository)
        {
            try
            {
                EA.Collection c = Repository.Models;
                return true;
            }
            catch
            {
                return false;
            }
        }

        public void EA_GetMenuState(EA.Repository Repository, string Location, string MenuName, string ItemName, ref bool IsEnabled, ref bool IsChecked)
        {
            if (IsProjectOpen(Repository))
            {
                IsEnabled = true;
            }
            else
                // If no open project, disable all menu options
                IsEnabled = false;
        }

        public void EA_MenuClick(EA.Repository Repository, string Location, string MenuName, string ItemName)
        {
            switch (ItemName)
            {
                case M_MenuFindEmptyDescriptions:
                    var g = new MissingDescriptions();
                    g.m_Repository = Repository;
                    g.Show();
                    g.Calculate();
                    break;
            }
        }

        public void EA_Disconnect()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
    }
}