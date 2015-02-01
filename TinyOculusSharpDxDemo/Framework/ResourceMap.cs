using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TinyOculusSharpDxDemo
{
	/// <summary>
	/// Wrapper of Dictionary<Id, DrawResource>
	/// </summary>
	/// <typeparam name="ResourceType">DrawResource</typeparam>
	public class ResourceMap<ResourceType> : Dictionary<String, ResourceType> 
		where ResourceType : ResourceBase
	{
		public void Add(ResourceType res)
		{
			Add(res.Uid, res);
		}

		public ResourceType Find(String uid)
		{
			ResourceType res = null;
			TryGetValue(uid, out res);
			return res;
		}
	}
}
