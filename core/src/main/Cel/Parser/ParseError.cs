using System;

/*
 * Copyright (C) 2022 Robert Yokota
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
namespace Cel.Parser
{
    using Location = global::Cel.Common.Location;

    public sealed class ParseError : Exception
    {
        private readonly Location location;

        public ParseError(Location location, string message) : base(message)
        {
            this.location = location;
        }

        public Location Location
        {
            get { return location; }
        }
    }
}