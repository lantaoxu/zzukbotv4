﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Xml.Serialization;
using ZzukBot.Core.Game.Objects;

namespace ProfileConverter.GUI
{
    public partial class CMD : Form
    {
        public CMD()
        {
            InitializeComponent();

            FormClosing += new FormClosingEventHandler(OnFormClosing);
        }

        void ConvertProfile_Click(object sender, EventArgs e)
        {
            var serializer = new XmlSerializer(typeof(Profile));
            var profile = new Profile();

            var ofd = new OpenFileDialog()
            {
                Filter = "XML files (*.xml)|*.xml"
            };

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                using (var sr = new StreamReader(ofd.FileName))
                {
                    profile = (Profile)serializer.Deserialize(sr);
                }
            }

            var model = new ProfileModel
            {
                Hotspots = new List<Location>(),
                VendorHotspots = new List<Location>(),
                VendorName = string.Empty
            };

            if(profile.Hotspots != null)
            {
                foreach (var hotspot in profile.Hotspots)
                    model.Hotspots.Add(new Location(hotspot.X, hotspot.Y, hotspot.Z));
            }
            if(profile.VendorHotspots != null)
            {
                foreach (var vendorHotspot in profile.VendorHotspots)
                    model.VendorHotspots.Add(new Location(vendorHotspot.X, vendorHotspot.Y, vendorHotspot.Z));
            }
            if(profile.Repair.Name != null)
            {
                model.VendorName = profile.Repair.Name;
            }

            var sfd = new SaveFileDialog
            {
                Filter = "JSON files (*.json)|*.json",
                FileName = ofd.FileName   
            };
            

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                using (var sw = new StreamWriter(sfd.FileName))
                {
                    sw.Write(JsonConvert.SerializeObject(model, Formatting.Indented));
                }
            }
        }

        void OnFormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }

        class ProfileModel
        {
            public List<Location> Hotspots { get; set; }
            public List<Location> VendorHotspots { get; set; }
            public string VendorName { get; set; }
        }
    }

    public class Profile
    {
        public List<Hotspot> Hotspots { get; set; }
        public List<VendorHotspot> VendorHotspots { get; set; }
        public Repair Repair { get; set; }
    }

    public class Hotspot
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
    }

    public class VendorHotspot
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }
    }

    public class Repair
    {
        public string Name { get; set; }
    }
}
