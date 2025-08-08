using OWOGame;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using UnityEngine;

namespace OWO_PEAK
{
    public class OWOSkin
    {
        private bool suitEnabled = false;
        private string modPath = "BepInEx\\Plugins";
        private Dictionary<String, Sensation> sensationsMap = new Dictionary<String, Sensation>();
        private Dictionary<String, Muscle[]> muscleMap = new Dictionary<String, Muscle[]>();

        public bool climbing = false;
        public bool climbingRope = false;
        private bool slipping = false;
        private bool heartBeatIsActive = false;
        private bool interactIsActive = false;

        public bool teleportIsActive = false;
        public bool rainingIsActive = false;

        public Dictionary<string, Sensation> SensationsMap { get => sensationsMap; set => sensationsMap = value; }

        public OWOSkin()
        {
            RegisterAllSensationsFiles();
            DefineAllMuscleGroups();
            InitializeOWO();
        }

        public void LOG(String msg)
        {
            Plugin.Log.LogInfo(msg);
        }

        #region Skin Configuration

        private void RegisterAllSensationsFiles()
        {
            string configPath = $"{modPath}\\OWO";
            DirectoryInfo d = new DirectoryInfo(configPath);
            FileInfo[] Files = d.GetFiles("*.owo", SearchOption.AllDirectories);
            for (int i = 0; i < Files.Length; i++)
            {
                string filename = Files[i].Name;
                string fullName = Files[i].FullName;
                string prefix = Path.GetFileNameWithoutExtension(filename);
                if (filename == "." || filename == "..")
                    continue;
                string tactFileStr = File.ReadAllText(fullName);
                try
                {
                    Sensation test = Sensation.Parse(tactFileStr);
                    SensationsMap.Add(prefix, test);
                }
                catch (Exception e) { LOG(e.Message); }
            }
        }
        private void DefineAllMuscleGroups()
        {
            Muscle[] leftArm = { Muscle.Arm_L.WithIntensity(70), Muscle.Pectoral_L.WithIntensity(90), Muscle.Dorsal_L.WithIntensity(70) };
            muscleMap.Add("Left Arm", leftArm);

            Muscle[] rightArm = { Muscle.Arm_R.WithIntensity(70), Muscle.Pectoral_R.WithIntensity(90), Muscle.Dorsal_R.WithIntensity(70) };
            muscleMap.Add("Right Arm", rightArm);

            Muscle[] bothArms = leftArm.Concat(rightArm).ToArray();
            muscleMap.Add("Both Arms", bothArms);
        }
        private async void InitializeOWO()
        {
            LOG("Initializing OWO skin");

            var gameAuth = GameAuth.Create(AllBakedSensations()).WithId("58103236");

            OWO.Configure(gameAuth);
            string[] myIPs = GetIPsFromFile("OWO_Manual_IP.txt");
            if (myIPs.Length == 0) await OWO.AutoConnect();
            else
            {
                await OWO.Connect(myIPs);
            }

            if (OWO.ConnectionState == OWOGame.ConnectionState.Connected)
            {
                suitEnabled = true;
                LOG("OWO suit connected.");
                Feel("Landing", 0);
            }
            if (!suitEnabled) LOG("OWO is not enabled?!?!");
        }

        public BakedSensation[] AllBakedSensations()
        {
            var result = new List<BakedSensation>();

            foreach (var sensation in SensationsMap.Values)
            {
                if (sensation is BakedSensation baked)
                {
                    LOG("Registered baked sensation: " + baked.name);
                    result.Add(baked);
                }
                else
                {
                    LOG("Sensation not baked? " + sensation);
                    continue;
                }
            }
            return result.ToArray();
        }

        public string[] GetIPsFromFile(string filename)
        {
            List<string> ips = new List<string>();
            string filePath = Directory.GetCurrentDirectory() + $"\\{modPath}\\OWO" + filename;
            if (File.Exists(filePath))
            {
                LOG("Manual IP file found: " + filePath);
                var lines = File.ReadLines(filePath);
                foreach (var line in lines)
                {
                    if (IPAddress.TryParse(line, out _)) ips.Add(line);
                    else LOG("IP not valid? ---" + line + "---");
                }
            }
            return ips.ToArray();
        }

        ~OWOSkin()
        {
            LOG("Destructor called");
            DisconnectOWO();
        }

