#!/usr/bin/env bash
#
# Copyright (C) 2022 Robert Yokota
#
# Licensed under the Apache License, Version 2.0 (the "License");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
#
# http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.
#

wd="$(dirname "$0")"

pid_file="${wd}/conformance-server.pid"

function kill_server() {
  if [[ -f ${pid_file} ]] ; then
    kill "$(cat "${pid_file}")" && rm "${pid_file}"
  fi
}

trap kill_server SIGINT SIGTERM

dotnet run --project "${wd}" 2>&1 | tee /tmp/conformance-server.log | grep Listening &
#echo "Listening on localhost:5000"

#(dotnet ${wd}/bin/Debug/net6.0/Cel.conformance.dll & echo $! > "${pid_file}") 2>&1 | tee /tmp/conformance-server.log | grep Listening
#dotnet "${wd}/bin/Debug/net6.0/Cel.conformance.dll" 2>&1 | tee /tmp/conformance-server.log | grep Listening

pid=$!
echo "${pid}" > "${pid_file}"
wait
