﻿/* This file is part of libWiiSharp
 * Copyright (C) 2009 Leathl
 * Copyright (C) 2020 - 2022 TheShadowEevee, Github Contributors
 * 
 * libWiiSharp is free software: you can redistribute it and/or
 * modify it under the terms of the GNU General Public License as published
 * by the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * libWiiSharp is distributed in the hope that it will be
 * useful, but WITHOUT ANY WARRANTY; without even the implied warranty
 * of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;

namespace libWiiSharp
{
    public struct ContentIndices : IComparable
    {
        private readonly int index;
        private readonly int contentIndex;

        public int Index => index;

        public int ContentIndex => contentIndex;

        public ContentIndices(int index, int contentIndex)
        {
            this.index = index;
            this.contentIndex = contentIndex;
        }

        public int CompareTo(object obj)
        {
            if (obj is ContentIndices contentIndices)
            {
                return contentIndex.CompareTo(contentIndices.contentIndex);
            }

            throw new ArgumentException();
        }
    }
}
