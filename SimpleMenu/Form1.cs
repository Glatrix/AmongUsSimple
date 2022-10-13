using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GlatrixMemory;

namespace SimpleMenu
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            UpdateTimer.Start();
        }

        public Memory mem = new Memory();
        public bool GameFound = false;

        private void UpdateTimer_Tick(object sender, EventArgs e)
        {
            //Only look for the game if we have not yet found it.
            if (!GameFound)
            {
                //If process with name "Among Us" is found, attatch to it.
                if(mem.Attatch("Among Us"))
                {
                    //If attatch success, set GameFound to true.
                    GameFound = true;
                    //let user know game was found.
                    label1.Text = "Process Found!!";
                }
            }
            //If the game was found at some point,
            else
            {
                //get Main game module base address
                IntPtr gameAsm = mem.GetModuleBaseAddress("GameAssembly.dll");
                //Read AllPlayers Pointer  [GameAssembly.dll +   01F8B900,  5C,  8,0] //0 is what is Read, so its not needed.
                IntPtr playerList = mem.Read<IntPtr>(gameAsm + 0x01F8B900,0x5C,0x8);

                //Offset for playerList.Count
                int _Size = 0x0C;
                //Read Count
                int count = mem.Read<int>(playerList + _Size);

                //Empty String (to hold player data)
                string playerListText = $"Found ({count}) Players\n\n";

                //foreach player in players
                for (int i = 0; i < count; i++)
                {
                    IntPtr playerAddy = mem.Read<IntPtr>(playerList + 0x8, (0x10 + (i * 0x4)));

                    //Username
                    string username = mem.ReadString(playerAddy + 0x3C,new int[] {0x1C, 0x80, 0xC},32, Encoding.Unicode);
                    if(username.Contains('\0'))
                        username = username.Split('\0')[0];

                    //Role
                    int roleId = mem.Read<int>(playerAddy + 0x48, 0x24, 0xC);
                    string roleName = GetRoleName(roleId);

                    //add text to the emtpy string we made before
                    playerListText += $"[{roleName}] {username} ";
                    //add a new line for the next player (if there is one)
                    playerListText += "\n";
                }
                //Set the label to the text we just made
                player_list_label.Text = playerListText;
            }
        }

        //RoleTypes by Id
        public static string GetRoleName(int roleId)
        {
            if (roleId == 0)
                return "CREWM8";
            else if (roleId == 1)
                return "IMPSTR";
            else if (roleId == 2)
                return "SCNTST";
            else if (roleId == 3)
                return "ENGNER";
            else if (roleId == 4)
                return "gANGEL";
            else if (roleId == 5)
                return "SHIFTR";

            return "CREWM8";
        }
    }
}
