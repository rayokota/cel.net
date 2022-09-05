using System;
using System.Collections.Generic;
using System.Threading;

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
	using Server = io.grpc.Server;
	using ServerBuilder = io.grpc.ServerBuilder;

	public class ConformanceServer : AutoCloseable
	{

	  private readonly Server server;

	  public ConformanceServer(Server server)
	  {
		this.server = server;
	  }

	  public static string GetListenHost(Server server)
	  {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in C#:
//ORIGINAL LINE: java.util.List<? extends java.net.SocketAddress> addrs = server.getListenSockets();
		IList<SocketAddress> addrs = server.getListenSockets();
		SocketAddress addr = addrs[0];
		InetSocketAddress ia = (InetSocketAddress) addr;
		InetAddress a = ia.getAddress();

		string host;
		if (a is Inet6Address)
		{
		  if (a.isAnyLocalAddress())
		  {
			host = "::1";
		  }
		  else
		  {
			host = a.getCanonicalHostName();
		  }
		}
		else
		{
		  if (a.isAnyLocalAddress())
		  {
			host = "127.0.0.1";
		  }
		  else
		  {
			host = a.getCanonicalHostName();
		  }
		}

		return host;
	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void blockUntilShutdown() throws InterruptedException
	  public virtual void BlockUntilShutdown()
	  {
		server.awaitTermination();
	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: @Override public void close() throws Exception
	  public override void close()
	  {
		server.shutdown().awaitTermination();
	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static void main(String[] args) throws Exception
	  public static void Main(string[] args)
	  {
		ConformanceServiceImpl service = new ConformanceServiceImpl();

		foreach (string arg in args)
		{
		  if ("--verbose".Equals(arg) || "-v".Equals(arg))
		  {
			service.VerboseEvalErrors = true;
		  }
		}

		Server c = ServerBuilder.forPort(0).addService(service).build();

		Thread hook = new Thread(c.shutdown);

		try
		{
				using (ConformanceServer cs = new ConformanceServer(c.start()))
				{
//JAVA TO C# CONVERTER TODO TASK: The following line has a Java format specifier which cannot be directly translated to .NET:
			  Console.Write("Listening on {0}:{1:D}%n", GetListenHost(cs.server), cs.server.getPort());
        
			  Runtime.getRuntime().addShutdownHook(hook);
			  cs.BlockUntilShutdown();
				}
		}
		finally
		{
		  try
		  {
			Runtime.getRuntime().removeShutdownHook(hook);
		  }
		  catch (System.InvalidOperationException)
		  {
			// ignore (might happen, when a JVM shutdown is already in progress)
		  }
		}
	  }
	}

}