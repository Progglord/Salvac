﻿// Salvac
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

using System;
using System.Threading.Tasks;

namespace Salvac.Interface.Rendering
{
    public interface IRenderable : IDisposable
    {
        event EventHandler Updated;

        bool IsDisposed { get; }
        bool IsLoaded { get; }
        bool IsEnabled { get; set; }
        int RenderPriority { get; }

        /// <summary>
        /// This method is always being called from the rendering thread.
        /// </summary>
        Task LoadAsync();
        void Render(Viewport viewport);
    }
}
