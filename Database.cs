﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using GTANetworkServer;

namespace RPGResource
{
    public static class Database
    {
        public const string ACCOUNT_FOLDER = "cnc_accounts";

        public static void Init()
        {
            if (!Directory.Exists(ACCOUNT_FOLDER))
                Directory.CreateDirectory(ACCOUNT_FOLDER);

            API.Public.consoleOutput("Database initialized!");
        }

        public static bool DoesAccountExist(string name)
        {
            var path = Path.Combine(ACCOUNT_FOLDER, name);
            return Directory.Exists(path);
        }

        public static bool IsPlayerLoggedIn(Client player)
        {
            return API.Public.getLocalEntityData(player, "LOGGED_IN") == true;
        }

        public static void CreatePlayerAccount(Client player, string password)
        {
            var path = Path.Combine(ACCOUNT_FOLDER, player.SocialClubName);

            //if (!path.StartsWith(Directory.GetCurrentDirectory())) return;

            var data = new PlayerData()
            {
                SocialClubName = player.SocialClubName,
                Password = API.Public.getHashSHA256(password),
            };

            var ser = API.Public.toJson(data);

            File.WriteAllText(path, ser);
        }

        public static bool TryLoginPlayer(Client player, string password)
        {
            var path = Path.Combine(ACCOUNT_FOLDER, player.SocialClubName);

            //if (!path.StartsWith(Directory.GetCurrentDirectory())) return false;

            var txt = File.ReadAllText(path);

            PlayerData playerObj = API.Public.fromJson(txt).ToObject<PlayerData>();

            return API.Public.getHashSHA256(password) == playerObj.Password;
        }

        public static void LoadPlayerAccount(Client player)
        {
            var path = Path.Combine(ACCOUNT_FOLDER, player.SocialClubName);

            //if (!path.StartsWith(Directory.GetCurrentDirectory())) return;

            var txt = File.ReadAllText(path);

            PlayerData playerObj = API.Public.fromJson(txt).ToObject<PlayerData>();

            API.Public.setLocalEntityData(player, "LOGGED_IN", true);

            foreach (var property in typeof(PlayerData).GetProperties())
            {
                if (property.GetCustomAttributes(typeof (XmlIgnoreAttribute), false).Length > 0) continue;

                API.Public.setLocalEntityData(player, property.Name, property.GetValue(playerObj, null));
            }
        }

        public static void SavePlayerAccount(Client player)
        {
            var path = Path.Combine(ACCOUNT_FOLDER, player.SocialClubName);

            //if (!path.StartsWith(Directory.GetCurrentDirectory())) return;

            if (!File.Exists(path)) return;

            var old = API.Public.fromJson(File.ReadAllText(path));

            var data = new PlayerData()
            {
                SocialClubName = player.SocialClubName,
                Password = old.Password,
            };

            foreach (var property in typeof(PlayerData).GetProperties())
            {
                if (property.GetCustomAttributes(typeof(XmlIgnoreAttribute), false).Length > 0) continue;

                if (API.Public.hasLocalEntityData(player, property.Name))
                {
                    property.SetValue(data, API.Public.getLocalEntityData(player, property.Name), null);
                }
            }

            var ser = API.Public.toJson(data);

            File.WriteAllText(path, ser);
        }
    }

    public class PlayerData
    {
        [XmlIgnore]
        public string SocialClubName { get; set; }
        [XmlIgnore]
        public string Password { get; set; }

        public int Level { get; set; }
        public int WantedLevel { get; set; }
        public int Money { get; set; }
        public List<int> Crimes { get; set; } 
        public bool Jailed { get; set; }
        public uint JailTime { get; set; }
    }
}
