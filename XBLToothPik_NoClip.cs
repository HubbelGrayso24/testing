using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTA;
using GTA.Math;
using GTA.Native;
using System.Windows.Forms;
using System.IO;
using System.Runtime.InteropServices;
namespace NoClipNoVirus
{
	    public static class Globals
		{
        private static bool VSBuild = false; //true on debug only!!!
        public static Structs.SetupFile MainSettings { get; set; }
        public static Keys ActivateKey { get; set; }
        public static Keys SpeedUpKey { get; set; }
        public static Keys SpeedDownKey { get; set; }
        public static Keys MoveUpKey { get; set; }
        public static Keys MoveDownKey { get; set; }

        public static void InitGlobals()
        {
            LoadSettings();
        }
        public static void LoadSettings()
        {
            Stream setupIn = null;
            if (VSBuild)
            {
                string settingsFileName = "scripts/noclip_settings.txt";
                if (File.Exists(settingsFileName))
                    setupIn = File.Open(settingsFileName, FileMode.Open, FileAccess.Read, FileShare.Read);
                else
                {
                    setupIn = File.Create(settingsFileName);
                    Stream defaultStream = Utils.GetResourceAsStream("DefaultControlSetup.txt");
                    defaultStream.Seek(0, SeekOrigin.Begin);
                    defaultStream.CopyTo(setupIn);

                    setupIn.Flush();
                    setupIn.Seek(0, SeekOrigin.Begin);
                }

            }
            else
            {
                //ON RELEASE BUILD ONLY!!
                string settingsFileName = "scripts/noclip_settings.txt";
				string settingsData = "IyBEZWZhdWx0IENvbnRyb2wgU2V0dXANCiMga2V5IGRvY3M6IGh0dHBzOi8vbXNkbi5taWNyb3NvZnQuY29tL2VuLXVzL2xpYnJhcnkvc3lzdGVtLndpbmRvd3MuZm9ybXMua2V5cyUyOHY9dnMuMTEwJTI5LmFzcHgNCiMgWEJMVG9vdGhQaWsNCiMgTm9DbGlwTm9WaXJ1cw0KDQouS2V5cw0KQWN0aXZhdGUgPSBOdW1QYWQwDQpVcFNwZWVkID0gTnVtUGFkMg0KRG93blNwZWVkID0gTnVtUGFkMQ0KTW92ZVVwID0gTFNoaWZ0S2V5DQpNb3ZlRG93biA9IExDb250cm9sS2V5";
                byte[] settingsDataBytes = Convert.FromBase64String(settingsData);
                if (File.Exists(settingsFileName))
                    setupIn = File.Open(settingsFileName, FileMode.Open, FileAccess.Read, FileShare.Read);
                else
                {
                    //Writing default settings file, base64 encoded
                    setupIn = File.Create(settingsFileName);
                    System.IO.BinaryWriter Writer = new System.IO.BinaryWriter(setupIn);
					Writer.Write(settingsDataBytes, 0, settingsDataBytes.Length);
                    Writer.Flush();
                    Writer.BaseStream.Seek(0, SeekOrigin.Begin);
                }
            }
            MainSettings = new Structs.SetupFile(setupIn);
			setupIn.Close();
            ActivateKey = MainSettings.GetEntryByName("Keys", "Activate").KeyValue;
            SpeedUpKey = MainSettings.GetEntryByName("Keys", "UpSpeed").KeyValue;
            SpeedDownKey = MainSettings.GetEntryByName("Keys", "DownSpeed").KeyValue;
            MoveUpKey = MainSettings.GetEntryByName("Keys", "MoveUp").KeyValue;
            MoveDownKey = MainSettings.GetEntryByName("Keys", "MoveDown").KeyValue;
        }
    }
	public static class Utils
    {

        public static Vector2 ScreenRes
        {
            get
            {
				return new Vector2(GTA.UI.WIDTH, GTA.UI.HEIGHT);
            }
        }
        public static System.IO.Stream GetResourceAsStream(string resName)
        {
            return System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(string.Format("NoClipNoVirus.Resources.{0}", resName));
        }
        public static Func<double, float> DegreesToRadians = angleR => (float)(angleR * Math.PI / 180f);
    }
	public static class Structs
    {
        public class SetupFile
        {
            Dictionary<string, List<SetupFileEntry>> Entries { get; set; }

