using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpDX;
using SharpDX.Windows;
using SharpDX.Direct3D11;
using System.Diagnostics;

namespace TinyOculusSharpDxDemo
{
	public class DrawResourceRepository : ResourceRepository
	{
		public DrawResourceRepository(DrawSystem.D3DData d3d)
		{
			m_renderTargetMap = new ResourceMap<RenderTarget>();
			m_shaderMap = new ResourceMap<Effect>();
            //m_modelMap = new ResourceMap<DrawModel>();
			m_texMap = new ResourceMap<TextureView>();

			// get a default render target
			var renderTarget = RenderTarget.CreateDefaultRenderTarget(d3d);
			AddResource(renderTarget);
		}
		
		public void AddResource(ResourceBase res)
		{
			var type = res.GetType();
			if (type == typeof(RenderTarget))
			{
				m_renderTargetMap.Add(res as RenderTarget);
			}
			else if (type == typeof(Effect))
			{
				m_shaderMap.Add(res as Effect);
			}
			else if (type == typeof(TextureView))
			{
				m_texMap.Add(res as TextureView);
			}
			else
			{
				Debug.Assert(false, type + "is not supported");
				return;
			}

			_AddResource(res);
		}

		public ResourceType FindResource<ResourceType>(String uid)
			where ResourceType : ResourceBase
		{
			var type = typeof(ResourceType);
			if (type == typeof(RenderTarget))
			{
				return m_renderTargetMap.Find(uid) as ResourceType;
			}
			else if (type == typeof(Effect))
			{
				return m_shaderMap.Find(uid) as ResourceType;
			}
			else if (type == typeof(TextureView))
			{
				return m_texMap.Find(uid) as ResourceType;
			}
			else
			{
				Debug.Assert(false, type + "is not supported");
				return null;
			}

		}

		/// <summary>
		/// Get a defualt render target
		/// </summary>
		/// <returns>render target</returns>
		public RenderTarget GetDefaultRenderTarget()
		{
			return FindResource<RenderTarget>("Default");
		}

		#region private members

		ResourceMap<RenderTarget> m_renderTargetMap = null;
		ResourceMap<Effect> m_shaderMap = null;
        //ResourceMap<DrawModel> m_modelMap = null;
		ResourceMap<TextureView> m_texMap = null;

		#endregion // private members
	}
}