        public void DisconnectOWO()
        {
            LOG("Disconnecting OWO skin.");
            OWO.Disconnect();
        }
        #endregion

        public void Feel(String key, int Priority = 0, int intensity = 0)
        {
            if (SensationsMap.ContainsKey(key))
            {
                Sensation toSend = SensationsMap[key];

                if (intensity != 0)
                {
                    toSend = toSend.WithMuscles(Muscle.All.WithIntensity(intensity));
                }

                OWO.Send(toSend.WithPriority(Priority));
            }

            else LOG("Feedback not registered: " + key);
        }

        public void FeelWithMuscles(String key, String muscleKey = "Right Arm", int Priority = 0, int intensity = 0)
        {
            LOG($"FEEL WITH MUSCLES: {key} - {muscleKey}");

            if (!muscleMap.ContainsKey(muscleKey))
            {
                LOG("MuscleGroup not registered: " + muscleKey);
                return;
            }

            if (SensationsMap.ContainsKey(key))
            {
                if (intensity != 0)
                {
                    OWO.Send(SensationsMap[key].WithMuscles(muscleMap[muscleKey].WithIntensity(intensity)).WithPriority(Priority));
                }
                else
                {
                    OWO.Send(SensationsMap[key].WithMuscles(muscleMap[muscleKey]).WithPriority(Priority));
                }
            }

            else LOG("Feedback not registered: " + key);
        }


        public void StopAllHapticFeedback()
        {
            StopClimbing();
            StopClimbingRope();
            StopSlipping();
            StopHeartBeat();
            StopTeleporting();
            StopRaining();
            OWO.Stop();
        }

        public bool CanFeel()
        {
            return suitEnabled;
        }

        #region Loops

        #region Climbing

        public void StartClimbing()
        {
            if (climbing) return;

            climbing = true;
            ClimbingFuncAsync();
        }

        public void StopClimbing()
        {
            climbing = false;
        }

        public async Task ClimbingFuncAsync()
        {
            while (climbing)
            {
                Feel("Climbing", 0);
                await Task.Delay(1000);
            }
        }

        #endregion

        #region Climbing Rope

        public void StartClimbingRope()
        {
            if (climbingRope) return;

            climbingRope = true;
            ClimbingRopeFuncAsync();
        }

        public void StopClimbingRope()
        {
            climbingRope = false;
        }

        public async Task ClimbingRopeFuncAsync()
        {
            while (climbingRope)
            {
                Feel("Climbing Rope", 0);
                await Task.Delay(1000);
            }
        }

        #endregion

        #region Slipping
        internal void StartSlipping()
        {
            if (slipping) return;
            if (!climbing && !climbingRope) return;
            slipping = true;
            StopClimbing();
            StopClimbingRope();
            SlippingFuncAsync();
        }

        public void StopSlipping()
        {
            slipping = false;
        }

        public async Task SlippingFuncAsync()
        {
            while (slipping)
            {
                Feel("Slipping", 0);
                await Task.Delay(1300);
            }
        }

        #endregion

        #region Heartbeat

        public void StartHeartBeat()
        {
            if (heartBeatIsActive) return;

            heartBeatIsActive = true;
            HeartBeatFuncAsync();
        }

        public void StopHeartBeat()
        {
            heartBeatIsActive = false;
        }

        public async Task HeartBeatFuncAsync()
        {
            while (heartBeatIsActive)
            {
                Feel("Heart Beat", 0);
                await Task.Delay(1000);
            }
        }

        #endregion

        #region Teleporting

        public void StartTeleporting()
        {
            if (teleportIsActive) return;

            teleportIsActive = true;
            TeleportingFuncAsync();
        }

        public void StopTeleporting()
        {
            teleportIsActive = false;
        }

        public async Task TeleportingFuncAsync()
        {
            while (teleportIsActive)
            {
                Feel("Teleporting", 2);
                await Task.Delay(900);
            }
        }

        #endregion

        #region Raining

        public void StartRaining()
        {
            if (rainingIsActive) return;

            rainingIsActive = true;
            RainingFuncAsync();
        }

        public void StopRaining()
        {
            rainingIsActive = false;
        }

        public async Task RainingFuncAsync()
        {
            while (rainingIsActive)
            {
                Feel("Raining", 0);
                await Task.Delay(500);
            }
        }

        #endregion
       
        #endregion
    }
}