            public string GetDataByName(string section, string name)
            {
                foreach (SetupFileEntry _entry in Entries[section])
                {
                    if (_entry.Name == name)
                        return _entry.Data;
                }
                return "NO_ENTRY";
            }
            public SetupFileEntry GetEntryByName(string section, string name)
            {
                foreach (SetupFileEntry _entry in Entries[section])
                {
                    if (_entry.Name == name)
                        return _entry;
                }
                return null;
            }
            public SetupFile()
            {
                Entries = new Dictionary<string, List<SetupFileEntry>>();
            }
            public SetupFile(System.IO.Stream xIn)
            {
                Entries = new Dictionary<string, List<SetupFileEntry>>();
                ReadFromStream(xIn);
            }
            public void ReadFromStream(System.IO.Stream xIn)
            {
                StreamReader reader = new StreamReader(xIn);

                string currentSection = string.Empty;
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    if (line == null || line == "" || string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                        continue;

                    if (line.StartsWith("."))
                    {
                        Entries.Add(line.Substring(1), new List<SetupFileEntry>());
                        currentSection = line.Substring(1);
                    }
                    else
                    {
                        int assignerIndex = line.IndexOf('=');
                        string entryName = line.Substring(0, assignerIndex - 1);
                        string entryData = line.Substring(assignerIndex + 2);
                        Entries[currentSection].Add(new SetupFileEntry() { Name = entryName, Data = entryData });

                    }
                }
            }
            public class SetupFileEntry
            {
                public string Name;
                public string Data;

                public int Intvalue
                {
                    get
                    {
                        return int.Parse(Data);
                    }
                }
                public float FloatValue
                {
                    get
                    {
                        return float.Parse(Data);
                    }
                }
                public bool BoolValue
                {
                    get
                    {
                        return bool.Parse(Data);
                    }
                }
                public  System.Windows.Forms.Keys KeyValue
                {
                    get
                    {
                        return (System.Windows.Forms.Keys)Enum.Parse(typeof(System.Windows.Forms.Keys), Data);
                    }
                }
            }
        }
    }
	public abstract class Keyboard
    {
        [Flags]
        private enum KeyStates
        {
            None = 0,
            Down = 1,
            Toggled = 2
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        private static extern short GetKeyState(int keyCode);

        private static KeyStates GetKeyState(Keys key)
        {
            KeyStates state = KeyStates.None;

            short retVal = GetKeyState((int)key);

            //If the high-order bit is 1, the key is down
            //otherwise, it is up.
            if ((retVal & 0x8000) == 0x8000)
                state |= KeyStates.Down;

            //If the low-order bit is 1, the key is toggled.
            if ((retVal & 1) == 1)
                state |= KeyStates.Toggled;

            return state;
        }

        public static bool IsKeyDown(Keys key)
        {
            return KeyStates.Down == (GetKeyState(key) & KeyStates.Down);
        }

        public static bool IsKeyToggled(Keys key)
        {
            return KeyStates.Toggled == (GetKeyState(key) & KeyStates.Toggled);
        }
    }
    public class Main : Script
    {
        public Main()
        {
            Tick += this.UpdateThis;
            KeyDown += this.OnKey;
            Globals.InitGlobals();
        }

        #region Variables
        bool Activated = false;
        float RangeANDSpeed = 1.0f;
        Ped MyPed { get { return Game.Player.Character; } }
        Vehicle MyVehicle { get { return MyPed.CurrentVehicle; } }
        #endregion

        #region Handlers
        public void OnKey(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Globals.ActivateKey)
                SwitchActivation();
            if (e.KeyCode == Globals.SpeedUpKey && Activated)
                SwitchSpeed(true);
            if (e.KeyCode == Globals.SpeedDownKey && Activated)
               SwitchSpeed(false);
        }
        void UpdateThis(object sender, EventArgs e)
        {
            if (Function.Call<bool>(Hash.IS_DISABLED_CONTROL_PRESSED, 2, 203) && Function.Call<bool>(Hash.IS_DISABLED_CONTROL_JUST_PRESSED, 2, 209))
                SwitchActivation();
            if (Activated)
            {
                UpdateNoClip();
            }
        }
        #endregion

