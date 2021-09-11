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

namespace RWP.Cache
{
    public class PrisonerState
    {
        public bool HasHungryPrisoner { get; set; } = false;

        public bool HasPrisonerForRelease { get; set; } = false;

        public bool HasPrisonerForExecution { get; set; } = false;

        public bool HasInteractablePrisoner { get; set; } = false;

        public bool HasPrisonerToBeFed { get; set; } = false;

        public bool HasPrisonerWithoutBed { get; set; } = false;
    }
}
