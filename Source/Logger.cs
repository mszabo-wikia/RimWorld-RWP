// Copyright 2021 Máté Szabó
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using Verse;

namespace RWP
{
    /// <summary>
    /// Logging utility that decorates each message with a consistent prefix.
    /// </summary>
    public class Logger
    {
        private const string LogPrefix = "<color=#1a5eb8>[RWP]</color>";

        public void Message(string message) => Log.Message(LogPrefix + " " + message);

        public void Error(string message) => Log.Error(LogPrefix + " " + message);
    }
}