        #region Methods
        void UpdateNoClip()
        {
            //Low level function by ap ii intense

            Function.Call(Hash.DISABLE_CONTROL_ACTION, 2, 26, true);
            Function.Call(Hash.DISABLE_CONTROL_ACTION, 2, 79, true);
            Function.Call(Hash.DISABLE_CONTROL_ACTION, 2, 37, true);

            if (Function.Call<bool>(Hash.IS_DISABLED_CONTROL_JUST_PRESSED, 2, 227))
                SwitchSpeed(true);
            else if (Function.Call<bool>(Hash.IS_DISABLED_CONTROL_JUST_PRESSED, 2, 226))
                SwitchSpeed(false);


            bool UsingVehicle = false;
            int IDToFly = 0;
            if (UsingVehicle = MyPed.IsInVehicle())
                IDToFly = MyVehicle.Handle;
            else
                IDToFly = MyPed.Handle;
           

            if (!UsingVehicle)
            {
                if (Function.Call<bool>(Hash.GET_PED_STEALTH_MOVEMENT, IDToFly))
                    Function.Call(Hash.SET_PED_STEALTH_MOVEMENT, IDToFly, 0, 0);
                if (Function.Call<bool>(Hash.GET_PED_COMBAT_MOVEMENT, IDToFly))
                    Function.Call(Hash.SET_PED_COMBAT_MOVEMENT, IDToFly, 0);
            }

            Function.Call(Hash.SET_ENTITY_HEADING, IDToFly, GameplayCamera.Rotation.Z);
            GameplayCamera.RelativeHeading = 0f;
            Function.Call(Hash.SET_GAMEPLAY_CAM_RELATIVE_PITCH, GameplayCamera.RelativePitch, 0f);
            Function.Call(Hash.FREEZE_ENTITY_POSITION, IDToFly, true);

            float LeftAxisX = Function.Call<float>(Hash.GET_CONTROL_NORMAL, 2, 218);
            float LeftAxisY = Function.Call<float>(Hash.GET_CONTROL_NORMAL, 2, 219);
            float lt = Function.Call<float>(Hash.GET_CONTROL_NORMAL, 2, 207);
            float rt = Function.Call<float>(Hash.GET_CONTROL_NORMAL, 2, 208);

            float newLeftAxisY = LeftAxisY * -1.0f * RangeANDSpeed;
            float newLt = lt * -1.0f * RangeANDSpeed;
            float newRt = rt * RangeANDSpeed + newLt;

            if (!Keyboard.IsKeyDown(Globals.MoveUpKey) && !Keyboard.IsKeyDown(Globals.MoveDownKey))
            {
                if (LeftAxisX == 0 && LeftAxisY == 0 && lt == 0 && rt == 0)
                    return;
            }
            else
            {
                if (Keyboard.IsKeyDown(Globals.MoveUpKey))
                    newRt = 1.0f * RangeANDSpeed;
                if (Keyboard.IsKeyDown(Globals.MoveDownKey))
                    newRt = -1.0f * RangeANDSpeed;
            }

            Vector3 unitCircle = Function.Call<Vector3>(Hash.GET_OFFSET_FROM_ENTITY_IN_WORLD_COORDS, IDToFly, LeftAxisX * RangeANDSpeed, newLeftAxisY, newRt);
            Function.Call(Hash.SET_ENTITY_COORDS_NO_OFFSET, IDToFly, unitCircle.X, unitCircle.Y, unitCircle.Z, 0, 0, 0);
        }

        void SwitchActivation()
        {
            GTA.UI.ShowSubtitle(string.Format("No Clip {0}!", ((Activated = !Activated) ? "~g~Activated" : "~r~Deactivated")));
            Function.Call(Hash.SET_PED_STEALTH_MOVEMENT, MyPed.Handle, 0, 0);
            Entity EntityToMod = MyPed.IsInVehicle() ? (Entity)MyPed.CurrentVehicle : (Entity)MyPed;
            if (!Activated)
            {
                EntityToMod.FreezePosition = false;
                if (MyPed.IsInVehicle())
                    EntityToMod.ApplyForce(Vector3.Zero);
            }
            else
                EntityToMod.Position = EntityToMod.Position;
                
        }
        void SwitchSpeed(bool inc)
        {
            float[] speeds = { 0.1f, 0.5f, 1.0f, 4.0f, 8.0f, 16.0f, 64.0f };
            string[] speedNames = { "Slowest", "Slow", "Normal", "Fast", "Super", "Extreme", "Beyond Extreme" };
            int currentSpeed = Array.IndexOf(speeds, RangeANDSpeed);
            if (inc)
            {
                if (currentSpeed == speeds.Length - 1)
                    currentSpeed = 0;
                else
                    currentSpeed++;
            }
            else
            {
                if (currentSpeed == 0)
                    currentSpeed = speeds.Length - 1;
                else
                    currentSpeed--;
            }
            RangeANDSpeed = speeds[currentSpeed];
            GTA.UI.ShowSubtitle(string.Format("Speed: ~g~{0}", speedNames[currentSpeed]));
        }
        #endregion
    }
}
