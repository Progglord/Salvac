// Salvac
// Copyright (C) 2014 Oliver Schmidt
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
using Salvac.Data.World;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Salvac.Data.Profiles
{
    public class Layer : IDisposable
    {
        private string _name;
        public string Name
        {
            get { return _name; }
            set
            {
                if (string.IsNullOrEmpty(value)) throw new ArgumentNullException("value");
                _name = value;
            }
        }

        private LayerTheme _theme;
        public LayerTheme Theme
        {
            get { return _theme; }
            set
            {
                if (value == null) throw new ArgumentNullException("value");
                _theme = value;
            }
        }

        private SortedEnablingSet<long> _content;
        public SortedEnablingSet<long> Content
        {
            get { return _content; }
            set
            {
                if (value == null) throw new ArgumentNullException("value");
                _content = value;
            }
        }


        public Layer(string name, SortedEnablingSet<long> content, LayerTheme theme)
        {
            this.Name = name;
            this.Theme = theme;
            this.Content = content;
        }

        public Layer(string name, LayerTheme theme) :
            this(name, new SortedEnablingSet<long>(Comparer<long>.Default), theme)
        { }


        public void AddFromNames(WorldModel model, string table, params string[] names)
        {
            using (var command = model.CreateCommand())
            {
                command.CommandText = string.Format("SELECT id FROM {0} WHERE name IN ({1})", table, String.Join(",", names.Select(s => "\"" + s + "\"")));
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        long id = (long)reader["id"];
                        this.Content.Add(id);
                    }
                }
            }
        }

        
        private void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.Theme != null)
                    this.Theme.Dispose();
                this.Theme = null;
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
