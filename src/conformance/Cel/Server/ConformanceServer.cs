using System;
using System.Collections.Generic;
using System.Collections.Generic;
using System.Threading.Tasks;
using Google.Api.Expr.Conformance.V1Alpha1;
using Grpc.Core;


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
namespace Cel.Server
{
	class Program2
	{
		const int Port = 30051;

		public static void Main2(string[] args)
		{
			Grpc.Core.Server server = new Grpc.Core.Server
			{
				Services = { ConformanceService.BindService(new ConformanceServiceImpl()) },
				Ports = { new ServerPort("localhost", Port, ServerCredentials.Insecure) }
			};
			server.Start();

			Console.WriteLine("Listening on {0}:{1}", "localhost", Port);
			server.ShutdownAsync().Wait();
			Thread.Sleep(int.MaxValue);
		}
	}
}