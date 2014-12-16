﻿using System;
using System.IO;
using Salvac.Data.Types;
using Salvac.Data.Profiles;
using Salvac.Data.World;
using System.Drawing;

namespace Salvac
{
    public sealed class ProfileManager : IDisposable
    {
        #region Singleton

        private static ProfileManager _current;
        public static ProfileManager Current
        {
            get
            {
                if (_current == null)
                    _current = new ProfileManager();
                return _current;
            }
        }

        #endregion

        public event EventHandler ProfileLoaded;


        public bool IsLoaded
        { get; private set; }

        public Profile Profile
        { get; private set; }


        private ProfileManager()
        {
            this.IsLoaded = false;
            this.Profile = null;
        }


        public void LoadDummyProfile()
        {
            if (!WorldManager.Current.IsLoaded) throw new NoWorldException();

            SectorView view = WorldManager.Current.Model.CreateSectorView("SECTOR_VIEW", null, 0d);
                //WorldManager.Current.Model.CreateSectorView("SECTOR_VIEW", new int[] { 5 }, 20 * Distance.MetersPerNauticalMile);

            Profile profile = new Profile(view, 4839, 5d);


            LayerTheme theme = LayerTheme.Default.Copy();
            theme.LineWidth = 0.8f;
            theme.LineColor = Color.FromArgb(40, 40, 40);

            Layer layer1 = new Layer("Controlzones", theme);
            layer1.AddFromNames(WorldManager.Current.Model, view.Name, "EDDW", "EDDV", "EDDT", "EDDH", "EDHL", "EDVE", "EDVK", "EDXW", "ETHB", "ETHC", "ETHS", "ETMN",
                "EDDK", "EDDL", "EDDM", "EDDM HX", "EDDN", "EDDP", "EDDS", "EDLN", "EDLP", "EDLW", "EDNY", "ETNW", "EDLV");

            theme = LayerTheme.Default.Copy();
            theme.LineWidth = 1.0f;
            theme.LineColor = Color.FromArgb(60, 60, 60);

            Layer layer2 = new Layer("FIR", theme);
            layer2.AddFromNames(WorldManager.Current.Model, view.Name, "EDWW N", "EDWW S", "EDWW E", "EDMM N", "EDGG W", "EDGG N", "EDMM S", "EDGG S", "EDGG N");

            profile.Layers.Add(layer1);
            profile.Layers.Add(layer2);

            this.NewProfileLoaded(profile);
        }


        private void NewProfileLoaded(Profile newProfile)
        {
            Profile old = this.Profile;
            this.Profile = newProfile;
            this.IsLoaded = true;

            if (ProfileLoaded != null)
                ProfileLoaded(this, EventArgs.Empty);

            if (old != null)
                old.Dispose();
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.Profile != null && !this.Profile.IsDisposed)
                    this.Profile.Dispose();
                this.Profile = null;
            }

            this.IsLoaded = false;
            _current = null;
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}