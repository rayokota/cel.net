using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using NodaTime;
using NodaTime.Text;

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
namespace Cel.Common.Types.Json.Types
{
	public class MetaTest
	{
		public string Hash {get; set; }

	  public string Committer {get; set; }

	  public string Author {get; set; }

	  public string SignedOffBy {get; set; }

	  public string Message {get; set; }

	  [JsonConverter(typeof(InstantConverter))]
	  public Instant? CommitTime {get; set; }

	  [JsonConverter(typeof(InstantConverter))]
	  public Instant? AuthorTime {get; set; }

	  public IDictionary<string, string> Properties { get; set; } = new Dictionary<string, string>();

	  public class InstantConverter : JsonConverter<Instant>
	  {
		public override void WriteJson(JsonWriter writer, Instant value, JsonSerializer serializer)
		{
			writer.WriteValue(InstantPattern.General.Format(value));
		}
		
		public override Instant ReadJson(JsonReader reader, Type objectType, Instant existingValue, bool hasExistingValue, JsonSerializer serializer)
		{
			return InstantPattern.General.Parse((string)reader.Value).GetValueOrThrow();
		}
	  }
	}

}